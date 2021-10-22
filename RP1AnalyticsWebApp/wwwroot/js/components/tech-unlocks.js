const TechUnlocks = {
    mixins: [DataTabMixin],
    methods: {
        queryData(careerId) {
            fetch(`/api/careerlogs/${careerId}/tech`)
                .then(res => res.json())
                .then(jsonItems => {
                    this.isLoading = false;
                    this.items = jsonItems;
                })
                .catch(error => alert(error));
        },
        formatFloat(val) {
            return typeof val === 'number' ? val.toFixed(2) : '';
        }
    },
    computed: {
        tabName() {
            return 'tech';
        }
    },
    template: `
        <div v-show="isVisible">
            <h2 class="subtitle">Unlocked Technologies</h2>
            
            <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Rate Multiplier</th>
                    <th>Completion Date</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td>{{ item.nodeDisplayName }}</td>
                    <td>{{ formatFloat(item.yearMult) }}</td>
                    <td>{{ formatDate(item.date) }}</td>
                </tr> 
            </tbody>
            </table>
        </div>
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
