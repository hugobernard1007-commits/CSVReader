const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7001/api';

async function request(url, options = {}) {
  const response = await fetch(`${API_BASE_URL}${url}`, options);

  if (!response.ok) {
    let errorMessage = `Erreur API ${response.status}`;

    try {
      const errorPayload = await response.json();
      errorMessage = errorPayload.message || errorMessage;
    } catch {
      // Ignore parsing error
    }

    throw new Error(errorMessage);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export const apiBaseUrl = API_BASE_URL;

export const api = {
  getEndpoints() {
    return request('/endpoints');
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

    const queryString = query.toString();

    return request(`/public/${slug}${queryString ? `?${queryString}` : ''}`);
  }
};