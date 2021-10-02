const MetaInformation = {
    props: ['meta', 'title', 'isLoading'],
    computed: {
        isVisible() {
            return this.meta && !this.isLoading;
        },
        nationalityFormatted() {
            if (!this.meta.nationality) return '?';

            const map = {
                'RU': 'Soviet/Russia',
                'EU': 'ESA',
                'JP': 'Japan',
                'CN': 'China'
            };
            const v = map[this.meta.nationality];
            return v ? v : this.meta.nationality;
        },
        versionFormatted() {
            return this.meta.versionTag ? this.meta.versionTag : '?';
        },
        dateFormatted() {
            return this.meta.creationDate ? this.meta.creationDate.split("T")[0] : '?';
        },
        descriptionShowdown() {
            const converter = new showdown.Converter();
            return converter.makeHtml(this.meta.descriptionText);
        }
    },

    template: `
        <div v-if="isVisible">
            <div class="box">
                <div class="columns is-multiline is-vcentered is-centered has-text-centered">
                    <div class="column is-half-tablet is-one-third-desktop is-one-quarter-fullhd">
                        <div class="title">{{meta.careerPlaystyle}}</div>
                        <div class="subtitle">Playstyle</div>
                    </div>

                    <div class="column is-half-tablet is-one-third-desktop is-one-quarter-fullhd">
                        <div class="title">{{meta.difficultyLevel}}</div>
                        <div class="subtitle">Difficulty</div>
                    </div>

                    <div class="column is-half-tablet is-one-third-desktop is-one-quarter-fullhd">
                        <div class="title">{{nationalityFormatted}}</div>
                        <div class="subtitle">Tech national pathway</div>
                    </div>

                    <div class="column is-half-tablet is-one-third-desktop is-one-quarter-fullhd">
                        <div class="title">{{meta.failureModel}}</div>
                        <div class="subtitle">Failure Model</div>
                    </div>

                    <div class="column is-half-tablet is-one-third-desktop is-one-quarter-fullhd">
                        <div class="title is-size-4">{{versionFormatted}}</div>
                        <div class="subtitle is-size-6">RP-1 Version</div>
                    </div>

                    <div class="column is-half-tablet is-one-third-desktop is-one-quarter-fullhd">
                        <div class="title is-size-4">{{meta.modRecency}}</div>
                        <div class="subtitle is-size-6">Recency</div>
                    </div>

                    <div class="column is-half-tablet is-one-third-desktop is-one-quarter-fullhd">
                        <div class="title is-size-4">{{dateFormatted}}</div>
                        <div class="subtitle is-size-6">Creation Date</div>
                    </div>
                </div>

                <div class="pt-4 block">
                    <div class="subtitle">Notes and Remarks</div>
                    <p v-html="descriptionShowdown"></p>
                </div>
            </div>
        </div>`
};
