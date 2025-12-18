/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{css,xml,html,vue,svelte,ts,tsx}'],
  darkMode: ['class', '.ns-dark'],
  theme: {
    extend: {
        colors: {
            'brand-gold': '#D3A54A',
            'brand-gold-light': '#E3B65B',
            'brand-gold-dark': '#B98E3C',
            'dark': '#0D0D0D',
            'dark-secondary': '#1A1A1A',
            'text-primary': '#F5F5F5',
            'text-secondary': '#B8B8B8',
            'highlight': '#8A7FFF',
            'danger-dark': '#4A1F24',
            'danger-dark-hover': '#61282D'
        },
    },
  },
  plugins: [],
  corePlugins: {
    preflight: false,
  },
};
