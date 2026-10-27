import * as VueRouter from 'vue-router';
import CSVDetails from '../components/CSVDetails.vue';
import CSVList from '../components/CSVList.vue';
import App from '../App.vue';
import * as Vue from 'vue';


const routes =  [
    { path : '/', name : "Home", component : CSVList },
    { path : '/details/:id/:name', name : "Details", component: CSVDetails },
];

const router = VueRouter.createRouter({
    history: VueRouter.createWebHistory(),
    routes
});

export default router;