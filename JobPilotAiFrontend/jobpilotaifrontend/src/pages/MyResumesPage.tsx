import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError, formatDate, formatFileSize } from '../api/client'
import type { AnalysisResponse, ResumeResponse } from '../api/types'
import AppLayout from '../components/AppLayout'
import '../app.css'

export default function MyResumesPage() {
  const navigate = useNavigate()
  const [resumes, setResumes] = useState<ResumeResponse[]>([])
  const [analyses, setAnalyses] = useState<AnalysisResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    void loadResumes()
  }, [])

  const loadResumes = async () => {
    setLoading(true)
    setError('')
    try {
      const [resumeData, analysisData] = await Promise.all([api.listResumes(), api.listAnalyses()])
      setResumes(resumeData)
      setAnalyses(analysisData)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to load resumes.')
    } finally {
      setLoading(false)
    }
  }

  const deleteResume = async (id: string) => {
    setError('')
    try {
      await api.deleteResume(id)
      setResumes(items => items.filter(item => item.id !== id))
      setAnalyses(items => items.filter(item => item.resumeId !== id))
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to delete resume.')
    }
  }

  const getAnalysisForResume = (resumeId: string) => {
    return analyses.find(a => a.resumeId === resumeId)
  }

  return (
    <AppLayout
      title="My Resumes"
      subtitle="Manage and optimize your resume portfolio."
      actions={
        <button className="btn-app-primary" onClick={() => navigate('/onboarding/resume')}>
          <span className="material-symbols-outlined" style={{ fontSize: 18 }}>upload</span>
          Upload New Resume
        </button>
      }
    >
      {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', padding: 10, marginBottom: 16 }}>{error}</div>}
      {loading && <div className="bento-card"><div className="bento-card-body">Loading resumes...</div></div>}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: 20 }}>
        {!loading && resumes.map(r => {
          const analysis = getAnalysisForResume(r.id)
          const hasAnalysis = Boolean(analysis)
          const atsScore = analysis?.atsScore ?? 0
          
          return (
            <div key={r.id} className="resume-card">
              <div className="resume-card-header">
                <div className="resume-card-icon">
                  <span className="material-symbols-outlined">description</span>
                </div>
                <span className="badge badge-blue">{r.fileType.toUpperCase()}</span>
              </div>
              <div>
                <div className="resume-card-name">{r.fileName}</div>
                <div className="resume-card-meta">Uploaded {formatDate(r.uploadedAt)} • {formatFileSize(r.fileSize)}</div>
              </div>
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                  <span style={{ fontSize: 12, fontWeight: 700, color: 'var(--app-on-surface-var)', letterSpacing: '0.06em', textTransform: 'uppercase' }}>Analysis Status</span>
                  <span style={{ fontSize: 13, fontWeight: 700, color: hasAnalysis ? 'var(--app-secondary)' : 'var(--app-on-surface-var)' }}>
                    {hasAnalysis ? `${atsScore}% Score` : 'Not Analyzed'}
                  </span>
                </div>
                <div className="progress-bar-track">
                  <div className="progress-bar-fill-primary" style={{ width: hasAnalysis ? `${atsScore}%` : '0%', background: hasAnalysis ? atsScore >= 75 ? 'var(--app-secondary)' : atsScore >= 50 ? 'var(--app-primary)' : '#f59e0b' : 'var(--app-surface-low)' }} />
                </div>
              </div>
              <div className="resume-card-actions">
                <button className="btn-app-primary" style={{ flex: 1, justifyContent: 'center', fontSize: 12 }} onClick={() => navigate('/ats-analysis')}>
                  <span className="material-symbols-outlined" style={{ fontSize: 16 }}>analytics</span> Analyze
                </button>
                <button className="btn-app-secondary" style={{ flex: 1, justifyContent: 'center', fontSize: 12 }} onClick={() => void deleteResume(r.id)}>
                  <span className="material-symbols-outlined" style={{ fontSize: 16 }}>delete</span> Delete
                </button>
              </div>
            </div>
          )
        })}

        {/* Upload new card */}
        <div
          onClick={() => navigate('/onboarding/resume')}
          style={{ border: '2px dashed var(--app-outline-var)', borderRadius: 14, padding: 32, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 12, cursor: 'pointer', transition: 'border-color 0.2s, background 0.2s', minHeight: 220 }}
          onMouseOver={e => (e.currentTarget.style.borderColor = 'var(--app-secondary)')}
          onMouseOut={e => (e.currentTarget.style.borderColor = 'var(--app-outline-var)')}
        >
          <div style={{ width: 48, height: 48, background: 'var(--app-surface-cont)', borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--app-on-surface-var)' }}>
            <span className="material-symbols-outlined" style={{ fontSize: 24 }}>add</span>
          </div>
          <div style={{ fontWeight: 700, fontSize: 15, color: 'var(--app-on-surface)' }}>Add New Resume</div>
          <div style={{ fontSize: 13, color: 'var(--app-on-surface-var)', textAlign: 'center' }}>Upload a PDF or DOCX to get started</div>
        </div>
      </div>
    </AppLayout>
  )
}
