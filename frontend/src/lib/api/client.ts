const API_BASE = '/api';

export interface ApiOptions extends RequestInit {
  queryParams?: Record<string, string | number | boolean>;
}

async function apiFetch<T>(path: string, options: ApiOptions = {}): Promise<T> {
  const { queryParams, ...fetchOptions } = options;
  let url = `${API_BASE}${path}`;

  if (queryParams) {
    const qs = new URLSearchParams(
      Object.entries(queryParams).map(([k, v]) => [k, String(v)])
    ).toString();
    url += `?${qs}`;
  }

  const res = await fetch(url, fetchOptions);
  if (!res.ok) {
    const text = await res.text();
    throw new Error(`API Error: ${res.status} ${res.statusText} - ${text}`);
  }
  return res.json() as Promise<T>;
}

export default apiFetch;
