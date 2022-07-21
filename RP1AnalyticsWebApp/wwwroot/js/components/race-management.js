const RaceManagement = {
    data() {
        return {
            items: null,
            isLoading: false
        }
    },
    methods: {
        queryData() {
            this.isLoading = true;
            fetch(`/api/careerLogs/list`)
                .then(res => res.json())
                .then(arr => {
                    arr.forEach(i => i.isUpdating = false);
                    this.isLoading = false;
                    this.items = arr;
                })
                .catch(error => alert(error));
        },
        saveData(item) {
            item.isUpdating = true;
            fetch(`/api/careerLogs/${item.id}/Race`, {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(item.race)
            })
                .catch(error => alert(error))
                .finally(() => {
                    item.isUpdating = false;
                });
        },
        getCareerUrl(c) {
            return `/?careerId=${c.id}`;
        }
    },
    mounted() {
        this.queryData();
    },
    template: `
        <table class="table is-bordered is-fullwidth" v-show="!isLoading">
            <thead>
                <tr>
                    <th>User</th>
                    <th>Career</th>
                    <th>Race</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td class="is-vcentered">{{item.userPreferredName}}</td>
                    <td class="is-vcentered"><a :href="getCareerUrl(item)">{{item.name}}</a></td>
                    <td class="is-vcentered">
                        <form>
                            <div class="field has-addons">
                                <div class="control is-expanded">
                                    <input class="input" type="text" name="ris-name" autocomplete="on" v-model="item.race" />
                                </div>
                                <div class="control">
                                  <button type="button" class="button is-primary" v-on:click="saveData(item)" :class="{ 'is-loading': item.isUpdating }">
                                      <span class="icon is-small"><i class="far fa-save fa-lg"></i></span>
                                  </button>
                                </div>
                            </div>
                        </form>
                    </td>
                </tr>
            </tbody>
        </table>
        <div v-if="isLoading" class="columns mt-4 is-centered is-vcentered">
            <loading-spinner></loading-spinner>
        </div>`
};
