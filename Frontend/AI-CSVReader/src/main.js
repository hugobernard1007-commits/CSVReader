import { createApp } from 'vue';
import PrimeVue from 'primevue/config';
import Aura from '@primeuix/themes/aura';

import ToastService from 'primevue/toastservice';

import Button from 'primevue/button';
import Card from 'primevue/card';
import Column from 'primevue/column';
import DataTable from 'primevue/datatable';
import Dialog from 'primevue/dialog';
import FileUpload from 'primevue/fileupload';
import InputText from 'primevue/inputtext';
import Message from 'primevue/message';
import MultiSelect from 'primevue/multiselect';
import Select from 'primevue/select';
import Sidebar from 'primevue/sidebar';
import Tag from 'primevue/tag';
import Textarea from 'primevue/textarea';
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
app.component('Column', Column);
app.component('DataTable', DataTable);
app.component('Dialog', Dialog);
app.component('FileUpload', FileUpload);
app.component('InputText', InputText);
app.component('Message', Message);
app.component('MultiSelect', MultiSelect);
app.component('Select', Select);
app.component('Sidebar', Sidebar);
app.component('Tag', Tag);
app.component('Textarea', Textarea);
app.component('Toast', Toast);
app.component('Toolbar', Toolbar);

app.mount('#app');