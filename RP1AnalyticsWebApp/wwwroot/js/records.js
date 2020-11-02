(() => {
    const app = Vue.createApp(Contracts);
    const vm = app.mount('#contracts');

    document.addEventListener('DOMContentLoaded', function () {
        var elems = document.querySelectorAll('.modal');
        var instances = M.Modal.init(elems);
    });

    window.getContracts = getContracts;

    function getContracts(contractName) {
        vm.contracts = null;
        if (contractName) {
            vm.contracts = null;
            fetch(`/api/careerlogs/contracts/${contractName}`)
                .then((res) => res.json())
                .then((jsonContracts) => {
                    vm.contracts = jsonContracts;
                })
                .catch((error) => alert(error));
        }
    }
})();