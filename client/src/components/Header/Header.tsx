import styles from './Header.module.css';
import { LanguageToggle } from '../LanguageToggle/LanguageToggle';
import { useAuth } from '../../auth/AuthContext';
import { useLanguage } from '../../i18n/LanguageContext';

function ShvaLogo() {
  return (
    <div className={styles.logo} aria-label="Shva">
      <img src="/shva-logo.svg" alt="shva" className={styles.logoImg} />
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
