import 'vite/modulepreload-polyfill';
import { createApp } from 'vue';
import CareerLog from './components/Careerlog.vue';

const app = createApp(CareerLog);
app.mount('#appWrapper');
