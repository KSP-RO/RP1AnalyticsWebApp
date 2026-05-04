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
    import { activeFilters, normalizeFilters } from '../utils/activeFilters';
    import type { Filters } from 'types';

    export default defineComponent({
        setup() {
            const modalVisible = ref(false);
            const filters = activeFilters;

            const hasActiveFilters = computed(() =>
                filters.players.length > 0 ||
                filters.races.length > 0 ||
                filters.rp1Versions.length > 0 ||
                filters.difficulties.length > 0 ||
                filters.playstyles.length > 0 ||
                filters.recordEligibility !== 'All' ||
                filters.careerDateMode !== 'All' ||
                filters.lastUpdateMode !== 'All'
            );

            function toggleFilters() {
                modalVisible.value = !modalVisible.value;
            }

            function applyFilters(newFilters: Filters) {
                Object.assign(filters, normalizeFilters(newFilters));

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
