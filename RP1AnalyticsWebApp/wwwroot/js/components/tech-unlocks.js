const TechUnlocks = {
    props: ['events'],
    computed: {
        isVisible() {
            return !!this.events;
        }
    },
    methods: {
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    template: `
        <h5>Tech research dates</h5>
        <ul class="collection">
            <li class="collection-item" v-for="item in events">
                {{ item.nodeDisplayName }} - {{ formatDate(item.date) }}
            </li>
        </ul>`
};
