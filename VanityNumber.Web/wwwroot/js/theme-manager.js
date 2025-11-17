// Theme Manager for Vanity Number Generator
// Handles theme switching: light, dark, or system default

export class ThemeManager {
    constructor() {
        this.THEME_KEY = 'vanity-theme-preference';
        this.themes = {
            LIGHT: 'light',
            DARK: 'dark',
            SYSTEM: 'system'
        };
        
        // Initialize theme
        this.init();
    }

    init() {
        // Get saved preference or default to system
        const savedTheme = this.getSavedTheme();
        this.applyTheme(savedTheme);
        
        // Listen for system theme changes
        this.watchSystemTheme();
    }

    getSavedTheme() {
        try {
            const saved = localStorage.getItem(this.THEME_KEY);
            return saved || this.themes.SYSTEM;
        } catch {
            return this.themes.SYSTEM;
        }
    }

    saveTheme(theme) {
        try {
            localStorage.setItem(this.THEME_KEY, theme);
        } catch (error) {
            console.warn('Could not save theme preference:', error);
        }
    }

    getSystemTheme() {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return this.themes.DARK;
        }
        return this.themes.LIGHT;
    }

    applyTheme(theme) {
        const body = document.body;
        
        // Remove existing theme classes
        body.classList.remove('theme-light', 'theme-dark', 'theme-system');
        
        // Determine which theme to actually apply
        let actualTheme = theme;
        if (theme === this.themes.SYSTEM) {
            actualTheme = this.getSystemTheme();
            body.classList.add('theme-system'); // Keep track that we're in system mode
        }
        
        // Apply theme class
        body.classList.add(`theme-${actualTheme}`);
        
        // Update data attribute for CSS
        body.setAttribute('data-theme', actualTheme);
        
        // Update meta theme-color for mobile browsers
        this.updateMetaThemeColor(actualTheme);
        
        // Save preference
        this.saveTheme(theme);
        
        // Dispatch custom event for components that need to know
        window.dispatchEvent(new CustomEvent('themechanged', {
            detail: { theme: actualTheme, preference: theme }
        }));
    }

    updateMetaThemeColor(theme) {
        let themeColor = '#667eea'; // Default light theme
        if (theme === this.themes.DARK) {
            themeColor = '#1e1e2e'; // Dark theme
        }
        
        let metaThemeColor = document.querySelector('meta[name="theme-color"]');
        if (!metaThemeColor) {
            metaThemeColor = document.createElement('meta');
            metaThemeColor.name = 'theme-color';
            document.head.appendChild(metaThemeColor);
        }
        metaThemeColor.content = themeColor;
    }

    setTheme(theme) {
        if (!Object.values(this.themes).includes(theme)) {
            console.warn(`Invalid theme: ${theme}`);
            return;
        }
        this.applyTheme(theme);
    }

    getCurrentTheme() {
        return this.getSavedTheme();
    }

    getActualTheme() {
        const preference = this.getCurrentTheme();
        if (preference === this.themes.SYSTEM) {
            return this.getSystemTheme();
        }
        return preference;
    }

    watchSystemTheme() {
        if (!window.matchMedia) return;
        
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        
        // Modern browsers
        if (mediaQuery.addEventListener) {
            mediaQuery.addEventListener('change', (e) => {
                // Only react if we're in system mode
                if (this.getCurrentTheme() === this.themes.SYSTEM) {
                    this.applyTheme(this.themes.SYSTEM);
                }
            });
        } 
        // Older browsers
        else if (mediaQuery.addListener) {
            mediaQuery.addListener((e) => {
                if (this.getCurrentTheme() === this.themes.SYSTEM) {
                    this.applyTheme(this.themes.SYSTEM);
                }
            });
        }
    }

    toggleTheme() {
        const current = this.getActualTheme();
        const next = current === this.themes.LIGHT ? this.themes.DARK : this.themes.LIGHT;
        this.setTheme(next);
    }
}

// Global instance
window.themeManager = new ThemeManager();

// Export for ES modules
export default window.themeManager;
