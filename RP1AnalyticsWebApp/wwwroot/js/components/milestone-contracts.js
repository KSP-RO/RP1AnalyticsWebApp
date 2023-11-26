const MilestoneContracts = {
    mixins: [DataTabMixin],
    methods: {
        queryData(careerId) {
            fetch(`/api/careerlogs/${careerId}/completedmilestones`)
                .then(res => res.json())
                .then(jsonContracts => {
                    this.isLoading = false;
                    this.items = jsonContracts;
                })
                .catch(error => alert(error));
        }
    },
    computed: {
        tabName() {
            return 'milestones';
        }
    },
    template: `
        <div v-show="isVisible">
            <h2 class="subtitle">Completed Milestones</h2>

            <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Completion Date</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td>{{ item.contractDisplayName }}</td>
                    <td class="date-col">{{ formatDate(item.date) }}</td>
                </tr>
            </tbody>
            </table>
        </div>
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
