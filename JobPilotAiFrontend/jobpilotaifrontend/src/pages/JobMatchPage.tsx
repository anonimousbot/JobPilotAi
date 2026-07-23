import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError } from '../api/client'
import type { JobMatchComparisonResponse, JobMatchResponse } from '../api/types'
import AppLayout from '../components/AppLayout'
import '../app.css'

export default function JobMatchPage() {
  const navigate = useNavigate()
  const [filter, setFilter] = useState('All')
  const [jobs, setJobs] = useState<JobMatchResponse[]>([])
  const [comparison, setComparison] = useState<JobMatchComparisonResponse | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    void loadJobs(filter)
  }, [filter])

  const loadJobs = async (nextFilter: string) => {
    setError('')
    try {
      if (nextFilter === 'Saved') {
        setJobs(await api.getJobMatches())
      } else {
        setJobs(await api.searchJobMatches(undefined, nextFilter === 'Remote' ? 'Remote' : undefined))
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to load job matches.')
    }
  }

  const toggleSave = async (job: JobMatchResponse) => {
    setError('')
    try {
      if (job.saved) {
        await api.unsaveJobMatch(job.id)
      } else {
        await api.saveJobMatch(job.id)
      }
      setJobs(items => items.map(item => item.id === job.id ? { ...item, saved: !job.saved } : item))
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to update saved job.')
    }
  }

  const compare = async (id: string) => {
    setError('')
    try {
      setComparison(await api.compareJobMatch(id))
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to compare this job.')
    }
  }

  return (
    <AppLayout
      title="Job Match Explorer"
      subtitle="AI-matched opportunities aligned with your profile."
      actions={
        <div style={{ display: 'flex', gap: 8 }}>
          {['All', 'Saved', 'Remote'].map(f => (
            <button key={f} onClick={() => setFilter(f)}
              style={{ padding: '7px 16px', borderRadius: 8, fontSize: 12, fontWeight: 700, cursor: 'pointer', border: '1px solid', transition: 'all 0.15s',
                background: filter === f ? 'var(--app-primary)' : 'transparent',
                color: filter === f ? 'white' : 'var(--app-on-surface-var)',
                borderColor: filter === f ? 'var(--app-primary)' : 'var(--app-outline-var)' }}
            >{f}</button>
          ))}
        </div>
      }
    >
      {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', padding: 10, marginBottom: 16 }}>{error}</div>}

      {comparison && (
        <div className="bento-card" style={{ marginBottom: 20 }}>
          <div className="bento-card-body">
            <div style={{ display: 'flex', justifyContent: 'space-between', gap: 16 }}>
              <div>
                <h3 style={{ color: 'var(--app-on-surface)', marginTop: 0 }}>{comparison.jobTitle} at {comparison.companyName}</h3>
                <p style={{ color: 'var(--app-on-surface-var)' }}>{comparison.insight}</p>
              </div>
              <div className="job-card-match">{comparison.matchScore}%<small>Match</small></div>
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginTop: 12 }}>
              <div style={{ color: 'var(--app-on-surface-var)' }}><strong>Matched:</strong> {comparison.matchedSkills.join(', ')}</div>
              <div style={{ color: 'var(--app-on-surface-var)' }}><strong>Missing:</strong> {comparison.missingSkills.join(', ')}</div>
            </div>
          </div>
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 16, marginBottom: 28 }}>
        {[
          { label: 'Total Matches', value: String(jobs.length), icon: 'hub' },
          { label: 'High Match (>80%)', value: String(jobs.filter(job => job.matchScore > 80).length), icon: 'star' },
          { label: 'Saved', value: String(jobs.filter(job => job.saved).length), icon: 'bookmark' },
        ].map(s => (
          <div key={s.label} className="stat-card" style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            <div style={{ width: 44, height: 44, borderRadius: 10, background: 'var(--app-surface-cont)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--app-on-surface)' }}>
              <span className="material-symbols-outlined" style={{ fontSize: 22 }}>{s.icon}</span>
            </div>
            <div>
              <div className="stat-card-value">{s.value}</div>
              <div className="stat-card-sub">{s.label}</div>
            </div>
          </div>
        ))}
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: 16 }}>
        {jobs.map(job => (
          <div key={job.id} className="job-card">
            <div className="job-card-header">
              <div style={{ flex: 1 }}>
                <div className="job-card-company">{job.companyName}</div>
                <div className="job-card-title">{job.jobTitle}</div>
                <div className="job-card-location">
                  <span className="material-symbols-outlined" style={{ fontSize: 14, verticalAlign: 'middle', marginRight: 4 }}>location_on</span>
                  {job.location}
                </div>
              </div>
              <div className="job-card-match">{job.matchScore}%<small>Match</small></div>
            </div>
            <p style={{ fontSize: 13, color: 'var(--app-on-surface-var)', lineHeight: 1.5 }}>{job.summary}</p>
            <div className="job-card-tags">
              {job.tags.map(t => <span key={t} className="job-tag">{t}</span>)}
              {job.saved && <span className="job-tag" style={{ background: 'var(--app-secondary-cont)' }}>Saved</span>}
            </div>
            <div style={{ display: 'flex', gap: 8 }}>
              <button className="btn-app-primary" style={{ flex: 1, justifyContent: 'center', fontSize: 12 }} onClick={() => navigate('/generate-cover-letter')}>
                <span className="material-symbols-outlined" style={{ fontSize: 15 }}>mail</span>
                Generate Cover Letter
              </button>
              <button className="btn-app-secondary" style={{ fontSize: 12, padding: '8px 12px' }} onClick={() => void compare(job.id)}>
                <span className="material-symbols-outlined" style={{ fontSize: 15 }}>analytics</span>
              </button>
              <button className="btn-app-secondary" style={{ fontSize: 12, padding: '8px 12px' }} onClick={() => void toggleSave(job)}>
                <span className="material-symbols-outlined" style={{ fontSize: 15 }}>{job.saved ? 'bookmark_remove' : 'bookmark_add'}</span>
              </button>
            </div>
          </div>
        ))}
      </div>
    </AppLayout>
  )
}
