import { useRef } from 'react';
import styles from './ApprovedTransactions.module.css';
import { TransactionCard } from '../TransactionCard/TransactionCard';
import { useLanguage } from '../../i18n/LanguageContext';
import type { ApprovedTransaction } from '../../types/api';

type ApprovedTransactionsProps = {
  transactions: ApprovedTransaction[];
  error: boolean;
};

export function ApprovedTransactions({ transactions, error }: ApprovedTransactionsProps) {
  const { t, language } = useLanguage();
  const listRef = useRef<HTMLDivElement>(null);

  const scrollByCards = (direction: 1 | -1) => {
    // In RTL, scrolling "forward" is to the left.
    const rtlFactor = language === 'he' ? -1 : 1;
    listRef.current?.scrollBy({ left: direction * rtlFactor * 500, behavior: 'smooth' });
  };

  return (
    <section className={styles.section}>
      <h2 className={styles.title}>{t('approvedTransactions')}</h2>

      {error && <p className={styles.message}>{t('loadError')}</p>}
      {!error && transactions.length === 0 && (
        <p className={styles.message}>{t('noApprovedYet')}</p>
      )}

      {transactions.length > 0 && (
        <div className={styles.carousel}>
          <button
            type="button"
            className={styles.arrow}
            onClick={() => scrollByCards(-1)}
            aria-label="Previous"
          >
            ←
          </button>
          <div className={styles.list} ref={listRef}>
            {transactions.map((transaction) => (
              <TransactionCard key={transaction.id} transaction={transaction} />
            ))}
          </div>
          <button
            type="button"
            className={styles.arrow}
            onClick={() => scrollByCards(1)}
            aria-label="Next"
          >
            →
          </button>
        </div>
      )}
    </section>
  );
}
