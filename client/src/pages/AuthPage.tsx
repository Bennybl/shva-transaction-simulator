import { useState } from 'react';
import type { FormEvent } from 'react';
import styles from './AuthPage.module.css';
import { login, signup } from '../api/authApi';
import { useAuth } from '../auth/AuthContext';
import { useLanguage } from '../i18n/LanguageContext';

export function AuthPage() {
  const { t } = useLanguage();
  const { signIn } = useAuth();

  const [mode, setMode] = useState<'login' | 'signup'>('login');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      const auth =
        mode === 'login'
          ? await login(username, password)
          : await signup(username, password);
      signIn(auth);
    } catch (e) {
      setError(e instanceof Error ? e.message : t('authError'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className={styles.page}>
      <form className={styles.card} onSubmit={handleSubmit}>
        <div className={styles.tabs}>
          <button
            type="button"
            className={mode === 'login' ? styles.tabActive : styles.tab}
            onClick={() => setMode('login')}
          >
            {t('login')}
          </button>
          <button
            type="button"
            className={mode === 'signup' ? styles.tabActive : styles.tab}
            onClick={() => setMode('signup')}
          >
            {t('signup')}
          </button>
        </div>

        <label className={styles.label}>
          {t('username')}
          <input
            className={styles.input}
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
            minLength={3}
            maxLength={50}
            autoComplete="username"
          />
        </label>

        <label className={styles.label}>
          {t('password')}
          <input
            className={styles.input}
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={6}
            maxLength={100}
            autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
          />
        </label>

        {error && <p className={styles.error}>{error}</p>}

        <button type="submit" className={styles.submit} disabled={submitting}>
          {mode === 'login' ? t('login') : t('signup')}
        </button>
      </form>
    </main>
  );
}
