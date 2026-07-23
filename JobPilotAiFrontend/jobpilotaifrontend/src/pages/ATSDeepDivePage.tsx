import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError, formatDate } from '../api/client'
import type { AnalysisResponse } from '../api/types'
import AppLayout from '../components/AppLayout'
import '../app.css'

export default function ATSDeepDivePage() {
  const navigate = useNavigate()
  const [analysis, setAnalysis] = useState<AnalysisResponse | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    void api.listAnalyses()
      .then(items => setAnalysis(items[0] ?? null))
      .catch((err: unknown) => setError(err instanceof ApiError ? err.message : 'Unable to load analysis.'))
  }, [])

  const sections = analysis ? [
    ['Keyword Match', analysis.keywordMatchScore],
    ['Skills Coverage', analysis.skillsCoverageScore],
    ['Formatting', analysis.formattingScore],
    ['Experience', analysis.experienceScore],
    ['Education', analysis.educationScore],
  ] as const : []

  return (
    <AppLayout
      title="ATS Deep Dive"
      subtitle="Detailed scoring and improvement guidance from your latest analysis."
      actions={<button className="btn-app-secondary" onClick={() => navigate('/ats-analysis')}>Back to ATS Analysis</button>}
    >
      {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', padding: 10, marginBottom: 16 }}>{error}</div>}
      {!analysis && (
        <div className="bento-card">
          <div className="bento-card-body">
            <h3 style={{ marginTop: 0, color: 'var(--app-on-surface)' }}>No analysis available</h3>
            <p style={{ color: 'var(--app-on-surface-var)' }}>Run an ATS analysis first to unlock the deep dive.</p>
            <button className="btn-app-primary" onClick={() => navigate('/ats-analysis')}>Run Analysis</button>
          </div>
        </div>
      )}

      {analysis && (
        <div className="bento-grid">
          <div className="bento-card col-span-4">
            <div className="bento-card-body" style={{ textAlign: 'center' }}>
              <div className="job-card-match" style={{ margin: '0 auto' }}>{analysis.atsScore}%<small>Overall</small></div>
              <p style={{ color: 'var(--app-on-surface-var)' }}>Created {formatDate(analysis.createdAt)}</p>
            </div>
          </div>

          <div className="bento-card col-span-8">
            <div className="bento-card-body">
              <div style={{ fontSize: 12, fontWeight: 700, textTransform: 'uppercase', color: 'var(--app-on-surface-var)', marginBottom: 20 }}>Detailed Metric Breakdown</div>
              {sections.map(([label, score]) => {
                const weight = label === 'Keyword Match' ? '35%' : label === 'Skills Coverage' ? '25%' : label === 'Formatting' ? '15%' : label === 'Experience' ? '15%' : '10%';
                const desc = label === 'Keyword Match' ? 'Matches required keywords, phrases, and title variants from the job description.' :
                             label === 'Skills Coverage' ? 'Identifies tech stack competencies, soft skills, and tool proficiency.' :
                             label === 'Formatting' ? 'Checks table parsability, standard fonts, hierarchy, and header structures.' :
                             label === 'Experience' ? 'Scans for impact metrics, action verbs, outcome statements, and dates.' :
                             'Verifies degrees, certifications, institutions, and graduation timelines.';
                const status = score >= 75 ? { color: '#006c49', text: 'Optimal' } : score >= 50 ? { color: '#d97706', text: 'Needs Work' } : { color: '#dc2626', text: 'Critical' };
                return (
                  <div key={label} style={{ marginBottom: 20, paddingBottom: 16, borderBottom: '1px solid var(--app-outline-var)' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6, alignItems: 'center' }}>
                      <div>
                        <span style={{ fontWeight: 700, color: 'var(--app-on-surface)', fontSize: 14 }}>{label}</span>
                        <span style={{ fontSize: 11, color: 'var(--app-on-surface-var)', marginLeft: 8 }}>(Weight: {weight})</span>
                      </div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <span style={{ fontSize: 11, fontWeight: 700, color: status.color, textTransform: 'uppercase' }}>{status.text}</span>
                        <span style={{ color: 'var(--app-on-surface)', fontWeight: 700, fontSize: 14 }}>{score}%</span>
                      </div>
                    </div>
                    <div className="progress-bar-track" style={{ height: 8, background: 'var(--app-surface-low)' }}>
                      <div style={{ 
                        width: `${score}%`, 
                        height: '100%', 
                        borderRadius: 4, 
                        background: score >= 75 ? 'var(--app-secondary)' : score >= 50 ? '#f59e0b' : '#ef4444' 
                      }} />
                    </div>
                    <p style={{ margin: '6px 0 0 0', fontSize: 12, color: 'var(--app-on-surface-var)', lineHeight: 1.4 }}>{desc}</p>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="bento-card col-span-6">
            <div style={{ padding: '14px 20px', borderBottom: '1px solid var(--app-outline-var)', fontWeight: 700, color: '#dc2626', display: 'flex', alignItems: 'center', gap: 6 }}>
              <span className="material-symbols-outlined" style={{ fontSize: 18 }}>warning</span>
              Key Weaknesses
            </div>
            <div style={{ padding: 20, display: 'flex', flexDirection: 'column', gap: 10 }}>
              {analysis.weaknesses.map(item => (
                <div key={item} style={{ display: 'flex', alignItems: 'flex-start', gap: 10, padding: '10px 14px', background: 'rgba(220,38,38,0.04)', borderRadius: 8, border: '1px solid rgba(220,38,38,0.1)' }}>
                  <span className="material-symbols-outlined" style={{ fontSize: 16, color: '#dc2626', marginTop: 2 }}>error_outline</span>
                  <span style={{ fontSize: 13, color: 'var(--app-on-surface)', lineHeight: 1.4 }}>{item}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="bento-card col-span-6">
            <div style={{ padding: '14px 20px', borderBottom: '1px solid var(--app-outline-var)', fontWeight: 700, color: 'var(--app-secondary)', display: 'flex', alignItems: 'center', gap: 6 }}>
              <span className="material-symbols-outlined" style={{ fontSize: 18 }}>thumb_up</span>
              Optimization Advice
            </div>
            <div style={{ padding: 20, display: 'flex', flexDirection: 'column', gap: 10 }}>
              {analysis.recommendations.map(item => (
                <div key={item} style={{ display: 'flex', alignItems: 'flex-start', gap: 10, padding: '10px 14px', background: 'rgba(0,108,73,0.04)', borderRadius: 8, border: '1px solid rgba(0,108,73,0.1)' }}>
                  <span className="material-symbols-outlined" style={{ fontSize: 16, color: 'var(--app-secondary)', marginTop: 2 }}>trending_up</span>
                  <span style={{ fontSize: 13, color: 'var(--app-on-surface)', lineHeight: 1.4 }}>{item}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}
    </AppLayout>
  )
}
