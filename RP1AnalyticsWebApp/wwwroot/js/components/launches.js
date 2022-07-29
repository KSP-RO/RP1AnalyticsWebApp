const Launches = {
    mixins: [DataTabMixin],
    methods: {
        queryData(careerId) {
            fetch(`/api/careerlogs/${careerId}/launches`)
                .then(res => res.json())
                .then(jsonItems => {
                    this.isLoading = false;
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
                <tr v-for="item in items">
                    <td><span class="icon is-small"><i class="fas" :class="getVesselIcon(item)" aria-hidden="true"></i></span></td>
                    <td>
                        <span>{{ item.vesselName }}</span>
                        <span v-if="hasFailures(item)" class="icon is-small inline-icon">
                            <img src="images/agathorn.webp" alt="failure" :title="getFailureIconTitle(item)" />
                        </span>
                    </td>
                    <td>{{ formatDate(item.date) }}</td>
                </tr>
            </tbody>
            </table>
        </div>
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
