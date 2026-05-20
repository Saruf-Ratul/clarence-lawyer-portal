# Clarence Lawyer Portal
## Business Requirements Specification (BRS)

**Version:** 1.1  
**Date:** May 20, 2026  
**Prepared For:** Clarence / Xceleran Team  
**Primary Persona:** High-volume landlord-tenant litigation attorney (NJ focus)

---

## 1) Product Vision
Clarence is a legal-operations cockpit for landlord-tenant attorneys who must manage high filing volume with courtroom-grade accuracy. The platform ingests case data from Rent Manager (RM), organizes it by **Client → Property → Case**, automates NJ form preparation, and synchronizes case progress back to RM.

Core value proposition:
- **RM supplies source data**
- **Clarence verifies, structures, automates, and tracks legal workflow**
- **RM receives case lifecycle updates back from Clarence**

---

## 2) User Avatar (Primary)
### Working Landlord-Tenant Attorney
This user:
- Personally appears in court and is accountable for proving case facts.
- Handles hundreds of matters and depends on throughput for profitability.
- Faces high downside risk from preventable data errors.

### Why accuracy is existential
Common errors and consequences:
- Wrong address/unit/county → adjournments, dismissed/refiled matters, rent loss windows.
- Wrong debt/balance → legal exposure, counters, credibility damage.
- Wrong status tracking → client confusion, missed deadlines, process failures.

Design implication: the UX must optimize for **precision at scale**, not just speed.

---

## 3) Problem Statement
Attorneys currently face four persistent constraints:
1. **Data management:** fragmented information across RM, email, spreadsheets, staff notes.
2. **Data accuracy:** high legal risk for stale or incorrect values.
3. **Filing speed:** repetitive form prep slows volume economics.
4. **Client communication:** status updates require duplicate manual effort.

---

## 4) Business Goals
Clarence MVP must:
- Pull intake submissions from RM.
- Require attorney/staff review before activating a case.
- Auto-map verified RM data into NJ landlord-tenant forms.
- Generate single and bulk PDF packets.
- Track case status and timeline in Clarence.
- Push selected statuses back to RM.
- Provide client/property/case views with actionable KPIs.

---

## 5) Information Architecture (Canonical Hierarchy)
1. **Client (RM account)**
2. **Property / Building**
3. **Case (tenant LT matter)**
4. **Case artifacts** (forms, filings, timeline, documents, notes, deadlines, audit)

This hierarchy is mandatory and drives navigation, permissions, reporting, and sync behavior.

---

## 6) Day-in-the-Life Workflow (Target UX)
1. Attorney logs in and sees portfolio summary (clients, properties, open cases, new RM submissions).
2. Attorney opens RM intake inbox and reviews new submissions.
3. Attorney accepts valid submissions (or rejects/requests clarification).
4. Clarence creates case record and pre-fills form data from RM.
5. Staff generates PDFs (single or bulk) for filing operations.
6. Attorney updates status through lifecycle milestones.
7. Clarence pushes configured statuses back to RM.
8. Attorney returns for additional forms, updates, settlement/compliance tracking.
9. Outcome and closure data remain searchable and auditable.

---

## 7) MVP Scope
### In Scope
- Auth + role-based access.
- Dashboard with KPI cards and action queues.
- RM intake inbox and Accept Case workflow.
- Client, property, and case detail views.
- RM data verification indicators.
- Form generation and PDF management.
- Status timeline with RM pushback.
- Notes/documents and audit trail.

### Out of Scope (MVP)
- Direct court e-filing automation.
- Legal advice/decisioning AI.
- Full billing/accounting.
- Multi-state form packs.
- Full calendaring engine/mobile app.

---

## 8) Dashboard Requirements
Must display:
- Active client count.
- Unique properties/buildings count.
- Open LT case count.
- New RM submissions pending review.
- Cases needing action.
- Upcoming deadlines/hearings/compliance dates.
- Recent status updates.
- RM sync health at portfolio level.

---

## 9) RM Intake Inbox Requirements
Each submission row should include:
- Client, property, tenant, unit.
- Balance owed, monthly rent.
- Date received.
- RM verification status.
- Actions: Review, Accept, Reject/Ignore, Request Clarification.

### Acceptance business rule
A submission **does not** become an active case until accepted by authorized attorney/admin user.

On accept:
- Create legal case record.
- Link client/property/tenant.
- Persist financial snapshot + RM source ID.
- Write initial status (`Accepted`).
- Log audit event.
- Queue optional RM status pushback.

---

## 10) Case Status Lifecycle (Recommended)
`New Submission → Under Review → Accepted → Forms Generated → Sent for Filing → Filed → Scheduled → Served → Court Appearance → Outcome Entered → Settled/Dismissed/Judgment/Closed`

RM pushback default:
- Yes: Accepted, Filed, Scheduled, Settlement, Dismissed, Judgment, Closed
- Optional: Served

---

## 11) Accuracy & Risk Controls
- Required-field validation before final PDF generation.
- Data drift flag if RM data changes after form generation.
- Manual override markers and compare views.
- County/court-path validation guardrails.
- Immutable audit logs for high-risk changes.
- Clear “verified / pending / failed sync” indicators.

---

## 12) Roles & Permissions (MVP)
- **Attorney:** full case review/approval/status authority.
- **Paralegal:** form prep, docs, filing support (approval boundaries configurable).
- **Admin:** user/client setup, integration settings.
- **Read-only client user (optional):** constrained visibility.
- **System admin:** platform-level configuration.

Key controls:
- Only attorneys/admins can accept RM submissions.
- RM pushback restricted to authorized roles.
- Sensitive values protected by role scope.

---

## 13) Audit & Compliance Requirements
Track at minimum:
- Submission received/accepted/rejected.
- Status changes and RM push attempts/results.
- PDF generated/downloaded/emailed.
- Manual overrides.
- Notes/doc uploads.
- Authentication and major access events.

Each event includes actor, timestamp, old/new values (where applicable), and source.

---

## 14) Non-Functional Requirements
### Security
RBAC, encrypted transport, secure credential storage, session controls, auditable access.

### Performance
Fast dashboard rendering with large case counts; paginated/filterable inbox and case tables; async/batched PDF generation.

### Reliability
Retry-capable RM operations, explicit sync failure visibility, non-blocking UI during integration jobs.

### Usability
Action-oriented controls, minimal click-depth from dashboard to case action, fast client/property/case toggling.

---

## 15) MVP Success Criteria
The MVP is successful when attorneys can:
- Process RM submissions without re-keying core data.
- Generate form packets accurately and quickly.
- Navigate cleanly from client → property → case.
- Push case progress back into RM with reliable logs.
- Reduce preventable data errors while maintaining high throughput.

---

## 16) Product Direction Guardrails
- Do not introduce unsupported legal terminology in UI or workflow labels.
- Keep form types and legal labels configurable and jurisdiction-aware.
- Prioritize high-trust interaction patterns over decorative complexity.
- Treat “speed with auditability” as the default design principle.
