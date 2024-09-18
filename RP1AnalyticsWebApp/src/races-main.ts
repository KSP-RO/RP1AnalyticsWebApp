import 'vite/modulepreload-polyfill';
import { createApp } from 'vue';
import RaceManagement from './components/RaceManagement.vue';

const app = createApp(RaceManagement);
app.mount('#app');
