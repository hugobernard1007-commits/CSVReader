import axios from 'axios';

// Créer une instance d'axios avec l'URL de base de l'API
const instance = axios.create({
  baseURL: 'http://localhost:5191/api',
});

// Fonction pour récupérer la liste des fichiers depuis l'API
export const fetchFiles = async () => {
  try {
    const response = await instance.get('/Data');
    return response.data;
  } catch (error) {
    console.error('Error fetching files:', error);
    throw error;
  }
};

// Fonction pour récupérer un fichier spécifique par son ID depuis l'API
export const fetchFileById = async (id) => {
  try {
    const response = await instance.get(`/Data/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching file with ID ${id}:`, error);
    throw error;
  }
};