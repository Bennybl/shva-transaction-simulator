import { Header } from './components/Header/Header';
import { HomePage } from './pages/HomePage';
import { AuthPage } from './pages/AuthPage';
import { useAuth } from './auth/AuthContext';

export function App() {
  const { isAuthenticated } = useAuth();

  return (
    <>
      <Header />
      {isAuthenticated ? <HomePage /> : <AuthPage />}
    </>
  );
}
