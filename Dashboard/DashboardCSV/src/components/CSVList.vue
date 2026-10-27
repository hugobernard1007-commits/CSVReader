

<template>
  <h1>Dashboard CSV</h1>
  <div>
    <h2>Fichiers CSV disponibles :</h2>
    <div v-if="loading">Chargement en cours...</div>
    <!-- Afficher la liste des fichiers si le chargement est terminé et qu'il n'y a pas d'erreur ! -->
    <ul v-else>
      <li v-for="file in files" :key="file.id">
          {{ file.name }}
        <RouterLink :to="`/details/${file.id}/${file.name}`">Voir les détails  →</RouterLink>
      </li>
    </ul>
    <div v-if="error">Error: {{ error.message }}</div>
  </div>
</template>

<script>
import { fetchFiles} from '@/services/api';

export default {
  data() {
    return {
      files: [], // liste des fichiers CSV récupérés depuis l'API
      loading: true, // état de chargement
      error: null, // état d'erreur
    };
  },

  async mounted() {
    try {
      this.files = await fetchFiles();
    } catch (err) {
      this.error = err;
    } finally {
      this.loading = false;
    }
  },
};
</script>

<style scooped>
h1 {
  align-items: baseline;
  align-self: baseline;
  align-content: baseline;
}



</style>