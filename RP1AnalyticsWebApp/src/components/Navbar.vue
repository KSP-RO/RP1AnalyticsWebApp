// Template markup is defined in _Layout.cshtml

<style>
    #navbar .navbar-item.active-filter {
        color: springgreen;
    }
</style>

<script lang="ts">
    import { defineComponent } from 'vue';
    import { activeFilters } from '../utils/activeFilters';
    import type { Filters } from 'types';

    interface ComponentData {
        modalVisible: boolean;
        filters: typeof activeFilters;
    }

    export default defineComponent({
        data(): ComponentData {
            return {
                modalVisible: false,
                filters: activeFilters
            }
        },
        methods: {
            toggleFilters() {
                this.modalVisible = !this.modalVisible;
            },
            applyFilters(filters: Filters) {
                this.filters.player = filters.player;
                this.filters.race = filters.race;
                this.filters.ingameDateOp = filters.ingameDateOp;
                this.filters.ingameDate = filters.ingameDate;
                this.filters.lastUpdateOp = filters.lastUpdateOp;
                this.filters.lastUpdate = filters.lastUpdate;
                this.filters.rp1verOp = filters.rp1verOp;
                this.filters.rp1ver = filters.rp1ver;
                this.filters.difficulty = filters.difficulty;
                this.filters.playstyle = filters.playstyle;

                localStorage.setItem('filters', JSON.stringify(this.filters));
            }
        },
        computed: {
            hasActiveFilters(): boolean {
                return (this.filters.player != null ||
                    this.filters.race != null ||
                    (this.filters.ingameDateOp != null && this.filters.ingameDate != null && this.filters.ingameDate !== '1951-01-01') ||
                    (this.filters.lastUpdateOp != null && this.filters.lastUpdate != null) ||
                    (this.filters.rp1verOp != null && this.filters.rp1ver != null) ||
                    this.filters.difficulty != null ||
                    this.filters.playstyle != null);
            }
        },
        mounted() {
            document.addEventListener('DOMContentLoaded', () => {
                const $navbarBurgers = Array.prototype.slice.call(document.querySelectorAll('.navbar-burger'), 0);

                if ($navbarBurgers.length > 0) {
                    $navbarBurgers.forEach(el => {
                        el.addEventListener('click', () => {
                            // Get the target from the "data-target" attribute
                            const target = el.dataset.target;
                            const $target = document.getElementById(target);

                            el.classList.toggle('is-active');
                            $target!.classList.toggle('is-active');
                        });
                    });
                }
            });
        }
    });
</script>
