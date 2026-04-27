/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.{html,cshtml}",
    "./Pages/**/*.{html,cshtml}",
    "./wwwroot/js/**/*.js"
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Linear/Vercel-inspired color palette
        gray: {
          50: '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          850: '#1a202c',
          900: '#0f172a',
          950: '#020617'
        },
        // Vercel-style accent colors
        vercel: {
          black: '#000000',
          white: '#ffffff',
          gray: '#171717',
          'gray-light': '#eaeaea',
          'gray-medium': '#a3a3a3',
          blue: '#0070f3',
          'blue-light': '#0070f320',
          purple: '#7928ca',
          'purple-light': '#7928ca20',
          pink: '#ff0080',
          'pink-light': '#ff008020',
          cyan: '#00d9ff',
          'cyan-light': '#00d9ff20',
          orange: '#ffa500',
          'orange-light': '#ffa50020'
        },
        // HumanNumbers brand colors
        human: {
          primary: '#0d6efd',
          'primary-light': '#3b8bfd',
          'primary-dark': '#0a58ca',
          secondary: '#6c757d',
          success: '#198754',
          warning: '#ffc107',
          danger: '#dc3545',
          info: '#0dcaf0'
        }
      },
      fontFamily: {
        'inter': ['Inter', 'ui-sans-serif', 'system-ui', '-apple-system', 'sans-serif'],
        'mono': ['JetBrains Mono', 'ui-monospace', 'SFMono-Regular', 'monospace']
      },
      fontSize: {
        'xs': ['0.75rem', { lineHeight: '1rem' }],
        'sm': ['0.875rem', { lineHeight: '1.25rem' }],
        'base': ['1rem', { lineHeight: '1.5rem' }],
        'lg': ['1.125rem', { lineHeight: '1.75rem' }],
        'xl': ['1.25rem', { lineHeight: '1.75rem' }],
        '2xl': ['1.5rem', { lineHeight: '2rem' }],
        '3xl': ['1.875rem', { lineHeight: '2.25rem' }],
        '4xl': ['2.25rem', { lineHeight: '2.5rem' }],
        '5xl': ['3rem', { lineHeight: '1' }],
        '6xl': ['3.75rem', { lineHeight: '1' }],
        '7xl': ['4.5rem', { lineHeight: '1' }],
        '8xl': ['6rem', { lineHeight: '1' }],
        '9xl': ['8rem', { lineHeight: '1' }]
      },
      spacing: {
        '18': '4.5rem',
        '88': '22rem',
        '128': '32rem'
      },
      maxWidth: {
        '8xl': '88rem',
        '9xl': '96rem'
      },
      animation: {
        'fade-in': 'fadeIn 0.5s ease-in-out',
        'slide-up': 'slideUp 0.3s ease-out',
        'slide-down': 'slideDown 0.3s ease-out',
        'scale-in': 'scaleIn 0.2s ease-out',
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite'
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' }
        },
        slideUp: {
          '0%': { transform: 'translateY(10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' }
        },
        slideDown: {
          '0%': { transform: 'translateY(-10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' }
        },
        scaleIn: {
          '0%': { transform: 'scale(0.95)', opacity: '0' },
          '100%': { transform: 'scale(1)', opacity: '1' }
        }
      },
      backdropBlur: {
        xs: '2px'
      },
      boxShadow: {
        'vercel': '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
        'vercel-lg': '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
        'vercel-xl': '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
        'inner-vercel': 'inset 0 2px 4px 0 rgba(0, 0, 0, 0.06)',
        'glow': '0 0 20px rgba(13, 110, 253, 0.3)',
        'glow-lg': '0 0 40px rgba(13, 110, 253, 0.4)',
        'glow-purple': '0 0 20px rgba(121, 40, 202, 0.3)'
      },
      gradientColorStops: {
        'vercel': {
          0: '#000000',
          100: '#171717'
        },
        'human-primary': {
          0: '#0d6efd',
          100: '#0a58ca'
        },
        'human-success': {
          0: '#198754',
          100: '#146c43'
        },
        'human-danger': {
          0: '#dc3545',
          100: '#a71d2a'
        },
        'human-warning': {
          0: '#ffc107',
          100: '#e0a800'
        },
        'human-info': {
          0: '#0dcaf0',
          100: '#0bacce'
        }
      },
      borderRadius: {
        'vercel': '4px',
        'vercel-lg': '8px',
        'vercel-xl': '12px'
      },
      borderWidth: {
        '0.5': '0.5px'
      }
    }
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
    require('@tailwindcss/aspect-ratio')
  ]
}
