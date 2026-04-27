// Animation utilities for HumanNumbers Showcase
class AnimationManager {
    constructor() {
        this.init();
    }

    init() {
        // Initialize intersection observer for scroll animations
        this.setupScrollAnimations();
        
        // Initialize number animation utilities
        this.setupNumberAnimations();
        
        // Initialize loading states
        this.setupLoadingStates();
    }

    setupScrollAnimations() {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-fade-in');
                    
                    // Special handling for different animation types
                    if (entry.target.dataset.animation) {
                        entry.target.classList.add(entry.target.dataset.animation);
                    }
                }
            });
        }, observerOptions);

        // Observe elements with animation data attributes
        document.querySelectorAll('[data-animate]').forEach(el => {
            observer.observe(el);
        });
    }

    setupNumberAnimations() {
        // Animate numbers counting up
        window.animateNumber = (element, target, duration = 1000) => {
            const start = 0;
            const increment = target / (duration / 16);
            let current = start;

            const timer = setInterval(() => {
                current += increment;
                if (current >= target) {
                    current = target;
                    clearInterval(timer);
                }
                element.textContent = Math.floor(current).toLocaleString();
            }, 16);
        };

        // Animate decimal numbers with precision
        window.animateDecimal = (element, target, duration = 1000, decimals = 2) => {
            const start = 0;
            const increment = target / (duration / 16);
            let current = start;

            const timer = setInterval(() => {
                current += increment;
                if (current >= target) {
                    current = target;
                    clearInterval(timer);
                }
                element.textContent = current.toFixed(decimals);
            }, 16);
        };
    }

    setupLoadingStates() {
        // Add loading skeleton utilities
        window.showLoading = (element, text = 'Loading...') => {
            element.classList.add('loading-skeleton');
            element.setAttribute('aria-busy', 'true');
            
            if (element.tagName === 'BUTTON') {
                const originalText = element.textContent;
                element.dataset.originalText = originalText;
                element.innerHTML = `
                    <svg class="animate-spin h-4 w-4 mr-2" fill="none" viewBox="0 0 24 24">
                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    ${text}
                `;
            }
        };

        window.hideLoading = (element) => {
            element.classList.remove('loading-skeleton');
            element.removeAttribute('aria-busy');
            
            if (element.tagName === 'BUTTON' && element.dataset.originalText) {
                element.textContent = element.dataset.originalText;
                delete element.dataset.originalText;
            }
        };
    }

    // Smooth scroll with offset
    smoothScrollTo(target, offset = 80) {
        const element = typeof target === 'string' ? document.querySelector(target) : target;
        if (!element) return;

        const targetPosition = element.offsetTop - offset;
        const startPosition = window.pageYOffset;
        const distance = targetPosition - startPosition;
        let startTime = null;

        const animation = (currentTime) => {
            if (startTime === null) startTime = currentTime;
            const timeElapsed = currentTime - startTime;
            const run = this.easeInOutQuad(timeElapsed, startPosition, distance, 500);
            window.scrollTo(0, run);
            if (timeElapsed < 500) requestAnimationFrame(animation);
        };

        requestAnimationFrame(animation);
    }

    easeInOutQuad(t, b, c, d) {
        t /= d / 2;
        if (t < 1) return c / 2 * t * t + b;
        t--;
        return -c / 2 * (t * (t - 2) - 1) + b;
    }

    // Add micro-interactions
    addMicroInteractions() {
        // Button hover effects
        document.querySelectorAll('.btn-vercel').forEach(button => {
            button.addEventListener('mouseenter', () => {
                button.style.transform = 'translateY(-1px)';
            });
            
            button.addEventListener('mouseleave', () => {
                button.style.transform = 'translateY(0)';
            });
        });

        // Card hover effects
        document.querySelectorAll('.card-vercel-hover').forEach(card => {
            card.addEventListener('mouseenter', () => {
                card.style.transform = 'translateY(-4px)';
            });
            
            card.addEventListener('mouseleave', () => {
                card.style.transform = 'translateY(0)';
            });
        });
    }
}

// Initialize animation manager
const animationManager = new AnimationManager();

// Export for global use
window.animationManager = animationManager;
