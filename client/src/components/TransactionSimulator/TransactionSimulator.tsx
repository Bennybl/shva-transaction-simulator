import { useState } from 'react';
import styles from './TransactionSimulator.module.css';
import { RegionSelect } from '../RegionSelect/RegionSelect';
import { TimeInput } from '../TimeInput/TimeInput';
import { useLanguage } from '../../i18n/LanguageContext';
import { useRegions } from '../../hooks/useRegions';
import { useSimulation } from '../../hooks/useSimulation';
import type { Region } from '../../types/api';

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

        <img src="/hero.svg" alt="" className={styles.illustration} aria-hidden="true" />
      </div>
    </section>
  );
}
