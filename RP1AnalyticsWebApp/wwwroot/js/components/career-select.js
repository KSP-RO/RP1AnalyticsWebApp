const CareerSelect = {
    props: ['careerItems', 'selectedCareer', 'filters'],
    emits: ['update:selectedCareer', 'careerChanged'],
    data() {
        return {
            items: null,
            isLoading: false
        }
    },
    methods: {
        queryData(filters) {
            this.isLoading = true;
            fetch(`/odata/careerListItems${constructFilterQueryString(filters)}`)
                .then(res => res.json())
                .then(odataResp => {
                    this.isLoading = false;

                    const groupedMap = odataResp.value.reduce(
                        (entryMap, e) => entryMap.set(e.user, [...entryMap.get(e.user) || [], e]),
                        new Map()
                    );

                    this.items = groupedMap;
                })
                .catch(error => alert(error));
        },
        careerChanged(careerId) {
            this.$emit('update:selectedCareer', careerId);
            this.$emit('careerChanged', careerId);
        }
    },
    watch: {
        filters(newFilters, oldFilters) {
            this.queryData(newFilters);
        }
    },
    template: `
        <div class="control has-icons-left">
            <div class="select is-rounded">
                <select @change="careerChanged($event.target.value)" class="browser-default" v-model="selectedCareer">
                    <option value="Select a career" disabled="" selected="">Select a career</option>
                    <optgroup v-for="[key, value] in items" :label="key">
                        <option v-for="option in value" :value="option.id">
                            {{ option.name }}
                        </option>
                    </optgroup>
                </select>
            </div>
            <div class="icon is-small is-left">
                <i class="fas fa-database"></i>
            </div>
        </div>`
};
