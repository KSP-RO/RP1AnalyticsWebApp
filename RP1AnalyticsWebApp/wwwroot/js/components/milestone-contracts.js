const MilestoneContracts = {
    props: ['contracts', 'isLoading', 'activeTab'],
    methods: {
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    computed: {
        isVisible() {
            return this.activeTab === 'milestones' && !this.isLoading && this.contracts;
        },
        isSpinnerShown() {
            return this.isLoading && this.activeTab === 'milestones';
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
                <tr v-for="item in contracts">
                    <td>{{ item.contractDisplayName }}</td>
                    <td>{{ formatDate(item.date) }}</td>
                </tr> 
            </tbody>
            </table>
        </div>
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
