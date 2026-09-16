import { environment } from '../../../environments/environment';

export function toApiUrl(url: string): string {
  const upstreamOrigin = new URL(environment.apiUpstreamOrigin).origin;

  let target: URL;
  try {
    target = new URL(url, document.baseURI);
  } catch {
    return url;
  }

  if (target.origin !== upstreamOrigin) {
    return url;
  }

  return `${environment.apiBaseUrl}${target.pathname}${target.search}${target.hash}`;
}
