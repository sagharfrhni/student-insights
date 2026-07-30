/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        brand: {
          teal: 'var(--color-teal)',
          bg: 'var(--bg-main)',
          amber: 'var(--color-amber)',
          peach: 'var(--bg-peach)',
          rose: 'var(--color-rose)',
          dark: 'var(--color-dark)',
        }
      },
      fontFamily: {
        sans: ['Vazirmatn', 'sans-serif'],
      }
    },
  },
  plugins: [],
}