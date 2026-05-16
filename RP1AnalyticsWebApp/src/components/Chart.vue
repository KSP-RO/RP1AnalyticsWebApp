<template>
    <section class="chart-panel" v-show="isVisible">
        <header class="chart-panel__header">
            <div>
                <p>Career history</p>
                <h2>Operations Timeline</h2>
            </div>
            <span>{{ periodCountLabel }}</span>
        </header>
        <div id="chart" class="chart-plot"></div>
    </section>
</template>

<style scoped>
    .chart-panel {
        --chart-ink: #17212e;
        --chart-muted: #65717f;
        --chart-line: rgba(36, 48, 63, 0.14);
        --chart-panel: rgba(255, 255, 255, 0.78);
        --chart-accent: #1d8f8a;
        border: 1px solid var(--chart-line);
        border-radius: 8px;
        margin: 1rem 0 2rem;
        padding: 1rem;
        background: var(--chart-panel);
        color: var(--chart-ink);
        box-shadow: 0 0.9rem 2rem rgba(12, 18, 26, 0.1);
    }

    .chart-panel__header {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
        align-items: end;
        margin-bottom: 0.65rem;
        border-bottom: 1px solid var(--chart-line);
        padding-bottom: 0.85rem;
    }

    .chart-panel__header p,
    .chart-panel__header h2 {
        margin: 0;
    }

    .chart-panel__header p {
        color: var(--chart-accent);
        font-size: 0.72rem;
        font-weight: 850;
        text-transform: uppercase;
    }

    .chart-panel__header h2 {
        margin-top: 0.15rem;
        font-size: 1.15rem;
        line-height: 1.1;
    }

    .chart-panel__header span {
        color: var(--chart-muted);
        font-size: 0.78rem;
        font-weight: 750;
        white-space: nowrap;
    }

    .chart-plot {
        height: max(min(calc(100vh * 0.875), 1500px), 600px);
    }

    @media (max-width: 700px) {
        .chart-panel {
            padding: 0.75rem;
        }

        .chart-panel__header {
            display: block;
        }

        .chart-panel__header span {
            display: block;
            margin-top: 0.35rem;
        }
    }

    @media (prefers-color-scheme: dark) {
        .chart-panel {
            --chart-ink: #edf3fb;
            --chart-muted: #a6b1bd;
            --chart-line: rgba(255, 255, 255, 0.13);
            --chart-panel: rgba(255, 255, 255, 0.045);
            --chart-accent: #5ed0c9;
            box-shadow: none;
        }
    }
</style>

<script setup lang="ts">
    import { computed, watch, onMounted } from 'vue';
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

    const props = defineProps<{
        career?: CareerLog;
        contractEvents?: BaseContractEvent[];
        programs?: ProgramItem[];
    }>();

    const isVisible = computed(() => props.career != null && props.contractEvents != null && props.programs != null);
    const periodCountLabel = computed(() => {
        const count = props.career?.careerLogEntries?.length ?? 0;
        return count === 1 ? '1 period' : `${new Intl.NumberFormat('en').format(count)} periods`;
    });

    function queueChartDraw() {
        setTimeout(() => {
            if (isVisible.value && props.career) {
                drawChart(props.career);
            }
        }, 0);
    }

    watch(isVisible, (newIsVisible) => {
        if (newIsVisible) {
            queueChartDraw();
        }
    }, { immediate: true });

    onMounted(() => {
        document.addEventListener('keydown', event => {
            hoverCurrentSubplotOnly = event.ctrlKey;
        });
        document.addEventListener('keyup', event => {
            hoverCurrentSubplotOnly = event.ctrlKey;
        });
    });

    function getValuesForField<T>(careerPeriods: CareerLogPeriod[], fieldName: keyof CareerLogPeriod): T[] {
        const arr = new Array<T>;
        careerPeriods.forEach(entry => arr.push(entry[fieldName] as T));
        return arr;
    }

    function getFundsEarned(careerPeriods: CareerLogPeriod[]) {
        const totals: number[] = [];
        let total = 0;
        careerPeriods.forEach(entry => {
            total += entry.programFunds + entry.otherFundsEarned;
            totals.push(total);
        });
        return totals;
    }

    function getRepCapForPeriods(careerPeriods: CareerLogPeriod[]): number[] {
        const arr = new Array<number>;
        const firstMapItem = yearRepMap.entries().next().value as Array<number>;
        careerPeriods.forEach(entry => {
            const dt = parseUtcDate(entry.endDate);
            const timestamp = dt.toUnixInteger();
            let prevKey = firstMapItem[0], prevVal = firstMapItem[1];
            for (const [key, value] of yearRepMap) {
                if (timestamp < key) {
                    const excess = timestamp - prevKey;
                    const range = key - prevKey;
                    const timeInRange = excess / range;
                    arr.push(lerp(prevVal, value, timeInRange));
                    return;
                }
                prevKey = key;
                prevVal = value;
            }
            arr.push(prevVal);
        });
        return arr;
    }

    function getCompletionDatesAndIndexesForContracts(careerLog: CareerLog, contractNames: string[]): ContractCompletionItem[] {
        const arr = new Array<ContractCompletionItem>;
        for (let i = 0; i < careerLog.contractEventEntries.length - 1; i++) {
            const entry = careerLog.contractEventEntries[i];
            if (entry.type === ContractEventTypes.Complete &&
                contractNames.find(c => entry.internalName === c) &&
                !arr.find(el => el.contract === entry.internalName)) {

                const dt = parseUtcDate(entry.date);
                const tmp = getLogPeriodForDate(careerLog.careerLogEntries, dt);
                arr.push({
                    contract: entry.internalName,
                    month: dt.toFormat('yyyy-MM'),
                    index: tmp.index
                });
            }
        }
        return arr;
    }

    function getLogPeriodForDate(periods: CareerLogPeriod[], dt: DateTime) {
        const idx = periods.findIndex(c => {
            const dtStart = parseUtcDate(c.startDate);
            const dtEnd = parseUtcDate(c.endDate);
            return dt > dtStart && dt <= dtEnd;
        });
        return {
            index: idx,
            el: idx >= 0 ? periods[idx] : null
        };
    }

    function drawChart(careerLog: CareerLog) {
        const careerPeriods = careerLog.careerLogEntries;
        if (!careerPeriods) return false;

        const programFundsTrace: Data = {
            name: 'Program funding',
            y: getValuesForField<number>(careerPeriods, 'programFunds'),
            type: 'scattergl',
            mode: 'lines'
        };
        const subsidySizeTrace: Data = {
            name: 'Subsidy size',
            y: getValuesForField<number>(careerPeriods, 'subsidySize'),
            type: 'scattergl',
            mode: 'lines'
        };
        const currentFundsTrace: Data = {
            name: 'Current Funds',
            y: getValuesForField<number>(careerPeriods, 'currentFunds'),
            type: 'scattergl',
            mode: 'lines',
            visible: 'legendonly'
        };
        const earnedFundsTrace: Data = {
            name: 'Earned Funds',
            y: getFundsEarned(careerPeriods),
            type: 'scattergl',
            mode: 'lines',
            visible: 'legendonly',
            line: { color: 'chartreuse' }
        };

        const sciEarnedTrace: Data = {
            name: 'Science Earned',
            y: getValuesForField<number>(careerPeriods, 'scienceEarned'),
            yaxis: 'y2',
            type: 'scattergl',
            mode: 'lines',
            line: { color: 'dodgerblue' }
        };

        const curSciTrace: Data = {
            name: 'Current Science',
            y: getValuesForField<number>(careerPeriods, 'currentSci'),
            yaxis: 'y2',
            type: 'scattergl',
            mode: 'lines',
            visible: 'legendonly'
        };

        const repTrace: Data = {
            name: 'Reputation',
            y: getValuesForField<number>(careerPeriods, 'reputation'),
            yaxis: 'y3',
            type: 'scattergl',
            mode: 'lines',
            line: { color: 'darkorange' }
        };

        const repCapTrace: Data = {
            name: 'Reputation cap',
            y: getRepCapForPeriods(careerPeriods),
            yaxis: 'y3',
            type: 'scattergl',
            mode: 'lines',
            line: { color: 'darkorange', dash: 'dot', width: 3 }
        };

        const confidenceTrace: Data = {
            name: 'Confidence',
            y: getValuesForField<number>(careerPeriods, 'confidence'),
            yaxis: 'y4',
            type: 'scattergl',
            mode: 'lines',
            line: { color: 'fuchsia' }
        };

        const engineersTrace: Data = {
            name: 'Engineers',
            y: getValuesForField<number>(careerPeriods, 'numEngineers'),
            yaxis: 'y5',
            type: 'scattergl',
            mode: 'lines',
            line: { color: 'red' }
        };
        const researchersTrace: Data = {
            name: 'Researchers',
            y: getValuesForField<number>(careerPeriods, 'numResearchers'),
            yaxis: 'y5',
            type: 'scattergl',
            mode: 'lines',
            line: { color: 'blue' }
        };
        const engEffTrace: Data = {
            name: 'Engineer Efficiency',
            y: getValuesForField<number>(careerPeriods, 'efficiencyEngineers'),
            yaxis: 'y6',
            type: 'scattergl',
            mode: 'lines',
            line: { color: 'red', dash: 'dot' }
        };

        const contractsTrace: Data = {
            name: 'Contracts',
            text: getValuesForField<string>(careerPeriods, 'startDate').map(genContractTooltip),
            hovertemplate: '%{text}',
            type: 'scatter',
            showlegend: false,
            marker: { color: '#fff0' }
        };

        const programsTrace: Data = {
            name: 'Programs',
            text: getValuesForField<string>(careerPeriods, 'startDate').map(genProgramTooltip),
            hovertemplate: '%{text}',
            type: 'scatter',
            showlegend: false,
            marker: { color: '#fff0' }
        };

        const traces = [
            programFundsTrace, subsidySizeTrace, earnedFundsTrace, currentFundsTrace,
            sciEarnedTrace, curSciTrace, repTrace, repCapTrace, confidenceTrace,
            engineersTrace, researchersTrace, engEffTrace, contractsTrace, programsTrace
        ];
        traces.forEach(t => {
            t.x = getValuesForField<string>(careerPeriods, 'startDate');
            t.connectgaps = true;
        });

        const layout: Layout = {
            hovermode: 'x unified',
            grid: {
                columns: 1,
                subplots: [['xy'], ['xy2'], ['xy3'], ['xy5']],
                ygap: 0.1
            },
            xaxis: { title: 'Date', type: 'date', autorange: true },
            yaxis: { title: 'Funds', autorange: true, type: 'linear', hoverformat: '.4s' },
            yaxis2: { title: 'Science', autorange: true, rangemode: 'nonnegative', type: 'linear', hoverformat: '.1f' },
            yaxis3: { title: 'Reputation', autorange: true, type: 'linear', hoverformat: '.1f' },
            yaxis4: {
                title: 'Confidence', autorange: true, rangemode: 'nonnegative', showgrid: false,
                type: 'linear', hoverformat: '.1f', overlaying: 'y3', side: 'right'
            },
            yaxis5: { title: 'Personnel', autorange: true, rangemode: 'nonnegative', type: 'linear' },
            yaxis6: {
                title: 'Efficiency', tickformat: ',.0%', hoverformat: ',.1%', showgrid: false,
                autorange: true, rangemode: 'nonnegative', type: 'linear', overlaying: 'y5', side: 'right'
            },
            font: { family: 'Poppins', size: 14 },
            margin: { t: 40, r: 20, b: 45, l: 80, pad: 4 }
        };

        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            layout.paper_bgcolor = '#00000000';
            layout.plot_bgcolor = '#00000000';
            layout.font.color = '#ebecf0';
            layout.hoverlabel = { bgcolor: '#000000' };
            layout.yaxis.gridcolor = layout.yaxis2.gridcolor = layout.yaxis3.gridcolor =
                layout.yaxis4.gridcolor = layout.yaxis5.gridcolor = layout.yaxis6.gridcolor =
                layout.xaxis.gridcolor = '#606060';
        }

        const annotations: Array<Partial<Annotations>> = [];
        const contractNames = [...MilestonesToShowOnChart.keys()];
        const completionArr = getCompletionDatesAndIndexesForContracts(careerLog, contractNames);
        completionArr.forEach(el => annotations.push({
            x: el.month, y: 0, yref: 'y',
            text: MilestonesToShowOnChart.get(el.contract),
            arrowhead: 6, ax: 0, ay: -35
        }));

        careerPeriods.forEach(p => {
            if (p.numNautsKilled > 0) {
                const dt = parseUtcDate(p.startDate);
                annotations.push({
                    x: dt.toFormat('yyyy-MM'), y: 0, yref: 'y3',
                    text: '💀', arrowhead: 6, ax: 0, ay: -25
                });
            }
        });

        if (annotations.length > 0) {
            layout.annotations = annotations;
        }

        const config: Partial<Plotly.Config> = { responsive: true };

        const plotDiv = document.getElementById('chart')!;
        Plotly.react(plotDiv, traces, layout, config)
            .then(plotlyEl => {
                if (!hoverListenerSetUp) {
                    plotlyEl.on('plotly_hover', eventData => {
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
    }

    function genContractTooltip(xaxis: string) {
        const dtStart = parseUtcDate(xaxis);
        const dtEnd = dtStart.plus({ months: 1 });
        const complete = props.contractEvents!.filter(c => c.type === ContractEventTypes.Complete &&
            parseUtcDate(c.date) > dtStart && parseUtcDate(c.date) <= dtEnd);
        const contractList = genTooltipContractRow('Completed', complete);
        return contractList ? `<span style='font-size:12px;'>${contractList}</span>` : 'N/A';
    }

    function genProgramTooltip(xaxis: string) {
        const dtStart = parseUtcDate(xaxis);
        const dtEnd = dtStart.plus({ months: 1 });
        const completed = props.programs!.filter(p => p.completed && parseUtcDate(p.completed) > dtStart && parseUtcDate(p.completed) <= dtEnd);
        const accepted = props.programs!.filter(p => parseUtcDate(p.accepted) > dtStart && parseUtcDate(p.accepted) <= dtEnd);

        const programList = genTooltipProgramRow('Completed', completed);
        const programList2 = genTooltipProgramRow('Accepted', accepted);
        return programList || programList2 ? `<span style='font-size:12px;'>${programList}${programList2}</span>` : 'N/A';
    }

    function genTooltipContractRow(title: string, contracts: BaseContractEvent[]) {
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

    function genTooltipProgramRow(title: string, programs: ProgramItem[]) {
        const res = programs.reduce(
            (acc, entry) => acc + `<br>    ${entry.title}`,
            ''
        );
        return res ? `<br><i>${title} :</i>${res}` : '';
    }

    function lerp(start: number, end: number, time: number): number {
        return (1 - time) * start + time * end;
    }
</script>
