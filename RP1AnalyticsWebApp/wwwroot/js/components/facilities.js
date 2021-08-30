const Facilities = {
    mixins: [DataTabMixin],
    methods: {
        queryData(careerId) {
            fetch(`/api/careerlogs/${careerId}/facilities`)
                .then(res => res.json())
                .then(jsonItems => {
                    this.isLoading = false;

                    const groupedMap = jsonItems.reduce(
                        (entryMap, e) => entryMap.set(e.facility + e.newLevel, [...entryMap.get(e.facility + e.newLevel) || [], e]),
                        new Map()
                    );

                    this.items = Array.from(groupedMap.values()).map(e => {
                        return {
                            facility: e[0].facility,
                            newLevel: e[0].newLevel,
                            startDate: e[0].date,
                            endDate: e.length > 1 ? e[1].date : null
                        }
                    });
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
                    <td>{{ formatDate(item.startDate) }}</td>
                    <td>{{ formatDate(item.endDate) }}</td>
                </tr>
            </tbody>
            </table>
        </div>
        <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
