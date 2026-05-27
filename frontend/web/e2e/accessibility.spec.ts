import AxeBuilder from '@axe-core/playwright';
import { expect, Page, test } from '@playwright/test';

async function expectNoA11yViolations(page: Page): Promise<void> {
  const results = await new AxeBuilder({ page })
    .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
    .analyze();

  expect(results.violations).toEqual([]);
}

async function mockAuthenticatedApi(page: Page): Promise<void> {
  await page.route('**/api/v1/**', async (route) => {
    const path = new URL(route.request().url()).pathname;

    if (path.endsWith('/notifications/unread-count')) {
      await route.fulfill({ json: { count: 2 } });
      return;
    }

    if (path.endsWith('/reports/dashboard/student')) {
      await route.fulfill({
        json: {
          paidOrders: 3,
          coursePurchases: 2,
          classPurchases: 1,
          upcomingSessions: 2,
          certificatesIssued: 7,
        },
      });
      return;
    }

    if (path.endsWith('/courses')) {
      await route.fulfill({
        json: {
          items: [],
          page: 1,
          pageSize: 20,
          totalCount: 0,
          totalPages: 0,
          hasPreviousPage: false,
          hasNextPage: false,
        },
      });
      return;
    }

    await route.fulfill({ json: {} });
  });

  await page.addInitScript(() => {
    sessionStorage.setItem('elearning_access', 'test-token');
    sessionStorage.setItem('elearning_refresh', 'test-refresh-token');
    sessionStorage.setItem(
      'elearning_user',
      JSON.stringify({
        id: 'test-admin',
        email: 'admin@example.test',
        firstName: 'Admin',
        lastName: 'User',
        fullName: 'Admin User',
        roles: ['Learner'],
      }),
    );
  });
}

test.describe('accessibility', () => {
  test('login page has no WCAG A/AA axe violations', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByRole('button', { name: 'Sign in' })).toBeVisible();

    await expectNoA11yViolations(page);
  });

  test('register page has no WCAG A/AA axe violations', async ({ page }) => {
    await page.goto('/register');
    await expect(page.getByRole('button', { name: 'Register' })).toBeVisible();

    await expectNoA11yViolations(page);
  });

  test('mobile navigation is reachable and exposes primary links', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await mockAuthenticatedApi(page);

    await page.goto('/dashboard');
    await expect(page.getByRole('button', { name: 'Open navigation' })).toBeVisible();

    await page.getByRole('button', { name: 'Open navigation' }).click();
    await expect(page.getByRole('navigation', { name: 'Primary navigation' })).toBeVisible();

    await page.getByRole('link', { name: 'Courses' }).click();
    await expect(page.getByRole('heading', { name: 'Courses' })).toBeVisible();

    await expectNoA11yViolations(page);
  });
});
