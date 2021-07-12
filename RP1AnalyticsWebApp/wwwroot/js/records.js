(() => {
    const app = Vue.createApp({
        data() {
            return {
                items: null
            };
        }
    });
    app.component('career-dates', CareerDates);
    const vm = app.mount('#careerDates');


    window.getContracts = getContracts;

    function getContracts(contractName) {
        vm.items = null;
        if (contractName) {

            const modal = document.querySelector('#modal1');

            vm.contracts = null;
            fetch(`/api/careerlogs/contracts/${contractName}`)
                .then((res) => res.json())
                .then((jsonContracts) => {
                    vm.items = jsonContracts;
                    modal.classList.add("is-active");
                })
                .catch((error) => alert(error));
        }
    }

    window.handleClose = handleClose;

    function handleClose() {
        document.querySelector('#modal1').classList.remove("is-active");
    }

})();