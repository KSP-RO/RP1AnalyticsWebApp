const Programs = {
    mixins: [DataTabMixin],
    methods: {
        queryData(careerId) {
            fetch(`/api/careerlogs/${careerId}/programs`)
                .then(res => res.json())
                .then(jsonItems => {
                    this.isLoading = false;
                    this.items = jsonItems;
                })
                .catch(error => alert(error));
        }
    },
    computed: {
        tabName() {
            return 'programs';
        }
    },
    template: `
        <div v-show="isVisible">
            <h2 class="subtitle">Programs</h2>

            <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Accepted</th>
                    <th>Objectives completed</th>
                    <th>Completed</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td>{{ item.title }}</td>
                    <td>{{ formatDate(item.accepted) }}</td>
                    <td>{{ formatDate(item.objectivesCompleted) }}</td>
                    <td>{{ formatDate(item.completed) }}</td>
                </tr>
            </tbody>
            </table>
        </div>
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
