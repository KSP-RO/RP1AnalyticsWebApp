const MetaInformation = {
    props: ['meta', 'title'],
    computed: {
        isVisible() {
            return !!this.meta;
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
        </div>
        
        <div class="block">
            <div class="subtitle">Notes and Remarks</div>
                <p>{{meta.descriptionText}}</p>
            </div>
        </div>
    `
};