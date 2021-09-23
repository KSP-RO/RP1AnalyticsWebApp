(() => {
    const app = Vue.createApp({
        data() {
            return {
                contractName: null,
                filters: null
            };
        },
        methods: {
            showContractLeaderboard(contract) {
                if (this.contractName === contract.contractInternalName) {
                    this.$refs.modal.isVisible = true;
                }
                this.contractName = contract.contractInternalName;
            },
            handleFiltersChange(filters) {
                this.filters = filters;
            }
        }
    });
    app.component('records-table', RecordsTable);
    app.component('contract-leaderboard-modal', ContractLeaderboardModal);
    app.component('career-dates', CareerDates);
    const vm = app.mount('#app');

    vm.handleFiltersChange(vmFilters.filters);

    window.filtersChanged = filters => {
        vm.handleFiltersChange(filters);
    }
})();
