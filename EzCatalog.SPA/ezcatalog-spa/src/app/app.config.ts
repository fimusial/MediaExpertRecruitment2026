import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { environment } from '../environments/environment';
import { apiLinkInterceptor } from './core/api/api-link-interceptor';
import { serverErrorInterceptor } from './core/api/server-error-interceptor';
import { provideApi } from './core/api/generated';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([apiLinkInterceptor, serverErrorInterceptor])),
    provideApi(environment.apiBaseUrl),
  ],
};
