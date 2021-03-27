const SelectionTab = {
    props: ['active'],
    template: `
    <div class="tabs is-centered is-toggle is-toggle-rounded">
        <ul>
            <li v-bind:class="{ 'is-active': active === 'milestones' }">
                <a @click="$emit('change-active', 'milestones')" v-bind:class="{ 'is-active': active === 'milestones' }">
                    <span class="icon is-small"><i class="fas fa-trophy" aria-hidden="true"></i></span>
                    <span>Contracts</span>
                </a>
            </li>
            <li v-bind:class="{ 'is-active': active === 'repeatables' }">
                <a @click="$emit('change-active', 'repeatables')">
                    <span class="icon is-small"><i class="fas fa-redo" aria-hidden="true"></i></span>
                    <span>Repeatables</span>
                </a>
            </li>
            <li v-bind:class="{ 'is-active': active === 'tech' }">
                <a @click="$emit('change-active', 'tech')">
                    <span class="icon is-small"><i class="fas fa-tools" aria-hidden="true"></i></span>
                    <span>Tech</span>
                </a>
            </li>
        </ul>
    </div>`
    
}