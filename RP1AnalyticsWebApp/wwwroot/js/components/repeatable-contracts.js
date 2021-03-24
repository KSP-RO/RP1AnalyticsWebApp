const RepeatableContracts = {
    props: ['contracts'],
    computed: {
        isVisible() {
            return !!this.contracts;
        }
    },
    methods: {
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    template: `
        <h5>Repeatable contract completions</h5>
        <ul class="collection">
            <li class="collection-item" v-for="item in contracts">
                {{ item.contractDisplayName }} - {{ item.count }}
            </li>
        </ul>`
};
