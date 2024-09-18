<script lang="ts">
    import { defineComponent } from 'vue';
    import { parseUtcDate } from '../utils/parseUtcDate';

    interface ComponentData {
        items: Object[] | null;
        isLoading: boolean;
    }

    export default defineComponent({
        props: {
            careerId: String,
            activeTab: String
        },
        data(): ComponentData {
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
            formatDate(date: string) {
                return date ? parseUtcDate(date).toFormat('yyyy-MM-dd') : '';
            },
            formatDatePlusTime(date: string) {
                return date ? parseUtcDate(date).toFormat('yyyy-MM-dd hh:mm:ss') : '';
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
    });
</script>
