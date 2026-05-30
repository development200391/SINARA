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

(() => {
    const root = document.documentElement;

    const readLocale = () => {
        const lang = (root.getAttribute('lang') || 'en').toLowerCase();
        return lang.startsWith('id') ? 'id-ID' : 'en-US';
    };

    const readDecimalSeparator = () => root.getAttribute('data-decimal-separator') === ',' ? ',' : '.';

    const normalizeFreeNumber = (value) => {
        const raw = String(value ?? '').trim();
        if (!raw) {
            return '';
        }

        const compact = raw
            .replace(/\s+/g, '')
            .replace(/\u00A0/g, '')
            .replace(/[^0-9.,-]/g, '');

        if (!compact) {
            return '';
        }

        const isNegative = compact.includes('-');
        const unsigned = compact.replace(/-/g, '');
        const lastDot = unsigned.lastIndexOf('.');
        const lastComma = unsigned.lastIndexOf(',');
        let normalized = unsigned;

        if (lastDot >= 0 && lastComma >= 0) {
            normalized = lastDot > lastComma
                ? unsigned.replace(/,/g, '')
                : unsigned.replace(/\./g, '').replace(',', '.');
        } else if (lastComma >= 0) {
            const commaCount = (unsigned.match(/,/g) || []).length;
            if (commaCount > 1) {
                normalized = unsigned.replace(/,/g, '');
            } else {
                const digitsAfter = unsigned.length - lastComma - 1;
                normalized = digitsAfter === 3
                    ? unsigned.replace(/,/g, '')
                    : unsigned.replace(',', '.');
            }
        } else if (lastDot >= 0) {
            const dotCount = (unsigned.match(/\./g) || []).length;
            if (dotCount > 1) {
                normalized = unsigned.replace(/\./g, '');
            } else {
                const digitsAfter = unsigned.length - lastDot - 1;
                normalized = digitsAfter === 3
                    ? unsigned.replace(/\./g, '')
                    : unsigned;
            }
        }

        normalized = normalized.replace(/[^0-9.]/g, '');

        const parts = normalized.split('.');
        const integerRaw = parts.shift() || '0';
        const integerPart = integerRaw.replace(/^0+(?=\d)/, '') || '0';
        const fractionPart = parts.join('');
        const withSign = isNegative ? `-${integerPart}` : integerPart;

        return fractionPart.length > 0
            ? `${withSign}.${fractionPart}`
            : withSign;
    };

    const formatDisplay = (value, locale, decimals) => {
        const normalized = normalizeFreeNumber(value);
        if (!normalized) {
            return '';
        }

        const parsed = Number.parseFloat(normalized);
        if (!Number.isFinite(parsed)) {
            return '';
        }

        return parsed.toLocaleString(locale, {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        });
    };

    const toModelValue = (value, decimalSeparator) => {
        const normalized = normalizeFreeNumber(value);
        if (!normalized) {
            return '';
        }

        return decimalSeparator === ','
            ? normalized.replace('.', ',')
            : normalized;
    };

    const bind = (input) => {
        if (!(input instanceof HTMLInputElement) || input.dataset.sinaraTextNumberBound === '1') {
            return;
        }

        input.dataset.sinaraTextNumberBound = '1';

        const locale = input.dataset.textNumberLocale || readLocale();
        const decimalsRaw = Number.parseInt(input.dataset.textNumberDecimals || '2', 10);
        const decimals = Number.isFinite(decimalsRaw)
            ? Math.min(Math.max(decimalsRaw, 0), 6)
            : 2;
        const decimalSeparator = input.dataset.textNumberDecimalSeparator || readDecimalSeparator();
        const form = input.form;

        const selectAllValue = () => {
            if (input.readOnly || input.disabled) {
                return;
            }

            window.requestAnimationFrame(() => {
                input.select();
            });
        };

        input.addEventListener('focus', selectAllValue);
        input.addEventListener('click', selectAllValue);
        input.addEventListener('blur', () => {
            const formatted = formatDisplay(input.value, locale, decimals);
            input.value = formatted || input.value.trim();
        });

        const initialValue = formatDisplay(input.value, locale, decimals);
        if (initialValue) {
            input.value = initialValue;
        }

        if (form instanceof HTMLFormElement) {
            // Capture phase: normalize before jQuery unobtrusive validation checks numeric range.
            form.addEventListener('submit', (event) => {
                const originalValue = input.value;
                input.value = toModelValue(input.value, decimalSeparator);

                window.setTimeout(() => {
                    if (event.defaultPrevented) {
                        const reverted = formatDisplay(originalValue, locale, decimals);
                        input.value = reverted || originalValue;
                    }
                }, 0);
            }, true);
        }
    };

    const bindAll = () => {
        document.querySelectorAll('input.sinara-text-number-input').forEach((element) => {
            if (element instanceof HTMLInputElement) {
                bind(element);
            }
        });
    };

    const patchJqueryValidation = () => {
        const jq = window.jQuery;
        if (!jq || !jq.validator || !jq.validator.methods) {
            return false;
        }

        if (jq.validator.methods.sinaraTextNumberPatched) {
            return true;
        }

        const isTargetInput = (element) => element instanceof HTMLInputElement && element.classList.contains('sinara-text-number-input');

        const parseRuleValue = (value) => {
            const normalized = normalizeFreeNumber(value);
            if (!normalized) {
                return Number.NaN;
            }

            const parsed = Number.parseFloat(normalized);
            return Number.isFinite(parsed) ? parsed : Number.NaN;
        };

        const originalNumber = jq.validator.methods.number;
        const originalRange = jq.validator.methods.range;
        const originalMin = jq.validator.methods.min;
        const originalMax = jq.validator.methods.max;

        jq.validator.methods.number = function (value, element) {
            if (!isTargetInput(element)) {
                return originalNumber.call(this, value, element);
            }

            if (this.optional(element)) {
                return true;
            }

            return !Number.isNaN(parseRuleValue(value));
        };

        jq.validator.methods.range = function (value, element, params) {
            if (!isTargetInput(element)) {
                return originalRange.call(this, value, element, params);
            }

            if (this.optional(element)) {
                return true;
            }

            const parsed = parseRuleValue(value);
            return !Number.isNaN(parsed)
                && parsed >= Number(params[0])
                && parsed <= Number(params[1]);
        };

        jq.validator.methods.min = function (value, element, param) {
            if (!isTargetInput(element)) {
                return originalMin.call(this, value, element, param);
            }

            if (this.optional(element)) {
                return true;
            }

            const parsed = parseRuleValue(value);
            return !Number.isNaN(parsed) && parsed >= Number(param);
        };

        jq.validator.methods.max = function (value, element, param) {
            if (!isTargetInput(element)) {
                return originalMax.call(this, value, element, param);
            }

            if (this.optional(element)) {
                return true;
            }

            const parsed = parseRuleValue(value);
            return !Number.isNaN(parsed) && parsed <= Number(param);
        };

        jq.validator.methods.sinaraTextNumberPatched = true;
        return true;
    };

    const scheduleValidationPatch = () => {
        if (patchJqueryValidation()) {
            return;
        }

        let tries = 0;
        const timer = window.setInterval(() => {
            tries += 1;
            if (patchJqueryValidation() || tries >= 50) {
                window.clearInterval(timer);
            }
        }, 100);
    };

    window.SinaraTextNumber = {
        bind,
        bindAll,
        normalizeFreeNumber,
        formatDisplay,
        toModelValue
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', bindAll);
    } else {
        bindAll();
    }

    scheduleValidationPatch();
})();
