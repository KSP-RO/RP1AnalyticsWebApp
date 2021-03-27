const CareerDates = {
    props: ['items'],
    computed: {
        isVisible() {
            return !!this.items;
        }
    },
    methods: {
        formatDate(date) {
            console.log("called me and working")
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    template: `
        <div v-if="isVisible">
        <h2 class="subtitle">{{items[0].contractDisplayName}}</h2>
            <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Name</th> 
                    <th>Completion Date</th> 
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in contracts">
                    <td>{{ item.careerName }}</td>
                    <td>{{ formatDate(item.date) }}</td>
                </tr> 
            </tbody>
            </table>
 
        </div>`
};
