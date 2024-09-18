<template>
    <div class="tabs is-centered is-toggle is-toggle-rounded">
        <ul>
            <li v-bind:class="{ 'is-active': active === 'accepted' }">
                <a @click="$emit('change-active', 'accepted')" :class="{ 'is-active': active === 'milestones' }">
                    <span>Accepted</span>
                </a>
            </li>
            <li v-bind:class="{ 'is-active': active === 'objectivesCompleted' }">
                <a @click="$emit('change-active', 'objectivesCompleted')">
                    <span>Objectives Completed</span>
                </a>
            </li>
            <li v-bind:class="{ 'is-active': active === 'completed' }">
                <a @click="$emit('change-active', 'completed')">
                    <span>Completed</span>
                </a>
            </li>
        </ul>
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';

    export default defineComponent({
        props: {
            active: String
        },
        emits: ['change-active']
    });
</script>
