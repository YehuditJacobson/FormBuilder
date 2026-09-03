/**
 * Development configuration. Requests to `/api` are forwarded to the .NET API by the
 * dev-server proxy (`proxy.conf.json`), so the client never needs the backend's absolute URL.
 */
export const environment = {
  production: false,
  apiBaseUrl: '/api',
};
