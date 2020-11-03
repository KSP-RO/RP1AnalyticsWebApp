(() => {
    const ContractEventTypes = Object.freeze({ 'Accept': 0, 'Complete': 1, 'Fail': 2, 'Cancel': 3 });
    const intFormatter = new Intl.NumberFormat('en-GB', { maximumFractionDigits: 0 });
    const floatFormatter = new Intl.NumberFormat('en-GB', { minimumFractionDigits: 1, maximumFractionDigits: 1 });

    const app = Vue.createApp(Contracts);
    const vm = app.mount('#contracts');

    let contractEvents = null;
    let chart = null;
    let tooltipDiv = null;
    let monthIdx = -1;

    window.addEventListener('resize', () => {
        if (!chart) return;
        chart.updateOptions({
            chart: {
                height: calculateChartHeight()
            }
        });
    });

    const urlParams = new URLSearchParams(window.location.search);
    const initialCareerId = urlParams.get('careerId');
    if (initialCareerId) {
        document.getElementById('Career').value = initialCareerId;
        getCareerLogs(initialCareerId);
    }

    window.careerSelectionChanged = getCareerLogs;

    function getCareerLogs(careerId) {
        console.log(`Getting Logs for ${careerId}...`);

        if (!careerId) {
            contractEvents = null;
            document.getElementById('chart').classList.toggle('hide', true);
            vm.contracts = null;
        }
        else {
            chart?.destroy();

            fetch(`/api/careerlogs/${careerId}`)
                .then((res) => res.json())
                .then((jsonLogs) => drawChart(jsonLogs))
                .then(() => document.getElementById('chart').classList.toggle('hide', false))
                .catch((error) => alert(error));

            fetch(`/api/careerlogs/${careerId}/completedmilestones`)
                .then((res) => res.json())
                .then((jsonContracts) => {
                    vm.contracts = jsonContracts;
                })
                .catch((error) => alert(error));

            fetch(`/api/careerlogs/${careerId}/contracts`)
                .then((res) => res.json())
                .then((jsonContracts) => {
                    contractEvents = jsonContracts;
                })
                .catch((error) => alert(error));
        }
    }

    function getValuesForField(careerLogs, fieldName) {
        let arr = [];
        careerLogs.forEach((entry) => {
            arr.push(entry[fieldName]);
        });

        return arr;
    }

    function getFundsEarned(careerLogs) {
        let totals = [];
        let total = 0;

        careerLogs.forEach((entry) => {
            total += entry.advanceFunds + entry.rewardFunds + entry.otherFundsEarned;
            totals.push(total);
        });

        return totals;
    }

    function getVabUpgrades(careerLogs) {
        let vabUpgrades = [];

        careerLogs.forEach((entry) => {
            vabUpgrades.push(entry.vabUpgrades + entry.sphUpgrades);
        });

        return vabUpgrades;
    }

    function getFirstSciSatMonth(careerLog) {
        for (let i = 0; i < careerLog.contractEventEntries.length - 1; i++) {
            let entry = careerLog.contractEventEntries[i];
            if (entry.internalName === 'first_OrbitScience' && entry.type === ContractEventTypes.Complete) {
                const dt = moment.utc(entry.date);

                const tmp = getLogPeriodForDate(careerLog.careerLogEntries, dt);
                return {
                    month: dt.format('YYYY-MM'),
                    index: tmp.index
                };
            }
        }
        return {
            month: null,
            index: -1
        };
    }

    function getLogPeriodForDate(periods, dt) {
        const idx = periods.findIndex(c => {
            const dtStart = moment.utc(c.startDate);
            const dtEnd = moment.utc(c.endDate);
            return dt > dtStart && dt <= dtEnd;
        });
        return {
            index: idx,
            el: idx >= 0 ? periods[idx] : null
        };
    }

    function drawChart(careerLog) {
        const careerPeriods = careerLog.careerLogEntries;
        if (!careerPeriods) return;

        const options = {
            chart: {
                type: 'line',
                height: calculateChartHeight(),
                width: '100%',
                events: {
                    legendClick: function (chartContext, seriesIndex, config) {
                        window.setTimeout(appendCustomTooltipDiv, 0);
                    }
                }
            },
            markers: {
                size: 1,
                colors: undefined,
                strokeColors: '#fff',
                strokeWidth: 1,
                strokeOpacity: 0.2,
                strokeDashArray: 0,
                fillOpacity: 1,
                shape: 'circle',
                radius: 1,
                offsetX: 0,
                offsetY: 0,
                discrete: [
                    {
                        seriesIndex: 0,
                        dataPointIndex: getFirstSciSatMonth(careerLog).index,
                        fillColor: 'red',
                        size: 5
                    }
                ]
            },
            stroke: {
                show: true,
                curve: 'straight',
                lineCap: 'butt',
                colors: undefined,
                width: 3,
                dashArray: 0
            },
            grid: {
                row: {
                    colors: ['#f3f3f3', 'transparent'],
                    opacity: 0.5
                }
            },
            tooltip: {
                x: {
                    format: 'yyyy-MM'
                }
            },
            series: [
                {
                    name: 'Current Funds',
                    data: getValuesForField(careerPeriods, 'currentFunds')
                },
                {
                    name: 'Funds Earned',
                    data: getFundsEarned(careerPeriods)
                },
                {
                    name: 'Advance Funds',
                    data: getValuesForField(careerPeriods, 'advanceFunds')
                },
                {
                    name: 'Reward Funds',
                    data: getValuesForField(careerPeriods, 'rewardFunds')
                },
                {
                    name: 'Science Earned',
                    data: getValuesForField(careerPeriods, 'scienceEarned')
                },
                {
                    name: 'VAB Upgrades',
                    data: getVabUpgrades(careerPeriods)
                },
                {
                    name: 'RnD Upgrades',
                    data: getValuesForField(careerPeriods, 'rndUpgrades')
                }
            ],
            xaxis: {
                type: 'datetime',
                categories: getValuesForField(careerPeriods, 'startDate')
            },
            yaxis: [
                {
                    seriesName: 'Current Funds',
                    min: 0,
                    labels: {
                        show: false,
                        formatter: (val) => val && val.toLocaleString('en-GB', { maximumFractionDigits: 0 })
                    }
                },
                {
                    seriesName: 'Current Funds',
                    min: 0,
                    labels: {
                        show: false,
                        formatter: (val) => val && val.toLocaleString('en-GB', { maximumFractionDigits: 0 })
                    }
                },
                {
                    seriesName: 'Current Funds',
                    showAlways: true,
                    min: 0,
                    title: {
                        text: 'Funds $'
                    },
                    axisTicks: {
                        show: true
                    },
                    axisBorder: {
                        show: true
                    },
                    labels: {
                        formatter: (val) => val && intFormatter.format(val)
                    }
                },
                {
                    seriesName: 'Current Funds',
                    min: 0,
                    showAlways: true,
                    labels: {
                        show: false,
                        formatter: (val) => val && intFormatter.format(val)
                    }
                },
                {
                    seriesName: 'Science Earned',
                    title: {
                        text: 'Sci'
                    },
                    show: false,
                    labels: {
                        formatter: (val) => val && floatFormatter.format(val)
                    }
                },
                {
                    seriesName: 'VAB Upgrades',
                    showAlways: true,
                    opposite: true,
                    axisTicks: {
                        show: true
                    },
                    axisBorder: {
                        show: true
                    },
                    title: {
                        text: 'Upgrade Points'
                    },
                    labels: {
                        formatter: (val) => val && intFormatter.format(val)
                    }
                },
                {
                    seriesName: 'VAB Upgrades',
                    showAlways: true,
                    labels: {
                        show: false,
                        formatter: (val) => val && intFormatter.format(val)
                    }
                }
            ]
        };

        chart = new ApexCharts(document.querySelector('#chart'), options);
        chart.render();
        chart.hideSeries('Funds Earned');
        chart.hideSeries('Advance Funds');
        chart.hideSeries('Reward Funds');

        appendCustomTooltipDiv();
    }

    function appendCustomTooltipDiv() {
        tooltipDiv = document.createElement('div');
        tooltipDiv.className = 'apexcharts-tooltip-series-group apexcharts-active contract-info';
        tooltipDiv.style.cssText = 'order: 999; display: flex;';

        const tooltipEl = chart.w.globals.tooltip.getElTooltip();
        if (!tooltipEl.contains(tooltipDiv)) {
            tooltipEl.appendChild(tooltipDiv);
        }

        const oldFn = chart.w.globals.tooltip.tooltipLabels.drawSeriesTexts;
        chart.w.globals.tooltip.tooltipLabels.drawSeriesTexts = ({ shared = true, ttItems, i = 0, j = null, y1, y2, e }) => {
            if (monthIdx !== j && contractEvents) {
                monthIdx = j;

                const sDate = chart.opts.xaxis.categories[monthIdx];
                const dtStart = moment.utc(sDate);
                const dtEnd = dtStart.clone().add(1, 'months');

                const complete = contractEvents.filter(c => c.type === ContractEventTypes.Complete &&
                    moment.utc(c.date) > dtStart && moment.utc(c.date) <= dtEnd);
                const accept = contractEvents.filter(c => c.type === ContractEventTypes.Accept &&
                    moment.utc(c.date) > dtStart && moment.utc(c.date) <= dtEnd);
                const fail = contractEvents.filter(c => c.type === ContractEventTypes.Fail &&
                    moment.utc(c.date) > dtStart && moment.utc(c.date) <= dtEnd);
                tooltipDiv.innerHTML = createTooltipContractRow('Completed', complete) +
                    createTooltipContractRow('Accepted', accept) +
                    createTooltipContractRow('Failed', fail);
            }

            oldFn.apply(chart.w.globals.tooltip.tooltipLabels, [{ shared, ttItems, i, j, y1, y2, e }]);
        };
    }

    function createTooltipContractRow(title, contracts) {
        const res = contracts.reduce((acc, c) => acc + (acc === '' ? '' : '; ') + c.contractDisplayName, '');
        return res ? `<div class="contract-group"><span>${title}:</span>${res}</div>` : '';
    }

    function calculateChartHeight() {
        return Math.max(Math.min(window.innerHeight * 0.85, 1000), 300);
    }
})();
