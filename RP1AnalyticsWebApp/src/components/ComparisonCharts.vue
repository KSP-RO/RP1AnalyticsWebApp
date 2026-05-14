<template>
    <section v-if="hasChartData" class="chart-suite comparison-section">
        <div class="section-heading chart-suite__heading">
            <h2>Comparison Charts</h2>
            <p>Career progress, program choices, infrastructure, economy, and launch operations against the selected comparison group.</p>
        </div>

        <section class="chart-block chart-block--hero">
            <div class="chart-block__header">
                <div>
                    <h3>Milestone Timing Ladder</h3>
                    <p>Each milestone uses its own one-year-padded timeline against the comparison middle 50%, median, historical benchmark, and current save date.</p>
                </div>
                <div class="ladder-header-meta">
                    <span>{{ ladderRows.length }} shown of {{ trackedMilestoneCount }} tracked</span>
                    <small v-if="excludedMilestoneCount">{{ excludedMilestoneCount }} path-excluded</small>
                    <small v-if="collapsedFutureMilestoneCount && !showAllLadderRows">{{ collapsedFutureMilestoneCount }} future hidden</small>
                    <time v-if="targetEndDate">Save date {{ formatDate(targetEndDate) }}</time>
                    <button v-if="collapsedFutureMilestoneCount && !showAllLadderRows"
                            type="button"
                            class="ladder-show-all"
                            @click="showAllLadderRows = true">
                        Show All
                    </button>
                </div>
            </div>
            <div class="chart-legend chart-legend--ladder">
                <span title="Your career completion date for this milestone."><i class="legend-dot legend-dot--target"></i>Career</span>
                <span title="The date range where the middle half of the comparison group completed this milestone."><i class="legend-line"></i>Middle 50%</span>
                <span title="Mapped real-life benchmark for this RP-1 milestone."><i class="legend-dot legend-dot--history"></i>Historical</span>
                <span title="Fastest completion in the selected comparison group."><i class="legend-star legend-star--record fas fa-star"></i>Record</span>
                <span title="The current in-game date of the selected career save."><i class="legend-current"></i>Save date</span>
            </div>
            <div v-if="ladderRows.length" class="timing-ladder">
                <div v-for="row in ladderRows" :key="`${row.kind}-${row.key}`" class="ladder-row"
                     :class="{ 'is-missing': !row.targetDate, 'is-future': isFutureMilestone(row), 'is-program': row.kind === 'Program' }">
                    <div class="ladder-row__label">
                        <strong>{{ row.label }}</strong>
                        <span>{{ row.kind }}<template v-if="isFutureMilestone(row)">, future</template></span>
                    </div>
                    <div class="ladder-row__timeline">
                        <span class="ladder-year-label ladder-year-label--start">{{ ladderStartYear(row) }}</span>
                        <div class="ladder-row__track" :class="{ 'is-empty': !ladderDomainForRow(row).hasData }">
                            <span v-if="showCurrentDateLine(row)" class="ladder-current-line"
                                  :style="ladderPointStyle(targetEndDate, row)"
                                  :title="`Current save date: ${formatDate(targetEndDate)}`" />
                            <span v-if="row.cohortP25Date && row.cohortP75Date" class="ladder-band"
                                  :style="ladderBandStyle(row)"
                                  :title="`Comparison middle 50%: ${formatDate(row.cohortP25Date)} to ${formatDate(row.cohortP75Date)}`" />
                            <span v-if="row.cohortMedianDate" class="ladder-tick ladder-tick--median"
                                  :style="ladderPointStyle(row.cohortMedianDate, row)"
                                  :title="`Comparison median: ${formatDate(row.cohortMedianDate)}`" />
                            <span v-if="row.historicalBenchmark" class="ladder-dot ladder-dot--history"
                                  :style="ladderPointStyle(row.historicalBenchmark.date, row)"
                                  :title="`${row.historicalBenchmark.title}: ${formatDate(row.historicalBenchmark.date)}`" />
                            <span v-if="row.cohortRecordDate" class="ladder-star ladder-star--record"
                                  :style="ladderPointStyle(row.cohortRecordDate, row)"
                                  :title="recordTitle(row)">
                                <i class="fas fa-star" aria-hidden="true"></i>
                            </span>
                            <span v-if="row.targetDate" class="ladder-dot ladder-dot--target"
                                  :style="ladderPointStyle(row.targetDate, row)"
                                  :title="`Career completed: ${formatDate(row.targetDate)}`" />
                        </div>
                        <span class="ladder-year-label ladder-year-label--end">{{ ladderEndYear(row) }}</span>
                    </div>
                    <div class="ladder-row__dates">
                        <span v-if="row.targetDate" class="ladder-career-date"
                              :class="completionTimingClass(row)"
                              :title="completionTimingTitle(row)">Career {{ formatDate(row.targetDate) }}</span>
                        <span>{{ comparisonDateLabel(row) }}</span>
                    </div>
                </div>
            </div>
            <p v-else class="chart-empty">No milestone timing data is available for this comparison group.</p>
        </section>

        <section class="chart-block">
            <div class="chart-block__header">
                <div>
                    <h3>Progress Over Time</h3>
                    <p>Cumulative progress lines with comparison median and middle 50% bands.</p>
                </div>
            </div>
            <div class="small-chart-grid">
                <article v-for="series in progressSeries" :key="series.key" class="mini-chart">
                    <h4>{{ series.label }}</h4>
                    <BandSeriesSvg :series="series" />
                </article>
            </div>
        </section>

        <section class="chart-grid-two">
            <article class="chart-block chart-block--wide">
                <div class="chart-block__header">
                    <div>
                        <h3>Launch Cadence + Outcomes</h3>
                        <p>Yearly target launches by outcome with comparison median cadence.</p>
                    </div>
                </div>
                <svg class="bar-chart" viewBox="0 0 720 260" role="img" aria-label="Launch cadence chart">
                    <line x1="42" y1="218" x2="690" y2="218" class="chart-axis" />
                    <g v-for="point in launchCadence" :key="point.year">
                        <rect v-for="segment in launchSegments(point)" :key="segment.key"
                              :x="launchX(point.year)" :y="segment.y" :width="launchBarWidth"
                              :height="segment.height" :class="segment.className" rx="2" />
                        <circle v-if="point.medianLaunches !== null" :cx="launchX(point.year) + launchBarWidth / 2"
                                :cy="launchY(point.medianLaunches)" r="3" class="chart-median-dot" />
                        <text v-if="showLaunchYear(point.year)" :x="launchX(point.year) + launchBarWidth / 2"
                              y="238" text-anchor="middle" class="chart-axis-label">{{ point.year }}</text>
                    </g>
                </svg>
                <div class="chart-legend">
                    <span><i class="legend-swatch success"></i>Success</span>
                    <span><i class="legend-swatch failure"></i>Failure</span>
                    <span><i class="legend-swatch partial"></i>Had failures</span>
                    <span><i class="legend-dot legend-dot--median"></i>Median</span>
                </div>
            </article>

            <article class="chart-block">
                <div class="chart-block__header">
                    <div>
                        <h3>Research Pace</h3>
                        <p>Tech unlocks over time compared with the selected group.</p>
                    </div>
                </div>
                <BandSeriesSvg v-if="researchSeries" :series="researchSeries" large />
                <p v-else class="chart-empty">No research series is available.</p>
            </article>
        </section>

        <section class="chart-block">
            <div class="chart-block__header">
                <div>
                    <h3>Program Path Map</h3>
                    <p>Which programs this career selected, plus how often the comparison group selected and completed each path.</p>
                </div>
            </div>
            <div class="program-paths">
                <div v-for="path in visibleProgramPaths" :key="path.key" class="program-row"
                     :class="{ 'is-selected': path.targetSelected }">
                    <div class="program-row__label">
                        <strong>{{ path.label }}</strong>
                        <span v-if="path.targetCompleted">Completed {{ formatDate(path.targetCompletedDate) }}</span>
                        <span v-else-if="path.targetSelected">Accepted {{ formatDate(path.targetAcceptedDate) }}</span>
                        <span v-else-if="path.exclusiveGroup">Alternate path</span>
                        <span v-else>Not selected</span>
                    </div>
                    <div class="program-row__bars">
                        <span class="program-bar program-bar--selected" :style="{ width: `${path.selectedPercent}%` }"></span>
                        <span class="program-bar program-bar--completed" :style="{ width: `${path.completedPercent}%` }"></span>
                    </div>
                    <div class="program-row__stats">
                        <span>{{ path.selectedPercent.toFixed(0) }}% selected</span>
                        <span>{{ path.completedPercent.toFixed(0) }}% completed</span>
                    </div>
                </div>
            </div>
        </section>

        <section class="chart-grid-two">
            <article class="chart-block">
                <div class="chart-block__header">
                    <div>
                        <h3>Infrastructure Timeline</h3>
                        <p>Completed builds and maximum launch complex mass over time.</p>
                    </div>
                </div>
                <div class="stacked-band-charts">
                    <BandSeriesSvg v-for="series in infrastructureSeries" :key="series.key" :series="series" />
                </div>
                <ol class="infrastructure-events">
                    <li v-for="event in visibleInfrastructureEvents" :key="`${event.date}-${event.type}-${event.label}-${event.detail}`">
                        <time>{{ formatDate(event.date) }}</time>
                        <div>
                            <strong>{{ event.label }}</strong>
                            <span>{{ event.type }}, {{ event.detail }}</span>
                        </div>
                    </li>
                </ol>
            </article>

            <article class="chart-block">
                <div class="chart-block__header">
                    <div>
                        <h3>Economy + Confidence</h3>
                        <p>Total spending and cumulative confidence earned in context with the comparison group.</p>
                    </div>
                </div>
                <div class="stacked-band-charts">
                    <BandSeriesSvg v-for="series in economySeries" :key="series.key" :series="series" />
                </div>
            </article>
        </section>
    </section>
</template>

<script setup lang="ts">
    import { computed, defineComponent, h, ref, type PropType } from 'vue';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import type {
        CareerComparison,
        ComparisonBandSeries,
        LaunchCadencePoint,
        MilestoneComparison
    } from '../types';

    const props = defineProps<{
        comparison: CareerComparison;
    }>();

    const chartWidth = 640;
    const chartHeight = 190;
    const chartPadding = { top: 18, right: 18, bottom: 30, left: 42 };
    const launchBarWidth = 16;
    const showAllLadderRows = ref(false);

    const progressSeries = computed(() => props.comparison.charts?.progressSeries ?? []);
    const researchSeries = computed(() => progressSeries.value.find(s => s.key === 'techUnlocks') ?? null);
    const launchCadence = computed(() => props.comparison.charts?.launchCadence ?? []);
    const infrastructureSeries = computed(() => props.comparison.charts?.infrastructureSeries ?? []);
    const economySeries = computed(() => props.comparison.charts?.economySeries ?? []);
    const targetEndDate = computed(() => props.comparison.cohort?.endDateFilter?.targetEndDate ?? null);
    const trackedMilestoneCount = computed(() => props.comparison.milestones?.length ?? 0);
    const excludedMilestoneCount = computed(() =>
        (props.comparison.milestones ?? []).filter(m => m.isExcludedByTargetProgramChoice).length
    );
    const visibleInfrastructureEvents = computed(() =>
        [...props.comparison.charts?.infrastructureEvents ?? []]
            .sort((a, b) => a.date < b.date ? 1 : -1)
            .slice(0, 8)
    );

    const orderedLadderRows = computed(() => {
        const milestones = (props.comparison.milestones ?? [])
            .filter(m => !m.isExcludedByTargetProgramChoice);

        return milestones.sort((a, b) =>
            ladderSortMillis(a) - ladderSortMillis(b) ||
            milestoneReferenceMillis(a) - milestoneReferenceMillis(b) ||
            a.kind.localeCompare(b.kind) ||
            a.label.localeCompare(b.label)
        );
    });

    const collapsedFutureMilestoneCount = computed(() =>
        orderedLadderRows.value.filter(shouldCollapseFutureMilestone).length
    );

    const ladderRows = computed(() =>
        showAllLadderRows.value
            ? orderedLadderRows.value
            : orderedLadderRows.value.filter(row => !shouldCollapseFutureMilestone(row))
    );

    const visibleProgramPaths = computed(() =>
        (props.comparison.charts?.programPaths ?? [])
            .filter(p => p.targetSelected || p.selectedPercent >= 12 || p.completedPercent >= 12 || p.exclusiveGroup)
            .slice(0, 18)
    );

    const hasChartData = computed(() =>
        orderedLadderRows.value.length > 0 ||
        progressSeries.value.length > 0 ||
        launchCadence.value.length > 0 ||
        visibleProgramPaths.value.length > 0 ||
        infrastructureSeries.value.length > 0 ||
        economySeries.value.length > 0
    );

    const BandSeriesSvg = defineComponent({
        name: 'BandSeriesSvg',
        props: {
            series: { type: Object as PropType<ComparisonBandSeries>, required: true },
            large: { type: Boolean, default: false }
        },
        setup(componentProps) {
            return () => h('svg', {
                class: ['band-chart', { 'band-chart--large': componentProps.large }],
                viewBox: `0 0 ${chartWidth} ${chartHeight}`,
                role: 'img',
                'aria-label': `${componentProps.series.label} chart`
            }, [
                h('line', { x1: chartPadding.left, y1: chartHeight - chartPadding.bottom, x2: chartWidth - chartPadding.right, y2: chartHeight - chartPadding.bottom, class: 'chart-axis' }),
                h('text', { x: chartPadding.left, y: 16, class: 'chart-title' }, componentProps.series.label),
                h('polygon', { points: bandPolygon(componentProps.series), class: 'chart-band' }),
                h('polyline', { points: seriesLine(componentProps.series, 'median'), class: 'chart-median-line' }),
                h('polyline', { points: seriesLine(componentProps.series, 'targetValue'), class: 'chart-target-line' }),
                ...axisLabels(componentProps.series).map(label => h('text', {
                    x: label.x,
                    y: chartHeight - 10,
                    'text-anchor': label.anchor,
                    class: 'chart-axis-label'
                }, label.text)),
                h('text', { x: chartWidth - chartPadding.right, y: 16, 'text-anchor': 'end', class: 'chart-value-label' }, latestValueLabel(componentProps.series))
            ]);
        }
    });

    function ladderBandStyle(row: MilestoneComparison) {
        const domain = ladderDomainForRow(row);
        const start = datePercent(row.cohortP25Date, domain);
        const end = datePercent(row.cohortP75Date, domain);
        return {
            left: `${Math.min(start, end)}%`,
            width: `${Math.max(1.5, Math.abs(end - start))}%`
        };
    }

    function ladderPointStyle(date: string | null | undefined, row: MilestoneComparison) {
        return { left: `${datePercent(date, ladderDomainForRow(row))}%` };
    }

    function ladderDomainForRow(row: MilestoneComparison): TimelineDomain {
        const dates = ladderReferenceDates(row)
            .map(dateMillis)
            .filter(Number.isFinite);
        return createPaddedTimeDomain(dates);
    }

    function ladderReferenceDates(row: MilestoneComparison) {
        return [
            row.targetDate,
            row.cohortP25Date,
            row.cohortMedianDate,
            row.cohortP75Date,
            row.cohortRecordDate,
            row.historicalBenchmark?.date
        ].filter(Boolean) as string[];
    }

    function ladderStartYear(row: MilestoneComparison) {
        return formatTimelineYear(ladderDomainForRow(row).minX, ladderDomainForRow(row).hasData);
    }

    function ladderEndYear(row: MilestoneComparison) {
        return formatTimelineYear(ladderDomainForRow(row).maxX, ladderDomainForRow(row).hasData);
    }

    function showCurrentDateLine(row: MilestoneComparison) {
        if (!targetEndDate.value) return false;
        const domain = ladderDomainForRow(row);
        const current = dateMillis(targetEndDate.value);
        return domain.hasData && current >= domain.minX && current <= domain.maxX;
    }

    function completionTimingClass(row: MilestoneComparison) {
        const comparison = completionTiming(row);
        if (!comparison) return '';
        if (comparison.deltaDays < 0 && comparison.severity === 'large') return 'is-ahead-large';
        if (comparison.deltaDays > 0 && comparison.severity === 'medium') return 'is-late-medium';
        if (comparison.deltaDays > 0 && comparison.severity === 'large') return 'is-late-large';
        return '';
    }

    function completionTimingTitle(row: MilestoneComparison) {
        const comparison = completionTiming(row);
        if (!comparison) return undefined;
        if (comparison.deltaDays === 0) return 'Completed on the comparison median date.';

        const direction = comparison.deltaDays < 0 ? 'before' : 'after';
        return `Completed ${formatDuration(Math.abs(comparison.deltaDays))} ${direction} the comparison median.`;
    }

    function recordTitle(row: MilestoneComparison) {
        const career = row.cohortRecordCareerName || 'Unknown career';
        const player = row.cohortRecordUserPreferredName || row.cohortRecordUserLogin;
        const owner = player ? `${career} by ${player}` : career;
        return `Record: ${owner}, ${formatDate(row.cohortRecordDate)}`;
    }

    function completionTiming(row: MilestoneComparison): CompletionTiming | null {
        if (!row.targetDate || !row.cohortMedianDate) return null;

        const deltaDays = Math.round((dateMillis(row.targetDate) - dateMillis(row.cohortMedianDate)) / millisPerDay);
        const spreadDays = Math.max(1, referenceSpreadDays(row));
        const mediumThreshold = Math.max(60, Math.round(spreadDays * 0.12));
        const largeThreshold = Math.max(180, Math.round(spreadDays * 0.25));
        const absDays = Math.abs(deltaDays);

        if (absDays >= largeThreshold) {
            return { deltaDays, severity: 'large' };
        }

        if (absDays >= mediumThreshold) {
            return { deltaDays, severity: 'medium' };
        }

        return { deltaDays, severity: 'small' };
    }

    function referenceSpreadDays(row: MilestoneComparison) {
        const dates = ladderReferenceDates(row).map(dateMillis);
        if (dates.length < 2) return 365;
        return (Math.max(...dates) - Math.min(...dates)) / millisPerDay;
    }

    function ladderSortMillis(milestone: MilestoneComparison) {
        const domain = ladderDomainForRow(milestone);
        return domain.hasData ? domain.minX : Number.MAX_SAFE_INTEGER;
    }

    function milestoneReferenceMillis(milestone: MilestoneComparison) {
        const referenceDate = milestone.targetDate ?? milestone.cohortMedianDate ?? milestone.historicalBenchmark?.date;
        return referenceDate ? dateMillis(referenceDate) : Number.MAX_SAFE_INTEGER;
    }

    function shouldCollapseFutureMilestone(milestone: MilestoneComparison) {
        if (!targetEndDate.value || milestone.targetDate) return false;
        const referenceMillis = milestoneReferenceMillis(milestone);
        if (!Number.isFinite(referenceMillis)) return false;
        return referenceMillis > parseUtcDate(targetEndDate.value).plus({ years: 1 }).toMillis();
    }

    function isFutureMilestone(milestone: MilestoneComparison) {
        if (!targetEndDate.value || milestone.targetDate) return false;
        const referenceDate = milestone.cohortMedianDate ?? milestone.historicalBenchmark?.date;
        return !!referenceDate && dateMillis(referenceDate) > dateMillis(targetEndDate.value);
    }

    function launchX(year: number) {
        const years = launchCadence.value.map(p => p.year);
        if (years.length <= 1) return 56;
        const min = Math.min(...years);
        const max = Math.max(...years);
        return 42 + ((year - min) / (max - min)) * (648 - launchBarWidth);
    }

    function launchY(value: number) {
        const max = Math.max(1, ...launchCadence.value.flatMap(p => [
            targetLaunchTotal(p),
            p.p75Launches ?? 0,
            p.medianLaunches ?? 0
        ]));
        return 218 - (value / max) * 176;
    }

    function launchSegments(point: LaunchCadencePoint) {
        const stacks = [
            { key: 'success', value: point.targetSuccesses, className: 'bar-success' },
            { key: 'partial', value: point.targetPartialFailures, className: 'bar-partial' },
            { key: 'failure', value: point.targetFailures, className: 'bar-failure' },
            { key: 'untagged', value: point.targetUntagged, className: 'bar-untagged' }
        ];
        let cursor = 218;
        let cumulative = 0;
        return stacks.map(stack => {
            cumulative += stack.value;
            const y = launchY(cumulative);
            const height = cursor - y;
            cursor = y;
            return { ...stack, y, height: Math.max(0, height) };
        }).filter(stack => stack.height > 0);
    }

    function showLaunchYear(year: number) {
        const years = launchCadence.value.map(p => p.year);
        const index = years.indexOf(year);
        const interval = years.length > 45 ? 10 : years.length > 24 ? 5 : 3;
        return index === 0 || index === years.length - 1 || year % interval === 0;
    }

    function targetLaunchTotal(point: LaunchCadencePoint) {
        return point.targetSuccesses + point.targetFailures + point.targetPartialFailures + point.targetUntagged;
    }

    function bandPolygon(series: ComparisonBandSeries) {
        const domain = seriesDomain(series);
        const upper = series.points
            .filter(p => p.p75 !== null)
            .map(p => pointString(p.date, p.p75, domain));
        const lower = [...series.points]
            .reverse()
            .filter(p => p.p25 !== null)
            .map(p => pointString(p.date, p.p25, domain));
        return [...upper, ...lower].join(' ');
    }

    function seriesLine(series: ComparisonBandSeries, key: 'targetValue' | 'median') {
        const domain = seriesDomain(series);
        return series.points
            .filter(p => p[key] !== null)
            .map(p => pointString(p.date, p[key], domain))
            .join(' ');
    }

    function pointString(date: string, value: number | null, domain: ChartDomain) {
        return `${xForDate(date, domain).toFixed(1)},${yForValue(value ?? 0, domain).toFixed(1)}`;
    }

    function seriesDomain(series: ComparisonBandSeries): ChartDomain {
        const dates = series.points.map(p => dateMillis(p.date));
        const values = series.points.flatMap(p => [p.targetValue, p.median, p.p25, p.p75])
            .filter(v => v !== null && Number.isFinite(v)) as number[];
        return createDomain(dates, values);
    }

    function axisLabels(series: ComparisonBandSeries) {
        const points = series.points;
        if (!points.length) return [];
        return [
            { x: chartPadding.left, anchor: 'start', text: parseUtcDate(points[0].date).toFormat('yyyy') },
            { x: chartWidth - chartPadding.right, anchor: 'end', text: parseUtcDate(points[points.length - 1].date).toFormat('yyyy') }
        ];
    }

    function latestValueLabel(series: ComparisonBandSeries) {
        const latest = [...series.points].reverse().find(p => p.targetValue !== null);
        return latest ? formatChartValue(latest.targetValue, series.unit) : '-';
    }

    function createDomain(dates: number[], values: number[]): ChartDomain {
        const time = createTimeDomain(dates);
        const max = Math.max(1, ...values);
        const min = Math.min(0, ...values);
        return { ...time, minY: min, maxY: max === min ? max + 1 : max };
    }

    function createTimeDomain(dates: number[]) {
        if (!dates.length) {
            return { minX: 0, maxX: 1 };
        }

        const minX = Math.min(...dates);
        const maxX = Math.max(...dates);
        return { minX, maxX: minX === maxX ? minX + 1 : maxX };
    }

    function createPaddedTimeDomain(dates: number[]): TimelineDomain {
        if (!dates.length) {
            return { minX: 0, maxX: 1, hasData: false };
        }

        const minDate = parseUtcDate(new Date(Math.min(...dates)).toISOString()).minus({ years: 1 });
        const maxDate = parseUtcDate(new Date(Math.max(...dates)).toISOString()).plus({ years: 1 });
        const minX = minDate.toMillis();
        const maxX = maxDate.toMillis();

        return {
            minX,
            maxX: minX === maxX ? maxDate.plus({ days: 1 }).toMillis() : maxX,
            hasData: true
        };
    }

    function xForDate(date: string, domain: ChartDomain) {
        return chartPadding.left + ((dateMillis(date) - domain.minX) / (domain.maxX - domain.minX)) * (chartWidth - chartPadding.left - chartPadding.right);
    }

    function yForValue(value: number, domain: ChartDomain) {
        return chartHeight - chartPadding.bottom -
            ((value - domain.minY) / (domain.maxY - domain.minY)) * (chartHeight - chartPadding.top - chartPadding.bottom);
    }

    function datePercent(date: string | null | undefined, domain: { minX: number; maxX: number }) {
        if (!date) return 0;
        const percent = ((dateMillis(date) - domain.minX) / (domain.maxX - domain.minX)) * 100;
        return Math.min(100, Math.max(0, percent));
    }

    function dateMillis(date: string) {
        return parseUtcDate(date).toMillis();
    }

    function formatDate(value?: string | null) {
        return value ? parseUtcDate(value).toFormat('yyyy-MM-dd') : '-';
    }

    function formatTimelineYear(value: number, hasData: boolean) {
        return hasData ? parseUtcDate(new Date(value).toISOString()).toFormat('yyyy') : '-';
    }

    function comparisonDateLabel(milestone: MilestoneComparison) {
        return milestone.cohortMedianDate ? `Median ${formatDate(milestone.cohortMedianDate)}` : 'Median not enough data';
    }

    function formatDuration(days: number) {
        if (days < 45) return `${days}d`;
        const months = Math.max(1, Math.round(days / 30.4375));
        const years = Math.floor(months / 12);
        const remMonths = months % 12;
        if (years === 0) return `${months}mo`;
        if (remMonths === 0) return `${years}y`;
        return `${years}y ${remMonths}mo`;
    }

    function formatChartValue(value: number | null, unit: string) {
        if (value === null || value === undefined) return '-';
        if (unit === 'funds') return new Intl.NumberFormat('en', { notation: 'compact', maximumFractionDigits: 1 }).format(value);
        if (unit === 'mass') return `${new Intl.NumberFormat('en', { maximumFractionDigits: 0 }).format(value)} t`;
        return new Intl.NumberFormat('en', { maximumFractionDigits: unit === 'score' ? 0 : 1 }).format(value);
    }

    interface ChartDomain {
        minX: number;
        maxX: number;
        minY: number;
        maxY: number;
    }

    interface TimelineDomain {
        minX: number;
        maxX: number;
        hasData: boolean;
    }

    interface CompletionTiming {
        deltaDays: number;
        severity: 'small' | 'medium' | 'large';
    }

    const millisPerDay = 24 * 60 * 60 * 1000;
</script>

<style>
    .chart-suite {
        display: grid;
        gap: 1rem;
    }

    .chart-suite__heading {
        margin-bottom: 0;
    }

    .chart-block {
        border: 1px solid var(--rp-line);
        border-radius: 8px;
        padding: 1rem;
        background: var(--rp-panel);
    }

    .chart-block--hero {
        background: color-mix(in srgb, var(--rp-panel) 82%, var(--rp-accent) 6%);
    }

    .chart-block__header {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
        align-items: start;
        margin-bottom: 0.75rem;
    }

    .chart-block__header h3,
    .mini-chart h4 {
        margin: 0;
        font-size: 0.98rem;
    }

    .chart-block__header p {
        margin: 0.2rem 0 0;
        color: var(--rp-muted);
        font-size: 0.82rem;
    }

    .chart-block__header > span,
    .ladder-header-meta {
        color: var(--rp-muted);
        font-size: 0.78rem;
        font-weight: 800;
        white-space: nowrap;
    }

    .ladder-header-meta {
        display: grid;
        gap: 0.15rem;
        justify-items: end;
        text-align: right;
    }

    .ladder-header-meta time {
        color: var(--rp-ink);
        font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
        font-size: 0.72rem;
    }

    .ladder-header-meta small {
        color: var(--rp-muted);
        font-size: 0.7rem;
        font-weight: 700;
    }

    .ladder-show-all {
        justify-self: end;
        border: 1px solid color-mix(in srgb, var(--rp-accent) 50%, var(--rp-line));
        border-radius: 999px;
        padding: 0.28rem 0.65rem;
        background: color-mix(in srgb, var(--rp-accent) 12%, transparent);
        color: var(--rp-ink);
        font: inherit;
        font-size: 0.72rem;
        font-weight: 800;
        line-height: 1.1;
    }

    .ladder-show-all:hover {
        background: color-mix(in srgb, var(--rp-accent) 22%, transparent);
    }

    .chart-grid-two,
    .small-chart-grid {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 1rem;
    }

    .chart-block--wide {
        grid-column: 1 / -1;
    }

    .mini-chart {
        min-width: 0;
    }

    .mini-chart h4 {
        margin-bottom: 0.3rem;
        color: var(--rp-muted);
        font-size: 0.82rem;
    }

    .band-chart,
    .bar-chart {
        display: block;
        width: 100%;
        min-height: 9.5rem;
        overflow: visible;
    }

    .band-chart--large {
        min-height: 13rem;
    }

    .chart-axis {
        stroke: var(--rp-line);
        stroke-width: 1;
    }

    .chart-band {
        fill: color-mix(in srgb, var(--rp-accent) 22%, transparent);
    }

    .chart-median-line {
        fill: none;
        stroke: color-mix(in srgb, var(--rp-muted) 74%, var(--rp-ink));
        stroke-width: 2;
        stroke-dasharray: 5 5;
    }

    .chart-target-line {
        fill: none;
        stroke: var(--rp-accent);
        stroke-width: 3;
    }

    .chart-title,
    .chart-value-label,
    .chart-axis-label {
        fill: var(--rp-muted);
        font-size: 0.72rem;
        font-weight: 750;
    }

    .chart-value-label {
        fill: var(--rp-ink);
    }

    .bar-success {
        fill: #1d8f8a;
    }

    .bar-failure {
        fill: #c85656;
    }

    .bar-partial {
        fill: #d5a23f;
    }

    .bar-untagged {
        fill: color-mix(in srgb, var(--rp-muted) 38%, transparent);
    }

    .chart-median-dot {
        fill: var(--rp-ink);
    }

    .timing-ladder {
        display: grid;
        gap: 0.35rem;
    }

    .ladder-row {
        display: grid;
        grid-template-columns: minmax(10rem, 1.2fr) minmax(12rem, 2fr) 9rem;
        gap: 0.75rem;
        align-items: center;
        min-height: 2.25rem;
        margin: 0 -0.35rem;
        padding: 0.18rem 0.35rem;
        border-radius: 6px;
    }

    .ladder-row__label strong,
    .ladder-row__label span,
    .ladder-row__dates span {
        display: block;
    }

    .ladder-row__label strong {
        font-size: 0.82rem;
        line-height: 1.2;
    }

    .ladder-row.is-program {
        background: color-mix(in srgb, #3f6f99 10%, transparent);
        box-shadow: inset 3px 0 0 #3f6f99;
    }

    .ladder-row.is-program .ladder-row__label strong {
        color: color-mix(in srgb, #244f75 78%, var(--rp-ink));
    }

    .ladder-row.is-program .ladder-row__label span {
        color: #3f6f99;
        font-weight: 800;
    }

    .ladder-row.is-missing .ladder-row__label strong {
        color: color-mix(in srgb, var(--rp-muted) 88%, var(--rp-ink));
    }

    .ladder-row.is-program.is-missing .ladder-row__label strong {
        color: color-mix(in srgb, #3f6f99 34%, var(--rp-muted));
    }

    .ladder-row.is-missing .ladder-row__label span {
        color: color-mix(in srgb, var(--rp-muted) 92%, var(--rp-ink));
    }

    .ladder-row.is-future .ladder-row__label strong {
        color: var(--rp-ink);
    }

    .ladder-row.is-program.is-future .ladder-row__label strong {
        color: color-mix(in srgb, #244f75 80%, var(--rp-ink));
    }

    .ladder-row.is-future .ladder-row__track {
        border-bottom-style: dashed;
    }

    .ladder-row__label span,
    .ladder-row__dates {
        color: var(--rp-muted);
        font-size: 0.72rem;
    }

    .ladder-row__timeline {
        display: grid;
        grid-template-columns: 3.1rem minmax(12rem, 1fr) 3.1rem;
        gap: 0.45rem;
        align-items: center;
        min-width: 0;
    }

    .ladder-year-label {
        color: var(--rp-muted);
        font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
        font-size: 0.68rem;
        font-weight: 750;
        line-height: 1;
        white-space: nowrap;
    }

    .ladder-year-label--start {
        text-align: right;
    }

    .ladder-year-label--end {
        text-align: left;
    }

    .ladder-row__track {
        position: relative;
        height: 0.8rem;
        border-bottom: 1px solid var(--rp-line);
        overflow: visible;
    }

    .ladder-row__track.is-empty {
        border-bottom-style: dotted;
        opacity: 0.55;
    }

    .ladder-band {
        position: absolute;
        top: 0.28rem;
        height: 0.38rem;
        border-radius: 999px;
        background: color-mix(in srgb, var(--rp-accent) 24%, transparent);
    }

    .ladder-dot,
    .ladder-tick,
    .ladder-star {
        position: absolute;
        top: 50%;
        transform: translate(-50%, -50%);
    }

    .ladder-dot {
        width: 0.55rem;
        height: 0.55rem;
        border-radius: 999px;
        border: 2px solid var(--rp-panel);
    }

    .ladder-dot--target {
        background: var(--rp-accent);
    }

    .ladder-dot--history {
        background: #d5a23f;
    }

    .ladder-star {
        z-index: 2;
        color: #d5a23f;
        font-size: 0.62rem;
        line-height: 1;
        filter: drop-shadow(0 0 2px var(--rp-panel));
    }

    .ladder-tick--median {
        width: 2px;
        height: 1rem;
        background: var(--rp-ink);
    }

    .ladder-current-line {
        position: absolute;
        top: -0.25rem;
        bottom: -0.15rem;
        z-index: 1;
        width: 2px;
        transform: translateX(-50%);
        border-radius: 999px;
        background: #c85656;
        opacity: 0.72;
    }

    .ladder-row__dates {
        display: grid;
        grid-template-columns: 1fr;
        gap: 0.1rem;
        font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
    }

    .ladder-career-date {
        color: var(--rp-muted);
    }

    .ladder-career-date.is-ahead-large {
        color: #2e9f6f;
        font-weight: 800;
    }

    .ladder-career-date.is-late-medium {
        color: #d5a23f;
        font-weight: 800;
    }

    .ladder-career-date.is-late-large {
        color: #c85656;
        font-weight: 800;
    }

    .chart-legend {
        display: flex;
        flex-wrap: wrap;
        gap: 0.75rem;
        margin-top: 0.75rem;
        color: var(--rp-muted);
        font-size: 0.76rem;
    }

    .chart-legend--ladder {
        margin: -0.15rem 0 0.75rem;
    }

    .chart-legend span {
        display: inline-flex;
        gap: 0.35rem;
        align-items: center;
    }

    .legend-dot,
    .legend-swatch {
        display: inline-block;
        width: 0.6rem;
        height: 0.6rem;
        border-radius: 999px;
    }

    .legend-dot--target,
    .legend-swatch.success {
        background: var(--rp-accent);
    }

    .legend-dot--history,
    .legend-swatch.partial {
        background: #d5a23f;
    }

    .legend-dot--median {
        background: var(--rp-ink);
    }

    .legend-star {
        color: #d5a23f;
        font-size: 0.72rem;
        line-height: 1;
    }

    .legend-swatch.failure {
        background: #c85656;
    }

    .legend-line {
        width: 1.2rem;
        height: 0.35rem;
        border-radius: 999px;
        background: color-mix(in srgb, var(--rp-accent) 24%, transparent);
    }

    .legend-current {
        display: inline-block;
        width: 2px;
        height: 0.85rem;
        border-radius: 999px;
        background: #c85656;
    }

    .program-paths {
        display: grid;
        gap: 0.5rem;
    }

    .program-row {
        display: grid;
        grid-template-columns: minmax(12rem, 1.2fr) minmax(12rem, 2fr) 9rem;
        gap: 0.8rem;
        align-items: center;
        padding: 0.5rem 0;
        border-top: 1px solid var(--rp-line);
    }

    .program-row:first-child {
        border-top: 0;
    }

    .program-row__label strong,
    .program-row__label span,
    .program-row__stats span {
        display: block;
    }

    .program-row__label strong {
        font-size: 0.86rem;
    }

    .program-row__label span,
    .program-row__stats {
        color: var(--rp-muted);
        font-size: 0.74rem;
    }

    .program-row.is-selected .program-row__label strong {
        color: var(--rp-accent);
    }

    .program-row__bars {
        position: relative;
        height: 0.85rem;
        border-radius: 999px;
        background: color-mix(in srgb, var(--rp-muted) 16%, transparent);
        overflow: hidden;
    }

    .program-bar {
        position: absolute;
        left: 0;
        top: 0;
        bottom: 0;
        border-radius: inherit;
    }

    .program-bar--selected {
        background: color-mix(in srgb, var(--rp-accent) 24%, transparent);
    }

    .program-bar--completed {
        background: var(--rp-accent);
        max-width: 100%;
    }

    .stacked-band-charts {
        display: grid;
        gap: 0.75rem;
    }

    .infrastructure-events {
        display: grid;
        gap: 0.35rem;
        list-style: none;
        margin: 0.85rem 0 0;
        padding: 0;
    }

    .infrastructure-events li {
        display: grid;
        grid-template-columns: 5.8rem minmax(0, 1fr);
        gap: 0.65rem;
        border-top: 1px solid var(--rp-line);
        padding-top: 0.45rem;
    }

    .infrastructure-events time {
        color: var(--rp-muted);
        font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
        font-size: 0.72rem;
    }

    .infrastructure-events strong,
    .infrastructure-events span {
        display: block;
    }

    .infrastructure-events span,
    .chart-empty {
        color: var(--rp-muted);
        font-size: 0.78rem;
    }

    .chart-empty {
        margin: 0;
    }

    @media (max-width: 1100px) {
        .chart-grid-two {
            grid-template-columns: 1fr;
        }
    }

    @media (max-width: 720px) {
        .small-chart-grid {
            grid-template-columns: 1fr;
        }

        .chart-block__header,
        .ladder-row,
        .program-row {
            display: block;
        }

        .ladder-row__track,
        .program-row__bars {
            margin: 0.45rem 0;
        }

        .ladder-row__timeline {
            grid-template-columns: 2.8rem minmax(0, 1fr) 2.8rem;
            margin: 0.35rem 0;
        }

        .ladder-row__dates,
        .program-row__stats {
            display: flex;
            justify-content: space-between;
            gap: 0.75rem;
        }
    }
</style>
