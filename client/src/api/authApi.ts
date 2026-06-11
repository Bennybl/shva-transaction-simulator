import { apiRequest } from './httpClient';
import type { AuthResponse } from '../types/api';

export function signup(username: string, password: string): Promise<AuthResponse> {
  return apiRequest<AuthResponse>('/api/auth/signup', {
    method: 'POST',
    body: { username, password },
  });
}

export function login(username: string, password: string): Promise<AuthResponse> {
  return apiRequest<AuthResponse>('/api/auth/login', {
    method: 'POST',
    body: { username, password },
  });
}
