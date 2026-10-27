<!-- CSVDetails.vue -->
<template>
  <div>
    <h1>Détails de {{ this.$route.params.name}}.csv</h1>

    <div v-if="loading">Loading...</div>
    <div v-else-if="error">Error: {{ error.message }}</div>

    <!-- Afficher les détails du fichier si le chargement est terminé et qu'il n'y a pas d'erreur -->
    <div v-else class="charts-grid">
      <h3 class="fileAttr">Attributs du fichier {{file.file.originalFileName }} :
        <ul>
         <li> {{headers.length }} colonnes</li>
         <li> {{ data.length }} lignes</li>
         <li> {{ file.file.sizeBytes }} bytes</li>
         <li> uploadé le {{ uploadDate }}</li>
        </ul>
      </h3>

      <div v-for="col in headers" :key="col" class="chart-card">
          <div class="chart-header">
            <h3>{{ col}}</h3>
            <button class="filter-toggle" @click="toggleFilterPanel(col)">
              Filtrer {{ openFilters[col] ? '▲' : '▼' }}
            </button>
            <button class="export-btn" @click="exportChart(col)">
                  Exporter
            </button>
          </div>
          <p class="chart-meta">{{ metaLabel(col) }}</p>

          <!-- Panneau de filtre catégoriel -->
          <div v-if="openFilters[col] && columnTypes[col] === 'categorical'" class="filter-panel">
            <label v-for="opt in rawCategoryOptions(col)" :key="opt" class="filter-option">
              <input
                type="checkbox"
                :checked="!isExcluded(col, opt)"
                @change="toggleCategory(col, opt)"
              />
              {{ opt }}
            </label>
            <button class="filter-reset" @click="resetFilter(col)">Réinitialiser</button>
          </div>

          <!-- Panneau de filtre numérique -->
          <div v-if="openFilters[col] && columnTypes[col] === 'numeric'" class="filter-panel">
            <div class="range-values">
              <span>{{ formatNumber(numericFilterMin[col] ?? rawNumericRange(col).min) }}</span>
              <span>{{ formatNumber(numericFilterMax[col] ?? rawNumericRange(col).max) }}</span>
            </div>

            <!-- Slider pour les bornes numériques-->
            <div class="range-slider">
              <input
                type="range"
                class="range-input range-min"
                :min="rawNumericRange(col).min"
                :max="rawNumericRange(col).max"
                :step="rawNumericRange(col).step"
                v-model.number="numericFilterMin[col]"
                @input="setNumericFilter(col, 'min')"
              />
              <input
                type="range"
                class="range-input range-max"
                :min="rawNumericRange(col).min"
                :max="rawNumericRange(col).max"
                :step="rawNumericRange(col).step"
                v-model.number="numericFilterMax[col]"
                @input="setNumericFilter(col, 'max')"
              />
            </div>

            <button class="filter-reset" @click="resetFilter(col)">Réinitialiser</button>
          </div>
          <div class="chart-wrapper" >
            <ChartRenderer :ref="el => setChartRef(col, el)"
              :type="columnCharts[col].type"
              :labels="columnCharts[col].labels"
              :values="columnCharts[col].values"
              :label="col"
            />
          </div>
      </div>
    </div>
  </div>
</template>

<script>
import { fetchFileById } from '@/services/api';
import ChartRenderer from './ChartRenderer.vue';

const MAX_CATEGORIES = 50;
const DOUGHNUT_THRESHOLD = 6;
const MIN_BINS = 4;
const MAX_BINS = 20;

export default {
  components: { ChartRenderer },
  data() {
    return {
      file: null, //fichier correspondant à la page
      headers: [], // liste des colonnes du fichier
      data: [], // liste des lignes du fichier
      loading: true, // état de chargement
      error: null, // état d'erreur
      uploadDate : null, // date d'upload du fichier

      openFilters: {}, // état d'ouverture des panneaux de filtre
      openGraph: {}, // état d'ouverture des graphiques
      filters: {},  // filtres appliqués aux colonnes
      numericFilterMin: {}, // valeurs minimales des filtres numériques
      numericFilterMax: {}, // valeurs maximales des filtres numériques
      chartRefs: {} // références aux composants ChartRenderer pour l'exportation des graphiques
    };
  },

  computed: {
    // Fonction pour déterminer le type de chaque colonne (numérique ou catégorielle)
    columnTypes() {
      const types = {};
      for (const h of this.headers) {
        const sample = this.data.slice(0, 30).map(row => row[h]);
        const numericCount = sample.filter(v => this.isNumeric(v)).length;
        const ratio = sample.length ? numericCount / sample.length : 0;
        const distinctNumeric = new Set(
          this.data.map(row => row[h]).filter(v => this.isNumeric(v))
        ).size;
        types[h] = (ratio > 0.8 && distinctNumeric > 8) ? 'numeric' : 'categorical';
      }
      return types;
    },

    // Fonction pour construire les données des graphiques pour chaque colonne
    columnCharts() {
      const result = {};
      for (const h of this.headers) {
        result[h] = this.columnTypes[h] === 'numeric'
          ? this.buildNumericStats(h)
          : this.buildCategoryChart(h);
      }
      return result;
    }
  },

  // Fonction pour récupérer les détails du fichier depuis l'API lors du montage du composant
  async mounted() {
    try {
      this.file = await fetchFileById(this.$route.params.id);
      this.headers = this.file.file.headers;
      this.data = this.file.file.rows;
      this.uploadDate = new Date(this.file.file.uploadedAt).toLocaleDateString('fr-FR');
    } catch (err) {
      this.error = err;
    } finally {
      this.loading = false;
    }
  },

  methods: {
    // Gestion des références aux composants ChartRenderer pour l'exportation des graphiques
    setChartRef(col, el) {
      if (el) this.chartRefs[col] = el;
    },

    // Fonction pour exporter un graphique en image PNG
    exportChart(col) {
      console.log('exportChart appelé pour', col);
      const chartRef = this.chartRefs[col];
      console.log('chartRef trouvé :', chartRef);
      const image = chartRef?.exportAsImage();
      console.log('image générée :', image ? 'oui' : 'non');
      if (!image) return;

      const link = document.createElement('a');
      link.href = image;
      link.download = `${this.sanitizeFilename(col)}.png`;
      link.click();
    },

    // Fonction pour nettoyer le nom de fichier pour l'exportation
    sanitizeFilename(name) {
      return name.replace(/[^a-z0-9_-]/gi, '_');
    },

    // Fonction pour vérifier si une valeur est numérique
    isNumeric(val) {
      if (val === null || val === undefined || val === '') return false;
      return !isNaN(parseFloat(val)) && isFinite(val);
    },

    // Fonction pour formater un nombre avec deux décimales si nécessaire
    formatNumber(n) {
      return Number.isInteger(n) ? String(n) : n.toFixed(2);
    },

    // Fonction pour générer le label de métadonnées pour chaque colonne
    metaLabel(col) {
      return this.columnTypes[col] === 'numeric'
        ? 'Numérique (distribution)'
        : `Catégories (${this.columnCharts[col].labels.length} valeurs affichées)`;
    },

    // Fonction pour basculer l'état d'ouverture du panneau de filtre pour une colonne donnée
    toggleFilterPanel(col) {
      this.openFilters = { ...this.openFilters, [col]: !this.openFilters[col] };
    },


    // --- Filtres catégoriels ---

    // Fonction pour obtenir les options de catégorie uniques pour une colonne donnée
    rawCategoryOptions(col) {
      const set = new Set(this.data.map(row => (row[col] ?? 'N/A').toString()));
      return Array.from(set).sort();
    },

    // Fonction pour vérifier si une valeur est exclue du filtre pour une colonne donnée
    isExcluded(col, value) {
      return this.filters[col]?.excluded?.has(value) ?? false;
    },

    // Fonction pour basculer l'état d'exclusion d'une valeur de catégorie pour une colonne donnée
    toggleCategory(col, value) {
      if (!this.filters[col]) {
        this.filters = { ...this.filters, [col]: { excluded: new Set() } };
      }
      const excluded = this.filters[col].excluded;
      if (excluded.has(value)) {
        excluded.delete(value);
      } else {
        excluded.add(value);
      }
      // force la réactivité (Set mutée en place)
      this.filters = { ...this.filters, [col]: { excluded: new Set(excluded) } };
    },

    // --- Filtres numériques ---

    // Fonction pour calculer la plage de valeurs numériques pour une colonne donnée
    rawNumericRange(col) {
      const nums = this.data.map(row => parseFloat(row[col])).filter(v => !isNaN(v));
      const min = Math.min(...nums);
      const max = Math.max(...nums);
      const step = Number.isInteger(min) && Number.isInteger(max) ? 1 : (max - min) / 100;
      return { min, max, step };
    },

    // Fonction pour définir les filtres numériques pour une colonne donnée
    setNumericFilter(col, edited) {
      const range = this.rawNumericRange(col);

      // Initialise si besoin
      if (this.numericFilterMin[col] === undefined) this.numericFilterMin[col] = range.min;
      if (this.numericFilterMax[col] === undefined) this.numericFilterMax[col] = range.max;

      // Empêche les deux poignées de se croiser
      if (edited === 'min' && this.numericFilterMin[col] > this.numericFilterMax[col]) {
        this.numericFilterMin[col] = this.numericFilterMax[col];
      }
      if (edited === 'max' && this.numericFilterMax[col] < this.numericFilterMin[col]) {
        this.numericFilterMax[col] = this.numericFilterMin[col];
      }

      this.filters = {
        ...this.filters,
        [col]: {
          min: this.numericFilterMin[col],
          max: this.numericFilterMax[col]
        }
      };
    },

    // Fonction pour réinitialiser les filtres pour une colonne donnée
    resetFilter(col) {
      const newFilters = { ...this.filters };
      delete newFilters[col];
      this.filters = newFilters;
      delete this.numericFilterMin[col];
      delete this.numericFilterMax[col];
    },

    // --- Construction des graphiques (avec filtre appliqué) ---

    // Fonction pour construire les données d'un graphique catégoriel pour une colonne donnée
    buildCategoryChart(col) {
      const excluded = this.filters[col]?.excluded ?? new Set();

      const counts = {};
      for (const row of this.data) {
        const key = (row[col] ?? 'N/A').toString();
        if (excluded.has(key)) continue;
        counts[key] = (counts[key] || 0) + 1;
      }

      let entries = Object.entries(counts).sort((a, b) => b[1] - a[1]);

      // Si le nombre de catégories est inférieur ou égal au seuil, on utilise un graphique en anneau
      if (entries.length <= DOUGHNUT_THRESHOLD) {
        return {
          type: 'doughnut',
          labels: entries.map(([k]) => k),
          values: entries.map(([, v]) => v)
        };
      }

      // Sinon, on limite le nombre de catégories affichées et on regroupe le reste dans "Autres"
      if (entries.length > MAX_CATEGORIES) {
        const top = entries.slice(0, MAX_CATEGORIES);
        const rest = entries.slice(MAX_CATEGORIES);
        const restSum = rest.reduce((sum, [, v]) => sum + v, 0);
        entries = [...top, ['Autres', restSum]];
      }

      return {
        type: 'horizontalBar',
        labels: entries.map(([k]) => k),
        values: entries.map(([, v]) => v)
      };
    },

    // Fonction pour construire les données d'un graphique numérique pour une colonne donnée
    buildNumericStats(col) {
      const range = this.filters[col];
      let nums = this.data.map(row => parseFloat(row[col])).filter(v => !isNaN(v));

      if (range && (range.min !== undefined || range.max !== undefined)) {
        nums = nums.filter(v => v >= range.min && v <= range.max);
      }

      if (nums.length === 0) return { type: 'line', labels: [], values: [] };

      const min = Math.min(...nums);
      const max = Math.max(...nums);

      if (min === max) {
        return { type: 'line', labels: [this.formatNumber(min)], values: [nums.length] };
      }

      const distinctCount = new Set(nums).size;
      const sturges = Math.ceil(Math.log2(nums.length) + 1);
      const binCount = Math.max(MIN_BINS, Math.min(MAX_BINS, sturges, distinctCount));
      const binSize = (max - min) / binCount;

      const bins = Array.from({ length: binCount }, () => []);
      for (const n of nums) {
        let idx = Math.floor((n - min) / binSize);
        if (idx >= binCount) idx = binCount - 1;
        bins[idx].push(n);
      }

      const labels = bins.map(binValues => {
        if (binValues.length === 0) return '';
        const binMin = Math.min(...binValues);
        const binMax = Math.max(...binValues);
        return binMin === binMax
          ? this.formatNumber(binMin)
          : `${this.formatNumber(binMin)} - ${this.formatNumber(binMax)}`;
      });

      return { type: 'line', labels, values: bins.map(b => b.length) };
    }
  }
};
</script>

<style scoped>
.charts-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 2rem;
}
.chart-card {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
}
.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.chart-header h3 {
  margin: 0;
}
.filter-toggle {
  background: none;
  border: 1px solid #ccc;
  border-radius: 6px;
  padding: 0.25rem 0.6rem;
  font-size: 0.8rem;
  cursor: pointer;
}
.chart-meta {
  font-size: 0.85rem;
  color: #888;
  margin: 0.25rem 0 0.75rem 0;
}
.filter-panel {
  background: #f7f7f7;
  border-radius: 6px;
  padding: 0.75rem;
  margin-bottom: 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  max-height: 180px;
  overflow-y: auto;
  font-size: 0.85rem;
}
.filter-option {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}
.filter-reset {
  align-self: flex-start;
  margin-top: 0.25rem;
  background: none;
  border: none;
  color: #42A5F5;
  cursor: pointer;
  font-size: 0.8rem;
  text-decoration: underline;
}
.chart-wrapper {
  position: relative;
  height: 400px;
  width: 600px;
}
@media (max-width: 900px) {
  .charts-grid {
    grid-template-columns: 1fr;
  }
}
.range-values {
  display: flex;
  justify-content: space-between;
  font-size: 0.8rem;
  font-weight: 600;
  margin-bottom: 0.25rem;
}

/* Les deux inputs range sont superposés sur la même zone */
.range-slider {
  position: relative;
  height: 32px;
}

.range-input {
  position: absolute;
  top: 8px;
  left: 0;
  width: 100%;
  margin: 0;
  pointer-events: none; /* le track ne capte pas les clics, seul le thumb réagit */
  appearance: none;
  background: transparent;
}

/* La piste grise n'est dessinée qu'une fois, par le premier input */
.range-input::-webkit-slider-runnable-track {
  height: 4px;
  background: #ddd;
  border-radius: 2px;
}
.range-input.range-max::-webkit-slider-runnable-track {
  background: transparent; /* évite de dessiner la piste 2 fois */
}

.range-input::-webkit-slider-thumb {
  appearance: none;
  pointer-events: auto; /* seule la poignée est cliquable */
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: #42A5F5;
  cursor: pointer;
  margin-top: -6px;
}

.range-input::-moz-range-track {
  height: 4px;
  background: #ddd;
  border-radius: 2px;
}
.range-input.range-max::-moz-range-track {
  background: transparent;
}
.range-input::-moz-range-thumb {
  pointer-events: auto;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: #42A5F5;
  border: none;
  cursor: pointer;
}

.fileAttr{
  font-size:x-large;
  align-items: center;
  align-self: center;
  align-content: center;
}
</style>