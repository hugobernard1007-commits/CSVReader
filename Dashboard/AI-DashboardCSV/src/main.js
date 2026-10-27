import { createApp } from 'vue';
import PrimeVue from 'primevue/config';
import Aura from '@primeuix/themes/aura';

import ToastService from 'primevue/toastservice';

import Button from 'primevue/button';
import Card from 'primevue/card';
import Chart from 'primevue/chart';
import Column from 'primevue/column';
import DataTable from 'primevue/datatable';
import Dialog from 'primevue/dialog';
import InputNumber from 'primevue/inputnumber';
import InputText from 'primevue/inputtext';
import Message from 'primevue/message';
import MultiSelect from 'primevue/multiselect';
import Panel from 'primevue/panel';
import Select from 'primevue/select';
import Tag from 'primevue/tag';
import Toast from 'primevue/toast';
import Toolbar from 'primevue/toolbar';

import 'primeicons/primeicons.css';
import './assets/style.css';

import App from './App.vue';

const app = createApp(App);

app.use(PrimeVue, {
  theme: {
    preset: Aura
  }
});

app.use(ToastService);

app.component('Button', Button);
app.component('Card', Card);
app.component('Chart', Chart);
app.component('Column', Column);
app.component('DataTable', DataTable);
app.component('Dialog', Dialog);
app.component('InputNumber', InputNumber);
app.component('InputText', InputText);
app.component('Message', Message);
app.component('MultiSelect', MultiSelect);
app.component('Panel', Panel);
app.component('Select', Select);
app.component('Tag', Tag);
app.component('Toast', Toast);
app.component('Toolbar', Toolbar);

app.mount('#app');