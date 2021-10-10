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
                    <td>{{ item.vesselName }}</td>
                    <td>{{ formatDate(item.date) }}</td>
                </tr>
            </tbody>
            </table>
        </div>
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
