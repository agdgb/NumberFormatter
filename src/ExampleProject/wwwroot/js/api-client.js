// API Client for HumanNumbers Showcase
class ApiClient {
    constructor() {
        this.baseUrl = '';
        this.defaultHeaders = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
    }

    // Generic request method with error handling
    async request(url, options = {}) {
        try {
            const response = await fetch(this.baseUrl + url, {
                headers: { ...this.defaultHeaders, ...options.headers },
                ...options
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({}));
                throw new Error(errorData.error || `HTTP ${response.status}: ${response.statusText}`);
            }

            return await response.json();
        } catch (error) {
            console.error(`API Error for ${url}:`, error);
            throw error;
        }
    }

    // Roman numeral conversion
    async convertToRoman(value) {
        return this.request(`/api/demo/roman/${value}`);
    }

    // Byte size conversion
    async convertBytes(value, binary = false) {
        return this.request(`/api/demo/bytes/${value}?binary=${binary}`);
    }

    // Parse human-readable number
    async parseNumber(input, culture = '') {
        const params = new URLSearchParams({ input });
        if (culture) params.append('culture', culture);
        return this.request(`/api/demo/parse?${params}`);
    }

    // Get available policies
    async getPolicies() {
        return this.request('/api/demo/policies');
    }

    // Format with policy
    async formatWithPolicy(value, policy, mode = 'number') {
        return this.request(`/api/demo/format-policy?value=${value}&policy=${policy}&mode=${mode}`);
    }

    // Financial playground
    async processFinancial(data) {
        return this.request('/api/demo/financial', {
            method: 'POST',
            body: JSON.stringify(data)
        });
    }

    // Get showcase data
    async getShowcase() {
        return this.request('/api/demo/showcase');
    }

    // Performance benchmark
    async runBenchmark() {
        return this.request('/api/demo/performance-benchmark');
    }
}

// Enhanced API client with caching and retry logic
class EnhancedApiClient extends ApiClient {
    constructor() {
        super();
        this.cache = new Map();
        this.cacheTimeout = 5 * 60 * 1000; // 5 minutes
    }

    // Cached request method
    async cachedRequest(url, options = {}) {
        const cacheKey = `${url}:${JSON.stringify(options)}`;
        const cached = this.cache.get(cacheKey);

        if (cached && Date.now() - cached.timestamp < this.cacheTimeout) {
            return cached.data;
        }

        try {
            const data = await this.request(url, options);
            this.cache.set(cacheKey, {
                data,
                timestamp: Date.now()
            });
            return data;
        } catch (error) {
            // Return cached data if available and request fails
            if (cached) {
                console.warn('Request failed, returning cached data:', error);
                return cached.data;
            }
            throw error;
        }
    }

    // Retry logic for failed requests
    async requestWithRetry(url, options = {}, maxRetries = 3) {
        let lastError;

        for (let attempt = 1; attempt <= maxRetries; attempt++) {
            try {
                return await this.request(url, options);
            } catch (error) {
                lastError = error;
                
                if (attempt < maxRetries) {
                    const delay = Math.pow(2, attempt) * 1000; // Exponential backoff
                    console.warn(`Request failed, retrying in ${delay}ms (attempt ${attempt}/${maxRetries}):`, error);
                    await new Promise(resolve => setTimeout(resolve, delay));
                }
            }
        }

        throw lastError;
    }

    // Clear cache
    clearCache() {
        this.cache.clear();
    }

    // Get cache size
    getCacheSize() {
        return this.cache.size;
    }
}

// Initialize API client
const apiClient = new EnhancedApiClient();

// Export for global use
window.apiClient = apiClient;
