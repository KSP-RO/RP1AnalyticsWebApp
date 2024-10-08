﻿<template>
    <div v-if="isVisible">
        <div class="box">
            <div class="columns is-vcentered is-centered has-text-centered">
                <div class="column">
                    <div class="title">{{meta.careerPlaystyle}}</div>
                    <div class="subtitle">Playstyle</div>
                </div>
                <div class="column">
                    <div class="title">{{meta.difficultyLevel}}</div>
                    <div class="subtitle">Difficulty</div>
                </div>
                <div class="column">
                    <div class="title">{{meta.failureModel}}</div>
                    <div class="subtitle">Failure Model</div>
                </div>
            </div>
            <div class="columns is-vcentered is-centered has-text-centered">
                <div class="column">
                    <div class="title is-size-4">{{versionFormatted}}</div>
                    <div class="subtitle is-size-6">RP-1 Version</div>
                </div>
                <div class="column">
                    <div class="title is-size-4">{{meta.modRecency}}</div>
                    <div class="subtitle is-size-6">Recency</div>
                </div>
            </div>
            <div class="columns is-vcentered is-centered has-text-centered">
                <div class="column">
                    <div class="title is-size-4">{{formatToLocalDate(meta.creationDate)}}</div>
                    <div class="subtitle is-size-6">Creation Date</div>
                </div>
                <div class="column">
                    <div class="title is-size-4" :title="formatToUTCDateTime(meta.lastUpdate)">{{formatToLocalDate(meta.lastUpdate)}}</div>
                    <div class="subtitle is-size-6">Last updated</div>
                </div>
            </div>

            <div class="pt-4 block">
                <div class="subtitle">Notes and Remarks</div>
                <p v-html="descriptionShowdown"></p>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    import type { PropType } from 'vue'
    import type { CareerLogMeta } from 'types';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import * as showdown from 'showdown';

    export default defineComponent({
        props: {
            meta: Object as PropType<CareerLogMeta>,
            title: String,
            isLoading: Boolean
        },
        methods: {
            formatToDate(date: string) {
                if (typeof date !== 'string') return '?';
                const m = parseUtcDate(date);
                return m.toFormat('yyyy-MM-dd');
            },
            formatToLocalDate(date: string) {
                if (typeof date !== 'string') return '?';
                const m = parseUtcDate(date);
                return m.toLocal().toFormat('yyyy-MM-dd');
            },
            formatToUTCDateTime(date: string) {
                if (typeof date !== 'string') return '?';
                const m = parseUtcDate(date);
                return m.toString();
            }
        },
        computed: {
            isVisible() {
                return this.meta && !this.isLoading;
            },
            versionFormatted() {
                return this.meta!.versionTag ? this.meta!.versionTag : '?';
            },
            descriptionShowdown() {
                const converter = new showdown.Converter();
                return converter.makeHtml(this.meta!.descriptionText);
            }
        }
    });
</script>
