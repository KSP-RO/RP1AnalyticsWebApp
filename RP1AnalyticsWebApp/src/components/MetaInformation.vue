<template>
    <section v-if="isVisible" class="career-summary-panel">
        <header class="career-summary-header">
            <div>
                <p class="eyebrow">Career summary</p>
                <h1>{{ title || 'Career' }}</h1>
            </div>
            <span class="recency-pill">{{ meta?.modRecency || 'Unknown recency' }}</span>
        </header>

        <div class="summary-grid">
            <article v-for="fact in facts" :key="fact.label" class="summary-fact" :class="{ 'summary-fact--wide': fact.wide }">
                <span>{{ fact.label }}</span>
                <strong :title="fact.title">{{ fact.value }}</strong>
            </article>
        </div>

        <section v-if="descriptionShowdown" class="summary-notes">
            <h2>Notes and Remarks</h2>
            <div v-html="descriptionShowdown"></div>
        </section>
    </section>
</template>

<script setup lang="ts">
    import { computed } from 'vue';
    import type { CareerLogMeta } from 'types';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import * as showdown from 'showdown';

    const props = defineProps<{
        meta?: CareerLogMeta;
        title?: string | null;
        currentInGameDate?: string | null;
        isLoading?: boolean;
    }>();

    const isVisible = computed(() => props.meta && !props.isLoading);
    const versionFormatted = computed(() => props.meta?.versionTag ?? '?');
    const descriptionShowdown = computed(() => {
        const text = props.meta?.descriptionText?.trim();
        if (!text) return '';
        const converter = new showdown.Converter();
        return converter.makeHtml(text);
    });

    const facts = computed(() => {
        const meta = props.meta;
        if (!meta) return [];

        return [
            { label: 'Playstyle', value: meta.careerPlaystyle || 'Unknown' },
            { label: 'Difficulty', value: meta.difficultyLevel || 'Unknown' },
            { label: 'Failure Model', value: meta.failureModel || 'Unknown' },
            { label: 'RP-1 Version', value: versionFormatted.value },
            { label: 'Creation Date', value: formatToLocalDate(meta.creationDate), title: formatToLocalDateTime(meta.creationDate) },
            { label: 'Current In-Game Date', value: formatToCareerDate(props.currentInGameDate), title: formatToUTCDateTime(props.currentInGameDate) },
            { label: 'Last Updated', value: formatToLocalDate(meta.lastUpdate), title: formatToLocalDateTime(meta.lastUpdate) }
        ];
    });

    function formatToLocalDate(date?: string | null) {
        if (typeof date !== 'string') return '?';
        return parseUtcDate(date).toLocal().toFormat('yyyy-MM-dd');
    }

    function formatToCareerDate(date?: string | null) {
        if (typeof date !== 'string') return '?';
        return parseUtcDate(date).toFormat('yyyy-MM-dd');
    }

    function formatToLocalDateTime(date?: string | null) {
        if (typeof date !== 'string') return '?';
        return parseUtcDate(date).toLocal().toString();
    }

    function formatToUTCDateTime(date?: string | null) {
        if (typeof date !== 'string') return '?';
        return parseUtcDate(date).toString();
    }
</script>

<style scoped>
    .career-summary-panel {
        --summary-ink: #17212e;
        --summary-muted: #65717f;
        --summary-line: rgba(36, 48, 63, 0.14);
        --summary-panel: rgba(255, 255, 255, 0.78);
        --summary-soft: rgba(29, 143, 138, 0.08);
        --summary-accent: #1d8f8a;
        border: 1px solid var(--summary-line);
        border-radius: 8px;
        padding: 1rem;
        background: var(--summary-panel);
        color: var(--summary-ink);
        box-shadow: 0 0.9rem 2rem rgba(12, 18, 26, 0.1);
    }

    .career-summary-header {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
        align-items: start;
        padding-bottom: 0.9rem;
        border-bottom: 1px solid var(--summary-line);
    }

    .eyebrow {
        margin: 0 0 0.25rem;
        color: var(--summary-accent);
        font-size: 0.72rem;
        font-weight: 850;
        letter-spacing: 0;
        text-transform: uppercase;
    }

    .career-summary-header h1 {
        margin: 0;
        font-size: clamp(1.35rem, 2.5vw, 2.25rem);
        line-height: 1.05;
    }

    .recency-pill {
        display: inline-flex;
        align-items: center;
        min-height: 1.9rem;
        border: 1px solid color-mix(in srgb, var(--summary-accent) 45%, var(--summary-line));
        border-radius: 999px;
        padding: 0.35rem 0.7rem;
        background: var(--summary-soft);
        color: var(--summary-accent);
        font-size: 0.78rem;
        font-weight: 800;
        white-space: nowrap;
    }

    .summary-grid {
        display: grid;
        grid-template-columns: repeat(4, minmax(0, 1fr));
        gap: 0.7rem;
        padding-top: 0.9rem;
    }

    .summary-fact {
        min-width: 0;
        border-left: 3px solid var(--summary-line);
        padding: 0.15rem 0.75rem;
    }

    .summary-fact span,
    .summary-notes h2 {
        display: block;
        margin: 0 0 0.25rem;
        color: var(--summary-muted);
        font-size: 0.72rem;
        font-weight: 850;
        text-transform: uppercase;
    }

    .summary-fact strong {
        display: block;
        min-width: 0;
        overflow: hidden;
        color: var(--summary-ink);
        font-size: clamp(1.05rem, 1.6vw, 1.45rem);
        line-height: 1.12;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .summary-notes {
        margin-top: 1rem;
        border-top: 1px solid var(--summary-line);
        padding-top: 0.85rem;
        color: var(--summary-muted);
        font-size: 0.92rem;
        line-height: 1.5;
    }

    .summary-notes :deep(p) {
        margin: 0.35rem 0 0;
    }

    .summary-notes :deep(p:first-child) {
        margin-top: 0;
    }

    @media (max-width: 900px) {
        .summary-grid {
            grid-template-columns: repeat(2, minmax(0, 1fr));
        }
    }

    @media (max-width: 620px) {
        .career-summary-header {
            display: block;
        }

        .recency-pill {
            margin-top: 0.75rem;
        }

        .summary-grid {
            grid-template-columns: 1fr;
        }

    }

    @media (prefers-color-scheme: dark) {
        .career-summary-panel {
            --summary-ink: #edf3fb;
            --summary-muted: #a6b1bd;
            --summary-line: rgba(255, 255, 255, 0.13);
            --summary-panel: rgba(255, 255, 255, 0.045);
            --summary-soft: rgba(94, 208, 201, 0.11);
            --summary-accent: #5ed0c9;
            box-shadow: none;
        }
    }
</style>
