<template>
    <div v-show="isVisible">
        <h2 class="subtitle">Launches</h2>

        <div class="table-wrapper">
            <table class="table is-bordered is-fullwidth is-hoverable">
                <thead>
                    <tr>
                        <th class="is-narrow"></th>
                        <th>Vessel Name</th>
                        <th>Date</th>
                    </tr>
                </thead>
                <tbody>
                    <template v-for="item in items">
                        <tr @click="toggleVisibility(item)" :class="{ 'is-selected': item.visible, 'clickable': canOpen(item) }">
                            <td><span class="icon is-small"><i class="fas" :class="getVesselIcon(item)" aria-hidden="true"></i></span></td>
                            <td>
                                <div class="success-fail-row">
                                    <span>{{ item.vesselName }}</span>
                                    <div class="success-fail-widget">
                                        <span v-if="hasFailures(item)" class="icon inline-icon">
                                            <img src="../assets/agathorn.webp" alt="failure" :title="getTFFailureIconTitle(item)" />
                                        </span>
                                        <div v-if="canEdit" class="buttons has-addons ml-2">
                                            <button class="button is-small is-success"
                                                    title="Tag the launch as a success"
                                                    :class="getSuccessStyleForEdit(item)"
                                                    @click.stop="updateLaunchSuccessState(item, true, $event)">
                                                <span class="icon is-small"><i class="fas fa-xl fa-check" aria-hidden="true"></i></span>
                                            </button>
                                            <button class="button is-small is-danger"
                                                    title="Tag the launch as a failure"
                                                    :class="getFailStyleForEdit(item)"
                                                    @click.stop="updateLaunchSuccessState(item, false, $event)">
                                                <span class="icon is-small"><i class="fas fa-xl fa-xmark" aria-hidden="true"></i></span>
                                            </button>
                                        </div>
                                        <span v-if="!canEdit" class="icon ml-2">
                                            <i class="fas fa-xl" aria-hidden="true"
                                               :class="getSuccessStateIconForDisp(item)"
                                               :title="getSuccessStateTitle(item)"></i>
                                        </span>
                                    </div>
                                </div>
                            </td>
                            <td class="date-col"><span :title="formatDatePlusTime(item.date)">{{ formatDate(item.date) }}</span></td>
                        </tr>
                        <tr v-if="item.visible">
                            <td colspan="3">
                                <h3>Failures</h3>
                                <ul>
                                    <li v-for="f in item.failures" class="failure">
                                        <span>{{f.part}}: {{f.type}} at </span>
                                        <span :title="formatDatePlusTime(f.date)">
                                            {{prettyPrintMET(item, f)}}
                                        </span>
                                    </li>
                                </ul>
                            </td>
                        </tr>
                    </template>
                </tbody>
            </table>
        </div>
        <div class="launch-totals mt-1">
            <span>{{getLaunchCount('VAB')}}</span>
            <span class="icon is-small ml-1"><i class="fas fa-rocket" aria-hidden="true"></i></span>
            <span class="ml-3">{{getLaunchCount('SPH')}}</span>
            <span class="icon is-small ml-1"><i class="fas fa-plane" aria-hidden="true"></i></span>
        </div>
    </div>
    <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
        <LoadingSpinner />
    </div>
</template>

<style>
    .success-fail-row {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .success-fail-widget {
        display: flex;
    }

    .success-fail-widget > * {
        align-self: center;
    }
</style>

<script lang="ts">
    import { defineComponent } from 'vue';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import { fetchLaunchesForCareer, patchLaunchMeta } from '../utils/api';
    import type { LaunchEventItem, Failure } from 'types';
    import DataTabMixin from '../components/DataTabMixin.vue';
    import LoadingSpinner from '../components/LoadingSpinner.vue';

    export default defineComponent({
        mixins: [DataTabMixin],
        components: {
            LoadingSpinner
        },
        props: {
            canEdit: Boolean
        },
        methods: {
            async queryData(careerId: string) {
                try {
                    const arr = await fetchLaunchesForCareer(careerId) as LaunchEventItem[];
                    arr.forEach(i => i.visible = false);
                    this.items = arr;
                }
                finally {
                    this.isLoading = false;
                }
            },
            async saveLaunchMetaChanges(launch: LaunchEventItem) {
                await patchLaunchMeta(this.careerId, launch.launchID, launch.metadata);
            },
            updateLaunchSuccessState(launch: LaunchEventItem, isSuccess: boolean, event: Event) {
                const meta = launch.metadata = launch.metadata || {};
                meta.success = meta.success === isSuccess ? null : isSuccess;
                event.target!.closest('button').blur();
                this.saveLaunchMetaChanges(launch);
            },
            getVesselIcon(launch: LaunchEventItem) {
                if (launch.builtAt === 'VAB') return 'fa-rocket';
                if (launch.builtAt === 'SPH') return 'fa-plane';
                return '';
            },
            getSuccessStyleForEdit(launch: LaunchEventItem) {
                if (launch.metadata && launch.metadata.success === true) return 'is-active';
                return 'is-outlined';
            },
            getFailStyleForEdit(launch: LaunchEventItem) {
                if (launch.metadata && launch.metadata.success === false)
                    return 'is-active';
                return 'is-outlined';
            },
            getSuccessStateIconForDisp(launch: LaunchEventItem) {
                if (!launch.metadata || launch.metadata.success == null)
                    return '';
                if (launch.metadata.success)
                    return 'fa-check has-text-success';
                return 'fa-xmark has-text-danger';
            },
            getSuccessStateTitle(launch: LaunchEventItem) {
                if (!launch.metadata || launch.metadata.success == null)
                    return '';
                if (launch.metadata.success)
                    return 'Launch met all or at least part of its goals';
                return 'Launch failed to meet its goals';
            },
            hasFailures(launch: LaunchEventItem) {
                return launch.failures && launch.failures.length > 0;
            },
            getTFFailureIconTitle(launch: LaunchEventItem) {
                return `Had ${launch.failures.length} failure${launch.failures.length === 1 ? '' : 's'}`;
            },
            canOpen(launch: LaunchEventItem) {
                return this.hasFailures(launch);
            },
            toggleVisibility(launch: LaunchEventItem) {
                if (!this.canOpen(launch)) return;

                const newState = !launch.visible;
                this.items.forEach(i => i.visible = false);
                launch.visible = newState;
            },
            getLaunchCount(builtAt: string) {
                if (!this.items) return 0;
                return this.items.filter((launch: LaunchEventItem) => launch.builtAt === builtAt).length;
            },
            prettyPrintMET(launch: LaunchEventItem, failure: Failure) {
                const m1 = parseUtcDate(launch.date);
                const m2 = parseUtcDate(failure.date);
                const msDuration = m2.diff(m1);
                let duration = msDuration.rescale();

                if (msDuration.milliseconds > 1000) {
                    duration = duration.set({ milliseconds: 0 });
                }

                if (msDuration.milliseconds > 60 * 60 * 1000) {
                    duration = duration.set({ seconds: 0 });
                }

                return duration.rescale().toHuman({ unitDisplay: 'short' });
            }
        },
        computed: {
            tabName() {
                return 'launches';
            }
        }
    });
</script>
