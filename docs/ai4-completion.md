# AI-4 Completion - Learner Risk Prediction

## Scope Delivered

- Added local deterministic learner risk provider:
  - `IAiLearnerRiskService`
  - `LocalLearnerRiskService`
- Added learner risk API:
  - `GET /api/v1/ai/learners/{userId}/risk`
  - Permission: `AI.Use`
- Added organization risk report API:
  - `GET /api/v1/ai/organizations/{organizationId}/risk-report`
  - Permission: `Organizations.Read`
- Risk scoring uses available LMS signals:
  - average video progress
  - average graded quiz score
  - days since last learning activity
  - active license count and nearest license expiry
- Response includes:
  - `riskScore`
  - `riskLevel`
  - `reasons`
  - `recommendedActions`
  - raw explainability signals
- Added AI request audit logging for learner risk and organization risk report requests.
- Added Angular organization detail risk UI:
  - "AI risk" action
  - high/medium/low summary badges
  - per-member risk badge
  - risk detail panel with reasons and recommended actions

## Verification

- Angular production build passed.
- Docker backend/frontend build passed.
- Playwright smoke test passed.

## Follow-up

- Add scheduled risk snapshot job for B2B reporting.
- Add tests for scoring thresholds, data isolation, and missing-data behavior.
- Add high-risk filter and dedicated organization learner risk report page.
- Include attendance and class timeline signals when the enrollment/attendance sprint is implemented.
