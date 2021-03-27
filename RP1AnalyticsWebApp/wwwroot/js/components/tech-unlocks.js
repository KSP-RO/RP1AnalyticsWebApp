const TechUnlocks = {
    props: ['events', 'isLoading', 'activeTab'],
    methods: {
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    computed: {
        isVisible() {
            return this.activeTab === 'tech';
        }
    },
    template: `
        <div v-show="isVisible">
            <h2 class="subtitle">Unlocked Technologies</h2>
            
            <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Name</th> 
                    <th>Completion Date</th> 
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in events">
                    <td>{{ item.nodeDisplayName }}</td>
                    <td>{{ formatDate(item.date) }}</td>
                </tr> 
            </tbody>
            </table>
        </div>
        <div v-if="isLoading" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
