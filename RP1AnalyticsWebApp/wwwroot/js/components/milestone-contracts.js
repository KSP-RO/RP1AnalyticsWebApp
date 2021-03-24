const MilestoneContracts = {
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
        <div v-if="isVisible">
            <h5>Completed milestones</h5>
            <ul class="collection">
                <li class="collection-item" v-for="item in contracts">
                    {{ item.contractDisplayName }} - {{ formatDate(item.date) }}
                </li>
            </ul>
        </div>`
};
