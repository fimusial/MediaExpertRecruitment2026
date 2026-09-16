export * from './products.service';
import { ProductsService } from './products.service';
export * from './products.serviceInterface';
export * from './root.service';
import { RootService } from './root.service';
export * from './root.serviceInterface';
export const APIS = [ProductsService, RootService];
