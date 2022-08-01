const Launches = {
    mixins: [DataTabMixin],
    methods: {
        queryData(careerId) {
            fetch(`/api/careerlogs/${careerId}/launches`)
                .then(res => res.json())
                .then(jsonItems => {
                    this.isLoading = false;
                    jsonItems.forEach(i => i.visible = false);
                    this.items = jsonItems;
                })
                .catch(error => alert(error));
        },
        getVesselIcon(launch) {
            if (launch.builtAt === 'VAB') return 'fa-rocket';
            if (launch.builtAt === 'SPH') return 'fa-plane';
            return '';
        },
        hasFailures(launch) {
            return launch.failures && launch.failures.length > 0;
        },
        getFailureIconTitle(launch) {
            return `Had ${launch.failures.length} failure${launch.failures.length === 1 ? '' : 's'}`;
        },
        canOpen(launch) {
            return this.hasFailures(launch);
        },
        toggleVisibility(item) {
            if (!this.canOpen(item)) return;

            const newState = !item.visible;
            this.items.forEach(i => i.visible = false);
            item.visible = newState;
        },
        prettyPrintMET(launch, failure) {
            const m1 = moment.utc(launch.date);
            const m2 = moment.utc(failure.date);
            const duration = moment.duration(m2.diff(m1));

            return duration.format(customTemplate);

            function customTemplate() {
                return this.duration.asSeconds() >= 3600 ? 'd [day] h [hour] m [minute]' :
                                                           'm [minute] s [second]';
            }
        }
    },
    computed: {
        tabName() {
            return 'launches';
        }
    },
    template: `
        <div v-show="isVisible">
            <h2 class="subtitle">Launches</h2>

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
                                <span>{{ item.vesselName }}</span>
                                <span v-if="hasFailures(item)" class="icon is-small inline-icon">
                                    <img src="images/agathorn.webp" alt="failure" :title="getFailureIconTitle(item)" />
                                </span>
                            </td>
                            <td><span :title="formatDatePlusTime(item.date)">{{ formatDate(item.date) }}</span></td>
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
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
