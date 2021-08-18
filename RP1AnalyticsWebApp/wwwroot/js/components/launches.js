const Launches = {
    props: ['launches', 'isLoading', 'activeTab'],
    methods: {
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    computed: {
        isVisible() {
            return this.activeTab === 'launches' && !this.isLoading;
        },
        isSpinnerShown() {
            return this.isLoading && this.activeTab === 'launches';
        }
    },
    template: `
        <div v-show="isVisible">
            <h2 class="subtitle">Launches</h2>

            <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Vessel Name</th>
                    <th>Date</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in launches">
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
