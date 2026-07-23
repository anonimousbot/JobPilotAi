import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError, formatDate } from '../api/client'
import type { AnalysisResponse, ResumeResponse } from '../api/types'
import AppLayout from '../components/AppLayout'
import '../app.css'

export default function ATSAnalysisPage() {
  const navigate = useNavigate()
  const [analyses, setAnalyses] = useState<AnalysisResponse[]>([])
  const [resumes, setResumes] = useState<ResumeResponse[]>([])
  const [selectedResumeId, setSelectedResumeId] = useState<string | null>(null)
  const [jobDescription, setJobDescription] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [creating, setCreating] = useState(false)
  const [copiedKeyword, setCopiedKeyword] = useState<string | null>(null)

  const handleCopy = (word: string) => {
    void navigator.clipboard.writeText(word)
    setCopiedKeyword(word)
    setTimeout(() => setCopiedKeyword(null), 2000)
  }
  const latest = analyses[0]
  const selectedResume = resumes.find(r => r.id === selectedResumeId) || resumes[0]
  const keywords = useMemo(() => {
    if (!latest) return []
    return [
      { name: 'Keywords', pct: latest.keywordMatchScore },
      { name: 'Skills Coverage', pct: latest.skillsCoverageScore },
      { name: 'Formatting', pct: latest.formattingScore },
      { name: 'Experience', pct: latest.experienceScore },
      { name: 'Education', pct: latest.educationScore },
    ]
  }, [latest])

  useEffect(() => {
    void loadData()
  }, [])

  const loadData = async () => {
    setLoading(true)
    setError('')
    try {
      const [analysisData, resumeData] = await Promise.all([api.listAnalyses(), api.listResumes()])
      setAnalyses(analysisData)
      setResumes(resumeData)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to load ATS analysis data.')
    } finally {
      setLoading(false)
    }
  }

  const createAnalysis = async () => {
    const resume = selectedResume
    if (!resume) {
      navigate('/resumes')
      return
    }

    setCreating(true)
    setError('')
    try {
      const created = await api.createAnalysis(resume.id, jobDescription || undefined)
      setAnalyses(items => [created, ...items.filter(item => item.id !== created.id)])
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to create analysis.')
    } finally {
      setCreating(false)
    }
  }

  return (
    <AppLayout
      title="ATS Analysis"
      subtitle="Understand exactly how algorithms score your resume."
      actions={
        <button className="btn-app-primary" onClick={() => void createAnalysis()} disabled={creating}>
          <span className="material-symbols-outlined" style={{ fontSize: 18 }}>upload_file</span>
          {creating ? 'Analyzing...' : 'Analyze Latest Resume'}
        </button>
      }
    >
      {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', padding: 10, marginBottom: 16 }}>{error}</div>}
      {loading && <div className="bento-card"><div className="bento-card-body">Loading analysis...</div></div>}
      
      {/* Resume selector */}
      {!loading && resumes.length > 0 && (
        <div style={{ marginBottom: 20 }}>
          <label className="app-label" style={{ marginBottom: 8, display: 'block' }}>Select Resume to Analyze</label>
          <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
            {resumes.map(resume => (
              <button
                key={resume.id}
                onClick={() => setSelectedResumeId(resume.id)}
                style={{
                  padding: '8px 16px',
                  borderRadius: 8,
                  fontSize: 12,
                  fontWeight: 700,
                  cursor: 'pointer',
                  border: '1px solid',
                  transition: 'all 0.15s',
                  background: (selectedResumeId || resumes[0]?.id) === resume.id ? 'var(--app-primary)' : 'transparent',
                  color: (selectedResumeId || resumes[0]?.id) === resume.id ? 'white' : 'var(--app-on-surface-var)',
                  borderColor: (selectedResumeId || resumes[0]?.id) === resume.id ? 'var(--app-primary)' : 'var(--app-outline-var)',
                }}
              >
                {resume.fileName}
              </button>
            ))}
          </div>
        </div>
      )}

      {!loading && !latest && (
        <div className="bento-card" style={{ marginBottom: 20 }}>
          <div className="bento-card-body">
            <h3 style={{ marginTop: 0, color: 'var(--app-on-surface)' }}>No ATS analysis yet</h3>
            <p style={{ color: 'var(--app-on-surface-var)' }}>Upload a resume, then run your first analysis from this page.</p>
            <button className="btn-app-primary" onClick={() => navigate('/resumes')}>Upload Resume</button>
          </div>
        </div>
      )}

      {/* Clipboard notification toast */}
      {copiedKeyword && (
        <div style={{
          position: 'fixed',
          bottom: 24,
          right: 24,
          background: 'var(--app-primary-cont)',
          color: 'var(--app-on-primary-fixed)',
          padding: '12px 24px',
          borderRadius: 8,
          boxShadow: '0 8px 30px rgba(0,0,0,0.15)',
          zIndex: 100,
          display: 'flex',
          alignItems: 'center',
          gap: 8,
          animation: 'slideUp 0.2s ease'
        }}>
          <span className="material-symbols-outlined" style={{ color: 'var(--color-primary)' }}>check_circle</span>
          <span style={{ fontSize: 14, fontWeight: 700 }}>Copied "{copiedKeyword}" to clipboard!</span>
        </div>
      )}

      <div className="bento-card" style={{ marginBottom: 20 }}>
        <div className="bento-card-body">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
            <label className="app-label" style={{ marginBottom: 0 }}>Target Job Description (Optional)</label>
            <span style={{ fontSize: 12, color: 'var(--app-on-surface-var)', fontStyle: 'italic' }}>
              {!jobDescription ? 'Comparing against generic ATS standard parser rules' : 'Tailoring analysis to job description'}
            </span>
          </div>
          <textarea 
            className="editor-textarea" 
            style={{ minHeight: 100 }} 
            value={jobDescription} 
            onChange={e => setJobDescription(e.target.value)} 
            placeholder="Paste the job description here. If left empty, the analyzer evaluates your resume against standard industry ATS conventions (formatting, structure, etc.)." 
          />
        </div>
      </div>

      {/* ATS Scoring Guide & Formulas */}
      <div className="bento-card" style={{ marginBottom: 20, background: 'rgba(0,108,73,0.04)', borderColor: 'rgba(0,108,73,0.15)' }}>
        <div className="bento-card-body">
          <h4 style={{ margin: '0 0 8px 0', color: 'var(--app-secondary)', display: 'flex', alignItems: 'center', gap: 8 }}>
            <span className="material-symbols-outlined">info</span>
            Understanding the ATS Scoring System
          </h4>
          <p style={{ margin: 0, fontSize: 13, color: 'var(--app-on-surface-var)', lineHeight: 1.5 }}>
            Applicant Tracking Systems use weighted algorithms to score resumes. Your <strong>Overall ATS Match Score</strong> is computed as:
          </p>
          <div style={{ 
            background: 'var(--app-surface-lowest)', 
            padding: '12px 16px', 
            borderRadius: 8, 
            marginTop: 10, 
            fontFamily: 'monospace', 
            fontSize: 13, 
            color: 'var(--app-on-surface)',
            border: '1px solid var(--app-outline-var)',
            display: 'flex',
            flexWrap: 'wrap',
            gap: 12,
            alignItems: 'center'
          }}>
            <span style={{ fontWeight: 700, color: 'var(--app-secondary)' }}>Overall Score =</span>
            <span>(Keyword Match × 35%)</span> +
            <span>(Skills Coverage × 25%)</span> +
            <span>(Formatting × 15%)</span> +
            <span>(Experience × 15%)</span> +
            <span>(Education × 10%)</span>
          </div>
        </div>
      </div>

      <div className="bento-grid">

        {/* Score overview */}
        <div className="bento-card col-span-4">
          <div className="bento-card-body" style={{ textAlign: 'center' }}>
            <div style={{ fontSize: 11, fontWeight: 700, letterSpacing: '0.1em', textTransform: 'uppercase', color: 'var(--app-on-surface-var)', marginBottom: 20 }}>Overall ATS Match Score</div>
            <div className="circular-progress-wrap" style={{ margin: '0 auto 16px' }}>
              <svg width="128" height="128" style={{ transform: 'rotate(-90deg)' }} viewBox="0 0 100 100">
                <circle cx="50" cy="50" r="40" fill="transparent" stroke="var(--app-surface-low)" strokeWidth="9" />
                <circle cx="50" cy="50" r="40" fill="transparent" stroke="var(--app-secondary)" strokeWidth="9" strokeDasharray="251.2" strokeDashoffset={latest ? 251.2 - (latest.atsScore / 100) * 251.2 : 251.2} strokeLinecap="round" />
              </svg>
              <div className="circular-progress-center">
                <span className="circular-progress-value" style={{ color: 'var(--app-secondary)' }}>{latest?.atsScore ?? 0}</span>
                <span className="circular-progress-sub">/ 100</span>
              </div>
            </div>
            
            {latest ? (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 8, alignItems: 'center' }}>
                <div className={`badge ${latest.atsScore >= 75 ? 'badge-green' : latest.atsScore >= 50 ? 'badge-orange' : 'badge-red'}`} style={{ padding: '6px 16px', fontSize: 13, fontWeight: 700 }}>
                  {latest.atsScore >= 75 ? 'Optimal Match' : latest.atsScore >= 50 ? 'Needs Improvement' : 'Critical Issues'}
                </div>
                <span style={{ fontSize: 11, color: 'var(--app-on-surface-var)' }}>Analyzed {formatDate(latest.createdAt)}</span>
              </div>
            ) : (
              <div className="badge badge-gray">No score yet</div>
            )}
            
            <p style={{ fontSize: 13, color: 'var(--app-on-surface-var)', marginTop: 14, lineHeight: 1.5 }}>
              {latest?.recommendations[0] ?? 'Run an analysis to see your personalized recommendations.'}
            </p>
            <button onClick={() => navigate('/ats-deep-dive')} className="btn-app-secondary" style={{ width: '100%', justifyContent: 'center', marginTop: 16 }}>
              <span className="material-symbols-outlined" style={{ fontSize: 16 }}>open_in_new</span>
              Deep Dive Analysis
            </button>
          </div>
        </div>

        {/* Keyword scores breakdown with descriptions */}
        <div className="bento-card col-span-8">
          <div className="bento-card-body">
            <div style={{ fontSize: 11, fontWeight: 700, letterSpacing: '0.1em', textTransform: 'uppercase', color: 'var(--app-on-surface-var)', marginBottom: 20 }}>Weighted Breakdown & Sub-scores</div>
            <div className="ats-keyword-list">
              {[
                { name: 'Keyword Match', pct: latest?.keywordMatchScore ?? 0, weight: '35%', desc: 'Matches required keywords, phrases, and title variants from the job description.' },
                { name: 'Skills Coverage', pct: latest?.skillsCoverageScore ?? 0, weight: '25%', desc: 'Identifies tech stack competencies, soft skills, and tool proficiency.' },
                { name: 'Formatting', pct: latest?.formattingScore ?? 0, weight: '15%', desc: 'Checks table parsability, standard fonts, hierarchy, and header structures.' },
                { name: 'Experience', pct: latest?.experienceScore ?? 0, weight: '15%', desc: 'Scans for impact metrics, action verbs, outcome statements, and dates.' },
                { name: 'Education', pct: latest?.educationScore ?? 0, weight: '10%', desc: 'Verifies degrees, certifications, institutions, and graduation timelines.' }
              ].map(k => {
                const status = k.pct >= 75 ? { color: '#006c49', text: 'Optimal' } : k.pct >= 50 ? { color: '#d97706', text: 'Needs Work' } : { color: '#dc2626', text: 'Critical' };
                return (
                  <div key={k.name} style={{ display: 'flex', flexDirection: 'column', gap: 4, paddingBottom: 16, borderBottom: '1px solid var(--app-outline-var)', marginBottom: 12 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <div>
                        <span style={{ fontWeight: 700, color: 'var(--app-on-surface)', fontSize: 14 }}>{k.name}</span>
                        <span style={{ fontSize: 11, color: 'var(--app-on-surface-var)', marginLeft: 8 }}>(Weight: {k.weight})</span>
                      </div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <span style={{ fontSize: 11, fontWeight: 700, color: status.color, textTransform: 'uppercase' }}>{status.text}</span>
                        <span style={{ fontWeight: 700, color: 'var(--app-on-surface)', fontSize: 14 }}>{k.pct}%</span>
                      </div>
                    </div>
                    <div className="ats-keyword-track" style={{ height: 8 }}>
                      <div className="ats-keyword-fill" style={{ width: `${k.pct}%`, background: k.pct >= 75 ? 'var(--app-secondary)' : k.pct >= 50 ? '#f59e0b' : '#ef4444' }} />
                    </div>
                    <span style={{ fontSize: 12, color: 'var(--app-on-surface-var)', lineHeight: 1.4 }}>{k.desc}</span>
                  </div>
                );
              })}
            </div>
          </div>
        </div>

        {/* Missing Keywords list with copy helper */}
        <div className="bento-card col-span-6">
          <div style={{ padding: '14px 20px', borderBottom: '1px solid var(--app-outline-var)', fontWeight: 700, fontSize: 14, color: 'var(--app-on-surface)', display: 'flex', alignItems: 'center', gap: 8 }}>
            <span className="material-symbols-outlined" style={{ fontSize: 18, color: '#dc2626' }}>warning</span>
            Missing Target Keywords (Click to Copy)
          </div>
          <div style={{ padding: '16px 20px' }}>
            <p style={{ fontSize: 12, color: 'var(--app-on-surface-var)', margin: '0 0 12px 0', lineHeight: 1.4 }}>
              These terms were found in the job description but are missing from your resume. Incorporate them contextually to improve your Keyword Match.
            </p>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
              {latest?.missingKeywords.length ? (
                latest.missingKeywords.map(k => (
                  <button 
                    key={k} 
                    onClick={() => handleCopy(k)}
                    className="job-tag"
                    style={{ 
                      display: 'flex', 
                      alignItems: 'center', 
                      gap: 6, 
                      cursor: 'pointer',
                      border: '1px dashed var(--app-outline-var)',
                      background: 'rgba(239, 68, 68, 0.04)',
                      transition: 'all 0.15s'
                    }}
                    title="Click to copy keyword"
                  >
                    <span className="material-symbols-outlined" style={{ fontSize: 13, color: 'var(--app-on-surface-var)' }}>content_copy</span>
                    <span>{k}</span>
                  </button>
                ))
              ) : (
                <div style={{ padding: '12px', background: 'var(--app-surface-low)', borderRadius: 8, width: '100%', fontSize: 13, color: 'var(--app-on-surface-var)' }}>
                  No missing keywords identified. Your resume matches the job description perfectly!
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Strengths */}
        <div className="bento-card col-span-6">
          <div style={{ padding: '14px 20px', borderBottom: '1px solid var(--app-outline-var)', fontWeight: 700, fontSize: 14, color: 'var(--app-on-surface)', display: 'flex', alignItems: 'center', gap: 8 }}>
            <span className="material-symbols-outlined" style={{ fontSize: 18, color: 'var(--app-secondary)' }}>check_circle</span>
            Resume Strengths
          </div>
          <div style={{ padding: '16px 20px', display: 'flex', flexDirection: 'column', gap: 10 }}>
            {(latest?.strengths.length ? latest.strengths : ['Upload a resume and run analysis to scan for strengths.']).map(k => (
              <div key={k} style={{ display: 'flex', alignItems: 'center', gap: 10, padding: '10px 14px', background: 'rgba(0,108,73,0.06)', borderRadius: 8, border: '1px solid rgba(0,108,73,0.15)' }}>
                <span className="material-symbols-outlined" style={{ fontSize: 16, color: 'var(--app-secondary)' }}>check</span>
                <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--app-on-surface)' }}>{k}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Recommendations */}
        <div className="bento-card col-span-12">
          <div style={{ padding: '14px 20px', borderBottom: '1px solid var(--app-outline-var)', fontWeight: 700, fontSize: 15, color: 'var(--app-on-surface)' }}>Step-by-Step Actionable Advice</div>
          <div style={{ padding: '20px', display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 16 }}>
            {(latest?.recommendations.length ? latest.recommendations : ['Upload a resume and create an analysis to unlock AI recommendations.']).map((recommendation, index) => (
              <div key={recommendation} style={{ padding: 16, background: 'var(--app-surface)', borderRadius: 12, border: '1px solid var(--app-outline-var)', display: 'flex', gap: 14 }}>
                <div style={{ width: 40, height: 40, borderRadius: 10, background: 'rgba(99,102,241,0.1)', display: 'flex', alignItems: 'center', justifycontent: 'center', color: '#6366f1', flexShrink: 0 }}>
                  <span className="material-symbols-outlined" style={{ fontSize: 20 }}>auto_awesome</span>
                </div>
                <div>
                  <div style={{ fontWeight: 700, fontSize: 14, color: 'var(--app-on-surface)', marginBottom: 4 }}>Action {index + 1}</div>
                  <div style={{ fontSize: 13, color: 'var(--app-on-surface-var)', lineHeight: 1.5 }}>{recommendation}</div>
                </div>
              </div>
            ))}
          </div>
        </div>

      </div>
    </AppLayout>
  )
}
