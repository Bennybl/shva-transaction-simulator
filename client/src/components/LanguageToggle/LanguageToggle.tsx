import styles from './LanguageToggle.module.css';
import { useLanguage } from '../../i18n/LanguageContext';

export function LanguageToggle() {
  const { language, setLanguage, t } = useLanguage();

  return (
    <div className={styles.toggle} role="group" aria-label="Language">
      <button
        type="button"
        className={language === 'en' ? styles.active : styles.button}
        onClick={() => setLanguage('en')}
      >
        {t('english')}
      </button>
      <button
        type="button"
        className={language === 'he' ? styles.active : styles.button}
        onClick={() => setLanguage('he')}
      >
        {t('hebrew')}
      </button>
    </div>
  );
}
