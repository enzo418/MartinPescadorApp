/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.cshtml',
    './Pages/**/*.razor',
    './Shared/**/*.razor'
  ],
  theme: {
    extend: {
      fontFamily: {
        body: ['Montserrat']
      }
    },
  },
  plugins: [],
}

