(() => {
    const app = Vue.createApp(Contracts);
    const vm = app.mount('#contracts');

    let chart = null;

    window.addEventListener('resize', () => {
        if (!chart) return;
        chart.updateOptions({
            chart: {
                height: calculateChartHeight()
            }
        });
    });

    window.careerSelectionChanged = function getCareerLogs(careerId) {
        console.log("Getting Logs for " + careerId + "...");

        if (!careerId) {
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

            fetch(`/api/careerlogs/${careerId}/contracts`)
                .then((res) => res.json())
                .then((jsonContracts) => {
                    vm.contracts = jsonContracts;
                })
                .catch((error) => alert(error));
        }
    };

    function getValuesForField(careerLogs, fieldName) {
        let arr = [];
        careerLogs.forEach((entry) => {
            arr.push(entry[fieldName]);
        });

        return arr;
    }

    function getFundsEarned(careerLogs) {
        console.log("Fetching earned funds from entries");

        let totals = [];
        let total = 0;

        careerLogs.forEach((entry) => {
            total += entry.advanceFunds + entry.rewardFunds + entry.otherFundsEarned;
            totals.push(total);
        });

        return totals;
    }

    function getVabUpgrades(careerLogs) {
        console.log("Fetching vab upgrades from entries");

        let vabUpgrades = [];

        careerLogs.forEach((entry) => {
            vabUpgrades.push(entry.vabUpgrades + entry.sphUpgrades);
        });

        return vabUpgrades;
    }

    function getFirstSatelliteMonth(careerLogs) {

        console.log("Fetching first Artificial Satellite month");

        for (let i = 0; i < careerLogs.length - 1; i++) {
            let entry = careerLogs[i];
            if (entry.internalName === 'first_OrbitUncrewed' && entry.type === 1) {
                return {
                    month: moment.utc(entry.date).format('YYYY-MM'),
                    index: i
                };
            }
        }
        return {
            month: "not yet reached",
            index: 0
        };
    }

    function drawChart(careerLog) {
        console.log("Drawing Chart");
        const careerPeriods = careerLog.careerLogEntries;
        if (!careerPeriods) return;

        const options = {
            chart: {
                type: "line",
                height: calculateChartHeight(),
                width: '100%',
            },
            markers: {
                size: 1,
                colors: undefined,
                strokeColors: "#fff",
                strokeWidth: 1,
                strokeOpacity: 0.2,
                strokeDashArray: 0,
                fillOpacity: 1,
                shape: "circle",
                radius: 1,
                offsetX: 0,
                offsetY: 0,
                //discrete: [
                //    {
                //        seriesIndex: 0,
                //        dataPointIndex: this.getFirstSatelliteMonth(careerLog.contractEventEntries).index,
                //        strokeColors: '#red',
                //        strokeWidth: 1,
                //        strokeOpacity: 0.2,
                //        strokeDashArray: 0,
                //        fillOpacity: 1,
                //        size: 5,
                //    },
                //],
            },
            stroke: {
                show: true,
                curve: "straight",
                lineCap: "butt",
                colors: undefined,
                width: 3,
                dashArray: 0,
            },
            grid: {
                row: {
                    colors: ["#f3f3f3", "transparent"],
                    opacity: 0.5,
                },
            },
            tooltip: {
                x: {
                    format: 'yyyy-MM'
                }
            },
            series: [
                {
                    name: "Current Funds",
                    data: getValuesForField(careerPeriods, 'currentFunds'),
                },
                {
                    name: "Funds Earned",
                    data: getFundsEarned(careerPeriods),
                },
                {
                    name: "Advance Funds",
                    data: getValuesForField(careerPeriods, 'advanceFunds'),
                },
                {
                    name: "Reward Funds",
                    data: getValuesForField(careerPeriods, 'rewardFunds'),
                },
                {
                    name: "Science Earned",
                    data: getValuesForField(careerPeriods, 'scienceEarned'),
                },
                {
                    name: "VAB Upgrades",
                    data: getVabUpgrades(careerPeriods),
                },
                {
                    name: "RnD Upgrades",
                    data: getValuesForField(careerPeriods, 'rndUpgrades'),
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
                        text: 'Funds $',
                    },
                    axisTicks: {
                        show: true,
                    },
                    axisBorder: {
                        show: true,
                    },
                    labels: {
                        formatter: (val) => val && val.toLocaleString('en-GB', { maximumFractionDigits: 0 })
                    }
                },
                {
                    seriesName: 'Current Funds',
                    min: 0,
                    showAlways: true,
                    labels: {
                        show: false,
                        formatter: (val) => val && val.toLocaleString('en-GB', { maximumFractionDigits: 0 })
                    }
                },
                {
                    seriesName: 'Science Earned',
                    title: {
                        text: 'Sci',
                    },
                    show: false,
                    labels: {
                        formatter: (val) => val && val.toLocaleString('en-GB', { minimumFractionDigits: 1, maximumFractionDigits: 1 })
                    }
                },
                {
                    seriesName: 'VAB Upgrades',
                    showAlways: true,
                    opposite: true,
                    axisTicks: {
                        show: true,
                    },
                    axisBorder: {
                        show: true,
                    },
                    title: {
                        text: 'Upgrade Points',
                    },
                    labels: {
                        formatter: (val) => val && val.toLocaleString('en-GB', { maximumFractionDigits: 0 })
                    }
                },
                {
                    seriesName: 'VAB Upgrades',
                    showAlways: true,
                    labels: {
                        show: false,
                        formatter: (val) => val && val.toLocaleString('en-GB', { maximumFractionDigits: 0 })
                    }
                }
            ],
        };

        chart = new ApexCharts(document.querySelector("#chart"), options);
        chart.render();
        chart.hideSeries('Funds Earned');
        chart.hideSeries('Advance Funds');
        chart.hideSeries('Reward Funds');
    }

    function calculateChartHeight() {
        return Math.max(Math.min(window.innerHeight * 0.85, 1000), 300);
    }
})();
