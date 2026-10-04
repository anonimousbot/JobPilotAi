import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import type { AnalysisResponse, CoverLetterResponse, ProfileResponse, ResumeResponse, SubscriptionResponse } from '../api/types'
import AppLayout from '../components/AppLayout'
import '../app.css'

export default function DashboardPage() {
  const navigate = useNavigate()
  const [profile, setProfile] = useState<ProfileResponse | null>(null)
  const [resumes, setResumes] = useState<ResumeResponse[]>([])
  const [analyses, setAnalyses] = useState<AnalysisResponse[]>([])
  const [letters, setLetters] = useState<CoverLetterResponse[]>([])
  const [subscription, setSubscription] = useState<SubscriptionResponse | null>(null)

  useEffect(() => {
    void Promise.all([
      api.getProfile().then(setProfile).catch(() => undefined),
      api.listResumes().then(setResumes).catch(() => undefined),
      api.listAnalyses().then(setAnalyses).catch(() => undefined),
      api.listCoverLetters().then(setLetters).catch(() => undefined),
      api.getCurrentSubscription().then(setSubscription).catch(() => undefined),
    ])
  }, [])

  const profileStrength = useMemo(() => {
    const fields = [profile?.firstName, profile?.lastName, profile?.careerGoal, profile?.targetRole, profile?.careerLevel, resumes[0], analyses[0]]
    return Math.round((fields.filter(Boolean).length / fields.length) * 100)
  }, [analyses, profile, resumes])

  return (
    <AppLayout
      title="Overview"
      subtitle="Here's what's happening with your job search today."
      actions={
        <>
          <button className="btn-app-secondary" onClick={() => navigate('/resumes')}>
            <span className="material-symbols-outlined" style={{ fontSize: 18 }}>upload_file</span>
            Upload Resume
          </button>
          <button className="btn-app-primary" onClick={() => navigate('/ats-analysis')}>
            <span className="material-symbols-outlined" style={{ fontSize: 18 }}>add</span>
            New Analysis
          </button>
        </>
      }
    >
      <div className="bento-grid">

        {/* Welcome card */}
        <div className="bento-card col-span-8" style={{ position: 'relative', overflow: 'hidden', minHeight: 220 }}>
          <div style={{ position: 'absolute', top: 0, right: 0, width: 220, height: 220, background: 'var(--app-surface-highest)', borderRadius: '50%', filter: 'blur(60px)', opacity: 0.6, transform: 'translate(30%, -30%)' }} />
          <div className="bento-card-body" style={{ position: 'relative', zIndex: 1 }}>
            <div style={{ display: 'inline-flex', alignItems: 'center', gap: 6, padding: '3px 10px', borderRadius: 999, background: 'var(--app-secondary-cont)', color: 'var(--app-on-sec-cont)', fontSize: 10, fontWeight: 700, letterSpacing: '0.1em', textTransform: 'uppercase', marginBottom: 14 }}>
              <span className="material-symbols-outlined" style={{ fontSize: 12 }}>trending_up</span>
              Market Momentum
            </div>
            <h3 style={{ fontFamily: 'Hanken Grotesk', fontSize: 22, fontWeight: 700, color: 'var(--app-on-surface)', marginBottom: 10 }}>Welcome back{profile?.firstName ? `, ${profile.firstName}` : ''}.</h3>
            <p style={{ fontSize: 14, color: 'var(--app-on-surface-var)', lineHeight: 1.65, marginBottom: 20, maxWidth: 480 }}>
              You have {resumes.length} resume{resumes.length === 1 ? '' : 's'}, {analyses.length} ATS analysis run{analyses.length === 1 ? '' : 's'}, and {letters.length} cover letter draft{letters.length === 1 ? '' : 's'} ready.
            </p>
            <a onClick={() => navigate('/ats-analysis')} style={{ display: 'inline-flex', alignItems: 'center', gap: 4, fontSize: 12, fontWeight: 700, color: 'var(--app-primary)', letterSpacing: '0.04em', borderBottom: '1px solid var(--app-primary)', paddingBottom: 1, cursor: 'pointer' }}>
              View Detailed Insights <span className="material-symbols-outlined" style={{ fontSize: 16 }}>arrow_forward</span>
            </a>
          </div>
        </div>

        {/* Profile Strength */}
        <div className="bento-card col-span-4">
          <div className="bento-card-body" style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', textAlign: 'center' }}>
            <div style={{ fontSize: 11, fontWeight: 700, letterSpacing: '0.1em', textTransform: 'uppercase', color: 'var(--app-on-surface-var)', marginBottom: 16 }}>Overall Profile Strength</div>
            <div className="circular-progress-wrap">
              <svg width="128" height="128" style={{ transform: 'rotate(-90deg)' }} viewBox="0 0 100 100">
                <circle cx="50" cy="50" r="40" fill="transparent" stroke="var(--app-surface-low)" strokeWidth="8" />
                <circle cx="50" cy="50" r="40" fill="transparent" stroke="var(--app-secondary)" strokeWidth="8" strokeDasharray="251.2" strokeDashoffset={251.2 - (profileStrength / 100) * 251.2} strokeLinecap="round" style={{ transition: 'stroke-dashoffset 1s ease' }} />
              </svg>
              <div className="circular-progress-center">
                <span className="circular-progress-value">{profileStrength}</span>
                <span className="circular-progress-sub">/100</span>
              </div>
            </div>
            <p style={{ fontSize: 13, color: 'var(--app-on-surface-var)', marginTop: 12 }}>{profileStrength >= 80 ? 'Strong foundation. Keep momentum with tailored analyses.' : 'Complete your profile and upload a resume to improve this score.'}</p>
          </div>
        </div>

        {/* ATS Analyses quota */}
        <div className="bento-card col-span-4">
          <div className="bento-card-body">
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 16 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <span className="material-symbols-outlined" style={{ padding: 6, background: 'var(--app-surface-cont)', borderRadius: 8, fontSize: 20 }}>analytics</span>
                <span style={{ fontWeight: 700, fontSize: 14, color: 'var(--app-on-surface)' }}>ATS Analyses</span>
              </div>
              <span style={{ fontSize: 12, color: 'var(--app-on-surface-var)' }}>{analyses.length}/10</span>
            </div>
            <div className="progress-bar-track">
              <div className="progress-bar-fill-primary" style={{ width: `${Math.min(100, (analyses.length / 10) * 100)}%` }} />
            </div>
            <p style={{ fontSize: 13, color: 'var(--app-on-surface-var)', marginTop: 10 }}>{subscription?.planType === 'Pro' ? 'Unlimited' : '10 per month'}</p>
          </div>
        </div>

        {/* Cover Letters quota */}
        <div className="bento-card col-span-4">
          <div className="bento-card-body">
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 16 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <span className="material-symbols-outlined" style={{ padding: 6, background: 'var(--app-surface-cont)', borderRadius: 8, fontSize: 20 }}>mail</span>
                <span style={{ fontWeight: 700, fontSize: 14, color: 'var(--app-on-surface)' }}>Cover Letters</span>
              </div>
              <span style={{ fontSize: 12, color: 'var(--app-on-surface-var)' }}>{letters.length}/10</span>
            </div>
            <div className="progress-bar-track">
              <div className="progress-bar-fill-primary" style={{ width: `${Math.min(100, (letters.length / 10) * 100)}%` }} />
            </div>
            <p style={{ fontSize: 13, color: 'var(--app-on-surface-var)', marginTop: 10 }}>{subscription?.planType === 'Pro' ? 'Unlimited' : '10 per month'}</p>
          </div>
        </div>

        {/* Subscription */}
        <div className="bento-card col-span-4">
          <div className="bento-card-body" style={{ display: 'flex', flexDirection: 'column', justifyContent: 'space-between', height: '100%' }}>
            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 6 }}>
                <span className="material-symbols-outlined" style={{ fontSize: 20, color: 'var(--app-secondary)' }}>verified</span>
                <span style={{ fontWeight: 700, fontSize: 14, color: 'var(--app-on-surface)' }}>{subscription?.planType ?? 'Free'} Plan {subscription?.status ?? 'Active'}</span>
              </div>
              <p style={{ fontSize: 13, color: 'var(--app-on-surface-var)' }}>Billing cycle: {subscription?.billingCycle ?? 'Monthly'}.</p>
            </div>
            <a onClick={() => navigate('/subscription')} style={{ fontSize: 12, fontWeight: 700, color: 'var(--app-primary)', cursor: 'pointer', marginTop: 16, display: 'inline-block' }}>Manage Billing</a>
          </div>
        </div>

        {/* Recent Activity */}
        <div className="bento-card col-span-12">
          <div style={{ padding: '14px 20px', borderBottom: '1px solid var(--app-outline-var)', display: 'flex', alignItems: 'center', justifyContent: 'space-between', background: 'var(--app-surface)' }}>
            <span style={{ fontWeight: 700, fontSize: 16, color: 'var(--app-on-surface)' }}>Recent Activity</span>
            {(resumes.length > 0 || analyses.length > 0 || letters.length > 0) && (
              <a onClick={() => navigate('/ats-analysis')} style={{ fontSize: 12, color: 'var(--app-primary)', cursor: 'pointer', fontWeight: 600 }}>
                View All Activity
              </a>
            )}
          </div>
          <div className="activity-list">
            {resumes.length === 0 && analyses.length === 0 && letters.length === 0 ? (
              <div style={{ padding: '32px 20px', textAlign: 'center', color: 'var(--app-on-surface-var)' }}>
                <span className="material-symbols-outlined" style={{ fontSize: 36, color: 'var(--app-outline)', marginBottom: 8, display: 'block' }}>history</span>
                <p style={{ fontWeight: 600, fontSize: 14, color: 'var(--app-on-surface)' }}>No activity recorded yet</p>
                <p style={{ fontSize: 13, marginTop: 4 }}>Upload your resume to kickstart your AI career suite.</p>
                <button className="btn-app-primary" style={{ marginTop: 14, fontSize: 12 }} onClick={() => navigate('/resumes')}>
                  Upload Resume
                </button>
              </div>
            ) : (
              [
                ...(analyses[0] ? [{
                  icon: 'analytics',
                  title: `ATS Analysis Run for Resume`,
                  sub: `Overall match score: ${analyses[0].atsScore}%`,
                  time: analyses[0].createdAt ? new Date(analyses[0].createdAt).toLocaleDateString() : 'Recent',
                  badge: `${analyses[0].atsScore}% Match`,
                  badgeClass: analyses[0].atsScore >= 75 ? 'badge-green' : 'badge-blue',
                  onClick: () => navigate('/ats-analysis')
                }] : []),
                ...(letters[0] ? [{
                  icon: 'mail',
                  title: `Cover Letter Created for ${letters[0].jobTitle}`,
                  sub: `Company: ${letters[0].companyName} • Tone: ${letters[0].tone}`,
                  time: letters[0].createdAt ? new Date(letters[0].createdAt).toLocaleDateString() : 'Recent',
                  badge: letters[0].tone,
                  badgeClass: 'badge-gray',
                  onClick: () => navigate('/cover-letter')
                }] : []),
                ...(resumes[0] ? [{
                  icon: 'description',
                  title: `Resume Uploaded: ${resumes[0].fileName}`,
                  sub: `File size: ${Math.round(resumes[0].fileSize / 1024)} KB • Format: ${resumes[0].fileType.toUpperCase()}`,
                  time: resumes[0].uploadedAt ? new Date(resumes[0].uploadedAt).toLocaleDateString() : 'Recent',
                  badge: 'Uploaded',
                  badgeClass: 'badge-blue',
                  onClick: () => navigate('/resumes')
                }] : [])
              ].map((item, i) => (
                <div key={i} className="activity-item" onClick={item.onClick} style={{ cursor: 'pointer' }}>
                  <div className="activity-icon">
                    <span className="material-symbols-outlined" style={{ fontSize: 20 }}>{item.icon}</span>
                  </div>
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div className="activity-title">{item.title}</div>
                    <div className="activity-sub">{item.sub}</div>
                  </div>
                  <div className="activity-meta">
                    <span className="activity-time">{item.time}</span>
                    {item.badge && <span className={`badge ${item.badgeClass}`} style={{ marginTop: 4 }}>{item.badge}</span>}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

      </div>
    </AppLayout>
  )
}
