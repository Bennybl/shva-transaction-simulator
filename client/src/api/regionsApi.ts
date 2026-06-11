import { apiRequest } from './httpClient';
import type { Region } from '../types/api';

export function fetchRegions(): Promise<Region[]> {
  return apiRequest<Region[]>('/api/regions');
}
