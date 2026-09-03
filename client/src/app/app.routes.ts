import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'forms' },
  {
    path: 'forms',
    loadChildren: () =>
      import('./features/form-builder/form-builder.routes').then((m) => m.formBuilderRoutes),
  },
  { path: '**', redirectTo: 'forms' },
];
