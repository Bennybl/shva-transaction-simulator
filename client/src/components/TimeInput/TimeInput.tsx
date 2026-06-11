import styles from './TimeInput.module.css';
import { useLanguage } from '../../i18n/LanguageContext';

type TimeInputProps = {
  hour: string;
  minute: string;
  onHourChange: (value: string) => void;
  onMinuteChange: (value: string) => void;
  onSubmit: () => void;
  onCancel: () => void;
  submitting: boolean;
};

function clampNumeric(value: string, max: number): string {
  const digits = value.replace(/\D/g, '').slice(0, 2);
  if (digits === '') return '';
  return Number(digits) > max ? String(max) : digits;
}

export function TimeInput({
  hour,
  minute,
  onHourChange,
  onMinuteChange,
  onSubmit,
  onCancel,
  submitting,
}: TimeInputProps) {
  const { t } = useLanguage();

  return (
    <div className={styles.panel}>
      <div className={styles.title}>{t('enterTime')}</div>

      <div className={styles.inputsRow} dir="ltr">
        <div className={styles.timeColumn}>
          <input
            className={styles.timeBox}
            value={hour}
            inputMode="numeric"
            placeholder="00"
            onChange={(e) => onHourChange(clampNumeric(e.target.value, 23))}
            aria-label={t('hour')}
          />
          <span className={styles.timeLabel}>{t('hour')}</span>
        </div>

        <span className={styles.colon}>:</span>

        <div className={styles.timeColumn}>
          <input
            className={styles.timeBoxMuted}
            value={minute}
            inputMode="numeric"
            placeholder="00"
            onChange={(e) => onMinuteChange(clampNumeric(e.target.value, 59))}
            aria-label={t('minute')}
          />
          <span className={styles.timeLabel}>{t('minute')}</span>
        </div>
      </div>

      <div className={styles.footer}>
        <svg
          className={styles.clockIcon}
          width="20"
          height="20"
          viewBox="0 0 24 24"
          fill="none"
          aria-hidden="true"
        >
          <circle cx="12" cy="12" r="9" stroke="currentColor" strokeWidth="1.8" />
          <path d="M12 7v5l3.5 2" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
        </svg>

        <div className={styles.footerActions}>
          <button type="button" className={styles.textButton} onClick={onCancel} disabled={submitting}>
            {t('cancel')}
          </button>
          <button type="button" className={styles.textButton} onClick={onSubmit} disabled={submitting}>
            {t('ok')}
          </button>
        </div>
      </div>
    </div>
  );
}
