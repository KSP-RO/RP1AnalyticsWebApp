<template>
    <div id="chart" v-show="isVisible"></div>
</template>

<style>
    #chart {
        margin: 10px 0 25px 0;
        height: max(min(calc(100vh * 0.875), 1000px), 350px);
    }
</style>

<script lang="ts">
    import { defineComponent } from 'vue';
    import type { PropType } from 'vue'
    import { calculateYearRepMap } from '../utils/calculateYearRepMap';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import type { CareerLog, BaseContractEvent, ProgramItem, CareerLogPeriod } from 'types';
    import type { Annotations, Layout, Data } from 'plotly.js-basic-dist';
    import Plotly from 'plotly.js-basic-dist';
    import { DateTime } from 'luxon';

    interface ContractCompletionItem {
        contract: string;
        month: string;
        index: number;
    }

    const ContractEventTypes = Object.freeze({ 'Accept': 0, 'Complete': 1, 'Fail': 2, 'Cancel': 3 });
    const MilestonesToShowOnChart = Object.freeze(new Map<string, string>([
        ['FirstScienceSat', 'FSO'],
        ['FirstScienceSat-Heavy', 'FSO'],
        ['LunarImpactor', 'Lunar Impactor'],
        ['first_OrbitCrewed', 'Crewed Orbit'],
        ['first_MoonLandingCrewed', 'Crewed Moon'],
        ['first_MoonLandingCrewedDirect', 'Crewed Moon (Direct)'],
        ['MarsLandingCrew', 'Crewed Mars'],
        ['first_spaceStation', 'Space Station']
    ]));
    const yearRepMap = calculateYearRepMap();

    let hoverCurrentSubplotOnly = false;
    let hoverListenerSetUp = false;

    export default defineComponent({
        props: {
            career: Object as PropType<CareerLog>,
            contractEvents: Object as PropType<BaseContractEvent[]>,
            programs: Object as PropType<ProgramItem[]>
        },
        computed: {
            isVisible(): boolean {
                return this.career != null && this.contractEvents != null && this.programs != null;
            }
        },
        watch: {
            isVisible(newIsVisible: boolean) {
                if (newIsVisible) {
                    setTimeout(() => {
                        this.drawChart(this.career!);
                    }, 0);
                }
            }
        },
        mounted() {
            this.$nextTick(function () {
                document.addEventListener('keydown', event => {
                    hoverCurrentSubplotOnly = event.ctrlKey;
                });
                document.addEventListener('keyup', event => {
                    hoverCurrentSubplotOnly = event.ctrlKey;
                });
            });
        },
        methods: {
            getValuesForField<T>(careerPeriods: CareerLogPeriod[], fieldName: keyof CareerLogPeriod): T[] {
                let arr = new Array<T>;
                careerPeriods.forEach((entry) => {
                    arr.push(entry[fieldName] as T);
                });

                return arr;
            },

            getFundsEarned(careerPeriods: CareerLogPeriod[]) {
                let totals: number[] = [];
                let total = 0;

                careerPeriods.forEach((entry) => {
                    total += entry.programFunds + entry.otherFundsEarned;
                    totals.push(total);
                });

                return totals;
            },

            getRepCapForPeriods(careerPeriods: CareerLogPeriod[]): number[] {
                const arr = new Array<number>;
                const firstMapItem = yearRepMap.entries().next().value as Array<number>;
                careerPeriods.forEach((entry) => {
                    const dt = parseUtcDate(entry.endDate);
                    const timestamp = dt.toUnixInteger();
                    let prevKey = firstMapItem[0], prevVal = firstMapItem[1];
                    for (const [key, value] of yearRepMap) {
                        if (timestamp < key) {
                            const excess = timestamp - prevKey;
                            const range = key - prevKey;
                            const timeInRange = excess / range;
                            const approxRep = this.lerp(prevVal, value, timeInRange)
                            arr.push(approxRep);
                            return;
                        }
                        prevKey = key;
                        prevVal = value;
                    }
                    arr.push(prevVal);
                });

                return arr;
            },

            getCompletionDatesAndIndexesForContracts(careerLog: CareerLog, contractNames: string[]): ContractCompletionItem[] {
                let arr = new Array<ContractCompletionItem>;
                for (let i = 0; i < careerLog.contractEventEntries.length - 1; i++) {
                    let entry = careerLog.contractEventEntries[i];
                    if (entry.type === ContractEventTypes.Complete &&
                        contractNames.find(c => entry.internalName === c) &&
                        !arr.find(el => el.contract === entry.internalName)) {

                        const dt = parseUtcDate(entry.date);
                        const tmp = this.getLogPeriodForDate(careerLog.careerLogEntries, dt);
                        arr.push({
                            contract: entry.internalName,
                            month: dt.toFormat('yyyy-MM'),
                            index: tmp.index
                        });
                    }
                }
                return arr;
            },

            getLogPeriodForDate(periods: CareerLogPeriod[], dt: DateTime) {
                const idx = periods.findIndex(c => {
                    const dtStart = parseUtcDate(c.startDate);
                    const dtEnd = parseUtcDate(c.endDate);
                    return dt > dtStart && dt <= dtEnd;
                });
                return {
                    index: idx,
                    el: idx >= 0 ? periods[idx] : null
                };
            },

            drawChart(careerLog: CareerLog) {
                const careerPeriods = careerLog.careerLogEntries;
                if (!careerPeriods) return false;

                const programFundsTrace: Data = {
                    name: 'Program funding',
                    y: this.getValuesForField<number>(careerPeriods, 'programFunds'),
                    type: 'scattergl',
                    mode: 'lines'
                };
                const subsidySizeTrace: Data = {
                    name: 'Subsidy size',
                    y: this.getValuesForField<number>(careerPeriods, 'subsidySize'),
                    type: 'scattergl',
                    mode: 'lines'
                };
                const currentFundsTrace: Data = {
                    name: 'Current Funds',
                    y: this.getValuesForField<number>(careerPeriods, 'currentFunds'),
                    type: 'scattergl',
                    mode: 'lines',
                    visible: 'legendonly'
                };
                const earnedFundsTrace: Data = {
                    name: 'Earned Funds',
                    y: this.getFundsEarned(careerPeriods),
                    type: 'scattergl',
                    mode: 'lines',
                    visible: 'legendonly',
                    line: {
                        color: 'chartreuse'
                    }
                };

                const sciEarnedTrace: Data = {
                    name: 'Science Earned',
                    y: this.getValuesForField<number>(careerPeriods, 'scienceEarned'),
                    yaxis: 'y2',
                    type: 'scattergl',
                    mode: 'lines',
                    line: {
                        color: 'dodgerblue',
                    }
                };

                const curSciTrace: Data = {
                    name: 'Current Science',
                    y: this.getValuesForField<number>(careerPeriods, 'currentSci'),
                    yaxis: 'y2',
                    type: 'scattergl',
                    mode: 'lines',
                    visible: 'legendonly'
                };

                const repTrace: Data = {
                    name: 'Reputation',
                    y: this.getValuesForField<number>(careerPeriods, 'reputation'),
                    yaxis: 'y3',
                    type: 'scattergl',
                    mode: 'lines',
                    line: {
                        color: 'darkorange',
                    }
                };

                const repCapTrace: Data = {
                    name: 'Reputation cap',
                    y: this.getRepCapForPeriods(careerPeriods),
                    yaxis: 'y3',
                    type: 'scattergl',
                    mode: 'lines',
                    line: {
                        color: 'darkorange',
                        dash: 'dot',
                        width: 3
                    }
                };

                const confidenceTrace: Data = {
                    name: 'Confidence',
                    y: this.getValuesForField<number>(careerPeriods, 'confidence'),
                    yaxis: 'y4',
                    type: 'scattergl',
                    mode: 'lines',
                    line: {
                        color: 'fuchsia'
                    }
                };

                const engineersTrace: Data = {
                    name: 'Engineers',
                    y: this.getValuesForField<number>(careerPeriods, 'numEngineers'),
                    yaxis: 'y5',
                    type: 'scattergl',
                    mode: 'lines',
                    line: {
                        color: 'red'
                    }
                }
                const researchersTrace: Data = {
                    name: 'Researchers',
                    y: this.getValuesForField<number>(careerPeriods, 'numResearchers'),
                    yaxis: 'y5',
                    type: 'scattergl',
                    mode: 'lines',
                    line: {
                        color: 'blue'
                    }
                }
                const engEffTrace: Data = {
                    name: 'Engineer Efficiency',
                    y: this.getValuesForField<number>(careerPeriods, 'efficiencyEngineers'),
                    yaxis: 'y6',
                    type: 'scattergl',
                    mode: 'lines',
                    line: {
                        color: 'red',
                        dash: 'dot'
                    }
                }

                // A fake 'trace' for displaying contract status in the hover text.
                const contractsTrace: Data = {
                    name: 'Contracts',
                    text: this.getValuesForField<string>(careerPeriods, 'startDate').map(this.genContractTooltip),
                    hovertemplate: '%{text}',
                    type: 'scatter',
                    showlegend: false,
                    marker: {
                        color: '#fff0'
                    }
                }

                // A fake 'trace' for displaying program status in the hover text.
                const programsTrace: Data = {
                    name: 'Programs',
                    text: this.getValuesForField<string>(careerPeriods, 'startDate').map(this.genProgramTooltip),
                    hovertemplate: '%{text}',
                    type: 'scatter',
                    showlegend: false,
                    marker: {
                        color: '#fff0'
                    }
                }

                const traces = [
                    programFundsTrace,
                    subsidySizeTrace,
                    earnedFundsTrace,
                    currentFundsTrace,
                    sciEarnedTrace,
                    curSciTrace,
                    repTrace,
                    repCapTrace,
                    confidenceTrace,
                    engineersTrace,
                    researchersTrace,
                    engEffTrace,
                    contractsTrace,
                    programsTrace
                ];
                traces.forEach(t => {
                    t.x = this.getValuesForField<string>(careerPeriods, 'startDate');
                    t.connectgaps = true;
                });

                const layout: Layout = {
                    hovermode: 'x unified',
                    grid: {
                        columns: 1,
                        subplots: [['xy'], ['xy2'], ['xy3'], ['xy5']],
                        ygap: 0.1
                    },
                    xaxis: {
                        title: 'Date',
                        type: 'date',
                        autorange: true
                    },
                    yaxis: {
                        title: 'Funds',
                        autorange: true,
                        type: 'linear',
                        hoverformat: '.4s'
                    },
                    yaxis2: {
                        title: 'Science',
                        autorange: true,
                        rangemode: 'nonnegative',
                        type: 'linear',
                        hoverformat: '.1f'
                    },
                    yaxis3: {
                        title: 'Reputation',
                        autorange: true,
                        type: 'linear',
                        hoverformat: '.1f'
                    },
                    yaxis4: {
                        title: 'Confidence',
                        autorange: true,
                        rangemode: 'nonnegative',
                        showgrid: false,
                        type: 'linear',
                        hoverformat: '.1f',
                        overlaying: 'y3',
                        side: 'right'
                    },
                    yaxis5: {
                        title: 'Personnel',
                        autorange: true,
                        rangemode: 'nonnegative',
                        type: 'linear'
                    },
                    yaxis6: {
                        title: 'Efficiency',
                        tickformat: ',.0%',
                        hoverformat: ',.1%',
                        showgrid: false,
                        autorange: true,
                        rangemode: 'nonnegative',
                        type: 'linear',
                        overlaying: 'y5',
                        side: 'right'
                    },
                    font: {
                        family: 'Poppins',
                        size: 14
                    },
                    margin: {
                        t: 40,
                        r: 20,
                        b: 0,
                        l: 80,
                        pad: 4
                    }
                };

                const annotations: Array<Partial<Annotations>> = [];
                const contractNames = [...MilestonesToShowOnChart.keys()];
                const completionArr = this.getCompletionDatesAndIndexesForContracts(careerLog, contractNames);
                completionArr.forEach(el => annotations.push({
                    x: el.month,
                    y: 0,
                    yref: 'y',
                    text: MilestonesToShowOnChart.get(el.contract),
                    arrowhead: 6,
                    ax: 0,
                    ay: -35
                }));

                careerPeriods.forEach((p) => {
                    if (p.numNautsKilled > 0) {
                        const dt = parseUtcDate(p.startDate);
                        annotations.push({
                            x: dt.toFormat('yyyy-MM'),
                            y: 0,
                            yref: 'y3',
                            text: '💀',
                            arrowhead: 6,
                            ax: 0,
                            ay: -25
                        });
                    }
                });

                if (annotations.length > 0) {
                    layout.annotations = annotations;
                }

                const config: Partial<Plotly.Config> = {
                    responsive: true
                };

                const plotDiv = document.getElementById('chart')!;
                Plotly.react(plotDiv, traces, layout, config)
                    .then((plotlyEl) => {
                        if (!hoverListenerSetUp) {
                            // Display hover for all subplots.
                            plotlyEl.on('plotly_hover', (eventData) => {
                                if (hoverCurrentSubplotOnly) return;
                                if (eventData.xvals) {
                                    Plotly.Fx.hover(
                                        plotlyEl,
                                        { xval: eventData.xvals[0] },
                                        ['xy', 'xy2', 'xy3', 'xy4', 'xy5', 'xy6']
                                    );
                                }
                            });
                            hoverListenerSetUp = true;
                        }
                    });

                return true;
            },

            genContractTooltip(xaxis: string) {
                const dtStart = parseUtcDate(xaxis);
                const dtEnd = dtStart.plus({ months: 1 });
                const complete = this.contractEvents!.filter(c => c.type === ContractEventTypes.Complete &&
                    parseUtcDate(c.date) > dtStart && parseUtcDate(c.date) <= dtEnd);
                const contractList = this.genTooltipContractRow('Completed', complete);
                return contractList ? `<span style='font-size:12px;'>${contractList}</span>` : 'N/A';
            },

            genProgramTooltip(xaxis: string) {
                const dtStart = parseUtcDate(xaxis);
                const dtEnd = dtStart.plus({ months: 1 });
                const completed = this.programs!.filter(p => p.completed && parseUtcDate(p.completed) > dtStart && parseUtcDate(p.completed) <= dtEnd);
                const accepted = this.programs!.filter(p => parseUtcDate(p.accepted) > dtStart && parseUtcDate(p.accepted) <= dtEnd);

                const programList = this.genTooltipProgramRow('Completed', completed);
                const programList2 = this.genTooltipProgramRow('Accepted', accepted);
                return programList || programList2 ? `<span style='font-size:12px;'>${programList}${programList2}</span>` : 'N/A';
            },

            genTooltipContractRow(title: string, contracts: BaseContractEvent[]) {
                const groupedMap = contracts.reduce(
                    (entryMap, e) => entryMap.set(e.contractInternalName, [...entryMap.get(e.contractInternalName) || [], e]),
                    new Map()
                );

                const res = Array.from(groupedMap.values()).reduce(
                    (acc, entry) => acc + '<br>    ' + (entry.length > 1 ? entry.length + "x " : '') + entry[0].contractDisplayName,
                    ''
                );
                return res ? `<br><i>${title} :</i>${res}` : '';
            },

            genTooltipProgramRow(title: string, programs: ProgramItem[]) {
                const res = programs.reduce(
                    (acc, entry) => acc + `<br>    ${entry.title}`,
                    ''
                );
                return res ? `<br><i>${title} :</i>${res}` : '';
            },

            lerp(start: number, end: number, time: number): number {
                return (1 - time) * start + time * end;
            }
        }
    });
</script>
