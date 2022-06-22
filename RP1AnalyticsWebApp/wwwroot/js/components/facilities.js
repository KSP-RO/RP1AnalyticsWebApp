const Facilities = {
    mixins: [DataTabMixin],
    methods: {
        queryData(careerId) {
            fetch(`/api/careerlogs/${careerId}/facilities`)
                .then(res => res.json())
                .then(jsonItems => {
                    this.isLoading = false;
                    this.items = jsonItems.filter(e => e.state != 'ConstructionCancelled');
                })
                .catch(error => alert(error));
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
                    <th>Level</th>
                    <th>Started</th>
                    <th>Completed</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td>{{ item.facility }}</td>
                    <td>{{ item.newLevel + 1 }}</td>
                    <td>{{ formatDate(item.started) }}</td>
                    <td>{{ formatDate(item.ended) }}</td>
                </tr>
            </tbody>
            </table>
        </div>
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
