import { Routes } from '@angular/router';
import { FormBuilderPage } from './pages/form-builder.page';
import { FormListPage } from './pages/form-list.page';
import { FormViewPage } from './pages/form-view.page';

/** Routes for the form-builder feature, lazy-loaded under `/forms`. */
export const formBuilderRoutes: Routes = [
  { path: '', component: FormListPage, title: 'תבניות טפסים' },
  { path: 'new', component: FormBuilderPage, title: 'יצירת טופס חדש' },
  { path: ':id', component: FormViewPage, title: 'צפייה בטופס' },
];
