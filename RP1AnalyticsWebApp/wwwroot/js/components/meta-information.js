const MetaInformation = {
    props: ['meta', 'title'],
    computed: {
        isVisible() {
            return !!this.meta;
        },
        dateFormatted() {
            return this.meta.creationDate.split("T")[0];
        }
    },

    template:
        `
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
                        <div class="title is-size-4">{{meta.versionTag}}</div>
                        <div class="subtitle is-size-6">RP-1 Version</div>
                </div>                
                <div class="column">
                        <div class="title is-size-4">{{meta.modRecency}}</div>
                        <div class="subtitle is-size-6">Recency</div>
                </div>                
                <div class="column">
                        <div class="title is-size-4">{{dateFormatted}}</div>
                        <div class="subtitle is-size-6">Creation Date</div>
                </div>        
        </div>
            
        <div class="pt-4 block">
            <div class="subtitle">Notes and Remarks</div>
                <p>{{meta.descriptionText}}</p>
            </div>
        </div>
    `
};