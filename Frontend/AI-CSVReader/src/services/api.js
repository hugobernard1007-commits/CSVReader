const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7001/api';

async function request(url, options = {}) {
  const response = await fetch(`${API_BASE_URL}${url}`, options);

  if (!response.ok) {
    let errorMessage = `Erreur API ${response.status}`;

    try {
      const errorPayload = await response.json();
      errorMessage = errorPayload.message || errorMessage;
    } catch {
      // Ignore JSON parsing error
    }

    throw new Error(errorMessage);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export const api = {
  uploadDataset(file) {
    const formData = new FormData();
    formData.append('file', file);

    return request('/datasets/upload', {
      method: 'POST',
      body: formData
    });
  },

  getDatasets() {
    return request('/datasets');
  },

  getDataset(id) {
    return request(`/datasets/${id}`);
  },

  getDatasetRows(id, params = {}) {
    const query = new URLSearchParams();

    if (params.skip !== undefined) {
      query.set('skip', params.skip);
    }

    if (params.take !== undefined) {
      query.set('take', params.take);
    }

    if (params.search) {
      query.set('search', params.search);
    }

    return request(`/datasets/${id}/rows?${query.toString()}`);
  },

  deleteDataset(id) {
    return request(`/datasets/${id}`, {
      method: 'DELETE'
    });
  },

  compareDatasets(payload) {
    return request('/comparisons', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });
  },

  getEndpoints() {
    return request('/endpoints');
  },

  createEndpoint(payload) {
    return request('/endpoints', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });
  },

  deleteEndpoint(id) {
    return request(`/endpoints/${id}`, {
      method: 'DELETE'
    });
  },

  getPublicData(slug, params = {}) {
    const query = new URLSearchParams();

    if (params.skip !== undefined) {
      query.set('skip', params.skip);
    }

    if (params.take !== undefined) {
      query.set('take', params.take);
    }

    if (params.search) {
      query.set('search', params.search);
    }

    return request(`/public/${slug}?${query.toString()}`);
  }
};