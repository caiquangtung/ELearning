import { test, expect } from '@playwright/test';

test('smoke: anonymous catalog converts through login returnUrl', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'Job-ready learning for modern teams.' })).toBeVisible();

  await page.getByRole('link', { name: /Explore courses/ }).click();
  await expect(page.getByRole('heading', { name: 'Find the right course before you commit.' })).toBeVisible();
  await expect(page.locator('.course-card').first()).toBeVisible();

  await page.getByRole('link', { name: 'Preview syllabus' }).first().click();
  await expect(page.getByText('Syllabus preview')).toBeVisible();

  await page.getByRole('button', { name: /Buy course|Start free/ }).click();
  await expect(page).toHaveURL(/\/login\?returnUrl=/);

  await page.getByLabel('Email').fill(process.env['E2E_EMAIL'] ?? 'demo.learner00001@seed.local');
  await page.getByLabel('Password').fill(process.env['E2E_PASSWORD'] ?? 'SeedPass123!');
  await page.getByRole('button', { name: 'Sign in' }).click();

  await expect(page).toHaveURL(/\/learn\/(checkout|courses\/)/);
});

test('smoke: learner asks AI tutor with a citation', async ({ page, request }) => {
  const apiBaseUrl = process.env['E2E_API_BASE_URL'] ?? 'http://localhost:5001';
  const origin = process.env['E2E_BASE_URL'] ?? 'http://localhost:4200';

  const adminLogin = await request.post(`${apiBaseUrl}/api/v1/identity/login`, {
    headers: { Origin: origin },
    data: {
      email: process.env['E2E_ADMIN_EMAIL'] ?? 'admin@localhost.local',
      password: process.env['E2E_ADMIN_PASSWORD'] ?? 'ChangeMe123!',
    },
  });
  expect(adminLogin.ok()).toBeTruthy();
  const adminAuth = await adminLogin.json();

  const reindex = await request.post(`${apiBaseUrl}/api/v1/ai/knowledge/reindex`, {
    headers: {
      Authorization: `Bearer ${adminAuth.accessToken}`,
      Origin: origin,
    },
    data: { courseId: null },
  });
  expect(reindex.ok()).toBeTruthy();

  await page.goto('/login');

  await page.getByLabel('Email').fill(process.env['E2E_EMAIL'] ?? 'demo.learner00001@seed.local');
  await page.getByLabel('Password').fill(process.env['E2E_PASSWORD'] ?? 'SeedPass123!');

  await page.getByRole('button', { name: 'Sign in' }).click();

  await expect(page.getByText('ELearning')).toBeVisible();

  await page.getByRole('link', { name: 'AI Tutor' }).click();
  await expect(page.getByRole('heading', { name: 'Ask questions from your course material.' })).toBeVisible();

  await page.getByRole('button', { name: 'New chat' }).click();
  await page
    .getByPlaceholder('Ask: What does JWT validation mean in this course?')
    .fill('What does the Data Analytics Fundamentals seed course contain?');
  await page.getByRole('button', { name: 'Send' }).click();

  await expect(
    page.locator('.message:not(.message--user) .message__bubble', {
      hasText: 'Contains structured lessons, quizzes, reviews',
    }).last(),
  ).toBeVisible();
  await expect(page.locator('.citation-card').first()).toBeVisible();
});
