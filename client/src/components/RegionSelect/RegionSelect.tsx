import { useEffect, useRef, useState } from 'react';
import styles from './RegionSelect.module.css';
import { useLanguage } from '../../i18n/LanguageContext';
import type { Region } from '../../types/api';

type RegionSelectProps = {
  regions: Region[];
  selectedRegion: Region | null;
  onSelect: (region: Region | null) => void;
};

export function RegionSelect({ regions, selectedRegion, onSelect }: RegionSelectProps) {
  const { t } = useLanguage();
  const [query, setQuery] = useState('');
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  // Close the dropdown when clicking outside.
  useEffect(() => {
    function handleClick(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  const filtered = regions.filter((r) =>
    r.displayName.toLowerCase().includes(query.toLowerCase()),
  );

  const displayValue = open ? query : (selectedRegion?.displayName ?? '');

  return (
    <div className={styles.container} ref={containerRef}>
      <fieldset className={styles.field}>
        <legend className={styles.legend}>{t('region')}</legend>
        <input
          className={styles.input}
          type="text"
          placeholder={t('searchRegion')}
          value={displayValue}
          onFocus={() => {
            setQuery('');
            setOpen(true);
          }}
          onChange={(e) => {
            setQuery(e.target.value);
            setOpen(true);
          }}
          aria-label={t('region')}
        />
        {(selectedRegion || query) && (
          <button
            type="button"
            className={styles.clear}
            aria-label="Clear"
            onClick={() => {
              setQuery('');
              onSelect(null);
              setOpen(false);
            }}
          >
            ⊗
          </button>
        )}
      </fieldset>

      {open && (
        <ul className={styles.dropdown} role="listbox">
          {filtered.length === 0 && <li className={styles.empty}>—</li>}
          {filtered.map((region) => (
            <li key={region.id}>
              <button
                type="button"
                className={styles.option}
                role="option"
                aria-selected={selectedRegion?.id === region.id}
                onClick={() => {
                  onSelect(region);
                  setQuery('');
                  setOpen(false);
                }}
              >
                {region.displayName}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
