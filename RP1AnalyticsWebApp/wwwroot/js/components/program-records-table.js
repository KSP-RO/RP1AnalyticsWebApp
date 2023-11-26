const ProgramRecordsTable = {
    props: ['mode', 'filters'],
    emits: ['programClicked'],
    data() {
        return {
            items: null,
            isLoading: false
        }
    },
    methods: {
        queryData(filters) {
            this.isLoading = true;
            fetch(`/odata/programRecords?type=${this.mode}&${constructFilterQueryString(this.filters, true)}`)
                .then(res => res.json())
                .then(odataResp => {
                    this.isLoading = false;
                    this.items = odataResp.value;
                })
                .catch(error => alert(error));
        },
        programClicked(contract) {
            this.$emit('programClicked', contract);
        },
        getCareerUrl(r) {
            return `/?careerId=${r.careerId}`;
        },
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    watch: {
        mode() {
            this.queryData();
        },
        filters() {
            this.queryData();
        }
    },
    template: `
        <table class="table is-bordered is-fullwidth is-hoverable" v-show="!isLoading">
            <thead>
                <tr>
                    <th>Program Name</th>
                    <th>Date</th>
                    <th>User</th>
                    <th>Career</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="r in items">
                    <td>
                        <a role="button" class="modal-trigger" @click="programClicked(r)">{{r.programDisplayName}}</a>
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
