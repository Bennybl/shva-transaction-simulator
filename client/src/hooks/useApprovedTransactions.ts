import { useCallback, useEffect, useState } from 'react';
import { fetchApprovedTransactions } from '../api/transactionsApi';
import type { ApprovedTransaction } from '../types/api';

export function useApprovedTransactions() {
  const [transactions, setTransactions] = useState<ApprovedTransaction[]>([]);
  const [error, setError] = useState(false);

  const refresh = useCallback(() => {
    fetchApprovedTransactions()
      .then((data) => {
        setTransactions(data);
        setError(false);
      })
      .catch(() => setError(true));
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  return { transactions, error, refresh };
}
