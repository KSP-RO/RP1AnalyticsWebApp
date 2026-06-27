<template>
    <div v-if="isVisible" class="box">
        <h2 class="subtitle">{{title}}</h2>
        <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>User</th>
                    <th>Career</th>
                    <th>Date</th>
                    <th v-for="def in extraFields">{{def.title}}</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td>{{ item.userPreferredName }}</td>
                    <td>{{ item.careerName }}</td>
                    <td class="date-col" :title="getAndFormatDateFull(item)">{{ getAndFormatDate(item) }}</td>
                    <td v-for="def in extraFields">{{item[def.field]}}</td>
                </tr>
            </tbody>
        </table>

    </div>
</template>

<script setup lang="ts">
    import { computed } from 'vue';
    import { parseUtcDate } from '../utils/parseUtcDate';

    const props = defineProps<{
        items?: any[];
        dateField: string;
        title?: string;
        extraFields?: { title: string; field: string }[];
    }>();

    const isVisible = computed(() => !!props.items);

    function getAndFormatDate(item: any) {
        return formatDate(item[props.dateField]);
    }

    function getAndFormatDateFull(item: any) {
        return formatDateFull(item[props.dateField]);
    }

    function formatDate(date: string) {
        return date ? parseUtcDate(date).toFormat('yyyy-MM-dd') : '';
    }

    function formatDateFull(date: string) {
        return date ? parseUtcDate(date).toFormat('yyyy-MM-dd HH:mm:ss') : '';
    }
</script>
