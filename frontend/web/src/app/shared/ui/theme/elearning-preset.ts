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
      50: '{sky.50}',
      100: '{sky.100}',
      200: '{sky.200}',
      300: '{sky.300}',
      400: '{sky.400}',
      500: '{sky.700}',
      600: '{sky.800}',
      700: '{sky.900}',
      800: '{sky.950}',
      900: '{sky.950}',
      950: '{sky.950}',
    },
  },
});
