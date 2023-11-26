const CareerDates = {
    props: ['items', 'dateField', 'extraFields', 'title'],
    computed: {
        isVisible() {
            return !!this.items;
        }
    },
    methods: {
        getAndFormatDate(item) {
            return this.formatDate(item[this.dateField]);
        },
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    template: `
        <div v-if="isVisible" class="box">
        <h2 class="subtitle">{{title}}</h2>
            <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>User</th>
                    <th>Career</th>
                    <th>Date</th>
                    <th v-for="def in extraFields">{{def.title}}</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td>{{ item.userPreferredName }}</td>
                    <td>{{ item.careerName }}</td>
                    <td>{{ getAndFormatDate(item) }}</td>
                    <td v-for="def in extraFields">{{item[def.field]}}</td>
                </tr> 
            </tbody>
            </table>
 
        </div>`
};
