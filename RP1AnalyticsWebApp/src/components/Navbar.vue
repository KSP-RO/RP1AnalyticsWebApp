<style>
    #navbar .navbar-item.active-filter {
        color: springgreen;
    }
</style>

<script lang="ts">
    // Template markup is defined in _Layout.cshtml
    //
    // NOTE: This component must use <script> (not <script setup>).
    // When <script setup> is used without a <template> block, the Vue SFC compiler
    // generates a setup() that returns an empty render function () => {} instead of
    // an object of bindings. Vue then uses that as the render function and ignores
    // the in-DOM template entirely, silently discarding all reactive state.

    import { defineComponent, ref, computed, onMounted } from 'vue';
    import { activeFilters } from '../utils/activeFilters';
    import type { Filters } from 'types';

    export default defineComponent({
        setup() {
            const modalVisible = ref(false);
            const filters = activeFilters;

            const hasActiveFilters = computed(() =>
                filters.player != null ||
                filters.race != null ||
                (filters.ingameDateOp != null && filters.ingameDate != null && filters.ingameDate !== '1951-01-01') ||
                (filters.lastUpdateOp != null && filters.lastUpdate != null) ||
                (filters.rp1verOp != null && filters.rp1ver != null) ||
                filters.difficulty != null ||
                filters.playstyle != null
            );

            function toggleFilters() {
                modalVisible.value = !modalVisible.value;
            }

            function applyFilters(newFilters: Filters) {
                filters.player = newFilters.player;
                filters.race = newFilters.race;
                filters.ingameDateOp = newFilters.ingameDateOp;
                filters.ingameDate = newFilters.ingameDate;
                filters.lastUpdateOp = newFilters.lastUpdateOp;
                filters.lastUpdate = newFilters.lastUpdate;
                filters.rp1verOp = newFilters.rp1verOp;
                filters.rp1ver = newFilters.rp1ver;
                filters.difficulty = newFilters.difficulty;
                filters.playstyle = newFilters.playstyle;

                localStorage.setItem('filters', JSON.stringify(filters));
            }

            onMounted(() => {
                document.addEventListener('DOMContentLoaded', () => {
                    const $navbarBurgers = Array.prototype.slice.call(document.querySelectorAll('.navbar-burger'), 0);

                    if ($navbarBurgers.length > 0) {
                        $navbarBurgers.forEach(el => {
                            el.addEventListener('click', () => {
                                const target = el.dataset.target;
                                const $target = document.getElementById(target);

                                el.classList.toggle('is-active');
                                $target!.classList.toggle('is-active');
                            });
                        });
                    }
                });
            });

            return { modalVisible, filters, hasActiveFilters, toggleFilters, applyFilters };
        }
    });
</script>
