<template>
    <nav class="career-tabs" aria-label="Career sections">
        <button v-for="tab in tabs"
                :key="tab.key"
                type="button"
                :class="{ 'is-active': active === tab.key }"
                @click="$emit('change-active', tab.key)">
            <i class="fas" :class="tab.icon" aria-hidden="true"></i>
            <span>{{ tab.label }}</span>
        </button>
    </nav>
</template>

<script setup lang="ts">
    defineProps<{
        active?: string;
    }>();

    defineEmits<{
        'change-active': [tabName: string];
    }>();

    const tabs = [
        { key: 'comparison', label: 'Comparison', icon: 'fa-chart-line' },
        { key: 'milestones', label: 'Contracts', icon: 'fa-trophy' },
        { key: 'repeatables', label: 'Repeatables', icon: 'fa-redo' },
        { key: 'programs', label: 'Programs', icon: 'fa-university' },
        { key: 'tech', label: 'Tech Unlocks', icon: 'fa-tools' },
        { key: 'launches', label: 'Launches', icon: 'fa-rocket' },
        { key: 'facilities', label: 'Facilities', icon: 'fa-industry' },
        { key: 'leaders', label: 'Leaders', icon: 'fa-crown' }
    ];
</script>

<style scoped>
    .career-tabs {
        display: flex;
        flex-wrap: wrap;
        justify-content: center;
        gap: 0.35rem;
        border: 1px solid rgba(36, 48, 63, 0.14);
        border-radius: 999px;
        width: fit-content;
        max-width: 100%;
        margin: 0 auto;
        padding: 0.3rem;
        background: rgba(255, 255, 255, 0.7);
        box-shadow: 0 0.6rem 1.3rem rgba(12, 18, 26, 0.08);
    }

    .career-tabs button {
        display: inline-flex;
        gap: 0.42rem;
        align-items: center;
        min-height: 2.15rem;
        border: 0;
        border-radius: 999px;
        padding: 0.42rem 0.78rem;
        background: transparent;
        color: #65717f;
        font: inherit;
        font-size: 0.88rem;
        font-weight: 750;
        line-height: 1;
        cursor: pointer;
        transition: background-color 140ms ease, color 140ms ease, box-shadow 140ms ease;
    }

    .career-tabs button:hover {
        background: rgba(29, 143, 138, 0.08);
        color: #17212e;
    }

    .career-tabs button.is-active {
        background: #1d8f8a;
        color: #fff;
        box-shadow: 0 0.45rem 1rem rgba(29, 143, 138, 0.24);
    }

    .career-tabs i {
        width: 1rem;
        text-align: center;
    }

    @media (max-width: 760px) {
        .career-tabs {
            justify-content: flex-start;
            width: 100%;
            border-radius: 8px;
            overflow-x: auto;
            flex-wrap: nowrap;
        }

        .career-tabs button {
            white-space: nowrap;
        }
    }

    @media (prefers-color-scheme: dark) {
        .career-tabs {
            border-color: rgba(255, 255, 255, 0.12);
            background: rgba(255, 255, 255, 0.045);
            box-shadow: none;
        }

        .career-tabs button {
            color: #a6b1bd;
        }

        .career-tabs button:hover {
            background: rgba(94, 208, 201, 0.11);
            color: #edf3fb;
        }

        .career-tabs button.is-active {
            background: #4e7cff;
            color: #fff;
            box-shadow: 0 0.45rem 1rem rgba(78, 124, 255, 0.24);
        }
    }
</style>
