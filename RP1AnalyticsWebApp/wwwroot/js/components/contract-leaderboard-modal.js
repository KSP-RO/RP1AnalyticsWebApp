const ContractLeaderboardModal = {
    props: ['contractName', 'filters'],
    data() {
        return {
            items: null,
            isLoading: false,
            isVisible: false
        }
    },
    watch: {
        contractName(newContractName, oldContractName) {
            if (newContractName !== oldContractName) {
                this.queryData(newContractName);
            }
        }
    },
    methods: {
        queryData(contractName) {
            this.items = null;
            if (contractName) {
                this.isLoading = true;
                fetch(`/odata/contracts('${contractName}')${constructFilterQueryString(this.filters)}`)
                    .then((res) => res.json())
                    .then((odataResp) => {
                        this.items = odataResp.value;
                        this.isVisible = true;
                        this.isLoading = false;
                    })
                    .catch((error) => alert(error));
            }
        },
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        },
        closeModal() {
            this.isVisible = false;
        }
    },
    template: `
        <div id="modal1" class="modal" :class="{ 'is-active': isVisible }">
            <div class="modal-background" @click="closeModal"></div>
            <div class="modal-content">
                <div id="careerDates" class="contracts-app">
                    <career-dates :items="items"></career-dates>
                </div>
            </div>
            <button @click="closeModal" class="modal-close is-large" aria-label="close"></button>
        </div>`
};
