import { useEffect, useState } from 'react';
import { fetchRegions } from '../api/regionsApi';
import type { Region } from '../types/api';

export function useRegions() {
  const [regions, setRegions] = useState<Region[]>([]);
  const [error, setError] = useState(false);

  useEffect(() => {
    let cancelled = false;

    fetchRegions()
      .then((data) => {
        if (!cancelled) setRegions(data);
      })
      .catch(() => {
        if (!cancelled) setError(true);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  return { regions, error };
}
