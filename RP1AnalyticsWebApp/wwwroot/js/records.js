(() => {
    const app = Vue.createApp({
        data() {
            return {
                programsMode: 'completed',
                programName: null,
                contractName: null,
                filters: null
            };
        },
        methods: {
            handleChangeActive(tabName) {
                this.programsMode = tabName;
            },
            showProgramLeaderboard(program) {
                if (this.programName === program.programName) {
                    this.$refs.programModal.isVisible = true;
                }
                this.programName = program.programName;
            },
            showContractLeaderboard(contract) {
                if (this.contractName === contract.contractInternalName) {
                    this.$refs.contractModal.isVisible = true;
                }
                this.contractName = contract.contractInternalName;
            },
            handleFiltersChange(filters) {
                this.filters = filters;
                this.contractName = null;
            }
        }
    });
    app.component('loading-spinner', LoadingSpinner);
    app.component('program-record-type-select', ProgramRecordTypeSelect);
    app.component('program-records-table', ProgramRecordsTable);
    app.component('program-leaderboard-modal', ProgramLeaderboardModal);
    app.component('contract-records-table', ContractRecordsTable);
    app.component('contract-leaderboard-modal', ContractLeaderboardModal);
    app.component('career-dates', CareerDates);
    const vm = app.mount('#app');

    vm.handleFiltersChange(vmFilters.filters);

    window.filtersChanged = filters => {
        vm.handleFiltersChange(filters);
    }
})();
