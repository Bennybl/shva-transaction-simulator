import { useState } from 'react';
import styles from './TransactionSimulator.module.css';
import { RegionSelect } from '../RegionSelect/RegionSelect';
import { TimeInput } from '../TimeInput/TimeInput';
import { useLanguage } from '../../i18n/LanguageContext';
import { useRegions } from '../../hooks/useRegions';
import { useSimulation } from '../../hooks/useSimulation';
import type { Region } from '../../types/api';

function Illustration() {
  return (
    <svg
      className={styles.illustration}
      viewBox="0 0 420 240"
      fill="none"
      role="img"
      aria-label="Shva payments illustration"
    >
      <ellipse cx="210" cy="210" rx="180" ry="22" fill="#f3effb" />
      <rect x="30" y="30" width="240" height="150" rx="10" fill="#ffffff" stroke="#d9d6e4" />
      <rect x="30" y="30" width="240" height="26" rx="10" fill="#ede9f8" />
      <circle cx="46" cy="43" r="4" fill="#5246b8" />
      <circle cx="60" cy="43" r="4" fill="#2bb3c0" />
      <rect x="48" y="74" width="120" height="11" rx="5" fill="#3a2f8c" />
      <rect x="48" y="94" width="160" height="11" rx="5" fill="#5246b8" opacity="0.75" />
      <rect x="48" y="122" width="200" height="7" rx="3.5" fill="#d9d6e4" />
      <rect x="48" y="137" width="180" height="7" rx="3.5" fill="#d9d6e4" />
      <rect x="48" y="152" width="190" height="7" rx="3.5" fill="#d9d6e4" />
      <rect x="270" y="55" width="110" height="170" rx="16" fill="#1f1f2e" />
      <rect x="277" y="68" width="96" height="144" rx="8" fill="#5246b8" />
      <rect x="287" y="84" width="76" height="9" rx="4.5" fill="#ffffff" opacity="0.9" />
      <rect x="287" y="101" width="56" height="9" rx="4.5" fill="#ffffff" opacity="0.6" />
      <circle cx="325" cy="160" r="24" stroke="#ffffff" strokeWidth="5" strokeDasharray="105 46" transform="rotate(-50 325 160)" fill="none" />
      <circle cx="86" cy="196" r="14" fill="#5246b8" />
      <rect x="74" y="190" width="24" height="12" rx="3" fill="#ffffff" />
      <rect x="74" y="193" width="24" height="3" fill="#5246b8" />
    </svg>
  );
}

type TransactionSimulatorProps = {
  onApproved: () => void;
};

export function TransactionSimulator({ onApproved }: TransactionSimulatorProps) {
  const { t } = useLanguage();
  const { regions, error: regionsError } = useRegions();
  const { result, submitting, error: submitError, simulate, reset } = useSimulation(onApproved);

  const [selectedRegion, setSelectedRegion] = useState<Region | null>(null);
  const [hour, setHour] = useState('');
  const [minute, setMinute] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);

  const handleSubmit = () => {
    if (!selectedRegion) {
      setValidationError(t('selectRegionError'));
      return;
    }
    setValidationError(null);
    void simulate(selectedRegion.id, Number(hour || '0'), Number(minute || '0'));
  };

  const handleCancel = () => {
    setHour('');
    setMinute('');
    setValidationError(null);
    reset();
  };

  return (
    <section className={styles.simulator}>
      <div className={styles.controls}>
        <RegionSelect
          regions={regions}
          selectedRegion={selectedRegion}
          onSelect={(region) => {
            setSelectedRegion(region);
            setValidationError(null);
          }}
        />
        <TimeInput
          hour={hour}
          minute={minute}
          onHourChange={setHour}
          onMinuteChange={setMinute}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          submitting={submitting}
        />

        {regionsError && <p className={styles.error}>{t('loadError')}</p>}
        {validationError && <p className={styles.error}>{validationError}</p>}
        {submitError && <p className={styles.error}>{submitError}</p>}
      </div>

      <div className={styles.presentation}>
        <span className={styles.chip}>{t('transactionSimulator')}</span>
        <h1 className={styles.question}>{t('question')}</h1>

        {result && (
          <div
            className={result.status === 'Approved' ? styles.resultApproved : styles.resultRejected}
            role="status"
          >
            <strong>{result.status === 'Approved' ? t('approved') : t('rejected')}</strong>
            <span className={styles.resultDetails}>
              {t('time')}: {result.localTime} · {t('timeZone')}: {result.regionName}
            </span>
          </div>
        )}

        <Illustration />
      </div>
    </section>
  );
}
