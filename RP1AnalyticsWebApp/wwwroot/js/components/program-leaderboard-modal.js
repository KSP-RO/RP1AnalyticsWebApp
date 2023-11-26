const ProgramLeaderboardModal = {
    props: ['programName', 'mode', 'filters'],
    data() {
        return {
            items: null,
            isLoading: false,
            isVisible: false,
            extraFields: [{
                title: 'Speed',
                field: 'speed'
            }]
        }
    },
    watch: {
        programName(newProgramName, oldProgramName) {
            if (newProgramName !== oldProgramName) {
                this.queryData(newProgramName);
            }
        }
    },
    methods: {
        queryData(programName) {
            this.items = null;
            if (programName) {
                this.isLoading = true;
                fetch(`/odata/programs('${programName}')?type=${this.mode}&${constructFilterQueryString(this.filters, true)}`)
                    .then((res) => res.json())
                    .then((odataResp) => {
                        const items = odataResp.value;
                        items.forEach((i) => i.speed = this.mapSpeed(i.speed));

                        this.items = odataResp.value;
                        this.isVisible = true;
                        this.isLoading = false;
                    })
                    .catch((error) => alert(error));
            }
        },
        formatDate(date) {
            return date ? moment.utc(date).format('YYYY-MM-DD') : '';
        },
        closeModal() {
            this.isVisible = false;
        },
        mapSpeed(speed) {
            const defs = {
                Slow: 'Normal',
                Normal: 'Fast',
                Fast: 'Breakneck'
            };
            const title = defs[speed];
            return title ? title : speed;
        }
    },
    computed: {
        dlgTitle() {
            return this.items && this.items[0].title;
        },
        dateField() {
            return this.mode;
        }
    },
    template: `
        <div id="modal2" class="modal" :class="{ 'is-active': isVisible }">
            <div class="modal-background" @click="closeModal"></div>
            <div class="modal-content">
                <div id="careerDates" class="contracts-app">
                    <career-dates :items="items" :date-field="dateField" :extra-fields="extraFields" :title="dlgTitle"></career-dates>
                </div>
            </div>
            <button @click="closeModal" class="modal-close is-large" aria-label="close"></button>
        </div>`
};
