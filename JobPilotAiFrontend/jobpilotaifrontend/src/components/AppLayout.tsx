import AppSidebar from '../components/AppSidebar'
import '../app.css'

export default function AppLayout({ children, title, subtitle, actions }: {
  children: React.ReactNode
  title?: string
  subtitle?: string
  actions?: React.ReactNode
}) {
  return (
    <div className="app-layout">
      <AppSidebar />
      <div className="app-main">
        {/* Mobile header */}
        <header className="app-mobile-header">
          <span className="app-mobile-logo">JobPilotAi</span>
          <button style={{ color: 'var(--app-on-surface-var)', background: 'none', border: 'none', cursor: 'pointer' }}>
            <span className="material-symbols-outlined">menu</span>
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
