import { useState, useEffect, useRef, createContext, useContext } from 'react'
import { useNavigate } from 'react-router-dom'
import './App.css'
import './app.css'

/* ============================================================
   THEME CONTEXT
   ============================================================ */
interface ThemeContextType {
  theme: 'dark' | 'light'
  toggle: () => void
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined)

export function useTheme() {
  const context = useContext(ThemeContext)
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider')
  }
  return context
}

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setTheme] = useState<'dark' | 'light'>(() => {
    const stored = localStorage.getItem('jp-theme')
    if (stored === 'dark' || stored === 'light') return stored
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'dark'
  })

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme)
    localStorage.setItem('jp-theme', theme)
  }, [theme])

  const toggle = () => setTheme(t => (t === 'dark' ? 'light' : 'dark'))

  return (
    <ThemeContext.Provider value={{ theme, toggle }}>
      {children}
    </ThemeContext.Provider>
  )
}

/* ============================================================
   INTERSECTION OBSERVER HOOK
   ============================================================ */
function useFadeInUp() {
  const ref = useRef<HTMLDivElement | null>(null)
  useEffect(() => {
    const el = ref.current
    if (!el) return
    const obs = new IntersectionObserver(
      ([entry]) => { if (entry.isIntersecting) el.classList.add('visible') },
      { threshold: 0.12 }
    )
    obs.observe(el)
    return () => obs.disconnect()
  }, [])
  return ref
}

/* ============================================================
   NAVIGATION
   ============================================================ */
function Navbar({ theme, onToggleTheme }: { theme: string; onToggleTheme: () => void }) {
  const [scrolled, setScrolled] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const navigate = useNavigate()

  useEffect(() => {
    const handler = () => setScrolled(window.scrollY > 20)
    window.addEventListener('scroll', handler)
    return () => window.removeEventListener('scroll', handler)
  }, [])

  return (
    <header className={`nav ${scrolled ? 'scrolled' : ''}`}>
      <nav className="container nav-inner">
        {/* Logo */}
        <div className="nav-logo">JobPilotAi</div>

        {/* Desktop links */}
        <div className="nav-links">
          <a href="#features">Features</a>
          <a href="#spotlight">Solutions</a>
          <a href="#testimonials">Pricing</a>
          <a href="#footer">About</a>
        </div>

        {/* Actions */}
        <div className="nav-actions">
          {/* Theme toggle */}
          <button
            id="theme-toggle"
            className="theme-toggle"
            onClick={onToggleTheme}
            aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
            title={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
          >
            <span className="material-symbols-outlined">
              {theme === 'dark' ? 'light_mode' : 'dark_mode'}
            </span>
          </button>

          <button id="nav-login" className="btn-ghost hidden sm:block" onClick={() => navigate('/login')}>Login</button>
          <button id="nav-get-started" className="btn-primary" onClick={() => navigate('/signup')}>Get Started</button>

          {/* Mobile hamburger */}
          <button
            id="nav-mobile-menu"
            className="nav-mobile-btn"
            onClick={() => setMobileOpen(o => !o)}
            aria-label="Toggle menu"
          >
            <span className="material-symbols-outlined">
              {mobileOpen ? 'close' : 'menu'}
            </span>
          </button>
        </div>
      </nav>

      {/* Mobile drawer */}
      {mobileOpen && (
        <div style={{
          background: 'var(--nav-bg)',
          backdropFilter: 'blur(16px)',
          padding: '16px 24px 24px',
          borderBottom: '1px solid var(--nav-border)',
        }}>
          {['Features', 'Solutions', 'Pricing', 'About'].map(link => (
            <a
              key={link}
              href={`#${link.toLowerCase()}`}
              onClick={() => setMobileOpen(false)}
              style={{
                display: 'block',
                padding: '12px 0',
                color: 'var(--color-on-surface-variant)',
                fontFamily: 'Inter, sans-serif',
                fontSize: '16px',
                borderBottom: '1px solid var(--nav-border)',
              }}
            >
              {link}
            </a>
          ))}
          <button
            id="mobile-login"
            className="btn-ghost"
            style={{ display: 'block', paddingTop: '16px', width: '100%', textAlign: 'left' }}
            onClick={() => { setMobileOpen(false); navigate('/login') }}
          >
            Login
          </button>
          <button
            id="mobile-get-started"
            className="btn-primary"
            style={{ display: 'block', width: '100%', marginTop: '12px', textAlign: 'center', padding: '12px 24px' }}
            onClick={() => { setMobileOpen(false); navigate('/signup') }}
          >
            Get Started
          </button>
        </div>
      )}
    </header>
  )
}

/* ============================================================
   HERO SECTION
   ============================================================ */
function Hero() {
  const navigate = useNavigate()
  return (
    <section className="hero">
      <div className="container">
        <div className="hero-grid">
          {/* Left: Text */}
          <div>
            <span className="hero-badge label-caps">AI-Powered Career Hub</span>
            <h1 className="hero-title">
              Your Career, Accelerated by{' '}
              <span className="accent">Precision AI.</span>
            </h1>
            <p className="hero-desc">
              The all-in-one platform to optimize your resume, bypass ATS filters,
              and land your dream job faster than ever before.
            </p>
            <div className="hero-actions">
              <button id="hero-cta-start" className="btn-hero-primary" onClick={() => navigate('/signup')}>
                Start for Free
              </button>
              <button id="hero-cta-demo" className="btn-hero-secondary" onClick={() => document.getElementById('spotlight')?.scrollIntoView({ behavior: 'smooth' })}>
                <span className="material-symbols-outlined">play_circle</span>
                See how it works
              </button>
            </div>
          </div>

          {/* Right: Dashboard preview image */}
          <div className="hero-image-wrapper">
            <div className="hero-image-glow" aria-hidden="true" />
            <div className="hero-image-card">
              <img
                src="https://lh3.googleusercontent.com/aida-public/AB6AXuCC2sY-OCMu-ya3jG5HWufC4U5l1oeq-hqd9G1Xfzl1TZi9u4WfxlWURegGSUovA5SIjCRzv0jAJEqGtPBoSuFwXKQxaEnpI0ADjvI9JQ9zm55sXFhBRHWcJegZIQCXTCVTTLruzPwj-9v2MH_0A0jsMsOSMIs3IxZ2bZImtY-Z40tGPN7_je-KqHmFA-zHFQc42KADwpW5HftVF8I73R7-jmTqOl7uf5UyFnp-wN9kZw0zWUcJos3ShMb4qp1JeWRp-MFdLzAir8UT"
                alt="JobPilotAi Dashboard Preview – AI job search platform interface with match scores and analytics"
              />
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}

/* ============================================================
   SOCIAL PROOF
   ============================================================ */
function SocialProof() {
  const ref = useFadeInUp()
  return (
    <section className="social-proof">
      <div className="container">
        <div ref={ref} className="fade-in-up">
          <p className="social-proof-label">Trusted by 10K+ Professionals at</p>
          <div className="social-proof-logos">
            {['TECHCORE', 'DATASTREAM', 'PIXELFLOW', 'NEXUS', 'QUANTUM'].map(name => (
              <span key={name}>{name}</span>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}

/* ============================================================
   FEATURES / VALUE PILLARS
   ============================================================ */
function Features() {
  const ref = useFadeInUp()
  const card1 = useFadeInUp()
  const card2 = useFadeInUp()
  const card3 = useFadeInUp()

  return (
    <section id="features" className="features">
      <div className="container">
        <div ref={ref} className="section-header fade-in-up">
          <h2 className="section-title">Master Every Step of the Application</h2>
          <p className="section-subtitle">
            Our precision AI handles the heavy lifting, from initial scan to the final interview preparation.
          </p>
        </div>

        <div className="features-grid">
          {/* Pillar 1: ATS Analysis */}
          <div ref={card1} className="feature-card feature-card--primary fade-in-up">
            <div className="feature-icon-wrap feature-icon-wrap--primary">
              <span className="material-symbols-outlined material-symbols-filled feature-icon--primary">
                analytics
              </span>
            </div>
            <h3 className="feature-title">Deep ATS Analysis</h3>
            <p className="feature-desc">
              Understand exactly how algorithms see your resume. Identify missing keywords
              and structural errors in seconds.
            </p>
            <div className="feature-demo">
              <div className="feature-demo-row">
                <span className="feature-demo-label label-caps">Match Score</span>
                <span className="feature-demo-value label-code">84% Optimal</span>
              </div>
              <div className="progress-track">
                <div className="progress-fill" style={{ width: '84%' }} />
              </div>
            </div>
          </div>

          {/* Pillar 2: AI Cover Letters */}
          <div ref={card2} className="feature-card feature-card--secondary fade-in-up delay-1">
            <div className="feature-icon-wrap feature-icon-wrap--secondary">
              <span className="material-symbols-outlined material-symbols-filled feature-icon--secondary">
                edit_note
              </span>
            </div>
            <h3 className="feature-title">AI Cover Letters</h3>
            <p className="feature-desc">
              Generate high-fidelity, tone-perfect cover letters that highlight your specific
              strengths for every single role.
            </p>
            <div className="feature-demo">
              <div className="demo-tags">
                <span className="demo-tag demo-tag--active">Professional</span>
                <span className="demo-tag demo-tag--muted">Enthusiastic</span>
              </div>
              <div className="demo-lines">
                <div className="demo-line" style={{ width: '100%' }} />
                <div className="demo-line" style={{ width: '90%' }} />
                <div className="demo-line" style={{ width: '95%' }} />
              </div>
            </div>
          </div>

          {/* Pillar 3: Smart Matching */}
          <div ref={card3} className="feature-card feature-card--tertiary fade-in-up delay-2">
            <div className="feature-icon-wrap feature-icon-wrap--tertiary">
              <span className="material-symbols-outlined material-symbols-filled feature-icon--tertiary">
                hub
              </span>
            </div>
            <h3 className="feature-title">Smart Job Matching</h3>
            <p className="feature-desc">
              Visualize the bridge between your experience and market requirements.
              Find jobs that actually fit your profile.
            </p>
            <div className="feature-demo">
              <div className="demo-connector">
                <div className="demo-node demo-node--primary" />
                <div className="demo-wire">
                  <div className="demo-wire-dot" />
                </div>
                <div className="demo-node demo-node--secondary" />
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}

/* ============================================================
   SPOTLIGHT (Terminal)
   ============================================================ */
function Spotlight() {
  const termRef = useFadeInUp()
  const textRef = useFadeInUp()

  return (
    <section id="spotlight" className="spotlight">
      <div className="spotlight-glow" aria-hidden="true" />
      <div className="container">
        <div className="spotlight-grid">
          {/* Terminal */}
          <div ref={termRef} className="fade-in-up" style={{ order: 1 }}>
            <div className="terminal">
              <div className="terminal-topbar">
                <div className="terminal-dot terminal-dot--red" />
                <div className="terminal-dot terminal-dot--yellow" />
                <div className="terminal-dot terminal-dot--green" />
                <span className="terminal-label">resume-scanner --live</span>
              </div>
              <div className="terminal-body">
                <div className="terminal-line">
                  <span className="terminal-prompt">➜</span>
                  <span className="terminal-text">Scanning: "Senior_Fullstack_Resume.pdf"</span>
                </div>
                <div className="terminal-line terminal-line--indent">
                  <span className="terminal-muted">[0.1s]</span>
                  <span>
                    Extracting experience hierarchy...{' '}
                    <span className="terminal-ok">OK</span>
                  </span>
                </div>
                <div className="terminal-line terminal-line--indent">
                  <span className="terminal-muted">[0.3s]</span>
                  <span>
                    Identifying 14 core competencies...{' '}
                    <span className="terminal-ok">OK</span>
                  </span>
                </div>
                <div className="terminal-line terminal-line--indent">
                  <span className="terminal-muted">[0.5s]</span>
                  <span className="terminal-muted">
                    Cross-referencing 2.4M job descriptions...
                  </span>
                </div>
                <div className="terminal-divider">
                  <div className="terminal-result">
                    <span className="terminal-result-label">Analysis Complete:</span>
                    <span className="terminal-result-value">PRO-GRADE READY</span>
                  </div>
                  <div className="terminal-progress">
                    <div className="terminal-progress-fill" />
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Text */}
          <div ref={textRef} className="fade-in-up" style={{ order: 2 }}>
            <h2 className="spotlight-title">
              Built for Efficiency. Designed for Results.
            </h2>
            <p className="spotlight-desc">
              Our command-center interface gives you granular control over your professional identity.
              Don't just spray and pray; target with technical precision using live analysis feedback.
            </p>
            <ul className="feature-list">
              <li className="feature-list-item">
                <div className="feature-list-icon">
                  <span className="material-symbols-outlined">check</span>
                </div>
                <div>
                  <p className="feature-list-heading">Instant Feedback Loop</p>
                  <p className="feature-list-text">Get real-time scoring as you edit your resume content.</p>
                </div>
              </li>
              <li className="feature-list-item">
                <div className="feature-list-icon">
                  <span className="material-symbols-outlined">check</span>
                </div>
                <div>
                  <p className="feature-list-heading">Competency Mapping</p>
                  <p className="feature-list-text">Visual map of how your skills align with top-tier job roles.</p>
                </div>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </section>
  )
}

/* ============================================================
   TESTIMONIALS
   ============================================================ */
const testimonials = [
  {
    id: 1,
    quote: `"I went from 2% response rate to nearly 60% after using the ATS optimization tools. It's like having a career coach in my pocket 24/7."`,
    name: 'David Chen',
    role: 'Senior Software Engineer',
    avatar: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCkj09AMIk7J1zBD5jTdcxK1JUXM9Z8DxDJadlgckxbnpKw6NS9eKdJ8emI4Oq5K2KdPxHLmJZaXCM96fHP9685o0maIEFf2ojWXUeWBHgrwPcGKHcPXqFT35vnF091wyhaC1XcPWrJkCyFzxGidko0698dxFKOB934FRy0-wQ7jzoZotietQX6CqxI3vUj8-DO7aGxs01qH4HeDdulOLkL3YTnJBuPfoJmTu9XJSoAS6LD1xB8SX7HuyyIuJJUS_6yVAVopZMxE4sX',
  },
  {
    id: 2,
    quote: `"The AI-generated cover letters are actually good. They don't sound like robots; they sound like the most professional version of me."`,
    name: 'Sarah Jenkins',
    role: 'Marketing Director',
    avatar: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDYfkYOT888BDFxG-5BxCKW7akyU1OZ2sIlgiI0JtQUd4rj05h3Xw-df826Htcnt6wHOr-SWJuG_tSDGRKbd_aMtarWRFxkdS4SFj4F-gQuPqvkCILYMaHclSS-9gAeMEIbfzhB9GHk9g8yp2K5ZJisxrureKAzB7791p-rqqIuHSxSeR_HgRn-Jc758lx4H0Z3LQlr01n0m83GoKM7vAkcpOunq8ZkCdEEU5V6Ri7k8Pu5PFoeJzAANGGXwsQgm5WwOs-IbDKk_pSj',
  },
  {
    id: 3,
    quote: `"Landed my dream role at a Fortune 500 company in just 3 weeks. The Job Matching feature pointed me exactly where I needed to go."`,
    name: 'Marcus Thorne',
    role: 'Product Operations',
    avatar: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDs-PCw_hYoDi-fasOKYyN0r90QGmf5T3HhRqRt1e7-cFHmXUJd0Zxz9UCJpbz4O0aEJG4qt_i04QKtjt7Diy3UEqupImeyoccUsjVHiuXicxvhIzsGoAgRQ8Numwm2ErZ-tT1v6XOvD7qAXoaQfn_2WWXH0UdCd3eGeHx3usz8frcotQXKJJVG2g2TnnbrQ1ZXeqoXfA0Xs67jFxgL86Nqo1_0UbaM5F07q9e7nRUwXx6DOgDoXEpjLbFu9pxsSyF6aw1wUHj3YtX8',
  },
]

function Testimonials() {
  const ref = useFadeInUp()

  return (
    <section id="testimonials" className="testimonials">
      <div className="container">
        <div ref={ref} className="section-header fade-in-up">
          <h2 className="section-title">Success Stories</h2>
          <p className="section-subtitle">
            Hear from professionals who accelerated their career path with JobPilotAi.
          </p>
        </div>

        <div className="testimonials-grid">
          {testimonials.map((t, i) => (
            <TestimonialCard key={t.id} testimonial={t} delay={i} />
          ))}
        </div>
      </div>
    </section>
  )
}

function TestimonialCard({
  testimonial,
  delay,
}: {
  testimonial: (typeof testimonials)[0]
  delay: number
}) {
  const ref = useFadeInUp()
  return (
    <div
      ref={ref}
      className={`testimonial-card fade-in-up ${delay > 0 ? `delay-${delay}` : ''}`}
    >
      <div className="testimonial-stars">
        {Array.from({ length: 5 }).map((_, i) => (
          <span key={i} className="material-symbols-outlined material-symbols-filled">
            star
          </span>
        ))}
      </div>
      <p className="testimonial-text">{testimonial.quote}</p>
      <div className="testimonial-author">
        <div className="testimonial-avatar">
          <img src={testimonial.avatar} alt={`${testimonial.name} profile`} />
        </div>
        <div>
          <p className="testimonial-name">{testimonial.name}</p>
          <p className="testimonial-role label-caps">{testimonial.role}</p>
        </div>
      </div>
    </div>
  )
}

/* ============================================================
   CTA SECTION
   ============================================================ */
function CTASection() {
  const ref = useFadeInUp()
  const navigate = useNavigate()
  return (
    <section className="cta-section">
      <div className="container">
        <div ref={ref} className="cta-inner fade-in-up">
          <h2 className="cta-title">Ready to upgrade your job search?</h2>
          <p className="cta-desc">
            Join thousands of professionals who have already secured their next big role with JobPilotAi.
          </p>
          <button id="cta-final-btn" className="btn-cta" onClick={() => navigate('/signup')}>
            Get Started Now
          </button>
        </div>
      </div>
    </section>
  )
}

/* ============================================================
   FOOTER
   ============================================================ */
function Footer() {
  return (
    <footer id="footer" className="footer">
      <div className="container">
        <div className="footer-grid">
          <div className="footer-brand">
            <div className="footer-brand-name">JobPilotAi</div>
            <p className="footer-brand-desc">
              Elevating career journeys through the fusion of artificial intelligence
              and professional strategy.
            </p>
          </div>

          <div>
            <h6 className="footer-col-title label-caps">Product</h6>
            <ul className="footer-links">
              <li><a href="#">Resume Builder</a></li>
              <li><a href="#">ATS Scanner</a></li>
              <li><a href="#">Interview AI</a></li>
            </ul>
          </div>

          <div>
            <h6 className="footer-col-title label-caps">Company</h6>
            <ul className="footer-links">
              <li><a href="#">About Us</a></li>
              <li><a href="#">Careers</a></li>
              <li><a href="#">Blog</a></li>
            </ul>
          </div>

          <div>
            <h6 className="footer-col-title label-caps">Legal</h6>
            <ul className="footer-links">
              <li><a href="#">Terms</a></li>
              <li><a href="#">Privacy</a></li>
              <li><a href="#">Security</a></li>
            </ul>
          </div>

          <div>
            <h6 className="footer-col-title label-caps">Support</h6>
            <ul className="footer-links">
              <li><a href="#">Help Center</a></li>
              <li><a href="#">API Docs</a></li>
              <li><a href="#">Community</a></li>
            </ul>
          </div>
        </div>

        <div className="footer-bottom">
          <p>© 2024 JobPilotAi. All rights reserved.</p>
        </div>
      </div>
    </footer>
  )
}

/* ============================================================
   APP ROOT
   ============================================================ */
export default function App() {
  const { theme, toggle } = useTheme()

  return (
    <>
      <Navbar theme={theme} onToggleTheme={toggle} />
      <main>
        <Hero />
        <SocialProof />
        <Features />
        <Spotlight />
        <Testimonials />
        <CTASection />
      </main>
      <Footer />
    </>
  )
}
