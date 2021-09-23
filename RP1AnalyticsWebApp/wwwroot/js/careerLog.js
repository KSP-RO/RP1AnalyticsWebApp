(() => {
    const ContractEventTypes = Object.freeze({ 'Accept': 0, 'Complete': 1, 'Fail': 2, 'Cancel': 3 });
    const MilestonesToShowOnChart = Object.freeze({
        'first_OrbitScience': 'FSO', 'first_MoonImpact': 'Lunar Impactor', 'first_OrbitCrewed': 'Crewed Orbit',
        'first_MoonLandingCrewed': 'Crewed Moon', 'MarsLandingCrew': 'Crewed Mars'
    });

    let contractEvents = null;
    let hoverCurrentSubplotOnly = false;
    let hoverListenerSetUp = false;

    const app = Vue.createApp({
        data() {
            return {
                careerId: null,
                careerTitle: null,
                careerLogMeta: null,
                isLoadingCareerMeta: false,
                activeTab: 'milestones',
                filters: null
            };
        },
        methods: {
            reset() {
                this.careerId = null;
                this.careerLogMeta = null;
                this.isLoadingCareerMeta = false;
                this.careerTitle = null;
                this.filters = null;
            },
            handleChangeActive(tabName) {
                this.activeTab = tabName;
                const url = new URL(window.location);
                url.searchParams.set('tab', tabName);
                window.history.replaceState({}, '', url);
            },
            handleCareerChange(careerId) {
                const url = new URL(window.location);
                url.searchParams.set('careerId', careerId);
                window.history.pushState({}, '', url);
                getCareerLogs(careerId);
            },
            handleFiltersChange(filters) {
                this.filters = filters;
            }
        }
    });
    app.component('career-select', CareerSelect);
    app.component('selection-tab', SelectionTab);
    app.component('milestone-contracts', MilestoneContracts);
    app.component('repeatable-contracts', RepeatableContracts);
    app.component('tech-unlocks', TechUnlocks);
    app.component('launches', Launches);
    app.component('facilities', Facilities);
    app.component('loading-spinner', LoadingSpinner);
    app.component('meta-information', MetaInformation);
    const vm = app.mount('#appWrapper');

    const urlParams = new URLSearchParams(window.location.search);
    const initialCareerId = urlParams.get('careerId');
    if (initialCareerId) {
        getCareerLogs(initialCareerId);
    }

    const tabId = urlParams.get('tab');
    if (tabId) {
        vm.activeTab = tabId;
    }

    bindEvents();
    vm.handleFiltersChange(vmFilters.filters);

    function bindEvents() {
        document.addEventListener('keydown', event => {
            hoverCurrentSubplotOnly = event.ctrlKey;
        });
        document.addEventListener('keyup', event => {
            hoverCurrentSubplotOnly = event.ctrlKey;
        });

        window.onpopstate = event => {
            const urlParams = new URLSearchParams(window.location.search);
            const initialCareerId = urlParams.get('careerId');
            if (initialCareerId) {
                getCareerLogs(initialCareerId);
            }
            else {
                vm.reset();
            }

            const tabId = urlParams.get('tab');
            if (tabId) {
                vm.activeTab = tabId;
            }
        }

        window.filtersChanged = filters => {
            vm.handleFiltersChange(filters);
        }
    }

    function getCareerLogs(careerId) {
        console.log(`Getting Logs for ${careerId}...`);

        document.getElementById('chart').classList.toggle('is-invisible', true);

        if (!careerId) {
            contractEvents = null;
            vm.reset();
        }
        else {
            vm.careerId = careerId;
            vm.isLoadingCareerMeta = true;

            Promise.all([
                fetch(`/api/careerlogs/${careerId}`)
                    .then((res) => res.json())
                    .then((jsonLogs) => {
                        vm.isLoadingCareerMeta = false;
                        vm.careerLogMeta = jsonLogs.careerLogMeta;
                        vm.careerTitle = jsonLogs.name;
                        return jsonLogs;
                    })
                    .catch((error) => alert(error)),
                fetch(`/api/careerlogs/${careerId}/contracts`)
                    .then((res) => res.json())
                    .then((jsonContracts) => {
                        contractEvents = jsonContracts;
                        return jsonContracts;
                    })
                    .catch((error) => alert(error))
            ]).then((values) => drawChart(values[0]))
              .then((chartDrawn) => document.getElementById('chart').classList.toggle('is-invisible', !chartDrawn))
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

    function getCompletionDatesAndIndexesForContracts(careerLog, contracts) {
        let arr = [];
        for (let i = 0; i < careerLog.contractEventEntries.length - 1; i++) {
            let entry = careerLog.contractEventEntries[i];
            if (entry.type === ContractEventTypes.Complete &&
                contracts.find(c => entry.internalName === c) &&
                !arr.find(el => el.contract === entry.internalName)) {

                const dt = moment.utc(entry.date);
                const tmp = getLogPeriodForDate(careerLog.careerLogEntries, dt);
                arr.push({
                    contract: entry.internalName,
                    month: dt.format('YYYY-MM'),
                    index: tmp.index
                });
            }
        }
        return arr;
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
        if (!careerPeriods) return false;

        const currentFundsTrace = {
            name: 'Current Funds',
            y: getValuesForField(careerPeriods, 'currentFunds'),
            type: 'scattergl',
            mode: 'lines',
        };
        const advanceFundsTrace = {
            name: 'Advance Funds',
            y: getValuesForField(careerPeriods, 'advanceFunds'),
            type: 'scattergl',
            mode: 'lines',
            visible: 'legendonly',
        };
        const rewardFundsTrace = {
            name: 'Reward Funds',
            y: getValuesForField(careerPeriods, 'rewardFunds'),
            type: 'scattergl',
            mode: 'lines',
            visible: 'legendonly',
        };
        const earnedFundsTrace = {
            name: 'Earned Funds',
            y: getFundsEarned(careerPeriods),
            type: 'scattergl',
            mode: 'lines',
            visible: 'legendonly',
        };

        const scienceTrace = {
            name: 'Science Earned',
            y: getValuesForField(careerPeriods, 'scienceEarned'),
            yaxis: 'y2',
            type: 'scattergl',
            mode: 'lines',
        };

        const vabUpgradesTrace = {
            name: 'VAB Upgrades',
            y: getVabUpgrades(careerPeriods),
            yaxis: 'y3',
            type: 'scattergl',
            mode: 'lines',
        }
        const rndUpgradesTrace = {
            name: 'RnD Upgrades',
            y: getValuesForField(careerPeriods, 'rndUpgrades'),
            yaxis: 'y3',
            type: 'scattergl',
            mode: 'lines',
        }

        // A fake 'trace' for displaying contract status in the hover text.
        const contractsTrace = {
            name: 'Contracts',
            y: 0,
            text: getValuesForField(careerPeriods, 'startDate').map(genContractTooltip),
            hovertemplate: '%{text}',
            type: 'scatter',
            showlegend: false,
            marker: {
                color: '#fff0'
            }
        }

        const traces = [
            currentFundsTrace,
            advanceFundsTrace,
            rewardFundsTrace,
            earnedFundsTrace,
            scienceTrace,
            vabUpgradesTrace,
            rndUpgradesTrace,
            contractsTrace,
        ];
        traces.forEach(t => {
            t.x = getValuesForField(careerPeriods, 'startDate');
            t.connectgaps = true;
        });

        const layout = {
            hovermode: 'x unified',
            grid: {
                columns: 1,
                subplots: [['xy'], ['xy2'], ['xy3']],
                ygap: 0.1,
            },
            xaxis: {
                title: 'Date',
                type: 'date',
                autorange: true,
            },
            yaxis: {
                title: 'Funds',
                autorange: true,
                type: 'linear',
                hoverformat: '.4s',
            },
            yaxis2: {
                title: 'Science',
                autorange: true,
                type: 'linear',
                hoverformat: '.1f',
            },
            yaxis3: {
                title: 'Upgrade Points',
                autorange: true,
                type: 'linear',
            },
            font: {
                family: 'Poppins',
                size: 14,
            },
            margin: {
                t: 40,
                r: 20,
                b: 200,
                l: 80,
            },
        };

        const annotations = [];
        const contractNames = Object.keys(MilestonesToShowOnChart);
        const completionArr = getCompletionDatesAndIndexesForContracts(careerLog, contractNames);
        completionArr.forEach(el => annotations.push({
            x: el.month,
            y: getValuesForField(careerPeriods, 'currentFunds')[el.index],
            yref: 'y',
            text: MilestonesToShowOnChart[el.contract],
            arrowhead: 6,
            ax: 0,
            ay: -35,
        }));

        if (annotations.length > 0) {
            layout.annotations = annotations;
        }

        const config = {
            responsive: true,
        };

        const plotDiv = document.querySelector('#chart');
        Plotly.react(plotDiv, traces, layout, config);

        if (!hoverListenerSetUp) {
            // Display hover for all subplots.
            plotDiv.on('plotly_hover', (eventData) => {
                if (hoverCurrentSubplotOnly) return;
                if (eventData.xvals) {
                    Plotly.Fx.hover(
                        plotDiv,
                        { xval: eventData.xvals[0] },
                        ['xy', 'xy2', 'xy3']
                    );
                }
            });
            hoverListenerSetUp = true;
        }

        return true;
    }

    function genContractTooltip(xaxis) {
        const dtStart = moment.utc(xaxis);
        const dtEnd = dtStart.clone().add(1, 'months');
        const complete = contractEvents.filter(c => c.type === ContractEventTypes.Complete &&
            moment.utc(c.date) > dtStart && moment.utc(c.date) <= dtEnd);
        const accept = contractEvents.filter(c => c.type === ContractEventTypes.Accept &&
            moment.utc(c.date) > dtStart && moment.utc(c.date) <= dtEnd);
        const fail = contractEvents.filter(c => c.type === ContractEventTypes.Fail &&
            moment.utc(c.date) > dtStart && moment.utc(c.date) <= dtEnd);

        const contractList = genTooltipContractRow('Completed', complete)
            + genTooltipContractRow('Accepted', accept)
            + genTooltipContractRow('Failed', fail);
        return contractList ? `<span style='font-size:12px;'>${contractList}</span>` : 'N/A';
    };

    function genTooltipContractRow(title, contracts) {
        const groupedMap = contracts.reduce(
            (entryMap, e) => entryMap.set(e.contractInternalName, [...entryMap.get(e.contractInternalName) || [], e]),
            new Map()
        );

        const res = Array.from(groupedMap.values()).reduce(
            (acc, entry) => acc + '<br>    ' + (entry.length > 1 ? entry.length + "x " : '') + entry[0].contractDisplayName,
            ''
        );
        return res ? `<br><i>${title} :</i>${res}` : '';
    }
})();
