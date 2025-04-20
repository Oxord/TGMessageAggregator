import axios, { AxiosRequestConfig, AxiosResponse } from 'axios';

// Configure the base URL for the API.
// Adjust this if your backend runs on a different port or host.
const API_BASE_URL = 'http://localhost:5008'; // Corrected port based on backend logs

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Send cookies with requests
});

// Generic function to handle API requests
// This simplifies making calls from components and centralizes error handling if needed later.
export const callApi = async <T = any>(config: AxiosRequestConfig): Promise<T> => {
  try {
    const response: AxiosResponse<T> = await apiClient.request<T>(config);
    return response.data;
  } catch (error) {
    // Re-throw the error so components can handle it (e.g., display messages)
    // Could add more sophisticated error handling/logging here later.
    console.error('API call failed:', error);
    throw error;
  }
};

export default apiClient; // Export the configured instance if needed directly
