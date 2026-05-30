import { test, expect } from '@playwright/test';

test('smoke: login then navigate to courses', async ({ page }) => {
  await page.goto('/login');

  await page.getByLabel('Email').fill(process.env['E2E_EMAIL'] ?? 'admin@localhost.local');
  await page.getByLabel('Password').fill(process.env['E2E_PASSWORD'] ?? 'ChangeMe123!');

  await page.getByRole('button', { name: 'Sign in' }).click();

  // App shell
  await expect(page.getByText('ELearning')).toBeVisible();

  // Navigate to Courses
  await page.getByRole('link', { name: 'Courses' }).click();
  await expect(page.getByRole('heading', { name: 'Courses' })).toBeVisible();
});
