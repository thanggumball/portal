import { StrictMode, Suspense } from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider } from 'react-router';
import { ConfigProvider, theme } from 'antd';
import { router } from './routes/router';
import { ThemeProvider, useThemeMode } from './contexts/ThemeContext';
import './index.css';
import { AppProvider } from './contexts/AppContext';

function ThemedApp() {
  const { mode } = useThemeMode();
  return (
    <ConfigProvider
      theme={{
        algorithm: mode === 'dark' ? theme.darkAlgorithm : theme.defaultAlgorithm,
      }}
    >
      <Suspense fallback={<div>Loading…</div>}>
        <RouterProvider router={router} />
      </Suspense>
    </ConfigProvider>
  );
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AppProvider>
      <ThemeProvider>
        <ThemedApp />
      </ThemeProvider>
    </AppProvider>
  </StrictMode>
);