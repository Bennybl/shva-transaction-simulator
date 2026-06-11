import { useCallback, useState } from 'react';
import { simulateTransaction } from '../api/transactionsApi';
import type { TransactionSimulationResponse } from '../types/api';

/**
 * Builds an ISO-8601 instant from today's date and the selected HH:mm
 * in the browser's local time zone (offset included). The backend converts
 * this instant to the selected region's local time.
 */
function buildSubmittedAt(hour: number, minute: number): string {
  const now = new Date();
  const local = new Date(
    now.getFullYear(),
    now.getMonth(),
    now.getDate(),
    hour,
    minute,
    0,
    0,
  );

  const offsetMinutes = -local.getTimezoneOffset();
  const sign = offsetMinutes >= 0 ? '+' : '-';
  const abs = Math.abs(offsetMinutes);
  const pad = (n: number) => String(n).padStart(2, '0');

  return (
    `${local.getFullYear()}-${pad(local.getMonth() + 1)}-${pad(local.getDate())}` +
    `T${pad(hour)}:${pad(minute)}:00${sign}${pad(Math.floor(abs / 60))}:${pad(abs % 60)}`
  );
}

export function useSimulation(onApproved: () => void) {
  const [result, setResult] = useState<TransactionSimulationResponse | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const simulate = useCallback(
    async (regionId: string, hour: number, minute: number) => {
      setSubmitting(true);
      setError(null);
      try {
        const response = await simulateTransaction({
          regionId,
          submittedAt: buildSubmittedAt(hour, minute),
        });
        setResult(response);
        if (response.status === 'Approved') {
          onApproved();
        }
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Request failed');
      } finally {
        setSubmitting(false);
      }
    },
    [onApproved],
  );

  const reset = useCallback(() => {
    setResult(null);
    setError(null);
  }, []);

  return { result, submitting, error, simulate, reset };
}
