const CareerDates = {
    props: ['items'],
    computed: {
        isVisible() {
            return !!this.items;
        }
    },
    methods: {
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    template: `
        <div v-if="isVisible">
            <h5>{{items[0].contractDisplayName}}</h5>
            <ul class="collection">
                <li class="collection-item" v-for="item in items">
                    {{ item.careerName }} - {{ formatDate(item.date) }}
                </li>
            </ul>
        </div>`
};
