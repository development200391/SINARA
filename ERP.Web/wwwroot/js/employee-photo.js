(() => {
    const maxFileSizeBytes = 5 * 1024 * 1024;
    const allowedMimeTypes = new Set(['image/jpeg', 'image/jpg', 'image/pjpeg', 'image/png']);
    const allowedExtensions = ['.jpg', '.jpeg', '.png'];
    const outputSize = 300;

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
        let previewObjectUrl = null;

        // The raw file the user picked, kept separate from photoInput.files so that
        // re-cropping always starts from the original photo instead of the
        // already-cropped output that gets swapped into the input on Apply.
        let originalFile = null;

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

            let imageReady = false;
            let modalReady = false;

            const tryInitCropper = () => {
                if (!imageReady || !modalReady || cropper) {
                    return;
                }

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

            cropImage.onload = () => {
                imageReady = true;
                tryInitCropper();
            };
            cropImage.src = objectUrl;

            modalElement.addEventListener('shown.bs.modal', () => {
                modalReady = true;
                tryInitCropper();
            }, { once: true });

            modalInstance.show();
        };

        const showRecropButton = () => {
            if (recropButton instanceof HTMLElement) {
                recropButton.classList.remove('d-none');
            }
        };

        const resetToExistingOrDefault = () => {
            const fallbackPath = photoPreview.dataset.persistedSrc || defaultAvatar;
            photoPreview.src = fallbackPath;
            originalFile = null;
            clearCropFields(cropXInput, cropYInput, cropWidthInput, cropHeightInput);
            if (recropButton instanceof HTMLElement) {
                recropButton.classList.add('d-none');
            }
        };

        photoInput.addEventListener('change', () => {
            clearCropFields(cropXInput, cropYInput, cropWidthInput, cropHeightInput);
            setError('');

            const [file] = photoInput.files ?? [];
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

            originalFile = file;
            openCropModal(URL.createObjectURL(file));
            showRecropButton();
        });

        if (recropButton instanceof HTMLButtonElement) {
            recropButton.addEventListener('click', () => {
                if (!originalFile) {
                    setError('Select a photo first.');
                    return;
                }

                setError('');
                openCropModal(URL.createObjectURL(originalFile));
            });
        }

        applyButton.addEventListener('click', () => {
            if (!cropper || !originalFile) {
                setError('Crop area is not ready yet.');
                return;
            }

            const croppedCanvas = cropper.getCroppedCanvas({
                width: outputSize,
                height: outputSize,
                imageSmoothingQuality: 'high'
            });

            if (!croppedCanvas) {
                setError('Failed to process the cropped photo.');
                return;
            }

            croppedCanvas.toBlob((blob) => {
                if (!blob) {
                    setError('Failed to process the cropped photo.');
                    return;
                }

                // Upload the exact pixels the user just previewed instead of the raw
                // photo plus numeric crop coordinates, so there is nothing left for the
                // client and server to disagree about (EXIF orientation, DPI, rounding, ...).
                const croppedFile = new File([blob], 'cropped-photo.png', { type: 'image/png' });
                const dataTransfer = new DataTransfer();
                dataTransfer.items.add(croppedFile);
                photoInput.files = dataTransfer.files;

                cropXInput.value = '0';
                cropYInput.value = '0';
                cropWidthInput.value = String(croppedCanvas.width);
                cropHeightInput.value = String(croppedCanvas.height);

                if (previewObjectUrl) {
                    URL.revokeObjectURL(previewObjectUrl);
                }
                previewObjectUrl = URL.createObjectURL(blob);
                photoPreview.src = previewObjectUrl;

                const modalInstance = ensureModal();
                if (modalInstance) {
                    modalInstance.hide();
                }

                setError('');
            }, 'image/png');
        });

        if (form instanceof HTMLFormElement) {
            form.addEventListener('submit', (event) => {
                if (!originalFile) {
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
