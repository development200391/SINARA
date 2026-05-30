(() => {
    const maxFileSizeBytes = 5 * 1024 * 1024;
    const allowedMimeTypes = new Set(['image/jpeg', 'image/jpg', 'image/pjpeg', 'image/png']);
    const allowedExtensions = ['.jpg', '.jpeg', '.png'];

    const parseNumber = (value) => {
        const parsed = Number.parseFloat(value ?? '');
        return Number.isFinite(parsed) ? parsed : null;
    };

    const hasValidCrop = (cropWidthInput, cropHeightInput) => {
        const width = parseNumber(cropWidthInput.value);
        const height = parseNumber(cropHeightInput.value);
        return width !== null && height !== null && width > 0 && height > 0;
    };

    const clearCropFields = (cropXInput, cropYInput, cropWidthInput, cropHeightInput) => {
        cropXInput.value = '';
        cropYInput.value = '';
        cropWidthInput.value = '';
        cropHeightInput.value = '';
    };

    const isValidFile = (file) => {
        const lowerName = file.name.toLowerCase();
        const hasAllowedExtension = allowedExtensions.some((extension) => lowerName.endsWith(extension));
        if (!hasAllowedExtension) {
            return 'Only JPG, JPEG, or PNG files are allowed.';
        }

        if (!allowedMimeTypes.has((file.type || '').toLowerCase())) {
            return 'Invalid file type. Allowed MIME types: image/jpeg, image/png.';
        }

        if (file.size > maxFileSizeBytes) {
            return 'File size must be 5 MB or less.';
        }

        return null;
    };

    const initPhotoComponent = (component) => {
        const photoInput = component.querySelector('[data-photo-input]');
        const photoPreview = component.querySelector('[data-photo-preview]');
        const cropXInput = component.querySelector('[data-photo-crop-x]');
        const cropYInput = component.querySelector('[data-photo-crop-y]');
        const cropWidthInput = component.querySelector('[data-photo-crop-width]');
        const cropHeightInput = component.querySelector('[data-photo-crop-height]');
        const recropButton = component.querySelector('[data-photo-recrop]');
        const errorElement = component.querySelector('[data-photo-error]');
        const modalElement = component.querySelector('[data-photo-modal]');
        const cropImage = component.querySelector('[data-photo-cropper-image]');
        const applyButton = component.querySelector('[data-photo-apply]');

        if (!(photoInput instanceof HTMLInputElement)
            || !(photoPreview instanceof HTMLImageElement)
            || !(cropXInput instanceof HTMLInputElement)
            || !(cropYInput instanceof HTMLInputElement)
            || !(cropWidthInput instanceof HTMLInputElement)
            || !(cropHeightInput instanceof HTMLInputElement)
            || !(modalElement instanceof HTMLElement)
            || !(cropImage instanceof HTMLImageElement)
            || !(applyButton instanceof HTMLButtonElement)) {
            return;
        }

        const defaultAvatar = component.dataset.defaultAvatar || '/images/default-avatar.svg';
        const form = component.closest('form');

        const setError = (message) => {
            if (!(errorElement instanceof HTMLElement)) {
                return;
            }

            if (!message) {
                errorElement.classList.add('d-none');
                errorElement.textContent = '';
                return;
            }

            errorElement.classList.remove('d-none');
            errorElement.textContent = message;
        };

        let cropper = null;
        let modal = null;
        let activeObjectUrl = null;

        const ensureModal = () => {
            if (modal || typeof bootstrap === 'undefined' || typeof bootstrap.Modal !== 'function') {
                return modal;
            }

            modal = new bootstrap.Modal(modalElement, {
                backdrop: 'static',
                keyboard: false
            });

            modalElement.addEventListener('hidden.bs.modal', () => {
                if (cropper) {
                    cropper.destroy();
                    cropper = null;
                }

                if (activeObjectUrl) {
                    URL.revokeObjectURL(activeObjectUrl);
                    activeObjectUrl = null;
                }
            });

            return modal;
        };

        const openCropModal = (objectUrl) => {
            const modalInstance = ensureModal();
            if (!modalInstance) {
                setError('Cropper library failed to initialize.');
                return;
            }

            if (cropper) {
                cropper.destroy();
                cropper = null;
            }

            if (activeObjectUrl) {
                URL.revokeObjectURL(activeObjectUrl);
            }

            activeObjectUrl = objectUrl;
            cropImage.src = objectUrl;

            cropImage.onload = () => {
                cropper = new Cropper(cropImage, {
                    aspectRatio: 1,
                    viewMode: 1,
                    dragMode: 'move',
                    autoCropArea: 1,
                    responsive: true,
                    background: false,
                    movable: true,
                    zoomable: true,
                    rotatable: false,
                    scalable: false
                });
            };

            modalInstance.show();
        };

        const getSelectedFile = () => {
            const [file] = photoInput.files ?? [];
            return file ?? null;
        };

        const showRecropButton = () => {
            if (recropButton instanceof HTMLElement) {
                recropButton.classList.remove('d-none');
            }
        };

        const resetToExistingOrDefault = () => {
            const fallbackPath = photoPreview.dataset.persistedSrc || defaultAvatar;
            photoPreview.src = fallbackPath;
            clearCropFields(cropXInput, cropYInput, cropWidthInput, cropHeightInput);
            if (recropButton instanceof HTMLElement) {
                recropButton.classList.add('d-none');
            }
        };

        photoInput.addEventListener('change', () => {
            clearCropFields(cropXInput, cropYInput, cropWidthInput, cropHeightInput);
            setError('');

            const file = getSelectedFile();
            if (!file) {
                resetToExistingOrDefault();
                return;
            }

            const validationMessage = isValidFile(file);
            if (validationMessage) {
                setError(validationMessage);
                photoInput.value = '';
                resetToExistingOrDefault();
                return;
            }

            if (typeof Cropper === 'undefined') {
                setError('Cropper library is not loaded.');
                return;
            }

            const objectUrl = URL.createObjectURL(file);
            openCropModal(objectUrl);
            showRecropButton();
        });

        if (recropButton instanceof HTMLButtonElement) {
            recropButton.addEventListener('click', () => {
                const file = getSelectedFile();
                if (!file) {
                    setError('Select a photo first.');
                    return;
                }

                setError('');
                const objectUrl = URL.createObjectURL(file);
                openCropModal(objectUrl);
            });
        }

        applyButton.addEventListener('click', () => {
            if (!cropper) {
                setError('Crop area is not ready yet.');
                return;
            }

            const data = cropper.getData(true);
            cropXInput.value = Number.isFinite(data.x) ? data.x.toFixed(2) : '';
            cropYInput.value = Number.isFinite(data.y) ? data.y.toFixed(2) : '';
            cropWidthInput.value = Number.isFinite(data.width) ? data.width.toFixed(2) : '';
            cropHeightInput.value = Number.isFinite(data.height) ? data.height.toFixed(2) : '';

            const previewCanvas = cropper.getCroppedCanvas({
                width: 300,
                height: 300,
                imageSmoothingQuality: 'high'
            });

            if (previewCanvas) {
                photoPreview.src = previewCanvas.toDataURL('image/webp', 0.85);
            }

            const modalInstance = ensureModal();
            if (modalInstance) {
                modalInstance.hide();
            }

            setError('');
        });

        if (form instanceof HTMLFormElement) {
            form.addEventListener('submit', (event) => {
                const file = getSelectedFile();
                if (!file) {
                    return;
                }

                if (!hasValidCrop(cropWidthInput, cropHeightInput)) {
                    event.preventDefault();
                    setError('Please crop the photo before saving.');
                    return;
                }

                setError('');
            });
        }
    };

    const bootstrapPhotoComponents = () => {
        document.querySelectorAll('.sinara-employee-photo-component').forEach((component) => {
            if (component instanceof HTMLElement) {
                initPhotoComponent(component);
            }
        });
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', bootstrapPhotoComponents);
    } else {
        bootstrapPhotoComponents();
    }
})();