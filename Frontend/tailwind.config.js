/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      container : {
        center : true,
        padding : '1rem'
      },
      fontFamily : {
        'display' : ["Playfair Display"],
        'regular' : ["Farsan"],
      },
      fontSize : {
        'header' : "32px",
        'secondary-header' : "20px",
        'normal' : "16px"
      }
    },
  },
  plugins: [],
};
