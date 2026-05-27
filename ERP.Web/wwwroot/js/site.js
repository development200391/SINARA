(() => {
    const root = document.documentElement;
    root.classList.add('sinara-ready');

    const storageKey = 'sinara_theme_primary';
    const defaultColor = '#0f766e';
    const themeInput = document.getElementById('sinara-theme-color-input');
    const resetButton = document.getElementById('sinara-theme-reset');
    const swatches = Array.from(document.querySelectorAll('.sinara-theme-swatch'));

    const clamp = (value, min, max) => Math.min(Math.max(value, min), max);

    const normalizeHex = (value) => {
        if (typeof value !== 'string') {
            return null;
        }

        const color = value.trim();
        const sixDigitMatch = /^#([0-9a-fA-F]{6})$/.exec(color);
        if (sixDigitMatch) {
            return `#${sixDigitMatch[1].toLowerCase()}`;
        }

        const threeDigitMatch = /^#([0-9a-fA-F]{3})$/.exec(color);
        if (!threeDigitMatch) {
            return null;
        }

        const [r, g, b] = threeDigitMatch[1].toLowerCase().split('');
        return `#${r}${r}${g}${g}${b}${b}`;
    };

    const hexToRgb = (hexColor) => {
        const normalized = normalizeHex(hexColor);
        if (!normalized) {
            return null;
        }

        const raw = normalized.slice(1);
        return {
            r: Number.parseInt(raw.slice(0, 2), 16),
            g: Number.parseInt(raw.slice(2, 4), 16),
            b: Number.parseInt(raw.slice(4, 6), 16)
        };
    };

    const rgbToHex = (rgb) => {
        const toHex = (value) => clamp(value, 0, 255).toString(16).padStart(2, '0');
        return `#${toHex(rgb.r)}${toHex(rgb.g)}${toHex(rgb.b)}`;
    };

    const toRgbString = (rgb) => `${rgb.r}, ${rgb.g}, ${rgb.b}`;

    const shadeHex = (hexColor, amount) => {
        const normalizedAmount = clamp(amount, -1, 1);
        const rgb = hexToRgb(hexColor);
        if (!rgb) {
            return hexColor;
        }

        const shaded = {
            r: normalizedAmount < 0
                ? Math.round(rgb.r * (1 + normalizedAmount))
                : Math.round(rgb.r + (255 - rgb.r) * normalizedAmount),
            g: normalizedAmount < 0
                ? Math.round(rgb.g * (1 + normalizedAmount))
                : Math.round(rgb.g + (255 - rgb.g) * normalizedAmount),
            b: normalizedAmount < 0
                ? Math.round(rgb.b * (1 + normalizedAmount))
                : Math.round(rgb.b + (255 - rgb.b) * normalizedAmount)
        };

        return rgbToHex(shaded);
    };

    const getContrastColor = (hexColor) => {
        const rgb = hexToRgb(hexColor);
        if (!rgb) {
            return '#f8fafc';
        }

        const yiq = ((rgb.r * 299) + (rgb.g * 587) + (rgb.b * 114)) / 1000;
        return yiq >= 145 ? '#0f172a' : '#f8fafc';
    };

    const setVar = (name, value) => {
        root.style.setProperty(name, value);
    };

    const saveTheme = (color) => {
        try {
            localStorage.setItem(storageKey, color);
        } catch {
            // Ignore storage failures (private mode / browser policies)
        }
    };

    const clearSavedTheme = () => {
        try {
            localStorage.removeItem(storageKey);
        } catch {
            // Ignore storage failures
        }
    };

    const loadSavedTheme = () => {
        try {
            return localStorage.getItem(storageKey);
        } catch {
            return null;
        }
    };

    const updateSwatches = (activeColor) => {
        swatches.forEach((swatch) => {
            const swatchColor = normalizeHex(swatch.dataset.themeColor);
            const isActive = swatchColor === activeColor;
            swatch.classList.toggle('active', isActive);
            swatch.setAttribute('aria-pressed', isActive ? 'true' : 'false');
        });
    };

    const applyTheme = (color, persist) => {
        const normalized = normalizeHex(color) ?? defaultColor;
        const primaryRgb = hexToRgb(normalized);
        if (!primaryRgb) {
            return;
        }

        const navbarBg = shadeHex(normalized, -0.35);
        const gridHeaderBg = shadeHex(normalized, -0.12);
        const gridHeaderText = getContrastColor(gridHeaderBg);
        const bgAccent = shadeHex(normalized, 0.82);
        const primaryRgbText = toRgbString(primaryRgb);

        setVar('--sinara-primary', normalized);
        setVar('--sinara-primary-rgb', primaryRgbText);
        setVar('--sinara-navbar-bg', navbarBg);
        setVar('--sinara-grid-header-bg', gridHeaderBg);
        setVar('--sinara-grid-header-text', gridHeaderText);
        setVar('--sinara-bg-accent', bgAccent);

        setVar('--bs-primary', normalized);
        setVar('--bs-primary-rgb', primaryRgbText);
        setVar('--bs-link-color-rgb', primaryRgbText);
        setVar('--bs-link-hover-color-rgb', primaryRgbText);
        setVar('--bs-focus-ring-color', `rgba(${primaryRgbText}, 0.28)`);

        if (themeInput instanceof HTMLInputElement) {
            themeInput.value = normalized;
        }

        updateSwatches(normalized);

        if (persist) {
            saveTheme(normalized);
        }
    };

    applyTheme(loadSavedTheme() ?? defaultColor, false);

    swatches.forEach((swatch) => {
        swatch.addEventListener('click', (event) => {
            event.preventDefault();
            const selectedColor = normalizeHex(swatch.dataset.themeColor);
            if (selectedColor) {
                applyTheme(selectedColor, true);
            }
        });
    });

    if (themeInput instanceof HTMLInputElement) {
        themeInput.addEventListener('input', () => {
            applyTheme(themeInput.value, true);
        });
    }

    if (resetButton instanceof HTMLButtonElement) {
        resetButton.addEventListener('click', () => {
            clearSavedTheme();
            applyTheme(defaultColor, false);
        });
    }
})();
