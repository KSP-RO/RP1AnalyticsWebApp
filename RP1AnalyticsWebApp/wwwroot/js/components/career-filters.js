(() => {
    const app = Vue.createApp({
        data() {
            return {
                modalVisible: false,
                filters: {}
            };
        },
        methods: {
            toggleFilters() {
                this.modalVisible = !this.modalVisible;
            },
            applyFilters(filters) {
                this.filters = filters;
                if (!filters) {
                    localStorage.removeItem('filters');
                }
                else {
                    localStorage.setItem('filters', JSON.stringify(filters));
                }

                if (window.filtersChanged) {
                    window.filtersChanged(filters);
                }
            }
        },
        computed: {
            hasActiveFilters() {
                return this.filters &&
                    (this.filters.player ||
                    (this.filters.ingameDateOp && this.filters.ingameDate && this.filters.ingameDate !== '1951-01-01') ||
                     (this.filters.rp1verOp && this.filters.rp1ver) ||
                     this.filters.difficulty ||
                     this.filters.playstyle);
            }
        },
        mounted: function () {
            this.$nextTick(function () {
                const sFilters = localStorage.getItem('filters');
                if (sFilters) {
                    this.filters = JSON.parse(sFilters);
                }
            });
        }
    });

    app.component('career-filters-modal', CareerFiltersModal);
    const vm = app.mount('#navbar');
    window.vmFilters = vm;

    window.constructFilterQueryString = function (filters) {
        if (!filters) return '';

        const arr = [];
        if (filters.player) {
            arr.push(`UserLogin eq '${filters.player}'`);
        }

        if (filters.ingameDateOp && filters.ingameDate && filters.ingameDate !== '1951-01-01') {
            arr.push(`EndDate ${filters.ingameDateOp} ${filters.ingameDate}T00:00:00.00Z`);
        }

        if (filters.rp1verOp && filters.rp1ver) {
            let sortableVer = 0;
            const split = filters.rp1ver.split('.');
            const mults = [1000000, 1000, 1];
            for (let i = 0; i < 3; i++) {
                if (i >= split.length) break;
                const num = parseInt(split[i]);
                if (!isNaN(num)) {
                    sortableVer += num * mults[i];
                }
            }

            arr.push(`CareerLogMeta/VersionSort ${filters.rp1verOp} ${sortableVer}`);
        }

        if (filters.difficulty) {
            arr.push(`CareerLogMeta/DifficultyLevel eq '${filters.difficulty}'`);
        }

        if (filters.playstyle) {
            arr.push(`CareerLogMeta/CareerPlaystyle eq '${filters.playstyle}'`);
        }

        if (arr.length === 0) return '';

        return `?$filter=${arr.join(' and ')}`;
    };
})();
