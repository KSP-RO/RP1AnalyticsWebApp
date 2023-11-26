const Facilities = {
    mixins: [DataTabMixin],
    methods: {
        queryData(careerId) {
            const p1 = fetch(`/api/careerlogs/${careerId}/facilities`)
                .then(res => res.json())
                .then(jsonItems => {
                    return jsonItems.filter(e => e.state != 'ConstructionCancelled');
                });
            const p2 = fetch(`/api/careerlogs/${careerId}/lcs`)
                .then(res => res.json())
                .then(jsonItems => {
                    jsonItems.forEach(i => i.visible = false);
                    return jsonItems.filter(e => e.state != 'ConstructionCancelled');
                });

            Promise.all([p1, p2])
                .then((values) => {
                    this.isLoading = false;
                    const facilityItems = values[0];
                    const lcItems = values[1];
                    const combinedItems = facilityItems.concat(lcItems);
                    combinedItems.sort((a, b) => {
                        const v1 = a.started ? a.started : a.constrStarted;
                        const v2 = b.started ? b.started : b.constrStarted;
                        if (v1 < v2) {
                            return -1;
                        }
                        if (v1 > v2) {
                            return 1;
                        }

                        return 0;
                    });
                    this.items = combinedItems;
                })
                .catch(error => alert(error));
        },
        getLCIcon(lc) {
            if (lc.lcType === 'Pad') return 'fa-rocket';
            if (lc.lcType === 'Hangar') return 'fa-plane';
            return '';
        },
        toggleVisibility(item) {
            const newState = !item.visible;
            this.items.forEach(i => i.visible = false);
            item.visible = newState;
        },
        formatMass(value) {
            if (value > 1e+38) return '∞';
            return value;
        },
        getFacilityTitle(val) {
            const defs = {
                ResearchAndDevelopment: 'R&D',
                Administration: 'Administration',
                MissionControl: 'Mission Control',
                TrackingStation: 'Tracking Station',
                AstronautComplex: 'Astronaut Complex'
            };
            const title = defs[val];
            return title ? title : val;
        }
    },
    computed: {
        tabName() {
            return 'facilities';
        }
    },
    template: `
        <div v-show="isVisible">
            <h2 class="subtitle">Facility construction and upgrades</h2>

            <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Facility</th>
                    <th>Level / Max tonnage</th>
                    <th>Started</th>
                    <th>Completed</th>
                </tr>
            </thead>
            <tbody>
                <template v-for="item in items">
                    <tr v-if="item.lcType" @click="toggleVisibility(item)" class="clickable" :class="{ 'is-selected': item.visible }">
                        <td>
                            <span class="icon is-small"><i class="fas mr-2" :class="getLCIcon(item)" aria-hidden="true"></i></span>
                            <span>{{ item.name }}</span>
                        </td>
                        <td>{{ formatMass(item.massMax) }}</td>
                        <td class="date-col">{{ formatDate(item.constrStarted) }}</td>
                        <td class="date-col">{{ formatDate(item.constrEnded) }}</td>
                    </tr>
                    <tr v-if="item.lcType && item.visible">
                        <td colspan="3">
                            <div>Cost: √{{item.modCost.toLocaleString('en', { maximumFractionDigits: 0 })}}</div>
                            <div>Height: {{item.sizeMax.y}}m</div>
                            <div>Width: {{item.sizeMax.x > item.sizeMax.z ? item.sizeMax.x : item.sizeMax.z}}m</div>
                            <div>Human-rated: {{item.isHumanRated ? 'Yes' : 'No'}}</div>
                        </td>
                    </tr>
                    <tr v-if="!item.lcType">
                        <td>{{ getFacilityTitle(item.facility) }}</td>
                        <td>{{ item.newLevel + 1 }}</td>
                        <td class="date-col">{{ formatDate(item.started) }}</td>
                        <td class="date-col">{{ formatDate(item.ended) }}</td>
                    </tr>
                </template>
            </tbody>
            </table>
        </div>
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
