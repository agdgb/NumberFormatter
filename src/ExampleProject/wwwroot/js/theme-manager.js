// Theme Manager for HumanNumbers Showcase
class ThemeManager {
    constructor() {
        this.storageKey = 'humanNumbers-theme';
        this.init();
    }

    init() {
        // Load saved theme or default to dark
        const savedTheme = localStorage.getItem(this.storageKey) || 'dark';
        this.setTheme(savedTheme, false);

        // Listen for system theme changes
        if (window.matchMedia) {
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                if (!localStorage.getItem(this.storageKey)) {
                    this.setTheme(e.matches ? 'dark' : 'light', false);
                }
            });
        }
    }

    setTheme(theme, save = true) {
        const html = document.documentElement;
        
        if (theme === 'dark') {
            html.classList.add('dark');
            html.classList.remove('light');
        } else {
            html.classList.add('light');
            html.classList.remove('dark');
        }

        if (save) {
            localStorage.setItem(this.storageKey, theme);
        }

        // Update theme toggle button
        this.updateThemeToggle(theme);
        
        // Emit theme change event
        window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
    }

    toggleTheme() {
        const currentTheme = document.documentElement.classList.contains('dark') ? 'dark' : 'light';
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        this.setTheme(newTheme);
    }

    updateThemeToggle(theme) {
        const toggle = document.getElementById('theme-toggle');
        if (!toggle) return;

        const sunIcon = toggle.querySelector('.dark\\:block');
        const moonIcon = toggle.querySelector('.block.dark\\:hidden');

        if (theme === 'dark') {
            sunIcon?.classList.remove('hidden');
            moonIcon?.classList.add('hidden');
        } else {
            sunIcon?.classList.add('hidden');
            moonIcon?.classList.remove('hidden');
        }
    }

    getCurrentTheme() {
        return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
    }
}

// Initialize theme manager
const themeManager = new ThemeManager();

// Export for global use
window.themeManager = themeManager;
