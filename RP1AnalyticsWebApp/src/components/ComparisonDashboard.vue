<template>
    <section v-if="comparison" class="comparison-workspace">
        <div class="comparison-header">
            <div>
                <p class="eyebrow">Comparison group</p>
                <p class="header-detail">{{ comparison.cohort.description }}</p>
            </div>
            <div class="cohort-meta">
                <div class="cohort-count">
                    <span>{{ comparison.cohort.careerCount }}</span>
                    <small>careers</small>
                </div>
                <p class="end-date-summary">{{ comparison.cohort.endDateFilter?.description || 'All career end dates' }}</p>
            </div>
        </div>

        <div class="metric-grid">
            <article v-for="metric in headlineMetrics" :key="metric.key" class="metric-panel">
                <div class="metric-panel__top">
                    <span>{{ metric.category }}</span>
                    <strong v-if="metric.rank">#{{ metric.rank }}</strong>
                </div>
                <h2>{{ formatMetric(metric) }}</h2>
                <p>{{ metric.label }}</p>
                <div class="metric-panel__range">
                    <span>Median {{ formatMetricValue(metric.cohortMedian, metric.unit) }}</span>
                    <span v-if="metric.percentile !== null">{{ metric.percentile }} percentile</span>
                </div>
            </article>
        </div>

        <ComparisonCharts :comparison="comparison" />

        <div class="comparison-layout">
            <section class="comparison-section">
                <div class="section-heading">
                    <h2>Comparison Metrics</h2>
                    <p>Target, median, interquartile range, and rank among comparable careers.</p>
                </div>
                <div class="metric-table-wrap">
                    <table class="comparison-table">
                        <thead>
                            <tr>
                                <th>Metric</th>
                                <th>Target</th>
                                <th>Median</th>
                                <th>Middle 50%</th>
                                <th>Rank</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="metric in comparison.metrics" :key="metric.key">
                                <td>
                                    <strong>{{ metric.label }}</strong>
                                    <span>{{ metric.description }}</span>
                                </td>
                                <td>{{ formatMetricValue(metric.targetValue, metric.unit) }}</td>
                                <td>{{ formatMetricValue(metric.cohortMedian, metric.unit) }}</td>
                                <td>{{ formatMetricValue(metric.cohortP25, metric.unit) }} - {{ formatMetricValue(metric.cohortP75, metric.unit) }}</td>
                                <td>{{ formatRank(metric.rank, metric.cohortCount) }}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </section>
        </div>

        <section class="comparison-section">
            <div class="section-heading timeline-heading">
                <h2>Timeline</h2>
                <label class="timeline-limit-control">
                    <span>Show last</span>
                    <input v-model.number="timelineLimit" type="number" min="5" :max="timelineTotal" step="5" />
                    <span>of {{ timelineTotal }}</span>
                </label>
            </div>
            <ol class="timeline-list">
                <li v-for="event in recentTimeline" :key="`${event.date}-${event.type}-${event.key}-${event.outcome}`">
                    <time>{{ formatDate(event.date) }}</time>
                    <span class="timeline-type-icon" :class="timelineTypeClass(event.type)" :title="timelineTypeTitle(event.type)" aria-hidden="true">
                        <i class="fas" :class="timelineIconClass(event.type)"></i>
                    </span>
                    <div>
                        <strong>{{ event.title }}</strong>
                        <span v-if="timelineEventDetail(event)">{{ timelineEventDetail(event) }}</span>
                    </div>
                </li>
            </ol>
        </section>
    </section>

    <div v-else-if="isLoading" class="comparison-loading">
        <LoadingSpinner />
    </div>
</template>

<script setup lang="ts">
    import { computed, ref } from 'vue';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import type { CareerComparison, CareerTimelineEvent, MetricComparison } from '../types';
    import LoadingSpinner from './LoadingSpinner.vue';
    import ComparisonCharts from './ComparisonCharts.vue';

    const props = defineProps<{
        comparison?: CareerComparison | null;
        isLoading?: boolean;
    }>();

    const timelineLimit = ref(24);

    const headlineMetrics = computed(() => {
        if (!props.comparison) return [];
        const keys = ['milestones', 'programsCompleted', 'techUnlocks', 'launches', 'currentReputation', 'successRate'];
        return keys.map(k => props.comparison!.metrics.find(m => m.key === k)).filter(Boolean) as MetricComparison[];
    });

    const sortedTimeline = computed(() =>
        [...props.comparison?.timeline ?? []].sort((a, b) => a.date < b.date ? 1 : -1)
    );

    const timelineTotal = computed(() => sortedTimeline.value.length);

    const recentTimeline = computed(() =>
        sortedTimeline.value.slice(0, normalizedTimelineLimit.value)
    );

    const normalizedTimelineLimit = computed(() => {
        const limit = Number(timelineLimit.value);
        if (!Number.isFinite(limit)) return 24;
        return Math.max(5, Math.min(timelineTotal.value || 5, Math.round(limit)));
    });

    function formatMetric(metric: MetricComparison) {
        return formatMetricValue(metric.targetValue, metric.unit);
    }

    function formatMetricValue(value: number | null, unit: string) {
        if (value === null || value === undefined) return '-';
        if (unit === 'funds') return `√${new Intl.NumberFormat('en', { maximumFractionDigits: 0 }).format(value)}`;
        if (unit === 'percent') return `${value.toFixed(1)}%`;
        if (unit === 'science') return value.toFixed(1);
        if (unit === 'count') return new Intl.NumberFormat('en', { maximumFractionDigits: 0 }).format(value);
        if (unit === 'score') return new Intl.NumberFormat('en', { maximumFractionDigits: 0 }).format(value);
        return value.toFixed(2);
    }

    function formatDate(value?: string | null) {
        if (!value) return '-';
        return parseUtcDate(value).toFormat('yyyy-MM-dd');
    }

    function formatRank(rank: number | null, count: number) {
        if (!rank || !count) return '-';
        return `${rank} / ${count}`;
    }

    function timelineIconClass(type: string) {
        return timelineTypeMeta(type).icon;
    }

    function timelineTypeClass(type: string) {
        return `timeline-type-icon--${timelineTypeMeta(type).key}`;
    }

    function timelineTypeTitle(type: string) {
        return timelineTypeMeta(type).label;
    }

    function timelineTypeMeta(type: string) {
        const key = type.toLowerCase();
        if (key === 'launch') return { key, label: 'Launch', icon: 'fa-rocket' };
        if (key === 'contract') return { key, label: 'Contract', icon: 'fa-file-signature' };
        if (key === 'program') return { key, label: 'Program', icon: 'fa-diagram-project' };
        if (key === 'research') return { key, label: 'Research', icon: 'fa-microscope' };
        if (key === 'infrastructure') return { key, label: 'Infrastructure', icon: 'fa-industry' };
        if (key === 'leader') return { key, label: 'Leader', icon: 'fa-crown' };
        return { key: 'other', label: type || 'Event', icon: 'fa-circle' };
    }

    function timelineEventDetail(event: CareerTimelineEvent) {
        return [event.outcome, event.detail].filter(Boolean).join(', ');
    }

</script>

<style scoped>
    .comparison-workspace {
        --rp-ink: #17212e;
        --rp-muted: #65717f;
        --rp-line: rgba(36, 48, 63, 0.14);
        --rp-panel: rgba(255, 255, 255, 0.78);
        --rp-accent: #1d8f8a;
        margin: 1rem 0 2rem;
        color: var(--rp-ink);
    }

    .comparison-header {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
        align-items: end;
        padding: 0.8rem 0 1rem;
        border-bottom: 1px solid var(--rp-line);
    }

    .eyebrow {
        margin: 0 0 0.25rem;
        color: var(--rp-accent);
        font-size: 0.76rem;
        font-weight: 800;
        text-transform: uppercase;
    }

    .header-detail {
        margin: 0;
        color: var(--rp-muted);
        font-size: 1rem;
    }

    .cohort-meta {
        text-align: right;
    }

    .cohort-count {
        margin-bottom: 0.65rem;
    }

    .cohort-count span {
        display: block;
        font-size: 2.6rem;
        font-weight: 800;
        line-height: 1;
    }

    .cohort-count small {
        color: var(--rp-muted);
        text-transform: uppercase;
        font-weight: 700;
    }

    .cohort-scope-control {
        display: flex;
        flex-wrap: wrap;
        justify-content: flex-end;
        gap: 0.35rem;
        max-width: 32rem;
    }

    .cohort-scope-button {
        border: 1px solid var(--rp-line);
        border-radius: 999px;
        padding: 0.35rem 0.65rem;
        background: transparent;
        color: var(--rp-muted);
        font: inherit;
        font-size: 0.76rem;
        font-weight: 750;
        line-height: 1.1;
        white-space: nowrap;
    }

    .cohort-scope-button:hover,
    .cohort-scope-button.is-active {
        border-color: color-mix(in srgb, var(--rp-accent) 55%, var(--rp-line));
        background: color-mix(in srgb, var(--rp-accent) 11%, transparent);
        color: var(--rp-ink);
    }

    .end-date-filter {
        display: grid;
        gap: 0.45rem;
        justify-items: end;
        margin-top: 0.7rem;
    }

    .end-date-mode-control,
    .end-date-fields {
        display: flex;
        flex-wrap: wrap;
        justify-content: flex-end;
        gap: 0.35rem;
    }

    .date-mode-button {
        border: 1px solid var(--rp-line);
        border-radius: 999px;
        padding: 0.3rem 0.55rem;
        background: transparent;
        color: var(--rp-muted);
        font: inherit;
        font-size: 0.72rem;
        font-weight: 750;
        line-height: 1.1;
        white-space: nowrap;
    }

    .date-mode-button:hover,
    .date-mode-button.is-active {
        border-color: color-mix(in srgb, var(--rp-accent) 50%, var(--rp-line));
        color: var(--rp-ink);
    }

    .end-date-fields label {
        display: flex;
        align-items: center;
        gap: 0.35rem;
        color: var(--rp-muted);
        font-size: 0.72rem;
        font-weight: 750;
        text-transform: uppercase;
    }

    .end-date-fields input {
        min-width: 8.5rem;
        border: 1px solid var(--rp-line);
        border-radius: 6px;
        padding: 0.32rem 0.42rem;
        background: var(--rp-panel);
        color: var(--rp-ink);
        font: inherit;
        font-size: 0.82rem;
        text-transform: none;
    }

    .end-date-summary {
        max-width: 32rem;
        margin: 0;
        color: var(--rp-muted);
        font-size: 0.76rem;
    }

    .metric-grid {
        display: grid;
        grid-template-columns: repeat(6, minmax(0, 1fr));
        gap: 0.75rem;
        margin: 1rem 0;
    }

    .metric-panel {
        border: 1px solid var(--rp-line);
        border-radius: 8px;
        padding: 0.85rem;
        background: var(--rp-panel);
    }

    .metric-panel__top,
    .metric-panel__range {
        display: flex;
        justify-content: space-between;
        gap: 0.5rem;
        color: var(--rp-muted);
        font-size: 0.78rem;
    }

    .metric-panel h2 {
        margin: 0.45rem 0 0.1rem;
        font-size: 1.35rem;
        line-height: 1.1;
    }

    .metric-panel p {
        min-height: 2.5rem;
        margin: 0 0 0.5rem;
        color: var(--rp-ink);
        font-weight: 650;
    }

    .comparison-layout {
        display: grid;
        grid-template-columns: minmax(0, 1fr);
        gap: 1rem;
        align-items: start;
    }

    .comparison-section {
        border-top: 1px solid var(--rp-line);
        padding: 1rem 0;
    }

    .section-heading {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
        align-items: baseline;
        margin-bottom: 0.75rem;
    }

    .section-heading h2 {
        margin: 0;
        font-size: 1.1rem;
    }

    .section-heading p {
        margin: 0;
        color: var(--rp-muted);
    }

    .metric-table-wrap {
        overflow-x: auto;
    }

    .comparison-table {
        width: 100%;
        border-collapse: collapse;
        font-size: 0.9rem;
    }

    .comparison-table th {
        border-bottom: 1px solid var(--rp-line);
        color: var(--rp-muted);
        font-size: 0.72rem;
        text-align: left;
        text-transform: uppercase;
        padding: 0.55rem 0.65rem;
    }

    .comparison-table td {
        border-bottom: 1px solid var(--rp-line);
        padding: 0.65rem;
        vertical-align: top;
    }

    .comparison-table td span,
    .timeline-list span {
        display: block;
        color: var(--rp-muted);
        font-size: 0.8rem;
    }

    .timeline-heading {
        align-items: center;
    }

    .timeline-limit-control {
        display: inline-flex;
        flex-wrap: wrap;
        gap: 0.4rem;
        align-items: center;
        color: var(--rp-muted);
        font-size: 0.78rem;
        font-weight: 750;
        white-space: nowrap;
    }

    .timeline-limit-control input {
        width: 4.8rem;
        border: 1px solid var(--rp-line);
        border-radius: 6px;
        padding: 0.28rem 0.4rem;
        background: var(--rp-panel);
        color: var(--rp-ink);
        font: inherit;
        text-align: right;
    }

    .timeline-list {
        list-style: none;
        margin: 0;
        padding: 0;
        display: grid;
        grid-template-columns: 1fr;
    }

    .timeline-list li {
        display: grid;
        grid-template-columns: 6.25rem 2rem minmax(0, 1fr);
        gap: 0.75rem;
        align-items: center;
        border-bottom: 1px solid var(--rp-line);
        padding: 0.62rem 0;
    }

    .timeline-list time {
        color: var(--rp-muted);
        font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
        font-size: 0.82rem;
        line-height: 1.8rem;
    }

    .timeline-list strong {
        display: block;
        min-width: 0;
        overflow-wrap: anywhere;
        font-size: 0.98rem;
        line-height: 1.25;
    }

    .timeline-list span {
        margin-top: 0.15rem;
    }

    .timeline-type-icon {
        display: inline-grid;
        box-sizing: border-box;
        width: 1.65rem;
        height: 1.65rem;
        place-items: center;
        align-self: center;
        justify-self: center;
        border: 1px solid color-mix(in srgb, var(--rp-muted) 22%, transparent);
        border-radius: 999px;
        background: color-mix(in srgb, var(--rp-muted) 8%, transparent);
        color: var(--rp-muted);
        font-size: 0.7rem;
        line-height: 1;
        margin-top: 0;
        text-align: center;
    }

    .timeline-type-icon i {
        display: block;
        width: 1em;
        height: 1em;
        line-height: 1;
    }

    .timeline-type-icon--launch {
        color: var(--rp-accent);
        background: color-mix(in srgb, var(--rp-accent) 12%, transparent);
    }

    .timeline-type-icon--contract {
        color: #d5a23f;
        background: color-mix(in srgb, #d5a23f 12%, transparent);
    }

    .timeline-type-icon--research {
        color: #7e8fe7;
        background: color-mix(in srgb, #7e8fe7 12%, transparent);
    }

    .timeline-type-icon--infrastructure {
        color: #3f6f99;
        background: color-mix(in srgb, #3f6f99 13%, transparent);
    }

    .timeline-type-icon--program {
        color: #5aa267;
        background: color-mix(in srgb, #5aa267 12%, transparent);
    }

    .timeline-type-icon--leader {
        color: #c887c9;
        background: color-mix(in srgb, #c887c9 12%, transparent);
    }

    .comparison-loading {
        display: flex;
        justify-content: center;
        padding: 3rem 0;
    }

    @media (max-width: 1100px) {
        .metric-grid {
            grid-template-columns: repeat(3, minmax(0, 1fr));
        }

        .comparison-layout {
            grid-template-columns: 1fr;
        }

    }

    @media (max-width: 720px) {
        .comparison-header,
        .section-heading {
            display: block;
        }

        .cohort-meta {
            margin-top: 1rem;
            text-align: left;
        }

        .cohort-scope-control {
            justify-content: flex-start;
        }

        .end-date-filter {
            justify-items: start;
        }

        .end-date-mode-control,
        .end-date-fields {
            justify-content: flex-start;
        }

        .metric-grid {
            grid-template-columns: 1fr;
        }

        .timeline-limit-control {
            margin-top: 0.6rem;
        }

        .timeline-list li {
            grid-template-columns: 5.75rem 1.75rem minmax(0, 1fr);
            gap: 0.55rem;
        }
    }

    @media (prefers-color-scheme: dark) {
        .comparison-workspace {
            --rp-ink: #edf3fb;
            --rp-muted: #a6b1bd;
            --rp-line: rgba(255, 255, 255, 0.13);
            --rp-panel: rgba(255, 255, 255, 0.045);
            --rp-accent: #5ed0c9;
        }
    }
</style>
