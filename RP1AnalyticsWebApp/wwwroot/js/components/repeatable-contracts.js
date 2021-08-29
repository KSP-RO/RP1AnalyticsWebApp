const RepeatableContracts = {
    mixins: [DataTabMixin],
    methods: {
        queryData(careerId) {
            fetch(`/api/careerlogs/${careerId}/completedRepeatables`)
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
            return 'repeatables';
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
                <tr v-for="item in items">
                    <td>{{ item.contractDisplayName }}</td>
                    <td>{{ item.count }}</td>
                </tr> 
            </tbody>
            </table>
        </div>
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
