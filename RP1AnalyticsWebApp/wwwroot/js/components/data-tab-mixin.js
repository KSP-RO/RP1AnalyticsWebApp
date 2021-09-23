const DataTabMixin = {
    props: ['careerId', 'activeTab'],
    data() {
        return {
            items: null,
            isLoading: false
        }
    },
    watch: {
        careerId(newCareerId, oldCareerId) {
            console.log(`${oldCareerId} -> ${newCareerId}`);
            if (newCareerId === oldCareerId) return;

            this.items = null;
            this.isLoading = !!newCareerId;
            if (newCareerId && this.isTabActive) {
                this.queryData(newCareerId);
            }
        },
        activeTab(newActiveTab, oldActiveTab) {
            if (newActiveTab !== oldActiveTab && this.isTabActive && this.careerId) {
                this.queryData(this.careerId);
            }
        }
    },
    methods: {
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        }
    },
    computed: {
        isTabActive() {
            return this.activeTab === this.tabName;
        },
        isVisible() {
            return this.isTabActive && !this.isLoading && this.items;
        },
        isSpinnerShown() {
            return this.isLoading && this.isTabActive;
        }
    }
}