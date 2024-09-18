import 'vite/modulepreload-polyfill';
import { createApp } from 'vue';
import Records from './components/Records.vue';

const app = createApp(Records);
app.mount('#app');
