function getCareerLogs() {
    console.log("Getting Logs...");

    const uuidFromForm = document.querySelector("#uuid_input").value;
    console.log(uuidFromForm);

    if (uuidFromForm !== 'null' && uuidFromForm !== 'undefined' && uuidFromForm.length > 10) {
        fetch(`/api/careerlogs/${uuidFromForm}`)
            .then((res) => res.json())
            .then((jsonLogs) => drawChart(jsonLogs))
            .catch((error) => alert(error));
    } else {
        window.alert("Please provide a valid log ID...");
    }
}

function getEpochs(careerLogs) {
    console.log("Fetching epochs from entries");

    let epochs = [];

    careerLogs.entries.forEach((entry) => {
        epochs.push(entry.epoch);
    });

    return epochs;
}

function getScienceEarned(careerLogs) {
    console.log("Fetching science earned from entries");
    let scienceEarned = [];
    careerLogs.entries.forEach((entry) => {
        scienceEarned.push(entry.scienceEarned);
    });

    return scienceEarned;
}

function getCurrentFunds(careerLogs) {
    console.log("Fetching current funds from entries");

    let currentFunds = [];

    careerLogs.entries.forEach((entry) => {
        currentFunds.push(entry.currentFunds);
    });

    return currentFunds;
}

function getVabUpgrades(careerLogs) {
    console.log("Fetching vab upgrades from entries");

    let vabUpgrades = [];

    careerLogs.entries.forEach((entry) => {
        vabUpgrades.push(entry.vabUpgrades);
    });

    return vabUpgrades;
}

function getSphUpgrades(careerLogs) {
    console.log("Fetching vab upgrades from entries");

    let sphUpgrades = [];

    careerLogs.entries.forEach((entry) => {
        sphUpgrades.push(entry.sphUpgrades);
    });

    return sphUpgrades;
}

function getRndUpgrades(careerLogs) {
    console.log("Fetching vab upgrades from entries");

    let rndUpgrades = [];

    careerLogs.entries.forEach((entry) => {
        rndUpgrades.push(entry.rndUpgrades);
    });

    return rndUpgrades;
}

function getFirstSatelliteMonth(careerLogs) {

    console.log("Fetching first Artificial Satellite month");

    for (let i = 0; i < careerLogs.entries.length - 1; i++) {
        let entry = careerLogs.entries[i];
        for (let contractEvent of entry.contractEvents) {
            if (contractEvent === 'First Artificial Satellite') {
                return {
                    month: entry.epoch,
                    index: i
                };
            }
        }
    }
    return {
        month: "not yet reached",
        index: 0
    };
}

function drawChart(careerLogs) {
    console.log("Drawing Chart");

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
            discrete: [
                {
                    seriesIndex: 0,
                    dataPointIndex: this.getFirstSatelliteMonth(careerLogs).index,
                    strokeColors: '#red',
                    strokeWidth: 1,
                    strokeOpacity: 0.2,
                    strokeDashArray: 0,
                    fillOpacity: 1,
                    size: 5,
                },
            ],
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
        series: [
            {
                name: "Science Earned",
                data: getScienceEarned(careerLogs),
            },
            {
                name: "VAB Upgrades",
                data: getVabUpgrades(careerLogs),
            },
            {
                name: "SPH Upgrades",
                data: getSphUpgrades(careerLogs),
            },
            {
                name: "RnD Upgrades",
                data: getRndUpgrades(careerLogs),
            },
            {
                name: "Current Funds",
                data: getCurrentFunds(careerLogs),
            },
        ],
        xaxis: {
            categories: getEpochs(careerLogs),
        },
        yaxis: [
            {
                title: {
                    text: 'Funds $',
                },
                axisTicks: {
                    show: true,
                },
                axisBorder: {
                    show: true,
                },
            },
            {
                seriesName: 'Total Funds',
                show: false,
            },
            {
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
            },
            {
                seriesName: 'VAB Upgrades',
                show: false,
            },
            {
                seriesName: 'VAB Upgrades',
                show: false,
            },
        ],
    };

    const chart = new ApexCharts(document.querySelector("#chart"), options);

    chart.render();
}
