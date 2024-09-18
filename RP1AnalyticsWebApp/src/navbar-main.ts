import 'vite/modulepreload-polyfill';
import { createApp } from 'vue';
import NavBar from './components/Navbar.vue';
import CareerFiltersModal from './components/CareerFiltersModal.vue';

const app = createApp(NavBar);
app.component('CareerFiltersModal', CareerFiltersModal);
app.mount('#navbar');
