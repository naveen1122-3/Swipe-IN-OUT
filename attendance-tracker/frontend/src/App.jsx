import { useCallback, useEffect, useMemo, useState } from 'react'
import './App.css'

const API_BASE = import.meta.env.VITE_API_BASE_URL || ''

async function apiRequest(path, options = {}) {
  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      ...(options.body ? { 'Content-Type': 'application/json' } : {}),
      ...options.headers,
    },
  })

  const body = await response.text()
  let result
  try {
    result = body ? JSON.parse(body) : null
  } catch {
    result = body
  }

  if (!response.ok) {
    const message = typeof result === 'string'
      ? result
      : result?.title || result?.message || `Request failed (${response.status})`
    throw new Error(message)
  }

  return result
}

function readSavedIdentity() {
  try {
    const saved = localStorage.getItem('attendance-identity')
    return saved ? JSON.parse(saved) : null
  } catch {
    return null
  }
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleString([], {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  })
}

function formatHours(value) {
  return `${Number(value || 0).toFixed(2)} hrs`
}

function statusLabel(status) {
  return {
    AUTO_APPROVED: 'Auto-approved',
    PENDING_MANAGER_REVIEW: 'Needs review',
    MANUALLY_APPROVED: 'Approved',
    MANUALLY_REJECTED: 'Declined',
  }[status] || status?.replaceAll('_', ' ') || 'Unknown'
}

function StatusBadge({ status }) {
  return <span className={`status-badge status-${status?.toLowerCase()}`}>{statusLabel(status)}</span>
}

function LoginScreen({ onLogin }) {
  const [role, setRole] = useState('employee')
  const [ids, setIds] = useState({ employeeId: '', managerId: '', projectId: '' })
  const [error, setError] = useState('')

  function submit(event) {
    event.preventDefault()
    const identity = {
      role,
      employeeId: Number(ids.employeeId),
      managerId: Number(ids.managerId),
      projectId: Number(ids.projectId),
    }

    if (role === 'employee'
      && (!identity.employeeId || !identity.managerId || !identity.projectId)) {
      setError('Enter your employee ID, assigned manager ID, and project ID.')
      return
    }

    if (role === 'manager' && !identity.managerId) {
      setError('Enter your manager ID to continue.')
      return
    }

    onLogin(identity)
  }

  return (
    <main className="login-page">
      <section className="login-card">
        <div className="brand-mark">S</div>
        <p className="eyebrow">ATTENDANCE, MADE CLEAR</p>
        <h1>Welcome to Swipe</h1>
        <p className="muted login-intro">Choose your workspace to continue.</p>

        <div className="role-switch" aria-label="Choose account type">
          <button
            className={role === 'employee' ? 'selected' : ''}
            onClick={() => { setRole('employee'); setError('') }}
            type="button"
          >
            Employee
          </button>
          <button
            className={role === 'manager' ? 'selected' : ''}
            onClick={() => { setRole('manager'); setError('') }}
            type="button"
          >
            Manager
          </button>
        </div>

        <form className="form-stack" onSubmit={submit}>
          {role === 'employee' ? (
            <label>
              Employee ID
              <input
                autoFocus
                min="1"
                onChange={(event) => setIds({ ...ids, employeeId: event.target.value })}
                placeholder="e.g. 1042"
                required
                type="number"
                value={ids.employeeId}
              />
            </label>
          ) : (
            <label>
              Manager ID
              <input
                autoFocus
                min="1"
                onChange={(event) => setIds({ ...ids, managerId: event.target.value })}
                placeholder="e.g. 204"
                required
                type="number"
                value={ids.managerId}
              />
            </label>
          )}
          {role === 'employee' && (
            <label>
              Manager ID
              <input
                min="1"
                onChange={(event) => setIds({ ...ids, managerId: event.target.value })}
                placeholder="Your assigned manager"
                required
                type="number"
                value={ids.managerId}
              />
            </label>
          )}
          <label>
            Project ID <span className="optional-label">Required for employee view</span>
            <input
              min="1"
              onChange={(event) => setIds({ ...ids, projectId: event.target.value })}
              placeholder={role === 'manager' ? 'Optional — show all projects' : 'Your assigned project'}
              required={role === 'employee'}
              type="number"
              value={ids.projectId}
            />
          </label>
          {error && <p className="form-error" role="alert">{error}</p>}
          <button className="button button-primary button-wide" type="submit">
            Open {role === 'employee' ? 'employee' : 'manager'} workspace <span aria-hidden="true">→</span>
          </button>
        </form>
        <p className="login-note">Demo workspace selector. Sign-in IDs are not verified credentials.</p>
      </section>
    </main>
  )
}

function AppShell({ identity, onLogout, children }) {
  return (
    <div className="app-shell">
      <header className="topbar">
        <a className="wordmark" href="#" onClick={(event) => event.preventDefault()}>
          <span className="brand-mark brand-mark-small">S</span>
          <span>swipe<span className="wordmark-dot">.</span></span>
        </a>
        <div className="topbar-right">
          <span className="role-pill">{identity.role}</span>
          <span className="identity-label">
            {identity.role === 'employee'
              ? `Employee #${identity.employeeId}`
              : `Manager #${identity.managerId}`}
          </span>
          <button className="button button-quiet" onClick={onLogout} type="button">Switch user</button>
        </div>
      </header>
      <main className="dashboard">{children}</main>
      <footer className="app-footer">Swipe attendance <span>·</span> Local workspace</footer>
    </div>
  )
}

function EmployeeWorkspace({ identity }) {
  const [attendance, setAttendance] = useState(null)
  const [requests, setRequests] = useState([])
  const [swipeBusy, setSwipeBusy] = useState(false)
  const [submitBusy, setSubmitBusy] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [topUpStart, setTopUpStart] = useState('')
  const [topUpEnd, setTopUpEnd] = useState('')

  const refresh = useCallback(async () => {
    try {
      const query = new URLSearchParams({
        employeeId: identity.employeeId,
        managerId: identity.managerId,
        projectId: identity.projectId,
      })
      const [status, history] = await Promise.all([
        apiRequest(`/api/attendance/status?${query}`),
        apiRequest(`/api/topup/employee/${identity.employeeId}?managerId=${identity.managerId}&projectId=${identity.projectId}`),
      ])
      setError('')
      setAttendance(status)
      setRequests(history)
    } catch (requestError) {
      setError(requestError.message)
    }
  }, [identity.employeeId, identity.managerId, identity.projectId])

  useEffect(() => {
    let active = true
    async function loadWorkspace() {
      try {
        const query = new URLSearchParams({
          employeeId: identity.employeeId,
          managerId: identity.managerId,
          projectId: identity.projectId,
        })
        const [status, history] = await Promise.all([
          apiRequest(`/api/attendance/status?${query}`),
          apiRequest(`/api/topup/employee/${identity.employeeId}?managerId=${identity.managerId}&projectId=${identity.projectId}`),
        ])
        if (active) {
          setError('')
          setAttendance(status)
          setRequests(history)
        }
      } catch (requestError) {
        if (active) setError(requestError.message)
      }
    }

    loadWorkspace()
    return () => { active = false }
  }, [identity.employeeId, identity.managerId, identity.projectId])

  async function recordSwipe(swipeType) {
    setSwipeBusy(true)
    setError('')
    setNotice('')
    try {
      const result = await apiRequest('/api/attendance/swipe', {
        method: 'POST',
        body: JSON.stringify({
          employeeId: identity.employeeId,
          managerId: identity.managerId,
          projectId: identity.projectId,
          swipeTime: new Date().toISOString(),
          swipeType,
        }),
      })
      setNotice(swipeType === 'IN'
        ? 'Swipe-in recorded. Remember to swipe out when you leave.'
        : `${formatHours(result.totalOfficeHours)} recorded for today.`)
      await refresh()
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setSwipeBusy(false)
    }
  }

  async function submitTopUp(event) {
    event.preventDefault()
    setSubmitBusy(true)
    setError('')
    setNotice('')
    try {
      const [result] = await apiRequest('/api/topup/approve', {
        method: 'POST',
        body: JSON.stringify([{
          id: 0,
          employeeId: identity.employeeId,
          managerId: identity.managerId,
          projectId: identity.projectId,
          topUpStart: new Date(topUpStart).toISOString(),
          topUpEnd: new Date(topUpEnd).toISOString(),
        }]),
      })
      setNotice(`Top-up submitted: ${statusLabel(result.approvalStatus)}.`)
      setTopUpStart('')
      setTopUpEnd('')
      await refresh()
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setSubmitBusy(false)
    }
  }

  const canSwipeIn = !attendance?.hasOpenSwipe
  const todayHours = Number(attendance?.totalOfficeHours || 0)

  return (
    <>
      <section className="page-heading">
        <div>
          <p className="eyebrow">EMPLOYEE WORKSPACE · PROJECT #{identity.projectId}</p>
          <h1>Your attendance</h1>
          <p className="muted">Swipe in and out, then submit top-up time for your manager’s rules.</p>
        </div>
        <button className="button button-quiet refresh-button" onClick={refresh} type="button">↻ Refresh</button>
      </section>

      {error && <div className="alert alert-error" role="alert">{error}</div>}
      {notice && <div className="alert alert-success" role="status">{notice}</div>}

      <section className="employee-grid">
        <article className="panel attendance-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">TODAY</p>
              <h2>Office attendance</h2>
            </div>
            <span className={`live-indicator ${attendance?.hasOpenSwipe ? 'live' : ''}`}>
              <span /> {attendance?.hasOpenSwipe ? 'On the clock' : 'Not clocked in'}
            </span>
          </div>
          <div className="hours-display">
            <strong>{todayHours.toFixed(2)}</strong>
            <span>hours recorded</span>
          </div>
          <p className="muted attendance-hint">
            {attendance?.hasOpenSwipe
              ? 'Your current session is open. Swipe out to finish recording your hours.'
              : 'Swipe in when you arrive. Your hours are calculated when you swipe out.'}
          </p>
          <div className="swipe-actions">
            <button
              className={`button swipe-button ${canSwipeIn ? 'button-primary' : 'button-secondary'}`}
              disabled={!canSwipeIn || swipeBusy}
              onClick={() => recordSwipe('IN')}
              type="button"
            >
              <span className="swipe-icon">↘</span>
              <span><strong>Swipe in</strong><small>Start your office hours</small></span>
            </button>
            <button
              className={`button swipe-button ${!canSwipeIn ? 'button-accent' : 'button-secondary'}`}
              disabled={canSwipeIn || swipeBusy}
              onClick={() => recordSwipe('OUT')}
              type="button"
            >
              <span className="swipe-icon">↗</span>
              <span><strong>Swipe out</strong><small>Calculate today’s hours</small></span>
            </button>
          </div>
        </article>

        <article className="panel topup-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">TIME ADJUSTMENT</p>
              <h2>Submit top-up hours</h2>
            </div>
            <span className="panel-icon">＋</span>
          </div>
          <p className="muted">Choose when you worked. We calculate the hours from the start and end times.</p>
          <form className="form-stack topup-form" onSubmit={submitTopUp}>
            <label>
              Top-up start
              <input
                max={topUpEnd || undefined}
                onChange={(event) => setTopUpStart(event.target.value)}
                required
                type="datetime-local"
                value={topUpStart}
              />
            </label>
            <label>
              Top-up end
              <input
                min={topUpStart || undefined}
                onChange={(event) => setTopUpEnd(event.target.value)}
                required
                type="datetime-local"
                value={topUpEnd}
              />
            </label>
            <p className="helper-text">Intervals must be on the same day and cannot overlap a previous submission.</p>
            <button className="button button-primary button-wide" disabled={submitBusy} type="submit">
              {submitBusy ? 'Submitting…' : 'Submit top-up request'}
            </button>
          </form>
        </article>
      </section>

      <section className="panel history-panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">YOUR REQUESTS</p>
            <h2>Top-up history</h2>
          </div>
          <span className="count-pill">{requests.length}</span>
        </div>
        {requests.length ? (
          <div className="request-list">
            {requests.map((request) => (
              <RequestCard key={request.id} request={request} />
            ))}
          </div>
        ) : (
          <EmptyState title="No top-up requests yet" text="Submitted requests and their review status will appear here." />
        )}
      </section>
    </>
  )
}

function RequestCard({ request, actions }) {
  return (
    <article className="request-card">
      <div className="request-main">
        <div className="request-title-row">
          <strong>Request #{request.id}</strong>
          <StatusBadge status={request.status} />
        </div>
        <p className="muted request-meta">
          Employee #{request.employeeId} <span>·</span> Project #{request.projectId}
        </p>
        <p className="request-dates">{formatDate(request.topUpStart)} – {formatDate(request.topUpEnd)}</p>
        {request.reason && <p className="request-reason">{request.reason}</p>}
      </div>
      <div className="request-hours">
        <strong>{formatHours(request.requestedHours)}</strong>
        <span>top-up</span>
        <small>{formatHours(request.officeHours)} swipe</small>
      </div>
      {actions}
    </article>
  )
}

function EmptyState({ title, text }) {
  return (
    <div className="empty-state">
      <span className="empty-mark">✓</span>
      <strong>{title}</strong>
      <p>{text}</p>
    </div>
  )
}

function ManagerWorkspace({ identity }) {
  const [requests, setRequests] = useState([])
  const [projectFilter, setProjectFilter] = useState(identity.projectId || '')
  const [rules, setRules] = useState({
    projectId: identity.projectId || '',
    requiredSwipeHours: 5,
    requiredOfficeHours: 5,
    requiredOfficeDaysPerWeek: 5,
  })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [comments, setComments] = useState({})

  const loadRequests = useCallback(async () => {
    try {
      const query = projectFilter ? `?projectId=${projectFilter}` : ''
      const results = await apiRequest(`/api/topup/manager/${identity.managerId}${query}`)
      setError('')
      setRequests(results)
    } catch (requestError) {
      setError(requestError.message)
    }
  }, [identity.managerId, projectFilter])

  useEffect(() => {
    let active = true
    async function loadQueue() {
      try {
        const query = projectFilter ? `?projectId=${projectFilter}` : ''
        const results = await apiRequest(`/api/topup/manager/${identity.managerId}${query}`)
        if (active) {
          setError('')
          setRequests(results)
        }
      } catch (requestError) {
        if (active) setError(requestError.message)
      }
    }

    loadQueue()
    return () => { active = false }
  }, [identity.managerId, projectFilter])

  const pendingRequests = useMemo(
    () => requests.filter((request) => request.status === 'PENDING_MANAGER_REVIEW'),
    [requests],
  )
  const autoApprovedRequests = useMemo(
    () => requests.filter((request) => request.status === 'AUTO_APPROVED'),
    [requests],
  )
  const reviewedRequests = useMemo(
    () => requests.filter((request) => ['MANUALLY_APPROVED', 'MANUALLY_REJECTED'].includes(request.status)),
    [requests],
  )

  async function saveRules(event) {
    event.preventDefault()
    setBusy(true)
    setError('')
    setNotice('')
    try {
      const saved = await apiRequest('/api/configuration', {
        method: 'POST',
        body: JSON.stringify({
          managerId: identity.managerId,
          projectId: Number(rules.projectId),
          requiredSwipeHours: Number(rules.requiredSwipeHours),
          requiredOfficeHours: Number(rules.requiredOfficeHours),
          requiredOfficeDaysPerWeek: Number(rules.requiredOfficeDaysPerWeek),
        }),
      })
      setNotice(`Rules saved for project #${saved.projectId}. Combined target: ${formatHours(saved.requiredTotalHours)}.`)
      setProjectFilter(String(saved.projectId))
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setBusy(false)
    }
  }

  async function reviewRequest(request, approved) {
    setBusy(true)
    setError('')
    setNotice('')
    try {
      await apiRequest(
        `/api/topup/manager/${identity.managerId}/requests/${request.id}/review`,
        {
          method: 'POST',
          body: JSON.stringify({
            managerId: identity.managerId,
            approved,
            comment: comments[request.id] || '',
          }),
        },
      )
      setNotice(`Request #${request.id} ${approved ? 'approved' : 'declined'}.`)
      await loadRequests()
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <>
      <section className="page-heading">
        <div>
          <p className="eyebrow">MANAGER WORKSPACE</p>
          <h1>Team overview</h1>
          <p className="muted">Review requests, track automatic decisions, and set project expectations.</p>
        </div>
        <button className="button button-quiet refresh-button" onClick={loadRequests} type="button">↻ Refresh</button>
      </section>

      {error && <div className="alert alert-error" role="alert">{error}</div>}
      {notice && <div className="alert alert-success" role="status">{notice}</div>}

      <section className="manager-stats">
        <StatCard label="Awaiting your review" value={pendingRequests.length} accent="orange" />
        <StatCard label="Auto-approved" value={autoApprovedRequests.length} accent="green" />
        <StatCard label="Reviewed by you" value={reviewedRequests.length} accent="purple" />
      </section>

      <section className="manager-columns">
        <div className="manager-queues">
          <section className="panel queue-panel">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">ACTION REQUIRED</p>
                <h2>Manual review</h2>
              </div>
              <span className="count-pill count-orange">{pendingRequests.length}</span>
            </div>
            {pendingRequests.length ? (
              <div className="request-list">
                {pendingRequests.map((request) => (
                  <RequestCard
                    actions={(
                      <div className="review-actions">
                        <input
                          aria-label={`Optional note for request ${request.id}`}
                          onChange={(event) => setComments({ ...comments, [request.id]: event.target.value })}
                          placeholder="Optional note"
                          value={comments[request.id] || ''}
                        />
                        <button
                          className="button button-primary"
                          disabled={busy}
                          onClick={() => reviewRequest(request, true)}
                          type="button"
                        >Approve</button>
                        <button
                          className="button button-danger"
                          disabled={busy}
                          onClick={() => reviewRequest(request, false)}
                          type="button"
                        >Decline</button>
                      </div>
                    )}
                    key={request.id}
                    request={request}
                  />
                ))}
              </div>
            ) : (
              <EmptyState title="You’re all caught up" text="Requests that need your decision will show up here." />
            )}
          </section>

          <section className="panel queue-panel">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">RULES SATISFIED</p>
                <h2>Auto-approved</h2>
              </div>
              <span className="count-pill count-green">{autoApprovedRequests.length}</span>
            </div>
            {autoApprovedRequests.length ? (
              <div className="request-list">
                {autoApprovedRequests.map((request) => (
                  <RequestCard key={request.id} request={request} />
                ))}
              </div>
            ) : (
              <EmptyState title="No automatic approvals yet" text="Approved requests will appear here separately from manual reviews." />
            )}
          </section>

          {reviewedRequests.length > 0 && (
            <section className="panel queue-panel">
              <div className="panel-heading">
                <div>
                  <p className="eyebrow">DECISIONS COMPLETE</p>
                  <h2>Reviewed requests</h2>
                </div>
                <span className="count-pill">{reviewedRequests.length}</span>
              </div>
              <div className="request-list">
                {reviewedRequests.map((request) => (
                  <RequestCard key={request.id} request={request} />
                ))}
              </div>
            </section>
          )}
        </div>

        <aside className="panel rules-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">PROJECT EXPECTATIONS</p>
              <h2>Manager rules</h2>
            </div>
            <span className="panel-icon">⚙</span>
          </div>
          <p className="muted">Set the attendance targets for a project. Requests below any target go to manual review.</p>
          <form className="form-stack rules-form" onSubmit={saveRules}>
            <label>
              Project ID
              <input
                min="1"
                onChange={(event) => setRules({ ...rules, projectId: event.target.value })}
                required
                type="number"
                value={rules.projectId}
              />
            </label>
            <label>
              Required swipe hours
              <input
                max="24"
                min="0"
                onChange={(event) => setRules({ ...rules, requiredSwipeHours: event.target.value })}
                required
                step="0.25"
                type="number"
                value={rules.requiredSwipeHours}
              />
            </label>
            <label>
              Required top-up hours
              <input
                max="24"
                min="0"
                onChange={(event) => setRules({ ...rules, requiredOfficeHours: event.target.value })}
                required
                step="0.25"
                type="number"
                value={rules.requiredOfficeHours}
              />
            </label>
            <label>
              Office days per week
              <input
                max="7"
                min="0"
                onChange={(event) => setRules({ ...rules, requiredOfficeDaysPerWeek: event.target.value })}
                required
                type="number"
                value={rules.requiredOfficeDaysPerWeek}
              />
            </label>
            <div className="combined-rule">
              <span>Combined hours target</span>
              <strong>{formatHours(Number(rules.requiredSwipeHours || 0) + Number(rules.requiredOfficeHours || 0))}</strong>
            </div>
            <button className="button button-primary button-wide" disabled={busy} type="submit">
              {busy ? 'Saving…' : 'Save project rules'}
            </button>
          </form>
          <div className="rules-default">
            <strong>Default when no rules are saved</strong>
            <p>5 swipe hours · 5 top-up hours · 10 combined hours · 5 days/week</p>
          </div>
        </aside>
      </section>
    </>
  )
}

function StatCard({ label, value, accent }) {
  return (
    <article className={`stat-card stat-${accent}`}>
      <span className="stat-dot" />
      <strong>{value}</strong>
      <span>{label}</span>
    </article>
  )
}

function App() {
  const [identity, setIdentity] = useState(readSavedIdentity)

  function login(nextIdentity) {
    localStorage.setItem('attendance-identity', JSON.stringify(nextIdentity))
    setIdentity(nextIdentity)
  }

  function logout() {
    localStorage.removeItem('attendance-identity')
    setIdentity(null)
  }

  if (!identity) return <LoginScreen onLogin={login} />

  return (
    <AppShell identity={identity} onLogout={logout}>
      {identity.role === 'employee'
        ? <EmployeeWorkspace identity={identity} />
        : <ManagerWorkspace identity={identity} />}
    </AppShell>
  )
}

export default App
