import styles from './Header.module.css';
import { LanguageToggle } from '../LanguageToggle/LanguageToggle';
import { useAuth } from '../../auth/AuthContext';
import { useLanguage } from '../../i18n/LanguageContext';

function ShvaLogo() {
  return (
    <div className={styles.logo} aria-label="Shva">
      <svg width="34" height="34" viewBox="0 0 34 34" fill="none" aria-hidden="true">
        <circle cx="17" cy="17" r="13" stroke="#5246b8" strokeWidth="4.5" strokeLinecap="round" strokeDasharray="58 24" transform="rotate(-50 17 17)" />
        <circle cx="17" cy="17" r="5.5" stroke="#2bb3c0" strokeWidth="3.5" strokeLinecap="round" strokeDasharray="24 11" transform="rotate(120 17 17)" />
      </svg>
      <span className={styles.logoText}>shva</span>
    </div>
  );
}

export function Header() {
  const { isAuthenticated, username, signOut } = useAuth();
  const { t } = useLanguage();

  return (
    <header className={styles.header}>
      <ShvaLogo />
      <div className={styles.actions}>
        <LanguageToggle />
        {isAuthenticated && (
          <button type="button" className={styles.logout} onClick={signOut}>
            {t('logout')} ({username})
          </button>
        )}
      </div>
    </header>
  );
}
