import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

/**
 * ELearning design tokens on top of Aura.
 * Add `semantic` / per-component overrides here instead of large global CSS.
 * @see https://primeng.org/theming/styled
 */
export const ELearningPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#f4f7fb',
      100: '#e7edf5',
      200: '#cfdbea',
      300: '#a9bfd6',
      400: '#789aba',
      500: '#4f789e',
      600: '#365f85',
      700: '#294a69',
      800: '#213b55',
      900: '#172a40',
      950: '#0d1b2a',
    },
  },
});
