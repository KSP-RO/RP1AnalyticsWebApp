(() => {
    const app = Vue.createApp(Contracts);
    const vm = app.mount('#contracts');

    window.careerSelectionChanged = function getCareerLogs(careerId) {
        console.log("Getting Logs for " + careerId + "...");

        if (!careerId) {
            document.getElementById('chart').classList.toggle('hide', true);
            vm.contracts = null;
        }
        else {
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

    function getEpochs(careerLogs) {
        console.log("Fetching epochs from entries");

        let epochs = [];

        careerLogs.forEach((entry) => {
            epochs.push(entry.startDate);
        });

        return epochs;
    }

    function getScienceEarned(careerLogs) {
        console.log("Fetching science earned from entries");
        let scienceEarned = [];
        careerLogs.forEach((entry) => {
            scienceEarned.push(entry.scienceEarned);
        });

        return scienceEarned;
    }

    function getCurrentFunds(careerLogs) {
        console.log("Fetching current funds from entries");

        let currentFunds = [];

        careerLogs.forEach((entry) => {
            currentFunds.push(entry.currentFunds);
        });

        return currentFunds;
    }

    function getVabUpgrades(careerLogs) {
        console.log("Fetching vab upgrades from entries");

        let vabUpgrades = [];

        careerLogs.forEach((entry) => {
            vabUpgrades.push(entry.vabUpgrades + entry.sphUpgrades);
        });

        return vabUpgrades;
    }

    function getRndUpgrades(careerLogs) {
        console.log("Fetching vab upgrades from entries");

        let rndUpgrades = [];

        careerLogs.forEach((entry) => {
            rndUpgrades.push(entry.rndUpgrades);
        });

        return rndUpgrades;
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

        const options = {
            chart: {
                type: "line",
                height: 700,
                width: 1800,
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
                    data: getCurrentFunds(careerPeriods),
                },
                {
                    name: "Science Earned",
                    data: getScienceEarned(careerPeriods),
                },
                {
                    name: "VAB Upgrades",
                    data: getVabUpgrades(careerPeriods),
                },
                {
                    name: "RnD Upgrades",
                    data: getRndUpgrades(careerPeriods),
                },
            ],
            xaxis: {
                type: 'datetime',
                categories: getEpochs(careerPeriods)
            },
            yaxis: [
                {
                    seriesName: 'Current Funds',
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
                    seriesName: 'RnD Upgrades',
                    show: false,
                    labels: {
                        formatter: (val) => val && val.toLocaleString('en-GB', { maximumFractionDigits: 0 })
                    }
                }
            ],
        };

        const chart = new ApexCharts(document.querySelector("#chart"), options);

        chart.render();
    }
})();
