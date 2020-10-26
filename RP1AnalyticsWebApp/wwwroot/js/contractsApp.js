const Contracts = {
    data() {
        return {
            contracts: null
        };
    },
    computed: {
        isVisible() {
            return !!this.contracts;
        }
    },
    methods: {
        formatDate(date) {
            return moment.utc(date).format('YYYY-MM-DD');
        }
    }
};
