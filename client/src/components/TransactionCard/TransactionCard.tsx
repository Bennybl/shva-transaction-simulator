import styles from './TransactionCard.module.css';
import { useLanguage } from '../../i18n/LanguageContext';
import type { ApprovedTransaction } from '../../types/api';

export function TransactionCard({ transaction }: { transaction: ApprovedTransaction }) {
  const { t } = useLanguage();

  return (
    <article className={styles.card}>
      <div className={styles.time}>
        {t('time')}: {transaction.localTime}
      </div>
      <div className={styles.zone}>
        {t('timeZone')}: {transaction.regionName}
      </div>
    </article>
  );
}
