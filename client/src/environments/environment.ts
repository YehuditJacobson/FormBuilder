/**
 * Production configuration. In a deployed environment the API is expected to be reachable
 * under the same origin at `/api` (for example behind a reverse proxy or a CDN rewrite).
 * The development overrides live in `environment.development.ts`.
 */
export const environment = {
  production: true,
  apiBaseUrl: '/api',
};
