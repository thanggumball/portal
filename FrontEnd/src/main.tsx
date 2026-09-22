import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
// import App from './App.tsx'
import { RouterProvider } from 'react-router/dom'
import { router } from './routes/router.tsx'
import { ToastProvider } from './components/Toast/ToastProvider.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ToastProvider>
      <RouterProvider router={router}>
      </RouterProvider>
    </ToastProvider>
  </StrictMode>,
)
