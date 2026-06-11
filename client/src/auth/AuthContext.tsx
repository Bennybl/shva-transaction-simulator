import { createContext, useCallback, useContext, useState } from 'react';
import type { ReactNode } from 'react';
import { clearToken, getStoredToken, storeToken } from '../api/httpClient';
import type { AuthResponse } from '../types/api';

type AuthContextValue = {
  isAuthenticated: boolean;
  username: string | null;
  signIn: (auth: AuthResponse) => void;
  signOut: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

const USERNAME_STORAGE_KEY = 'shva.auth.username';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(getStoredToken);
  const [username, setUsername] = useState<string | null>(() =>
    localStorage.getItem(USERNAME_STORAGE_KEY),
  );

  const signIn = useCallback((auth: AuthResponse) => {
    storeToken(auth.token);
    localStorage.setItem(USERNAME_STORAGE_KEY, auth.username);
    setToken(auth.token);
    setUsername(auth.username);
  }, []);

  const signOut = useCallback(() => {
    clearToken();
    localStorage.removeItem(USERNAME_STORAGE_KEY);
    setToken(null);
    setUsername(null);
  }, []);

  return (
    <AuthContext.Provider value={{ isAuthenticated: token !== null, username, signIn, signOut }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
