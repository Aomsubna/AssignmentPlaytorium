// lib/api.ts
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5224', // API ตัวอย่าง
  headers: {
    'Content-Type': 'application/json',
  },
  // timeout: 5000,
});

api.interceptors.response.use(
  (res) => res,
  (err) => {
    console.error('[API ERROR]', err.message);
    return Promise.reject(err);
  }
);

export default api;
