import styles from './HomePage.module.css';
import { TransactionSimulator } from '../components/TransactionSimulator/TransactionSimulator';
import { ApprovedTransactions } from '../components/ApprovedTransactions/ApprovedTransactions';
import { useApprovedTransactions } from '../hooks/useApprovedTransactions';

export function HomePage() {
  const { transactions, error, refresh } = useApprovedTransactions();

  return (
    <main className={styles.main}>
      <TransactionSimulator onApproved={refresh} />
      <ApprovedTransactions transactions={transactions} error={error} />
    </main>
  );
}
