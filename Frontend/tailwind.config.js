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
        'display' : ["Bebas Neue"],
        'regular' : ["Montserrat"],
      },
      fontSize : {
        'header' : "60px",
        'secondary-header' : "20px",
        'normal' : "16px"
      }
    },
  },
  plugins: [],
};
