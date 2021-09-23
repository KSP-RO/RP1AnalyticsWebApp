const CareerFiltersModal = {
    props: ['isVisible', 'filters'],
    emits: ['update:isVisible', 'update:filters', 'applyFilters'],
    data() {
        return {
            players: null
        }
    },
    computed: {
        localFilters() {
            const copy = { ...this.filters };
            if (!copy.ingameDateOp)
                copy.ingameDateOp = 'ge';

            if (!copy.ingameDate)
                copy.ingameDate = '1951-01-01';

            if (!copy.difficulty)
                copy.difficulty = '';

            if (!copy.playstyle)
                copy.playstyle = '';

            return copy;
        }
    },
    methods: {
        closeModal() {
            this.$emit('update:isVisible', false);
        },
        applyFilters() {
            this.$emit('update:filters', this.localFilters);
            this.$emit('applyFilters', this.localFilters);
            this.closeModal();
        },
        clearFilters() {
            const emptyFilters = {}
            this.$emit('update:filters', emptyFilters);
            this.$emit('applyFilters', emptyFilters);
            this.closeModal();
        },
        queryPlayers() {
            fetch(`/api/users`)
                .then((res) => res.json())
                .then((jsonPlayers) => {
                    this.players = jsonPlayers;
                })
                .catch((error) => alert(error));
        },
    },
    watch: {
        isVisible(newIsVisible) {
            if (newIsVisible && !this.players)
                this.queryPlayers();
        }
    },
    template: `
        <div class="modal is-active" v-show="isVisible">
            <div class="modal-background" @click="closeModal"></div>
            <div class="modal-content">
                <div class="box">
                    <div class="columns">
                        <div class="column">
                            <div class="field">
                                <label class="label">Player</label>
                                <div class="control has-icons-left">
                                    <div class="select is-rounded">
                                        <span class="select">
                                            <select class="browser-default" v-model="localFilters.player">
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
                            <label class="label">Ingame date</label>
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
                                    <input class="input" type="date" v-model="localFilters.ingameDate" />
                                </p>
                            </div>
                        </div>
                    </div>
                    <div class="columns">
                        <div class="column">
                            <label class="label">RP-1 version</label>
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
                                    <input class="input" type="text" v-model="localFilters.rp1ver" />
                                </p>
                            </div>
                        </div>
                    </div>
                    <div class="columns">
                        <div class="column">
                            <div class="field">
                                <label class="label">Difficulty</label>
                                <div class="control">
                                    <span class="select">
                                        <select v-model="localFilters.difficulty">
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
                                <label class="label">Playstyle</label>
                                <div class="control">
                                    <span class="select">
                                        <select v-model="localFilters.playstyle">
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
        </div>`
};
