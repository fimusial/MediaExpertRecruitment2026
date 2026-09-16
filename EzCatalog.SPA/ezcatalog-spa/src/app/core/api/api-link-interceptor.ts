import { HttpInterceptorFn } from '@angular/common/http';
import { toApiUrl } from './api-url';

export const apiLinkInterceptor: HttpInterceptorFn = (request, next) => {
  const url = toApiUrl(request.url);
  return next(url === request.url ? request : request.clone({ url }));
};
