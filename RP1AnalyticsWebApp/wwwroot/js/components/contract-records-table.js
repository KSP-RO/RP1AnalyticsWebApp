const ContractRecordsTable = {
    props: ['filters'],
    emits: ['contractClicked'],
    data() {
        return {
            items: null,
            isLoading: false
        }
    },
    methods: {
        queryData(filters) {
            this.isLoading = true;
            fetch(`/odata/contractRecords${constructFilterQueryString(filters)}`)
                .then(res => res.json())
                .then(odataResp => {
                    this.isLoading = false;
                    this.items = odataResp.value;
                })
                .catch(error => alert(error));
        },
        contractClicked(contract) {
            this.$emit('contractClicked', contract);
        },
        getCareerUrl(r) {
            return `/?careerId=${r.careerId}`;
        },
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    watch: {
        filters(newFilters, oldFilters) {
            this.queryData(newFilters);
        }
    },
    template: `
        <table class="table is-bordered is-fullwidth is-hoverable" v-show="!isLoading">
            <thead>
                <tr>
                    <th>Contract Name</th>
                    <th>Completion Date</th>
                    <th>User</th>
                    <th>Career</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="r in items">
                    <td>
                        <a role="button" class="modal-trigger" @click="contractClicked(r)">{{r.contractDisplayName}}</a>
                    </td>
                    <td class="date-col">{{formatDate(r.date)}}</td>
                    <td>{{r.userPreferredName}}</td>
                    <td>
                        <a :href="getCareerUrl(r)">{{r.careerName}}</a>
                    </td>
                </tr>
            </tbody>
        </table>
        <div v-if="isLoading" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
