import { apiRequest } from './httpClient';
import type {
  ApprovedTransaction,
  SimulateTransactionRequest,
  TransactionSimulationResponse,
} from '../types/api';

export function simulateTransaction(
  request: SimulateTransactionRequest,
): Promise<TransactionSimulationResponse> {
  return apiRequest<TransactionSimulationResponse>('/api/transactions/simulate', {
    method: 'POST',
    body: request,
  });
}

export function fetchApprovedTransactions(limit = 20): Promise<ApprovedTransaction[]> {
  return apiRequest<ApprovedTransaction[]>(`/api/transactions/approved?limit=${limit}`);
}
