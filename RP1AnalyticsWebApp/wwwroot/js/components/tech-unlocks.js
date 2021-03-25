const TechUnlocks = {
    props: ['events', 'isLoading'],
    computed: {
        isVisible() {
            return this.events && !this.isLoading;
        }
    },
    methods: {
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    template: `
        <div v-if="isVisible">
            <h5>Tech research dates</h5>
            <ul class="collection">
                <li class="collection-item" v-for="item in events">
                    {{ item.nodeDisplayName }} - {{ formatDate(item.date) }}
                </li>
            </ul>
        </div>
        <loading-spinner v-if="isLoading"></loading-spinner>`
};
