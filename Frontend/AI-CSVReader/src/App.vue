<template>
  <div class="app-shell">
    <Toast />

    <header class="header">
      <h1>Application 1 - CSV, comparaison et API REST</h1>
      <p>
        Front-end Vue + PrimeVue connecté au back-end .NET pour charger, consulter,
        comparer et exposer des fichiers CSV via API.
      </p>
    </header>

    <main class="container">
      <nav class="nav">
        <Button
          label="Chargement CSV"
          icon="pi pi-upload"
          :severity="activeView === 'upload' ? 'primary' : 'secondary'"
          @click="activeView = 'upload'"
        />

        <Button
          label="Données"
          icon="pi pi-table"
          :severity="activeView === 'data' ? 'primary' : 'secondary'"
          @click="activeView = 'data'"
        />

        <Button
          label="Comparaison"
          icon="pi pi-code-branch"
          :severity="activeView === 'compare' ? 'primary' : 'secondary'"
          @click="activeView = 'compare'"
        />

        <Button
          label="Création API"
          icon="pi pi-link"
          :severity="activeView === 'api' ? 'primary' : 'secondary'"
          @click="activeView = 'api'"
        />
      </nav>

      <section v-if="activeView === 'upload'" class="grid">
        <Card>
          <template #title>
            Chargement d’un fichier CSV
          </template>

          <template #content>
            <div class="upload-zone">
              <FileUpload
                name="file"
                accept=".csv,text/csv"
                mode="advanced"
                :multiple="false"
                :customUpload="true"
                chooseLabel="Choisir un CSV"
                uploadLabel="Envoyer au back-end"
                cancelLabel="Annuler"
                @uploader="handleCsvUpload"
              >
                <template #empty>
                  <p>Glisse un fichier CSV ici ou clique sur le bouton de sélection.</p>
                </template>
              </FileUpload>
            </div>

            <Message severity="info" style="margin-top: 1rem">
              Les fichiers sont envoyés au back-end .NET, validés, parsés puis stockés
              dans le dossier <strong>App_Data</strong>.
            </Message>
          </template>
        </Card>

        <Card>
          <template #title>
            Datasets disponibles
          </template>

          <template #content>
            <Toolbar>
              <template #start>
                <Button
                  label="Rafraîchir"
                  icon="pi pi-refresh"
                  severity="secondary"
                  @click="loadDatasets"
                />
              </template>
            </Toolbar>

            <DataTable
              :value="datasets"
              :paginator="true"
              :rows="10"
              stripedRows
              showGridlines
              tableStyle="min-width: 60rem"
            >
              <template #empty>
                Aucun dataset chargé.
              </template>

              <Column field="name" header="Nom" sortable />
              <Column field="originalFileName" header="Fichier original" sortable />
              <Column field="rowCount" header="Lignes" sortable />
              <Column field="columnCount" header="Colonnes" sortable />
              <Column field="uploadedAt" header="Date d’upload" sortable>
                <template #body="{ data }">
                  {{ formatDate(data.uploadedAt) }}
                </template>
              </Column>

              <Column header="Actions">
                <template #body="{ data }">
                  <div class="actions">
                    <Button
                      label="Voir"
                      icon="pi pi-eye"
                      size="small"
                      @click="selectDatasetForData(data.id)"
                    />

                    <Button
                      label="Supprimer"
                      icon="pi pi-trash"
                      size="small"
                      severity="danger"
                      @click="deleteDataset(data.id)"
                    />
                  </div>
                </template>
              </Column>
            </DataTable>
          </template>
        </Card>
      </section>

      <section v-if="activeView === 'data'" class="grid">
        <Card>
          <template #title>
            Consultation des données CSV
          </template>

          <template #content>
            <div class="grid-2">
              <div class="form-row">
                <label class="label">Dataset à afficher</label>
                <Select
                  v-model="selectedDatasetId"
                  :options="datasetOptions"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Sélectionner un dataset"
                  @change="loadSelectedDatasetRows"
                />
              </div>

              <div class="form-row">
                <label class="label">Recherche serveur</label>
                <InputText
                  v-model="datasetSearch"
                  placeholder="Rechercher dans les lignes..."
                  @keyup.enter="loadSelectedDatasetRows"
                />
              </div>
            </div>

            <div class="actions" style="margin-bottom: 1rem">
              <Button
                label="Charger les lignes"
                icon="pi pi-search"
                :disabled="!selectedDatasetId"
                @click="loadSelectedDatasetRows"
              />

              <Button
                label="Choisir les colonnes"
                icon="pi pi-list"
                severity="secondary"
                :disabled="!currentColumns.length"
                @click="openColumnSidebar"
              />

              <Button
                label="Exporter CSV"
                icon="pi pi-download"
                severity="secondary"
                :disabled="!currentRows.length"
                @click="exportMainCsv"
              />
            </div>

            <div class="stat-grid">
              <div class="stat-card">
                <div class="stat-label">Dataset</div>
                <div class="stat-value">{{ selectedDatasetName || 'Aucun' }}</div>
              </div>

              <div class="stat-card">
                <div class="stat-label">Total serveur</div>
                <div class="stat-value">{{ currentTotal }}</div>
              </div>

              <div class="stat-card">
                <div class="stat-label">Lignes affichées</div>
                <div class="stat-value">{{ currentRows.length }}</div>
              </div>

              <div class="stat-card">
                <div class="stat-label">Colonnes affichées</div>
                <div class="stat-value">{{ selectedColumns.length }}</div>
              </div>
            </div>

            <DataTable
              ref="mainTableRef"
              :value="currentRows"
              :paginator="true"
              :rows="10"
              :rowsPerPageOptions="[10, 25, 50, 100]"
              :globalFilterFields="selectedColumns"
              v-model:filters="filters"
              sortMode="multiple"
              removableSort
              stripedRows
              showGridlines
              scrollable
              scrollHeight="600px"
              tableStyle="min-width: 60rem"
              exportFilename="donnees_csv"
            >
              <template #header>
                <InputText
                  v-model="globalFilter"
                  placeholder="Filtre local sur les lignes chargées..."
                  style="min-width: 300px"
                />
              </template>

              <template #empty>
                Aucun dataset sélectionné ou aucune ligne trouvée.
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

      <section v-if="activeView === 'compare'" class="grid">
        <Card>
          <template #title>
            Comparaison de deux datasets
          </template>

          <template #content>
            <div class="grid-2">
              <div class="form-row">
                <label class="label">Premier dataset</label>
                <Select
                  v-model="compareForm.firstDatasetId"
                  :options="datasetOptions"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Sélectionner le premier dataset"
                  @change="onCompareDatasetChange"
                />
              </div>

              <div class="form-row">
                <label class="label">Deuxième dataset</label>
                <Select
                  v-model="compareForm.secondDatasetId"
                  :options="datasetOptions"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Sélectionner le deuxième dataset"
                  @change="onCompareDatasetChange"
                />
              </div>
            </div>

            <div class="grid-2">
              <div class="form-row">
                <label class="label">Colonne clé</label>
                <Select
                  v-model="compareForm.keyColumn"
                  :options="compareKeyOptions"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Sélectionner une clé"
                />
              </div>

              <div class="form-row">
                <label class="label">Colonnes à comparer</label>
                <MultiSelect
                  v-model="compareForm.columns"
                  :options="commonCompareColumns"
                  placeholder="Colonnes communes"
                  display="chip"
                  style="width: 100%"
                />
              </div>
            </div>

            <div class="actions" style="margin-bottom: 1rem">
              <Button
                label="Comparer"
                icon="pi pi-code-branch"
                :disabled="!canRunComparison"
                @click="runComparison"
              />

              <Button
                label="Exporter comparaison"
                icon="pi pi-download"
                severity="secondary"
                :disabled="!comparisonRows.length"
                @click="exportDiffCsv"
              />
            </div>

            <div class="stat-grid">
              <div class="stat-card">
                <div class="stat-label">Différences</div>
                <div class="stat-value">{{ comparisonResult.totalDifferences || 0 }}</div>
              </div>

              <div class="stat-card">
                <div class="stat-label">Ajouts</div>
                <div class="stat-value">{{ comparisonResult.addedCount || 0 }}</div>
              </div>

              <div class="stat-card">
                <div class="stat-label">Suppressions</div>
                <div class="stat-value">{{ comparisonResult.removedCount || 0 }}</div>
              </div>

              <div class="stat-card">
                <div class="stat-label">Modifications</div>
                <div class="stat-value">{{ comparisonResult.modifiedCount || 0 }}</div>
              </div>
            </div>

            <DataTable
              ref="diffTableRef"
              :value="comparisonRows"
              :paginator="true"
              :rows="10"
              :rowsPerPageOptions="[10, 25, 50, 100]"
              v-model:filters="diffFilters"
              :globalFilterFields="['status', 'key', 'column', 'oldValue', 'newValue']"
              sortMode="multiple"
              removableSort
              stripedRows
              showGridlines
              scrollable
              scrollHeight="600px"
              tableStyle="min-width: 70rem"
              exportFilename="comparaison_csv"
            >
              <template #header>
                <InputText
                  v-model="diffGlobalFilter"
                  placeholder="Filtrer les différences..."
                  style="min-width: 300px"
                />
              </template>

              <template #empty>
                Aucune différence à afficher.
              </template>

              <Column field="status" header="Type" sortable>
                <template #body="{ data }">
                  <span :class="getDiffClass(data.status)">
                    {{ data.status }}
                  </span>
                </template>
              </Column>

              <Column field="key" header="Clé" sortable />
              <Column field="column" header="Colonne" sortable />
              <Column field="oldValue" header="Valeur fichier 1" sortable />
              <Column field="newValue" header="Valeur fichier 2" sortable />
            </DataTable>
          </template>
        </Card>
      </section>

      <section v-if="activeView === 'api'" class="grid-2">
        <Card>
          <template #title>
            Création d’un endpoint API réel
          </template>

          <template #content>
            <Message severity="info">
              Les endpoints créés ici sont maintenant envoyés au back-end .NET et exposés
              via <strong>/api/public/{slug}</strong>.
            </Message>

            <div class="form-row">
              <label class="label">Nom de l’endpoint</label>
              <InputText
                v-model="endpointForm.name"
                placeholder="Exemple : Clients importés"
              />
            </div>

            <div class="form-row">
              <label class="label">Slug</label>
              <InputText
                v-model="endpointForm.slug"
                placeholder="Exemple : clients-importes"
              />
              <small class="small-muted">
                URL finale : {{ apiBaseUrl }}/public/{{ endpointForm.slug || 'mon-endpoint' }}
              </small>
            </div>

            <div class="form-row">
              <label class="label">Dataset source</label>
              <Select
                v-model="endpointForm.datasetId"
                :options="datasetOptions"
                optionLabel="label"
                optionValue="value"
                placeholder="Sélectionner un dataset"
                @change="onEndpointDatasetChange"
              />
            </div>

            <div class="form-row">
              <label class="label">Colonnes exposées</label>
              <MultiSelect
                v-model="endpointForm.columns"
                :options="endpointColumns"
                placeholder="Choisir les colonnes"
                display="chip"
                style="width: 100%"
              />
            </div>

            <div class="form-row">
              <label class="label">Description</label>
              <Textarea
                v-model="endpointForm.description"
                rows="4"
                placeholder="Description fonctionnelle de l’endpoint..."
              />
            </div>

            <div class="actions">
              <Button
                label="Créer l’endpoint"
                icon="pi pi-plus"
                :disabled="!canCreateEndpoint"
                @click="createEndpoint"
              />

              <Button
                label="Réinitialiser"
                icon="pi pi-refresh"
                severity="secondary"
                @click="resetEndpointForm"
              />
            </div>
          </template>
        </Card>

        <Card>
          <template #title>
            Endpoints disponibles
          </template>

          <template #content>
            <Toolbar>
              <template #start>
                <Button
                  label="Rafraîchir"
                  icon="pi pi-refresh"
                  severity="secondary"
                  @click="loadEndpoints"
                />
              </template>
            </Toolbar>

            <Message v-if="!endpoints.length" severity="warn">
              Aucun endpoint créé.
            </Message>

            <div
              v-for="endpoint in endpoints"
              :key="endpoint.id"
              class="endpoint-card"
            >
              <div class="actions" style="justify-content: space-between">
                <div>
                  <h3 style="margin: 0">{{ endpoint.name }}</h3>
                  <p class="small-muted" style="margin: 0.25rem 0">
                    Slug : {{ endpoint.slug }}
                  </p>
                </div>

                <Tag value="GET" severity="success" />
              </div>

              <p>{{ endpoint.description || 'Aucune description.' }}</p>

              <div class="endpoint-url">
                GET {{ apiBaseUrl }}{{ endpoint.url?.replace('/api', '') }}
              </div>

              <p class="small-muted">
                Colonnes exposées : {{ endpoint.columns?.join(', ') }}
              </p>

              <div class="actions">
                <Button
                  label="Prévisualiser"
                  icon="pi pi-eye"
                  severity="secondary"
                  @click="previewEndpoint(endpoint)"
                />

                <Button
                  label="Supprimer"
                  icon="pi pi-trash"
                  severity="danger"
                  @click="deleteEndpoint(endpoint.id)"
                />
              </div>
            </div>
          </template>
        </Card>
      </section>
    </main>

    <Sidebar
      v-model:visible="columnSidebarVisible"
      position="right"
      header="Choix des colonnes"
      style="width: 420px"
    >
      <p class="small-muted">
        Sélectionne les colonnes à afficher dans la datagrid et à exporter.
      </p>

      <MultiSelect
        v-model="selectedColumns"
        :options="currentColumns"
        placeholder="Colonnes"
        display="chip"
        style="width: 100%"
      />

      <div class="actions" style="margin-top: 1rem">
        <Button
          label="Tout sélectionner"
          icon="pi pi-check"
          @click="selectedColumns = [...currentColumns]"
        />

        <Button
          label="Tout retirer"
          icon="pi pi-times"
          severity="secondary"
          @click="selectedColumns = []"
        />
      </div>
    </Sidebar>

    <Dialog
      v-model:visible="payloadDialogVisible"
      modal
      header="Payload API"
      style="width: 75vw"
    >
      <pre>{{ endpointPayloadPreview }}</pre>

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
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { FilterMatchMode } from '@primevue/core/api';
import { useToast } from 'primevue/usetoast';
import { api } from './services/api';

const toast = useToast();

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7001/api';

const activeView = ref('upload');

const datasets = ref([]);
const endpoints = ref([]);

const selectedDatasetId = ref(null);
const selectedDatasetName = ref('');
const datasetSearch = ref('');

const currentRows = ref([]);
const currentColumns = ref([]);
const currentTotal = ref(0);
const selectedColumns = ref([]);

const mainTableRef = ref(null);
const diffTableRef = ref(null);

const columnSidebarVisible = ref(false);

const globalFilter = ref('');
const diffGlobalFilter = ref('');

const filters = ref({
  global: {
    value: null,
    matchMode: FilterMatchMode.CONTAINS
  }
});

const diffFilters = ref({
  global: {
    value: null,
    matchMode: FilterMatchMode.CONTAINS
  }
});

const compareForm = reactive({
  firstDatasetId: null,
  secondDatasetId: null,
  keyColumn: '__index',
  columns: []
});

const firstCompareDataset = ref(null);
const secondCompareDataset = ref(null);
const comparisonResult = ref({});
const comparisonRows = ref([]);

const endpointForm = reactive({
  name: '',
  slug: '',
  datasetId: null,
  columns: [],
  description: ''
});

const endpointColumns = ref([]);

const payloadDialogVisible = ref(false);
const endpointPayloadPreview = ref('');

const datasetOptions = computed(() => {
  return datasets.value.map(dataset => ({
    label: `${dataset.name} - ${dataset.rowCount} lignes`,
    value: dataset.id
  }));
});

const commonCompareColumns = computed(() => {
  if (!firstCompareDataset.value || !secondCompareDataset.value) {
    return [];
  }

  return firstCompareDataset.value.columns.filter(column =>
    secondCompareDataset.value.columns.includes(column)
  );
});

const compareKeyOptions = computed(() => {
  const options = [
    {
      label: 'Index de ligne',
      value: '__index'
    }
  ];

  commonCompareColumns.value.forEach(column => {
    options.push({
      label: column,
      value: column
    });
  });

  return options;
});

const canRunComparison = computed(() => {
  return (
    compareForm.firstDatasetId &&
    compareForm.secondDatasetId &&
    compareForm.firstDatasetId !== compareForm.secondDatasetId
  );
});

const canCreateEndpoint = computed(() => {
  return (
    endpointForm.name.trim().length > 0 &&
    endpointForm.slug.trim().length > 0 &&
    endpointForm.datasetId &&
    endpointForm.columns.length > 0
  );
});

watch(globalFilter, value => {
  filters.value.global.value = value;
});

watch(diffGlobalFilter, value => {
  diffFilters.value.global.value = value;
});

onMounted(async () => {
  await Promise.all([
    loadDatasets(),
    loadEndpoints()
  ]);
});

async function handleCsvUpload(event) {
  const file = event.files?.[0];

  if (!file) {
    showToast('warn', 'Aucun fichier', 'Aucun fichier CSV sélectionné.');
    return;
  }

  try {
    const result = await api.uploadDataset(file);

    showToast(
      'success',
      'CSV chargé',
      `${result.originalFileName} chargé avec ${result.rowCount} lignes.`
    );

    await loadDatasets();

    selectedDatasetId.value = result.datasetId;
    activeView.value = 'data';

    await loadSelectedDatasetRows();
  } catch (error) {
    showToast('error', 'Erreur upload', error.message);
  }
}

async function loadDatasets() {
  try {
    datasets.value = await api.getDatasets();
  } catch (error) {
    showToast('error', 'Erreur datasets', error.message);
  }
}

async function loadSelectedDatasetRows() {
  if (!selectedDatasetId.value) {
    return;
  }

  try {
    const result = await api.getDatasetRows(selectedDatasetId.value, {
      skip: 0,
      take: 1000,
      search: datasetSearch.value
    });

    currentRows.value = result.rows || [];
    currentColumns.value = result.columns || [];
    currentTotal.value = result.total || 0;

    selectedColumns.value = [...currentColumns.value];

    const selectedDataset = datasets.value.find(dataset => dataset.id === selectedDatasetId.value);
    selectedDatasetName.value = selectedDataset?.name || '';
  } catch (error) {
    showToast('error', 'Erreur chargement lignes', error.message);
  }
}

async function selectDatasetForData(datasetId) {
  selectedDatasetId.value = datasetId;
  activeView.value = 'data';
  await loadSelectedDatasetRows();
}

async function deleteDataset(datasetId) {
  try {
    await api.deleteDataset(datasetId);

    if (selectedDatasetId.value === datasetId) {
      selectedDatasetId.value = null;
      selectedDatasetName.value = '';
      currentRows.value = [];
      currentColumns.value = [];
      selectedColumns.value = [];
      currentTotal.value = 0;
    }

    await loadDatasets();

    showToast('success', 'Dataset supprimé', 'Le dataset a été supprimé.');
  } catch (error) {
    showToast('error', 'Erreur suppression', error.message);
  }
}

async function onCompareDatasetChange() {
  comparisonResult.value = {};
  comparisonRows.value = [];
  compareForm.columns = [];

  firstCompareDataset.value = null;
  secondCompareDataset.value = null;

  try {
    if (compareForm.firstDatasetId) {
      firstCompareDataset.value = await api.getDataset(compareForm.firstDatasetId);
    }

    if (compareForm.secondDatasetId) {
      secondCompareDataset.value = await api.getDataset(compareForm.secondDatasetId);
    }

    compareForm.keyColumn = '__index';
    compareForm.columns = [...commonCompareColumns.value];
  } catch (error) {
    showToast('error', 'Erreur préparation comparaison', error.message);
  }
}

async function runComparison() {
  try {
    const result = await api.compareDatasets({
      firstDatasetId: compareForm.firstDatasetId,
      secondDatasetId: compareForm.secondDatasetId,
      keyColumn: compareForm.keyColumn,
      columns: compareForm.columns
    });

    comparisonResult.value = result;
    comparisonRows.value = result.differences || [];

    showToast(
      'success',
      'Comparaison terminée',
      `${result.totalDifferences} différence(s) trouvée(s).`
    );
  } catch (error) {
    showToast('error', 'Erreur comparaison', error.message);
  }
}

async function onEndpointDatasetChange() {
  endpointForm.columns = [];
  endpointColumns.value = [];

  if (!endpointForm.datasetId) {
    return;
  }

  try {
    const dataset = await api.getDataset(endpointForm.datasetId);
    endpointColumns.value = dataset.columns || [];
    endpointForm.columns = [...endpointColumns.value];
  } catch (error) {
    showToast('error', 'Erreur dataset endpoint', error.message);
  }
}

async function loadEndpoints() {
  try {
    endpoints.value = await api.getEndpoints();
  } catch (error) {
    showToast('error', 'Erreur endpoints', error.message);
  }
}

async function createEndpoint() {
  try {
    const endpoint = await api.createEndpoint({
      name: endpointForm.name,
      slug: endpointForm.slug,
      datasetId: endpointForm.datasetId,
      columns: endpointForm.columns,
      description: endpointForm.description
    });

    showToast(
      'success',
      'Endpoint créé',
      `Endpoint disponible sur ${endpoint.url}`
    );

    resetEndpointForm();
    await loadEndpoints();
  } catch (error) {
    showToast('error', 'Erreur création endpoint', error.message);
  }
}

async function deleteEndpoint(endpointId) {
  try {
    await api.deleteEndpoint(endpointId);
    await loadEndpoints();

    showToast('success', 'Endpoint supprimé', 'L’endpoint a été supprimé.');
  } catch (error) {
    showToast('error', 'Erreur suppression endpoint', error.message);
  }
}

async function previewEndpoint(endpoint) {
  try {
    const result = await api.getPublicData(endpoint.slug, {
      skip: 0,
      take: 20
    });

    endpointPayloadPreview.value = JSON.stringify(result, null, 2);
    payloadDialogVisible.value = true;
  } catch (error) {
    showToast('error', 'Erreur prévisualisation', error.message);
  }
}

function resetEndpointForm() {
  endpointForm.name = '';
  endpointForm.slug = '';
  endpointForm.datasetId = null;
  endpointForm.columns = [];
  endpointForm.description = '';
  endpointColumns.value = [];
}

function openColumnSidebar() {
  columnSidebarVisible.value = true;
}

function exportMainCsv() {
  mainTableRef.value?.exportCSV();
}

function exportDiffCsv() {
  diffTableRef.value?.exportCSV();
}

function getDiffClass(status) {
  if (status === 'Ajouté') {
    return 'diff-added';
  }

  if (status === 'Supprimé') {
    return 'diff-removed';
  }

  return 'diff-modified';
}

function formatDate(value) {
  if (!value) {
    return '';
  }

  return new Intl.DateTimeFormat('fr-FR', {
    dateStyle: 'short',
    timeStyle: 'short'
  }).format(new Date(value));
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