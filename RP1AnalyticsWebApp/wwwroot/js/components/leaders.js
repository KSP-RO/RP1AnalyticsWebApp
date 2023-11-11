const Leaders = {
    mixins: [DataTabMixin],
    methods: {
        queryData(careerId) {
            fetch(`/api/careerlogs/${careerId}/leaders`)
                .then(res => res.json())
                .then(jsonItems => {
                    this.isLoading = false;
                    this.items = jsonItems;
                })
                .catch(error => alert(error));
        },
        formatFloat(val) {
            return typeof val === 'number' ? val.toFixed(1) : '';
        },
        getTypeTitle(val) {
            const defs = {
                Admin: 'Admin',
                Engineer: 'Chief Designer',
                FlightDirector: 'Flight Director',
                Scientist: 'Chief Scientist',
                MainContractor: 'Main Contractor',
                Subcontractor: 'Subcontractor'
            };
            const title = defs[val];
            return title ? title : val;
        }
    },
    computed: {
        tabName() {
            return 'leaders';
        }
    },
    template: `
        <div v-show="isVisible">
            <h2 class="subtitle">Leaders</h2>

            <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Added</th>
                    <th>Removed</th>
                    <th>Remove cost</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td>{{ item.title }}</td>
                    <td>{{ getTypeTitle(item.type) }}</td>
                    <td>{{ formatDate(item.dateAdd) }}</td>
                    <td>{{ formatDate(item.dateRemove) }}</td>
                    <td>{{ item.dateRemove ? formatFloat(item.fireCost) : ''}}</td>
                </tr>
            </tbody>
            </table>
        </div>
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
