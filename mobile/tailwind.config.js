/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{css,xml,html,vue,svelte,ts,tsx}'],
  darkMode: ['class', '.ns-dark'],
  theme: {
    extend: {
      colors: {
        accent: '#6366F1',
      },
    },
  },
  plugins: [],
  corePlugins: {
    preflight: false,
  },
};
