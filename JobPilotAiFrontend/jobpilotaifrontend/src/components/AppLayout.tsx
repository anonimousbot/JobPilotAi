import { useState } from 'react'
import AppSidebar from '../components/AppSidebar'
import '../App.css'

export default function AppLayout({ children, title, subtitle, actions }: {
  children: React.ReactNode
  title?: string
  subtitle?: string
  actions?: React.ReactNode
}) {
  const [mobileOpen, setMobileOpen] = useState(false)

  return (
    <div className="app-layout">
      <AppSidebar isOpen={mobileOpen} onClose={() => setMobileOpen(false)} />
      <div className="app-main">
        {/* Mobile header */}
        <header className="app-mobile-header">
          <span className="app-mobile-logo">JobPilotAi</span>
          <button
            onClick={() => setMobileOpen(o => !o)}
            style={{ color: 'var(--app-on-surface-var)', background: 'none', border: 'none', cursor: 'pointer' }}
            aria-label="Toggle navigation menu"
          >
            <span className="material-symbols-outlined">{mobileOpen ? 'close' : 'menu'}</span>
          </button>
        </header>

        <div className="app-content">
          {(title || actions) && (
            <div className="page-header">
              <div>
                {title && <h1 className="page-title">{title}</h1>}
                {subtitle && <p className="page-subtitle">{subtitle}</p>}
              </div>
              {actions && <div className="page-actions">{actions}</div>}
            </div>
          )}
          {children}
        </div>
      </div>
    </div>
  )
}
