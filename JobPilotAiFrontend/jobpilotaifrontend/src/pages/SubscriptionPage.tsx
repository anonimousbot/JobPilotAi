import { useEffect, useState } from 'react'
import { api, ApiError, formatDate } from '../api/client'
import type { SubscriptionResponse } from '../api/types'
import AppLayout from '../components/AppLayout'
import '../app.css'

const plans = [
  {
    name: 'Free', price: '$0', period: '/month',
    features: ['10 ATS analyses per month', '10 cover letters per month', 'Basic job matching'],
  },
  {
    name: 'Premium', price: '$19', period: '/month',
    features: ['Unlimited ATS analyses', 'Unlimited cover letters', 'Advanced matching', 'Priority AI processing'],
  },
  {
    name: 'Enterprise', price: '$49', period: '/month',
    features: ['Unlimited analyses', 'Unlimited cover letters', 'Team-ready workflows', 'Dedicated support'],
  },
]

export default function SubscriptionPage() {
  const [subscription, setSubscription] = useState<SubscriptionResponse | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    void loadSubscription()
  }, [])

  const loadSubscription = async () => {
    setError('')
    try {
      setSubscription(await api.getCurrentSubscription())
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to load subscription.')
    }
  }

  const upgrade = async () => {
    setError('')
    try {
      setSubscription(await api.upgradeSubscription())
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to upgrade subscription.')
    }
  }

  const cancel = async () => {
    setError('')
    try {
      setSubscription(await api.cancelSubscription())
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to cancel subscription.')
    }
  }

  return (
    <AppLayout title="Subscription" subtitle="Manage your plan and billing details.">
      {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', padding: 10, marginBottom: 16 }}>{error}</div>}

      <div style={{ background: 'linear-gradient(135deg, var(--app-primary-cont), #1e2a45)', borderRadius: 16, padding: 28, marginBottom: 32, display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 16 }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 6 }}>
            <span className="material-symbols-outlined" style={{ color: 'var(--app-secondary)', fontSize: 22, fontVariationSettings: "'FILL' 1" }}>verified</span>
            <span style={{ fontFamily: 'Hanken Grotesk', fontWeight: 700, fontSize: 18, color: 'white' }}>
              {subscription?.planType ?? 'Loading'} Plan - {subscription?.status ?? 'Checking'}
            </span>
          </div>
          <p style={{ fontSize: 14, color: 'rgba(255,255,255,0.6)', lineHeight: 1.5 }}>
            {subscription ? `${subscription.billingCycle} cycle started ${formatDate(subscription.startDate)}` : 'Fetching plan details...'}
          </p>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button className="btn-app-secondary" onClick={() => void cancel()} style={{ color: 'rgba(255,255,255,0.8)', borderColor: 'rgba(255,255,255,0.2)', fontSize: 13 }}>
            Cancel
          </button>
          <button onClick={() => void upgrade()} style={{ padding: '8px 18px', background: 'var(--app-secondary)', color: 'white', border: 'none', borderRadius: 10, fontSize: 13, fontWeight: 700, cursor: 'pointer' }}>
            Upgrade
          </button>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 20, marginBottom: 40 }}>
        {plans.map(plan => {
          const current = subscription?.planType === plan.name
          return (
            <div key={plan.name} className={`plan-card${current ? ' featured' : ''}`}>
              {current && (
                <div style={{ display: 'inline-block', padding: '3px 12px', background: 'var(--app-secondary)', color: 'white', borderRadius: 999, fontSize: 11, fontWeight: 700, letterSpacing: '0.06em', textTransform: 'uppercase', marginBottom: 14 }}>
                  Current Plan
                </div>
              )}
              <div className="plan-card-name">{plan.name}</div>
              <div style={{ margin: '10px 0 20px' }}>
                <span className="plan-card-price">{plan.price}</span>
                <span className="plan-card-period">{plan.period}</span>
              </div>
              <div style={{ display: 'flex', flexDirection: 'column', marginBottom: 24 }}>
                {plan.features.map(feature => (
                  <div key={feature} className="plan-card-feature">
                    <span className="material-symbols-outlined" style={{ fontSize: 18, color: 'var(--app-secondary)', fontVariationSettings: "'FILL' 1" }}>check_circle</span>
                    <span style={{ color: 'var(--app-on-surface)', fontSize: 14 }}>{feature}</span>
                  </div>
                ))}
              </div>
              <button
                className={current ? 'btn-app-secondary' : 'btn-app-primary'}
                style={{ width: '100%', justifyContent: 'center', padding: '12px', fontSize: 14, background: current ? 'transparent' : undefined }}
                onClick={() => plan.name === 'Free' ? void cancel() : void upgrade()}
                disabled={current}
              >
                {current ? 'Current Plan' : plan.name === 'Free' ? 'Downgrade' : 'Upgrade'}
              </button>
            </div>
          )
        })}
      </div>

      <div className="bento-card">
        <div style={{ padding: '16px 24px', borderBottom: '1px solid var(--app-outline-var)', fontWeight: 700, fontSize: 15, color: 'var(--app-on-surface)' }}>Production Billing Note</div>
        <div style={{ padding: '24px', color: 'var(--app-on-surface-var)', lineHeight: 1.6 }}>
          Plan state is connected to the backend subscription API. Payment provider checkout can be attached behind the Upgrade action when billing keys are configured.
        </div>
      </div>
    </AppLayout>
  )
}
