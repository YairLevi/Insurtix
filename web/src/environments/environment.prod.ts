declare global {
  interface Window {
    __env?: { apiUrl?: string };
  }
}

export const environment = {
  apiUrl: window.__env?.apiUrl ?? 'http://localhost:8080',
};
