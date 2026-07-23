import { NavLink, useNavigate } from 'react-router-dom'
import { api, clearStoredAuth, getStoredAuth } from '../api/client'
import { useTheme } from '../App'

const navItems = [
  { icon: 'dashboard', label: 'Dashboard', to: '/dashboard' },
  { icon: 'person', label: 'Profile', to: '/profile' },
  { icon: 'description', label: 'My Resumes', to: '/resumes' },
  { icon: 'analytics', label: 'ATS Analysis', to: '/ats-analysis' },
  { icon: 'mail', label: 'Cover Letters', to: '/cover-letter' },
  { icon: 'hub', label: 'Job Matching', to: '/job-match' },
  { icon: 'payments', label: 'Subscription', to: '/subscription' },
]

export default function AppSidebar() {
  const navigate = useNavigate()
  const { theme, toggle } = useTheme()

  const handleLogout = async () => {
    const auth = getStoredAuth()
    clearStoredAuth()
    if (auth?.refreshToken) {
      try {
        await api.logout(auth.refreshToken)
      } catch {
        // Local logout should still complete if the server token is already gone.
      }
    }
    navigate('/')
  }

  return (
    <nav className="app-sidebar">
      {/* Logo */}
      <div className="sidebar-logo-wrap">
        <div className="sidebar-logo-icon">
          <span className="material-symbols-outlined">work</span>
        </div>
        <div>
          <div className="sidebar-logo-name">JobPilotAi</div>
          <div className="sidebar-logo-sub">Professional Suite</div>
        </div>
      </div>

      {/* Theme toggle */}
      <button
        onClick={toggle}
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 8,
          padding: '12px 16px',
          margin: '0 16px 16px',
          background: 'var(--app-surface)',
          border: '1px solid var(--app-outline-var)',
          borderRadius: 8,
          cursor: 'pointer',
          fontSize: 13,
          fontWeight: 600,
          color: 'var(--app-on-surface)',
        }}
      >
        <span className="material-symbols-outlined">
          {theme === 'dark' ? 'light_mode' : 'dark_mode'}
        </span>
        {theme === 'dark' ? 'Light Mode' : 'Dark Mode'}
      </button>

      {/* Nav Links */}
      <div className="sidebar-nav">
        {navItems.map(item => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `sidebar-nav-item${isActive ? ' active' : ''}`
            }
          >
            <span
              className="material-symbols-outlined"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              {item.icon}
            </span>
            <span>{item.label}</span>
          </NavLink>
        ))}
      </div>

      {/* Bottom */}
      <div className="sidebar-bottom">
        <button
          className="sidebar-upgrade-btn"
          onClick={() => navigate('/subscription')}
        >
          <span className="material-symbols-outlined" style={{ fontSize: 18 }}>
            workspace_premium
          </span>
          Upgrade to Pro
        </button>
        <div className="sidebar-footer-links">
          <NavLink to="/profile" className="sidebar-footer-link">
            <span className="material-symbols-outlined" style={{ fontSize: 18 }}>settings</span>
            <span>Settings</span>
          </NavLink>
          <button type="button" onClick={handleLogout} className="sidebar-footer-link" style={{ background: 'none', border: 'none', width: '100%', cursor: 'pointer' }}>
            <span className="material-symbols-outlined" style={{ fontSize: 18 }}>logout</span>
            <span>Logout</span>
          </button>
        </div>
      </div>
    </nav>
  )
}
