import { computed, watch, type Ref } from 'vue';
import { parseUtcDate } from './parseUtcDate';

export function useDataTab(
    tabName: string,
    props: Readonly<{ careerId?: string | null; activeTab?: string }>,
    items: Ref<any[] | null>,
    isLoading: Ref<boolean>,
    queryDataFn: (careerId: string) => Promise<void>
) {
    const isTabActive = computed(() => props.activeTab === tabName);
    const isVisible = computed(() => isTabActive.value && !isLoading.value && items.value !== null);
    const isSpinnerShown = computed(() => isLoading.value && isTabActive.value);

    watch(() => props.careerId, (newCareerId, oldCareerId) => {
        if (newCareerId === oldCareerId) return;
        items.value = null;
        isLoading.value = !!newCareerId;
        if (newCareerId && isTabActive.value) {
            queryDataFn(newCareerId);
        }
    });

    watch(() => props.activeTab, (newActiveTab, oldActiveTab) => {
        if (newActiveTab !== oldActiveTab && isTabActive.value && props.careerId) {
            queryDataFn(props.careerId);
        }
    });

    function formatDate(date: string) {
        return date ? parseUtcDate(date).toFormat('yyyy-MM-dd') : '';
    }

    function formatDatePlusTime(date: string) {
        return date ? parseUtcDate(date).toFormat('yyyy-MM-dd hh:mm:ss') : '';
    }

    return { isTabActive, isVisible, isSpinnerShown, formatDate, formatDatePlusTime };
}
