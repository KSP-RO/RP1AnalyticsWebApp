const RepeatableContracts = {
    props: ['contracts', 'isLoading', 'activeTab'],
    computed: {
        isVisible() {
            return this.activeTab === 'repeatables';
        }
    },
    methods: {
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    template: `
        <div v-show="isVisible">
            <h2 class="subtitle">Completed Repeatables</h2>
            
            <table class="table is-bordered is-fullwidth">
            <thead>
                <tr>
                    <th>Name</th> 
                    <th># of Completions</th> 
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in contracts">
                    <td>{{ item.contractDisplayName }}</td>
                    <td>{{ item.count }}</td>
                </tr> 
            </tbody>
            </table>
        </div>
        <div v-if="isLoading" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
