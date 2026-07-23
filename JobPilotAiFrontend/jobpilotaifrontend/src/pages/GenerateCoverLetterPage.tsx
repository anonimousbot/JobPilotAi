import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError } from '../api/client'
import type { ResumeResponse } from '../api/types'
import AppLayout from '../components/AppLayout'
import '../app.css'

const tones = ['Professional', 'Enthusiastic', 'Concise', 'Creative']

export default function GenerateCoverLetterPage() {
  const navigate = useNavigate()
  const [resumes, setResumes] = useState<ResumeResponse[]>([])
  const [resumeId, setResumeId] = useState('')
  const [jobTitle, setJobTitle] = useState('')
  const [companyName, setCompanyName] = useState('')
  const [jobDescription, setJobDescription] = useState('')
  const [tone, setTone] = useState('Professional')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    void api.listResumes().then(items => {
      setResumes(items)
      setResumeId(items[0]?.id ?? '')
    }).catch(() => undefined)
  }, [])

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await api.createCoverLetter({
        resumeId: resumeId || null,
        jobTitle,
        companyName,
        jobDescription,
        tone,
      })
      navigate('/cover-letter')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to generate cover letter.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <AppLayout title="Generate Cover Letter" subtitle="Create a tailored draft from a role description.">
      {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', padding: 10, marginBottom: 16 }}>{error}</div>}

      <form onSubmit={handleSubmit} className="editor-layout">
        <div className="editor-panel">
          <div className="editor-panel-header"><span className="editor-panel-title">Role Details</span></div>
          <div style={{ padding: 20, display: 'flex', flexDirection: 'column', gap: 16 }}>
            <div className="app-input-group">
              <label className="app-label">Resume</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">description</span>
                <select className="app-input" value={resumeId} onChange={e => setResumeId(e.target.value)}>
                  <option value="">No resume selected</option>
                  {resumes.map(resume => <option key={resume.id} value={resume.id}>{resume.fileName}</option>)}
                </select>
              </div>
            </div>
            <div className="app-input-group">
              <label className="app-label">Job Title</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">work</span>
                <input className="app-input" value={jobTitle} onChange={e => setJobTitle(e.target.value)} required />
              </div>
            </div>
            <div className="app-input-group">
              <label className="app-label">Company</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">business</span>
                <input className="app-input" value={companyName} onChange={e => setCompanyName(e.target.value)} required />
              </div>
            </div>
            <div className="app-input-group">
              <label className="app-label">Tone</label>
              <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                {tones.map(item => (
                  <button key={item} type="button" className={tone === item ? 'btn-app-primary' : 'btn-app-secondary'} onClick={() => setTone(item)}>{item}</button>
                ))}
              </div>
            </div>
          </div>
        </div>

        <div className="editor-panel">
          <div className="editor-panel-header"><span className="editor-panel-title">Job Description</span></div>
          <textarea className="editor-textarea" value={jobDescription} onChange={e => setJobDescription(e.target.value)} required placeholder="Paste the job description here." />
          <div style={{ padding: '12px 20px', borderTop: '1px solid var(--app-outline-var)', display: 'flex', gap: 10, background: 'var(--app-surface)' }}>
            <button className="btn-app-primary" type="submit" disabled={loading}>
              <span className="material-symbols-outlined" style={{ fontSize: 16 }}>auto_awesome</span>
              {loading ? 'Generating...' : 'Generate Draft'}
            </button>
            <button type="button" className="btn-app-secondary" onClick={() => navigate('/cover-letter')}>Cancel</button>
          </div>
        </div>
      </form>
    </AppLayout>
  )
}
