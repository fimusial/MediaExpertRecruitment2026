import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./products/product-list/product-list').then((m) => m.ProductList),
  },
  {
    path: 'products/new',
    loadComponent: () => import('./products/product-form/product-form').then((m) => m.ProductForm),
  },
];
