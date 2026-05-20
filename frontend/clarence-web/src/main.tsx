import React, { useEffect, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';

type Dashboard = { activeClients: number; uniqueProperties: number; openCases: number; newSubmissions: number };
type Client = { id: string; name: string; properties: number; openCases: number; rmSyncStatus: string };
type Property = { id: string; clientId: string; address: string; openCases: number; rmSyncStatus: string };
type CaseSummary = { id: string; caseNumber: string; clientName: string; propertyAddress: string; tenantName: string; stage: string; balanceOwed: number; nextDeadline?: string };
type CaseDetail = { id: string; caseNumber: string; clientName: string; propertyAddress: string; tenantName: string; unit: string; monthlyRent: number; balanceOwed: number; lateFees: number; courtCosts: number; stage: string; timeline: { atUtc: string; stage: string; actor: string; notes: string }[] };
type Submission = { id: string; clientName: string; propertyAddress: string; tenantName: string; unit: string; balanceOwed: number; monthlyRent: number; receivedAtUtc: string; verificationStatus: string };

const api = async <T,>(url: string, init?: RequestInit) => (await fetch(url, { headers: { 'Content-Type': 'application/json' }, ...init })).json() as Promise<T>;

function App() {
  const [tab, setTab] = useState('dashboard');
  const [dashboard, setDashboard] = useState<Dashboard | null>(null);
  const [clients, setClients] = useState<Client[]>([]);
  const [selectedClient, setSelectedClient] = useState<string>('');
  const [properties, setProperties] = useState<Property[]>([]);
  const [cases, setCases] = useState<CaseSummary[]>([]);
  const [selectedCaseId, setSelectedCaseId] = useState<string>('');
  const [caseDetail, setCaseDetail] = useState<CaseDetail | null>(null);
  const [inbox, setInbox] = useState<Submission[]>([]);

  useEffect(() => { api<Dashboard>('/api/dashboard/kpis').then(setDashboard).catch(() => {}); }, []);
  useEffect(() => { api<Client[]>('/api/clients').then((x) => { setClients(x); if (x[0]) setSelectedClient(x[0].id); }).catch(() => {}); }, []);
  useEffect(() => { if (selectedClient) api<Property[]>(`/api/clients/${selectedClient}/properties`).then(setProperties).catch(() => {}); }, [selectedClient]);
  useEffect(() => { api<CaseSummary[]>('/api/cases').then((x) => { setCases(x); if (x[0]) setSelectedCaseId(x[0].id); }).catch(() => {}); }, [inbox.length]);
  useEffect(() => { if (selectedCaseId) api<CaseDetail>(`/api/cases/${selectedCaseId}`).then(setCaseDetail).catch(() => {}); }, [selectedCaseId]);
  useEffect(() => { api<Submission[]>('/api/rm-inbox').then(setInbox).catch(() => {}); }, [tab]);

  const menu = ['dashboard', 'inbox', 'clients', 'properties', 'cases', 'case-detail', 'forms', 'reports', 'settings'];
  const selectedClientName = useMemo(() => clients.find(c => c.id === selectedClient)?.name ?? 'Client', [clients, selectedClient]);

  async function acceptSubmission(id: string) {
    await api('/api/rm-inbox/accept', { method: 'POST', body: JSON.stringify({ submissionId: id, acceptedBy: 'Attorney User' }) });
    setInbox((prev) => prev.filter((x) => x.id !== id));
    setTab('cases');
  }

  return <div style={{ display: 'grid', gridTemplateColumns: '240px 1fr', minHeight: '100vh', fontFamily: 'Inter,system-ui,sans-serif', background: '#f4f7f6' }}>
    <aside style={{ background: '#0f2a3d', color: '#fff', padding: 16 }}>
      <h2>Clarence</h2>
      {menu.map(m => <button key={m} onClick={() => setTab(m)} style={{ display: 'block', width: '100%', margin: '8px 0', padding: 10, borderRadius: 8, border: 'none', textAlign: 'left', background: tab === m ? '#57d36b' : '#16394f', color: tab === m ? '#0f2a3d' : '#fff' }}>{m}</button>)}
    </aside>
    <main style={{ padding: 20 }}>
      {tab === 'dashboard' && <section><h1>Dashboard</h1><p>{dashboard ? `${dashboard.activeClients} active clients • ${dashboard.uniqueProperties} properties • ${dashboard.openCases} open cases` : 'Loading...'}</p></section>}
      {tab === 'inbox' && <section><h1>RM Intake Inbox</h1><table><thead><tr><th>Client</th><th>Property</th><th>Tenant</th><th>Unit</th><th>Balance</th><th>Status</th><th/></tr></thead><tbody>{inbox.map(s => <tr key={s.id}><td>{s.clientName}</td><td>{s.propertyAddress}</td><td>{s.tenantName}</td><td>{s.unit}</td><td>${s.balanceOwed}</td><td>{s.verificationStatus}</td><td><button onClick={() => acceptSubmission(s.id)}>Accept Case</button></td></tr>)}</tbody></table></section>}
      {tab === 'clients' && <section><h1>Clients</h1>{clients.map(c => <div key={c.id}>{c.name} - {c.properties} properties - {c.openCases} open cases</div>)}</section>}
      {tab === 'properties' && <section><h1>Properties for {selectedClientName}</h1>{properties.map(p => <div key={p.id}>{p.address} ({p.openCases} open)</div>)}</section>}
      {tab === 'cases' && <section><h1>All Cases</h1>{cases.map(c => <div key={c.id}><button onClick={() => { setSelectedCaseId(c.id); setTab('case-detail'); }}>{c.caseNumber}</button> — {c.clientName} — {c.stage}</div>)}</section>}
      {tab === 'case-detail' && <section><h1>Case Detail</h1>{caseDetail ? <div><p><b>{caseDetail.caseNumber}</b> | {caseDetail.clientName} | {caseDetail.propertyAddress}</p><p>Tenant: {caseDetail.tenantName} Unit {caseDetail.unit} | Rent ${caseDetail.monthlyRent} | Owed ${caseDetail.balanceOwed}</p><h3>Status Timeline</h3>{caseDetail.timeline.map((t, i) => <div key={i}>{new Date(t.atUtc).toLocaleString()} — {t.stage} — {t.actor} — {t.notes}</div>)}</div> : 'Select case'}</section>}
      {tab === 'forms' && <section><h1>Forms & Filings</h1><p>Module scaffolded per BRS. Next: NJ template mapping and PDF generation APIs.</p></section>}
      {tab === 'reports' && <section><h1>Reports</h1><p>Module scaffolded per BRS for client/property/case reporting.</p></section>}
      {tab === 'settings' && <section><h1>Settings</h1><p>Module scaffolded for RM integration, roles, and terminology config.</p></section>}
    </main>
  </div>;
}

createRoot(document.getElementById('root')!).render(<App />);
