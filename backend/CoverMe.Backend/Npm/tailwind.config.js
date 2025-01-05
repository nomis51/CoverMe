/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "../Components/**/*.{html,cshtml,razor}",
        "../Pages/**/*.{html,cshtml,razor}",
        "../Layouts/**/*.{html,cshtml,razor}",
        "../App.razor",
        "./src/**/*.{js,ts}"
    ],
    theme: {
        extend: {},
    },
    plugins: [],
}

