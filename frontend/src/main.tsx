import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
// Must precede index.css: CSS requires @import rules to come first, so importing
// the fonts here keeps them from fighting `@import "tailwindcss"`.
import '@fontsource-variable/inter/index.css'
import '@fontsource-variable/manrope/index.css'
import './index.css'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
