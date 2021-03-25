const RepeatableContracts = {
    props: ['contracts', 'isLoading'],
    computed: {
        isVisible() {
            return this.contracts && !this.isLoading;
        }
    },
    methods: {
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    template: `
        <div v-if="isVisible">
            <h5>Repeatable contract completions</h5>
            <ul class="collection">
                <li class="collection-item" v-for="item in contracts">
                    {{ item.contractDisplayName }} - {{ item.count }}
                </li>
            </ul>
        </div>
        <loading-spinner v-if="isLoading"></loading-spinner>`
};
