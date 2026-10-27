<!-- ChartRenderer.vue -->
<template>
  <div class="wrapper" ref="wrapperEl">
    <Doughnut 
      v-if="type === 'doughnut'"
      ref="chartRef"
      :data="chartData"
      :options="doughnutOptions"
    />
    <Line
      v-else-if="type === 'line'"
      ref="chartRef"
      :data="chartData"
      :options="lineOptions"
    />
    <Bar
      v-else
      ref="chartRef"
      :data="chartData"
      :options="barOptions"
    />
  </div>
</template>

<script>
import { Bar, Doughnut, Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  Title, Tooltip, Legend,
  BarElement, ArcElement, LineElement, PointElement,
  CategoryScale, LinearScale
} from 'chart.js'

ChartJS.register(
  Title, Tooltip, Legend,
  BarElement, ArcElement, LineElement, PointElement,
  CategoryScale, LinearScale
)

// Palette de couleurs pour les graphiques
const PALETTE = ['#42A5F5', '#66BB6A', '#FFA726', '#EF5350', '#AB47BC', '#26C6DA', '#8D6E63', '#78909C']

export default {
  name: 'ChartRenderer',
  components: { Bar, Doughnut, Line },
  props: {
    type: { type: String, default: 'bar' }, // 'bar', 'line', 'doughnut'
    labels: { type: Array, default: () => [] },
    values: { type: Array, default: () => [] },
    label: { type: String, default: 'Valeurs' }
  },
  computed: {
    // Prépare les données pour le graphique en fonction du type
    chartData() {
      // Si le type est 'line', on retourne un objet de données spécifique pour le graphique linéaire
      if (this.type === 'line') {
        return {
          labels: this.labels,
          datasets: [{
            label: this.label,
            data: this.values,
            borderColor: '#42A5F5',
            backgroundColor: 'rgba(66, 165, 245, 0.15)',
            fill: true,
            tension: 0.3,
            pointRadius: 4,
            pointBackgroundColor: '#42A5F5'
          }]
        }
      }
      return {
        labels: this.labels,
        datasets: [{
          label: this.label,
          data: this.values,
          backgroundColor: this.type === 'doughnut'
            ? this.labels.map((_, i) => PALETTE[i % PALETTE.length])
            : '#42A5F5'
        }]
      }
    },

    // Options spécifiques pour les graphiques en barres
    barOptions() {
      return {
        responsive: true,
        maintainAspectRatio: false,
        indexAxis: this.type === 'horizontalBar' ? 'y' : 'x',
        plugins: { legend: { display: false } },
        scales: {
          x: { ticks: { autoSkip: true, maxRotation: 45, minRotation: 0 } }
        }
      }
    },

    // Options spécifiques pour les graphiques linéaires
    lineOptions() {
      return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: false }
        }
      }
    },

    // Options spécifiques pour les graphiques en anneau (doughnut)
    doughnutOptions() {
      return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: true, position: 'right' } }
      }
    }
  },
  methods: {
    // Retourne l'image en base64 (PNG) du graphique actuellement affiché
    exportAsImage() {
      const canvas = this.$refs.wrapperEl?.querySelector('canvas');
      if (!canvas) return null;

      const chartInstance = ChartJS.getChart(canvas);
      if (!chartInstance) return null;

      return chartInstance.toBase64Image('image/png', 1);
    }
  }
}
</script>

<style>
.wrapper {
  position: relative;
  height: 400px;
  width: 600px;
}
</style>