/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{js,jsx,ts,tsx}"],
  presets: [require("nativewind/preset")],
  theme: {
    extend: {
      colors: {
        sage: {
          50: "#F4F6F2",
          100: "#E3EAE0",
          200: "#C8D6C2",
          300: "#A9BDA1",
          400: "#8FA395",
          500: "#7B9080",
          600: "#5F7264",
          700: "#4A594F",
          800: "#3A453E",
          900: "#2D332C",
        },
        cream: "#F5F1EA",
        coral: "#D08573",
        peach: "#EDD7C2",
        mint: "#B5D0B3",
      },
      fontFamily: {
        display: ["Fraunces_600SemiBold", "serif"],
        "display-medium": ["Fraunces_500Medium", "serif"],
      },
    },
  },
  plugins: [],
};
