import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'products/new',
    loadComponent: () => import('./products/product-form/product-form').then((m) => m.ProductForm),
  },
  { path: '', pathMatch: 'full', redirectTo: 'products/new' },
];
