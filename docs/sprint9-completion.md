---
title: Sprint 9 Completion - Certificate & Completion
scope: Backend MVP
status: completed
---

# Sprint 9: Certificate & Completion - COMPLETED

## Backend Implementation

- Certificate aggregate with issuance, revocation, verification code, expiry, and completion rule validation.
- CertificateTemplate aggregate and database mapping for future admin template editing.
- Completion rules implemented for the Sprint 9 MVP: attendance >= 80%, progress = 100%, and quiz passed.
- Certificate APIs:
  - `POST /api/v1/certificates`
  - `GET /api/v1/certificates/{id}`
  - `GET /api/v1/certificates/{id}/pdf`
  - `GET /api/v1/certificates/verify/{verificationCode}` public verification endpoint
- Built-in PDF generation service for downloadable single-page certificate PDFs.
- Certificate repository and EF Core configuration.
- Migration `Sprint9_Certificates` for `certificates` and `certificate_templates`.
- Certificate permissions: `Certificates.Read`, `Certificates.Issue`.

## Deferred / Follow-up

- Angular certificate template editor, download UI, student certificate view, and public verification page remain frontend follow-up.
- Rich HTML/template-driven PDF rendering remains a later enhancement; the current PDF service renders the issued certificate fields directly.
- Automatic completion calculation from VOD/watch events should be expanded in Sprint 12 when progress tracking exists.

## Verification

- Domain unit tests cover issuance, completion-rule rejection, and revocation verification behavior.
