<template>
    <div class="modal is-active" v-show="isVisible">
        <div class="modal-background" @click="closeModal"></div>
        <div class="modal-content">
            <div class="box">
                <div class="columns">
                    <div class="column">
                        <div class="field">
                            <label class="label" for="filter-player">Player</label>
                            <div class="control has-icons-left">
                                <div class="select is-rounded">
                                    <span class="select">
                                        <select id="filter-player" class="browser-default" v-model="localFilters.player">
                                            <option v-for="p in players" :value="p.userName">
                                                {{ p.preferredName ? p.preferredName : p.userName }}
                                            </option>
                                        </select>
                                    </span>
                                </div>
                                <div class="icon is-small is-left">
                                    <i class="fas fa-user"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="columns">
                    <div class="column">
                        <label class="label" for="filter-ingame-date">Ingame date</label>
                        <div class="field has-addons">
                            <p class="control">
                                <span class="select">
                                    <select v-model="localFilters.ingameDateOp">
                                        <option value="le">&le;</option>
                                        <option value="ge">&ge;</option>
                                    </select>
                                </span>
                            </p>
                            <p class="control">
                                <input id="filter-ingame-date" class="input" type="date" v-model="localFilters.ingameDate" />
                            </p>
                        </div>
                    </div>
                </div>
                <div class="columns">
                    <div class="column">
                        <label class="label" for="filter-last-update">Last update</label>
                        <div class="field has-addons">
                            <p class="control">
                                <span class="select">
                                    <select v-model="localFilters.lastUpdateOp">
                                        <option value="le">&le;</option>
                                        <option value="ge">&ge;</option>
                                    </select>
                                </span>
                            </p>
                            <p class="control">
                                <input id="filter-last-update" class="input" type="date" v-model="localFilters.lastUpdate" />
                            </p>
                        </div>
                    </div>
                </div>
                <div class="columns">
                    <div class="column">
                        <label class="label" for="filter-rp1ver">RP-1 version</label>
                        <div class="field has-addons">
                            <p class="control">
                                <span class="select">
                                    <select v-model="localFilters.rp1verOp">
                                        <option value="eq">=</option>
                                        <option value="le">&le;</option>
                                        <option value="ge">&ge;</option>
                                    </select>
                                </span>
                            </p>
                            <p class="control">
                                <input id="filter-rp1ver" class="input" type="text" v-model="localFilters.rp1ver" />
                            </p>
                        </div>
                    </div>
                </div>
                <div class="columns">
                    <div class="column">
                        <div class="field">
                            <label class="label" for="filter-difficulty">Difficulty</label>
                            <div class="control">
                                <span class="select">
                                    <select id="filter-difficulty" v-model="localFilters.difficulty">
                                        <option value="">Any</option>
                                        <option value="Easy">Easy</option>
                                        <option value="Normal">Normal</option>
                                        <option value="Moderate">Moderate</option>
                                        <option value="Hard">Hard</option>
                                    </select>
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="columns">
                    <div class="column">
                        <div class="field">
                            <label class="label" for="filter-playstyle">Playstyle</label>
                            <div class="control">
                                <span class="select">
                                    <select id="filter-playstyle" v-model="localFilters.playstyle">
                                        <option value="">Any</option>
                                        <option value="Normal">Normal</option>
                                        <option value="Historic">Historic</option>
                                        <option value="Caveman">Caveman</option>
                                    </select>
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="columns">
                    <div class="column">
                        <div class="field">
                            <label class="label" for="filter-race">Race</label>
                            <div class="control">
                                <span class="select">
                                    <select id="filter-race" v-model="localFilters.race">
                                        <option value=""></option>
                                        <option v-for="r in races" :value="r">
                                            {{ r }}
                                        </option>
                                    </select>
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="field is-grouped">
                    <div class="control">
                        <button type="button" class="button is-link" @click="applyFilters">Apply</button>
                    </div>
                    <div class="control">
                        <button type="button" class="button is-link" @click="clearFilters">Clear all</button>
                    </div>
                </div>
            </div>
        </div>
        <button @click="closeModal" class="modal-close is-large" aria-label="close"></button>
    </div>
</template>

<script setup lang="ts">
    import { ref, watch } from 'vue';
    import type { Filters, UserData } from 'types';
    import { fetchUsers, fetchRaces } from '../utils/api';
    import { createEmptyFilters } from '../utils/activeFilters';

    const props = defineProps<{
        filters?: Filters;
        isVisible?: boolean;
    }>();

    const emit = defineEmits<{
        'update:isVisible': [value: boolean];
        'update:filters': [value: Filters];
        'applyFilters': [value: Filters];
    }>();

    const players = ref<UserData[] | null>(null);
    const races = ref<string[] | null>(null);
    const localFilters = ref<Filters>(makeLocalFilters());

    function makeLocalFilters(): Filters {
        const copy = { ...props.filters } as Filters;
        if (!copy.ingameDateOp) copy.ingameDateOp = 'ge';
        if (!copy.ingameDate) copy.ingameDate = '1951-01-01';
        if (!copy.lastUpdateOp) copy.lastUpdateOp = 'ge';
        if (!copy.difficulty) copy.difficulty = '';
        if (!copy.playstyle) copy.playstyle = '';
        return copy;
    }

    watch(() => props.isVisible, (newIsVisible) => {
        if (newIsVisible) {
            localFilters.value = makeLocalFilters();
            if (!players.value) queryPlayers();
            if (!races.value) queryRaces();
        }
    });

    function closeModal() {
        emit('update:isVisible', false);
    }

    function applyFilters() {
        emit('applyFilters', localFilters.value);
        closeModal();
    }

    function clearFilters() {
        emit('applyFilters', createEmptyFilters());
        closeModal();
    }

    async function queryPlayers() {
        players.value = await fetchUsers();
    }

    async function queryRaces() {
        races.value = await fetchRaces();
    }
</script>
