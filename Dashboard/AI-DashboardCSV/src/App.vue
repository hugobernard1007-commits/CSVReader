<template>
  <div class="app-shell">
    <Toast />

    <header class="header">
      <h1>Application 2 - Dashboard de consultation API</h1>
      <p>
        Front-end Vue + PrimeVue consommant les endpoints publics générés par l’Application 1.
      </p>
    </header>

    <main class="container">
      <section class="grid">
        <Card>
          <template #title>
            Connexion aux données exposées par l’Application 1
          </template>

          <template #content>
            <Message severity="info">
              Cette application ne possède pas de back-end dédié. Elle consomme directement
              l’API REST exposée par l’Application 1.
            </Message>

            <div class="endpoint-url" style="margin: 1rem 0">
              API configurée : {{ apiUrl }}
            </div>

            <div class="grid-3">
              <div class="form-row">
                <label class="label">Endpoint disponible</label>
                <Select
                  v-model="selectedEndpointSlug"
                  :options="endpointOptions"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Sélectionner un endpoint"
                  @change="onEndpointChange"
                />
              </div>

              <div class="form-row">
                <label class="label">Filtre serveur</label>
                <InputText
                  v-model="serverSearch"
                  placeholder="Recherche côté API..."
                  @keyup.enter="loadDashboardData"
                />
              </div>

              <div class="form-row">
                <label class="label">Nombre de lignes à charger</label>
                <InputNumber
                  v-model="take"
                  :min="10"
                  :max="1000"
                  showButtons
                />
              </div>
            </div>

            <div class="actions">
              <Button
                label="Charger les endpoints"
                icon="pi pi-refresh"
                severity="secondary"
                @click="loadEndpoints"
              />

              <Button
                label="Charger les données"
                icon="pi pi-database"
                :disabled="!selectedEndpointSlug"
                @click="loadDashboardData"
              />

              <Button
                label="Réinitialiser filtres"
                icon="pi pi-filter-slash"
                severity="secondary"
                @click="resetFilters"
              />
            </div>
          </template>
        </Card>

        <Card v-if="selectedEndpoint">
          <template #title>
            Endpoint sélectionné
          </template>

          <template #content>
            <div class="grid-2">
              <div>
                <h3 style="margin-top: 0">{{ selectedEndpoint.name }}</h3>
                <p class="small-muted">
                  {{ selectedEndpoint.description || 'Aucune description.' }}
                </p>

                <div class="endpoint-url">
                  GET {{ apiUrl }}/public/{{ selectedEndpoint.slug }}
                </div>
              </div>

              <div>
                <p>
                  <strong>Colonnes exposées :</strong>
                </p>

                <div class="actions">
                  <Tag
                    v-for="column in selectedEndpoint.columns"
                    :key="column"
                    :value="column"
                    severity="info"
                  />
                </div>
              </div>
            </div>
          </template>
        </Card>
      </section>

      <section class="stat-grid" style="margin-top: 1rem">
        <div class="stat-card">
          <div class="stat-label">Endpoint</div>
          <div class="stat-value">{{ dashboardTitle }}</div>
        </div>

        <div class="stat-card">
          <div class="stat-label">Total API</div>
          <div class="stat-value">{{ totalRows }}</div>
        </div>

        <div class="stat-card">
          <div class="stat-label">Lignes chargées</div>
          <div class="stat-value">{{ rows.length }}</div>
        </div>

        <div class="stat-card">
          <div class="stat-label">Colonnes</div>
          <div class="stat-value">{{ columns.length }}</div>
        </div>

        <div class="stat-card">
          <div class="stat-label">Complétude</div>
          <div class="stat-value">{{ completenessRate }} %</div>
        </div>
      </section>

      <section class="grid-2">
        <Card class="chart-card">
          <template #title>
            Répartition des valeurs par colonne
          </template>

          <template #content>
            <div class="form-row">
              <label class="label">Colonne d’analyse</label>
              <Select
                v-model="selectedGroupColumn"
                :options="columns"
                placeholder="Sélectionner une colonne"
              />
            </div>

            <Chart
              v-if="barChartData"
              type="bar"
              :data="barChartData"
              :options="barChartOptions"
            />

            <Message v-else severity="warn">
              Charge des données et sélectionne une colonne pour afficher le graphique.
            </Message>
          </template>
        </Card>

        <Card class="chart-card">
          <template #title>
            Complétude de la colonne
          </template>

          <template #content>
            <div class="form-row">
              <label class="label">Colonne de complétude</label>
              <Select
                v-model="selectedCompletenessColumn"
                :options="columns"
                placeholder="Sélectionner une colonne"
              />
            </div>

            <Chart
              v-if="doughnutChartData"
              type="doughnut"
              :data="doughnutChartData"
              :options="doughnutChartOptions"
            />

            <Message v-else severity="warn">
              Aucune donnée disponible pour calculer la complétude.
            </Message>
          </template>
        </Card>
      </section>

      <section class="grid" style="margin-top: 1rem">
        <Card class="chart-card">
          <template #title>
            Tendance numérique
          </template>

          <template #content>
            <div class="grid-2">
              <div class="form-row">
                <label class="label">Colonne numérique</label>
                <Select
                  v-model="selectedNumericColumn"
                  :options="numericColumns"
                  placeholder="Sélectionner une colonne numérique"
                />
              </div>

              <div class="form-row">
                <label class="label">Information</label>
                <Message v-if="!numericColumns.length" severity="warn">
                  Aucune colonne numérique détectée dans les lignes chargées.
                </Message>
                <Message v-else severity="info">
                  Le graphique utilise les lignes chargées depuis l’API.
                </Message>
              </div>
            </div>

            <Chart
              v-if="lineChartData"
              type="line"
              :data="lineChartData"
              :options="lineChartOptions"
            />
          </template>
        </Card>
      </section>

      <section class="grid" style="margin-top: 1rem">
        <Card>
          <template #title>
            Données détaillées
          </template>

          <template #content>
            <Toolbar>
              <template #start>
                <div class="actions">
                  <InputText
                    v-model="localSearch"
                    placeholder="Filtre local..."
                    style="min-width: 280px"
                  />

                  <MultiSelect
                    v-model="selectedColumns"
                    :options="columns"
                    placeholder="Colonnes affichées"
                    display="chip"
                    style="min-width: 320px"
                  />
                </div>
              </template>

              <template #end>
                <div class="actions">
                  <Button
                    label="Exporter CSV"
                    icon="pi pi-download"
                    severity="secondary"
                    :disabled="!rows.length"
                    @click="exportCsv"
                  />

                  <Button
                    label="Exporter JSON"
                    icon="pi pi-file-export"
                    severity="secondary"
                    :disabled="!rows.length"
                    @click="exportJson"
                  />

                  <Button
                    label="Voir payload"
                    icon="pi pi-eye"
                    severity="secondary"
                    :disabled="!rows.length"
                    @click="showPayload"
                  />
                </div>
              </template>
            </Toolbar>

            <DataTable
              ref="tableRef"
              :value="rows"
              :paginator="true"
              :rows="10"
              :rowsPerPageOptions="[10, 25, 50, 100]"
              v-model:filters="tableFilters"
              :globalFilterFields="selectedColumns"
              sortMode="multiple"
              removableSort
              stripedRows
              showGridlines
              scrollable
              scrollHeight="620px"
              tableStyle="min-width: 70rem"
              exportFilename="dashboard_export"
            >
              <template #empty>
                Aucune donnée à afficher. Sélectionne un endpoint puis charge les données.
              </template>

              <Column
                v-for="column in selectedColumns"
                :key="column"
                :field="column"
                :header="column"
                sortable
              />
            </DataTable>
          </template>
        </Card>
      </section>
    </main>

    <Dialog
      v-model:visible="payloadDialogVisible"
      modal
      header="Payload API"
      style="width: 75vw"
    >
      <pre>{{ payloadPreview }}</pre>

      <template #footer>
        <Button
          label="Fermer"
          icon="pi pi-times"
          @click="payloadDialogVisible = false"
        />
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue';
import { FilterMatchMode } from '@primevue/core/api';
import { useToast } from 'primevue/usetoast';
import { api, apiBaseUrl } from './services/api';

const toast = useToast();

const apiUrl = apiBaseUrl;

const endpoints = ref([]);
const selectedEndpointSlug = ref(null);
const selectedEndpoint = ref(null);

const serverSearch = ref('');
const localSearch = ref('');
const take = ref(500);

const rows = ref([]);
const columns = ref([]);
const selectedColumns = ref([]);
const totalRows = ref(0);

const selectedGroupColumn = ref(null);
const selectedCompletenessColumn = ref(null);
const selectedNumericColumn = ref(null);

const tableRef = ref(null);

const payloadDialogVisible = ref(false);
const payloadPreview = ref('');

const tableFilters = ref({
  global: {
    value: null,
    matchMode: FilterMatchMode.CONTAINS
  }
});

const endpointOptions = computed(() => {
  return endpoints.value.map(endpoint => ({
    label: `${endpoint.name} - /public/${endpoint.slug}`,
    value: endpoint.slug
  }));
});

const dashboardTitle = computed(() => {
  return selectedEndpoint.value?.name || 'Aucun';
});

const numericColumns = computed(() => {
  if (!rows.value.length) {
    return [];
  }

  return columns.value.filter(column => {
    const sampleValues = rows.value
      .map(row => row[column])
      .filter(value => value !== null && value !== undefined && String(value).trim() !== '')
      .slice(0, 30);

    if (!sampleValues.length) {
      return false;
    }

    return sampleValues.every(value => !Number.isNaN(Number(String(value).replace(',', '.'))));
  });
});

const completenessRate = computed(() => {
  if (!rows.value.length || !columns.value.length) {
    return 0;
  }

  const totalCells = rows.value.length * columns.value.length;
  const filledCells = rows.value.reduce((count, row) => {
    return count + columns.value.filter(column => hasValue(row[column])).length;
  }, 0);

  return Math.round((filledCells / totalCells) * 100);
});

const barChartData = computed(() => {
  if (!selectedGroupColumn.value || !rows.value.length) {
    return null;
  }

  const map = new Map();

  rows.value.forEach(row => {
    const rawValue = row[selectedGroupColumn.value];
    const value = hasValue(rawValue) ? String(rawValue) : 'Vide';
    map.set(value, (map.get(value) || 0) + 1);
  });

  const topValues = [...map.entries()]
    .sort((a, b) => b[1] - a[1])
    .slice(0, 10);

  return {
    labels: topValues.map(item => item[0]),
    datasets: [
      {
        label: `Nombre de lignes par ${selectedGroupColumn.value}`,
        data: topValues.map(item => item[1]),
        backgroundColor: '#2563eb',
        borderColor: '#1d4ed8',
        borderWidth: 1
      }
    ]
  };
});

const doughnutChartData = computed(() => {
  if (!selectedCompletenessColumn.value || !rows.value.length) {
    return null;
  }

  let filled = 0;
  let empty = 0;

  rows.value.forEach(row => {
    if (hasValue(row[selectedCompletenessColumn.value])) {
      filled += 1;
    } else {
      empty += 1;
    }
  });

  return {
    labels: ['Renseigné', 'Vide'],
    datasets: [
      {
        data: [filled, empty],
        backgroundColor: ['#0f766e', '#e11d48'],
        hoverBackgroundColor: ['#115e59', '#be123c']
      }
    ]
  };
});

const lineChartData = computed(() => {
  if (!selectedNumericColumn.value || !rows.value.length) {
    return null;
  }

  const values = rows.value
    .map((row, index) => ({
      index: index + 1,
      value: toNumber(row[selectedNumericColumn.value])
    }))
    .filter(item => item.value !== null)
    .slice(0, 100);

  if (!values.length) {
    return null;
  }

  return {
    labels: values.map(item => item.index),
    datasets: [
      {
        label: selectedNumericColumn.value,
        data: values.map(item => item.value),
        fill: false,
        borderColor: '#7c3aed',
        tension: 0.25
      }
    ]
  };
});

const baseChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'bottom'
    }
  }
};

const barChartOptions = {
  ...baseChartOptions,
  scales: {
    y: {
      beginAtZero: true
    }
  }
};

const doughnutChartOptions = {
  ...baseChartOptions
};

const lineChartOptions = {
  ...baseChartOptions,
  scales: {
    y: {
      beginAtZero: true
    }
  }
};

watch(localSearch, value => {
  tableFilters.value.global.value = value;
});

onMounted(async () => {
  await loadEndpoints();
});

async function loadEndpoints() {
  try {
    endpoints.value = await api.getEndpoints();

    if (!endpoints.value.length) {
      showToast(
        'warn',
        'Aucun endpoint',
        'Crée d’abord un endpoint dans l’Application 1.'
      );
    }
  } catch (error) {
    showToast('error', 'Erreur endpoints', error.message);
  }
}

async function onEndpointChange() {
  selectedEndpoint.value =
    endpoints.value.find(endpoint => endpoint.slug === selectedEndpointSlug.value) || null;

  rows.value = [];
  columns.value = [];
  selectedColumns.value = [];
  totalRows.value = 0;

  if (selectedEndpoint.value) {
    await loadDashboardData();
  }
}

async function loadDashboardData() {
  if (!selectedEndpointSlug.value) {
    return;
  }

  try {
    const result = await api.getPublicData(selectedEndpointSlug.value, {
      skip: 0,
      take: take.value || 500,
      search: serverSearch.value
    });

    rows.value = result.rows || [];
    columns.value = result.columns || [];
    totalRows.value = result.total || 0;
    selectedColumns.value = [...columns.value];

    selectedGroupColumn.value = columns.value[0] || null;
    selectedCompletenessColumn.value = columns.value[0] || null;
    selectedNumericColumn.value = numericColumns.value[0] || null;

    showToast(
      'success',
      'Données chargées',
      `${rows.value.length} ligne(s) chargée(s) depuis l’API.`
    );
  } catch (error) {
    showToast('error', 'Erreur données', error.message);
  }
}

function resetFilters() {
  serverSearch.value = '';
  localSearch.value = '';
  tableFilters.value.global.value = null;
}

function exportCsv() {
  tableRef.value?.exportCSV();
}

function exportJson() {
  const payload = {
    endpoint: selectedEndpoint.value,
    total: totalRows.value,
    exportedAt: new Date().toISOString(),
    rows: rows.value
  };

  downloadFile(
    JSON.stringify(payload, null, 2),
    `dashboard_${selectedEndpointSlug.value || 'export'}.json`,
    'application/json'
  );
}

function showPayload() {
  payloadPreview.value = JSON.stringify(
    {
      endpoint: selectedEndpoint.value,
      total: totalRows.value,
      loadedRows: rows.value.length,
      columns: columns.value,
      sample: rows.value.slice(0, 20)
    },
    null,
    2
  );

  payloadDialogVisible.value = true;
}

function downloadFile(content, fileName, mimeType) {
  const blob = new Blob([content], { type: mimeType });
  const url = URL.createObjectURL(blob);

  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  link.click();

  URL.revokeObjectURL(url);
}

function hasValue(value) {
  return value !== null && value !== undefined && String(value).trim() !== '';
}

function toNumber(value) {
  if (!hasValue(value)) {
    return null;
  }

  const parsed = Number(String(value).replace(',', '.'));

  return Number.isNaN(parsed) ? null : parsed;
}

function showToast(severity, summary, detail) {
  toast.add({
    severity,
    summary,
    detail,
    life: 3500
  });
}
</script>