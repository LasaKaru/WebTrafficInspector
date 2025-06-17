// Web Traffic Inspector Start Page JavaScript
// Copyright 2025 Googlex Technologies

// Global variables
let adsterraLoaded = false;
let adsInitialized = false;

// Initialize page functionality
document.addEventListener('DOMContentLoaded', function () {
    initializePage();
    setupEventListeners();
    loadAdsterraAds();
});

// Initialize page components
function initializePage() {
    console.log('Web Traffic Inspector Start Page loaded');

    // Add loading indicators to ad containers
    showAdLoadingIndicators();

    // Initialize Google frame security
    setupGoogleFrameSecurity();

    // Setup responsive behavior
    setupResponsiveHandling();

    // Initialize tooltips and interactions
    initializeInteractions();
}

// Setup event listeners
function setupEventListeners() {
    // Handle window resize for responsive ads
    window.addEventListener('resize', debounce(handleWindowResize, 250));

    // Handle Google frame load events
    const googleFrame = document.querySelector('.google-frame');
    if (googleFrame) {
        googleFrame.addEventListener('load', handleGoogleFrameLoad);
        googleFrame.addEventListener('error', handleGoogleFrameError);
    }

    // Setup feature card interactions
    setupFeatureCardInteractions();

    // Setup ad interaction tracking
    setupAdInteractionTracking();
}

// Load Adsterra advertisements
function loadAdsterraAds() {
    if (adsterraLoaded) return;

    console.log('Loading Adsterra advertisements...');

    try {
        // Load header banner ad
        loadHeaderBannerAd();

        // Load native banner ad
        loadNativeBannerAd();

        // Load footer banner ad
        loadFooterBannerAd();

        // Load social bar ad
        loadSocialBarAd();

        adsterraLoaded = true;
        adsInitialized = true;

        console.log('Adsterra ads loaded successfully');

    } catch (error) {
        console.error('Error loading Adsterra ads:', error);
        handleAdLoadError();
    }
}

// Load header banner advertisement
function loadHeaderBannerAd() {
    const headerAdContainer = document.getElementById('header-banner-ad');
    if (!headerAdContainer) return;

    // Replace with your actual Adsterra banner code
    const bannerCode = `
        <script type="text/javascript">
            atOptions = {
                'key' : 'YOUR_ADSTERRA_BANNER_KEY_HERE',
                'format' : 'iframe',
                'height' : 90,
                'width' : 728,
                'params' : {}
            };
            document.write('<scr' + 'ipt type="text/javascript" src="//www.topcreativeformat.com/' + atOptions.key + '/invoke.js"></scr' + 'ipt>');
        </script>
    `;

    headerAdContainer.innerHTML = bannerCode;
    removeLoadingIndicator(headerAdContainer);
}

// Load native banner advertisement
function loadNativeBannerAd() {
    const nativeAdContainer = document.getElementById('native-banner-ad');
    if (!nativeAdContainer) return;

    // Replace with your actual Adsterra native banner code
    const nativeCode = `
        <script type="text/javascript">
            atOptions = {
                'key' : 'YOUR_ADSTERRA_NATIVE_KEY_HERE',
                'format' : 'iframe',
                'height' : 250,
                'width' : 300,
                'params' : {}
            };
            document.write('<scr' + 'ipt type="text/javascript" src="//www.topcreativeformat.com/' + atOptions.key + '/invoke.js"></scr' + 'ipt>');
        </script>
    `;

    nativeAdContainer.innerHTML = nativeCode;
    removeLoadingIndicator(nativeAdContainer);
}

// Load footer banner advertisement
function loadFooterBannerAd() {
    const footerAdContainer = document.getElementById('footer-banner-ad');
    if (!footerAdContainer) return;

    // Replace with your actual Adsterra footer banner code
    const footerCode = `
        <script type="text/javascript">
            atOptions = {
                'key' : 'YOUR_ADSTERRA_FOOTER_KEY_HERE',
                'format' : 'iframe',
                'height' : 90,
                'width' : 728,
                'params' : {}
            };
            document.write('<scr' + 'ipt type="text/javascript" src="//www.topcreativeformat.com/' + atOptions.key + '/invoke.js"></scr' + 'ipt>');
        </script>
    `;

    footerAdContainer.innerHTML = footerCode;
    removeLoadingIndicator(footerAdContainer);
}

// Load social bar advertisement
function loadSocialBarAd() {
    const socialBarContainer = document.getElementById('social-bar-ad');
    if (!socialBarContainer) return;

    // Replace with your actual Adsterra social bar code
    const socialBarCode = `
        <script type="text/javascript">
            atOptions = {
                'key' : 'YOUR_ADSTERRA_SOCIAL_BAR_KEY_HERE',
                'format' : 'iframe',
                'height' : 50,
                'width' : 320,
                'params' : {}
            };
            document.write('<scr' + 'ipt type="text/javascript" src="//www.topcreativeformat.com/' + atOptions.key + '/invoke.js"></scr' + 'ipt>');
        </script>
    `;

    socialBarContainer.innerHTML = socialBarCode;
}

// Show loading indicators for ads
function showAdLoadingIndicators() {
    const adContainers = [
        'header-banner-ad',
        'native-banner-ad',
        'footer-banner-ad'
    ];

    adContainers.forEach(containerId => {
        const container = document.getElementById(containerId);
        if (container) {
            container.innerHTML = '<div class="ad-loading"></div><span style="margin-left: 10px; color: #999;">Loading advertisement...</span>';
        }
    });
}

// Remove loading indicator
function removeLoadingIndicator(container) {
    setTimeout(() => {
        const loadingElements = container.querySelectorAll('.ad-loading');
        loadingElements.forEach(element => {
            element.parentNode.removeChild(element);
        });
    }, 1000);
}

// Handle ad loading errors
function handleAdLoadError() {
    console.warn('Some advertisements failed to load');

    const adContainers = document.querySelectorAll('[id$="-ad"]');
    adContainers.forEach(container => {
        if (container.innerHTML.includes('ad-loading')) {
            container.innerHTML = '<div style="color: #999; text-align: center; padding: 20px;">Advertisement</div>';
        }
    });
}

// Setup Google frame security
function setupGoogleFrameSecurity() {
    const googleFrame = document.querySelector('.google-frame');
    if (googleFrame) {
        // Add security attributes
        googleFrame.setAttribute('sandbox', 'allow-scripts allow-same-origin allow-forms allow-popups allow-popups-to-escape-sandbox');
        googleFrame.setAttribute('loading', 'lazy');
    }
}

// Handle Google frame load
function handleGoogleFrameLoad() {
    console.log('Google frame loaded successfully');
    // Add any post-load functionality here
}

// Handle Google frame error
function handleGoogleFrameError() {
    console.warn('Google frame failed to load');
    const googleFrame = document.querySelector('.google-frame');
    if (googleFrame) {
        googleFrame.style.display = 'none';
        const errorMessage = document.createElement('div');
        errorMessage.innerHTML = `
            <div style="text-align: center; padding: 50px; color: #999;">
                <h3>Unable to load Google Search</h3>
                <p>Please check your internet connection and try again.</p>
                <button onclick="location.reload()" style="margin-top: 20px; padding: 10px 20px; background: #4CAF50; color: white; border: none; border-radius: 5px; cursor: pointer;">Retry</button>
            </div>
        `;
        googleFrame.parentNode.appendChild(errorMessage);
    }
}

// Setup feature card interactions
function setupFeatureCardInteractions() {
    const featureCards = document.querySelectorAll('.feature-card');
    featureCards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-5px) scale(1.02)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });
}

// Setup ad interaction tracking
function setupAdInteractionTracking() {
    // Track ad visibility
    if ('IntersectionObserver' in window) {
        const adObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    console.log('Ad visible:', entry.target.id);
                    // Add your ad visibility tracking here
                }
            });
        });

        document.querySelectorAll('[id$="-ad"]').forEach(ad => {
            adObserver.observe(ad);
        });
    }
}

// Handle window resize
function handleWindowResize() {
    // Adjust ad layouts for responsive design
    const sidebarAd = document.querySelector('.sidebar-ad');
    if (sidebarAd && window.innerWidth <= 768) {
        sidebarAd.style.position = 'static';
        sidebarAd.style.transform = 'none';
        sidebarAd.style.width = '100%';
        sidebarAd.style.margin = '20px 0';
    } else if (sidebarAd) {
        sidebarAd.style.position = 'fixed';
        sidebarAd.style.transform = 'translateY(-50%)';
        sidebarAd.style.width = '300px';
        sidebarAd.style.margin = '0';
    }
}

// Setup responsive handling
function setupResponsiveHandling() {
    handleWindowResize(); // Initial call
}

// Initialize interactions
function initializeInteractions() {
    // Add smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
    });
}

// Utility function: Debounce
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Footer link handlers
function showAbout() {
    alert('Web Traffic Inspector v1.0.0\n\nDeveloped by Lasakru\nCopyright © 2025 Googlex Technologies\n\nA professional HTTP/HTTPS traffic analysis tool.');
}

function showSupport() {
    if (confirm('Open support page in external browser?')) {
        window.open('mailto:support@googlex-technologies.com?subject=Web Traffic Inspector Support', '_blank');
    }
}

function showPrivacy() {
    alert('Privacy Policy\n\nWeb Traffic Inspector respects your privacy. All traffic analysis is performed locally on your computer. No data is collected or transmitted to external servers.');
}

function showTerms() {
    alert('Terms of Service\n\nBy using Web Traffic Inspector, you agree to use this software responsibly and in compliance with applicable laws and regulations.');
}

function showContact() {
    if (confirm('Contact support via email?')) {
        window.open('mailto:support@googlex-technologies.com', '_blank');
    }
}


// Load header banner advertisement - UPDATED WITH YOUR CODE
function loadHeaderBannerAd() {
    const headerAdContainer = document.getElementById('header-banner-ad');
    if (!headerAdContainer) return;

    // Your actual Adsterra banner code
    const bannerCode = `
        <script type="text/javascript">
            atOptions = {
                'key' : '7a98005bb568ba89829eacd9dbebec92',
                'format' : 'iframe',
                'height' : 90,
                'width' : 728,
                'params' : {}
            };
        </script>
        <script type="text/javascript" src="//www.highperformanceformat.com/7a98005bb568ba89829eacd9dbebec92/invoke.js"></script>
    `;

    headerAdContainer.innerHTML = bannerCode;
    removeLoadingIndicator(headerAdContainer);
}

// Load footer banner advertisement - UPDATED WITH YOUR CODE
function loadFooterBannerAd() {
    const footerAdContainer = document.getElementById('footer-banner-ad');
    if (!footerAdContainer) return;

    // Using same banner code for footer
    const footerCode = `
        <script type="text/javascript">
            atOptions = {
                'key' : '7a98005bb568ba89829eacd9dbebec92',
                'format' : 'iframe',
                'height' : 90,
                'width' : 728,
                'params' : {}
            };
        </script>
        <script type="text/javascript" src="//www.highperformanceformat.com/7a98005bb568ba89829eacd9dbebec92/invoke.js"></script>
    `;

    footerAdContainer.innerHTML = footerCode;
    removeLoadingIndicator(footerAdContainer);
}


// Export functions for global access
window.WebTrafficInspector = {
    loadAdsterraAds,
    showAbout,
    showSupport,
    showPrivacy,
    showTerms,
    showContact
};
