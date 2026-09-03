import { Routes } from '@angular/router';
import { FormBuilderPage } from './pages/form-builder.page';

/**
 * Routes for the form-builder feature, lazy-loaded under `/forms`.
 * Step 09 adds the list at `''` and the read-only view at `':id'`.
 */
export const formBuilderRoutes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'new' },
  { path: 'new', component: FormBuilderPage, title: 'יצירת טופס חדש' },
];
