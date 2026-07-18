// LinkUpPro - Pure JS Helpers (no DOM dependencies for unit testability)
// These helpers are available globally as window.LinkUpPro

(function (window) {
    'use strict';

    // ============================================================
    // 0. THEME MANAGER (dark/light mode)
    // ============================================================

    const Theme = (() => {
        const STORAGE_KEY = 'linkup-theme';

        function getStored() {
            try { return localStorage.getItem(STORAGE_KEY); } catch (e) { return null; }
        }

        function setStored(value) {
            try { localStorage.setItem(STORAGE_KEY, value); } catch (e) { /* ignore */ }
        }

        function systemPrefers() {
            return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
                ? 'dark'
                : 'light';
        }

        function current() {
            return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
        }

        function resolveInitial() {
            const stored = getStored();
            if (stored === 'dark' || stored === 'light') return stored;
            return systemPrefers();
        }

        function apply(value) {
            if (value === 'dark') {
                document.documentElement.classList.add('dark');
            } else {
                document.documentElement.classList.remove('dark');
            }
        }

        function init() {
            apply(resolveInitial());
        }

        function set(value) {
            const v = value === 'dark' ? 'dark' : 'light';
            setStored(v);
            apply(v);
            updateButtonUI();
        }

        function toggle() {
            set(current() === 'dark' ? 'light' : 'dark');
        }

        function updateButtonUI() {
            const btn = document.getElementById('dark-mode-toggle');
            const text = document.getElementById('dark-mode-text');
            if (!btn) return;
            const isDark = current() === 'dark';
            const existingIcon = document.getElementById('dark-mode-icon');
            if (existingIcon) existingIcon.remove();
            const newIcon = document.createElement('i');
            newIcon.id = 'dark-mode-icon';
            newIcon.setAttribute('data-lucide', isDark ? 'sun' : 'moon');
            newIcon.className = 'w-5 h-5 shrink-0';
            btn.insertBefore(newIcon, btn.firstChild);
            if (text) text.textContent = isDark ? 'Modo Claro' : 'Modo Oscuro';
            if (window.lucide) window.lucide.createIcons();
        }

        function bindToggle() {
            const btn = document.getElementById('dark-mode-toggle');
            if (!btn || btn.dataset.bound) return;
            btn.dataset.bound = '1';
            btn.addEventListener('click', function () {
                if (document.startViewTransition) {
                    const transition = document.startViewTransition(() => {
                        toggle();
                    });
                } else {
                    toggle();
                }
            });
            updateButtonUI();
        }

        return { init, set, toggle, current, getStored, setStored, bindToggle, updateButtonUI };
    })();

    // Run theme init synchronously before any paint to avoid flash
    Theme.init();

    // ============================================================
    // 1. PURE HELPERS (no DOM access, unit-testable)
    // ============================================================

    function formatDate(iso, options) {
        if (!iso) return '';
        const defaultOpts = {
            day: '2-digit', month: 'short', year: 'numeric',
            hour: '2-digit', minute: '2-digit', hour12: false
        };
        try {
            return new Date(iso).toLocaleDateString('es-DO', Object.assign({}, defaultOpts, options || {}));
        } catch (e) { return iso; }
    }

    function formatTimeAgo(iso) {
        if (!iso) return '';
        const diff = Math.floor((Date.now() - new Date(iso).getTime()) / 1000);
        if (diff < 60) return 'hace unos segundos';
        if (diff < 3600) return 'hace ' + Math.floor(diff / 60) + ' min';
        if (diff < 86400) return 'hace ' + Math.floor(diff / 3600) + ' h';
        if (diff < 604800) return 'hace ' + Math.floor(diff / 86400) + ' d';
        return formatDate(iso, { hour: undefined, minute: undefined });
    }

    function debounce(fn, ms) {
        let t;
        return function () {
            const args = arguments;
            const ctx = this;
            clearTimeout(t);
            t = setTimeout(function () { fn.apply(ctx, args); }, ms);
        };
    }

    function extractYouTubeId(url) {
        if (!url) return null;
        const patterns = [
            /(?:youtube\.com\/embed\/)([a-zA-Z0-9_-]{11})/,
            /(?:youtube\.com\/watch\?v=)([a-zA-Z0-9_-]{11})/,
            /(?:youtu\.be\/)([a-zA-Z0-9_-]{11})/,
            /(?:youtube\.com\/shorts\/)([a-zA-Z0-9_-]{11})/
        ];
        for (let i = 0; i < patterns.length; i++) {
            const match = url.match(patterns[i]);
            if (match) return match[1];
        }
        return null;
    }

    function coordsToIndex(x, y) { return y * 12 + x; }
    function indexToCoords(i) { return { x: i % 12, y: Math.floor(i / 12) }; }

    function getShipCells(startX, startY, size, direction) {
        const cells = [];
        for (let i = 0; i < size; i++) {
            let x = startX, y = startY;
            if (direction === 0) y -= i;
            else if (direction === 1) y += i;
            else if (direction === 2) x -= i;
            else if (direction === 3) x += i;
            if (x >= 0 && x < 12 && y >= 0 && y < 12) cells.push({ x: x, y: y });
        }
        return cells;
    }

    function isValidPlacement(startX, startY, size, direction, occupiedCells) {
        const cells = getShipCells(startX, startY, size, direction);
        if (cells.length !== size) return false;
        for (let i = 0; i < cells.length; i++) {
            const c = cells[i];
            for (let j = 0; j < occupiedCells.length; j++) {
                if (occupiedCells[j].x === c.x && occupiedCells[j].y === c.y) return false;
            }
        }
        return true;
    }

    function buildYouTubeEmbedUrl(url) {
        const videoId = extractYouTubeId(url);
        return videoId ? 'https://www.youtube.com/embed/' + videoId : '';
    }

    // ============================================================
    // 2. DOM HELPERS (init at DOMContentLoaded)
    // ============================================================

    const Toast = (() => {
        let container = null;
        function getContainer() {
            if (!container) {
                container = document.createElement('div');
                container.id = 'toast-container';
                container.className = 'fixed top-4 right-4 z-[9999] space-y-2 pointer-events-none';
                document.body.appendChild(container);
            }
            return container;
        }
        function show(type, message, opts) {
            opts = opts || {};
            const colors = {
                success: 'bg-green-50 dark:bg-green-900/30 border-green-200 dark:border-green-700 text-green-700 dark:text-green-300',
                error: 'bg-red-50 dark:bg-red-900/30 border-red-200 dark:border-red-700 text-red-700 dark:text-red-300',
                warning: 'bg-amber-50 dark:bg-amber-900/30 border-amber-200 dark:border-amber-700 text-amber-700 dark:text-amber-300',
                info: 'bg-blue-50 dark:bg-blue-900/30 border-blue-200 dark:border-blue-700 text-blue-700 dark:text-blue-300'
            };
            const icons = { success: 'check-circle', error: 'x-circle', warning: 'alert-triangle', info: 'info' };
            const el = document.createElement('div');
            el.className = `toast-enter pointer-events-auto flex items-center gap-3 px-4 py-3 border ${colors[type]} rounded-xl shadow-lg backdrop-blur-sm min-w-[250px] max-w-md`;
            el.setAttribute('role', 'alert');
            el.innerHTML = `<i data-lucide="${icons[type]}" class="w-5 h-5 shrink-0"></i><span class="text-sm font-medium flex-1"></span>`;
            el.querySelector('span').textContent = message;
            getContainer().appendChild(el);
            if (window.lucide) window.lucide.createIcons();
            const dur = opts.duration || 4000;
            setTimeout(() => {
                el.style.opacity = '0';
                el.style.transform = 'translateX(100%)';
                el.style.transition = 'all 0.3s';
                setTimeout(() => el.remove(), 300);
            }, dur);
        }
        return {
            success: (msg, opts) => show('success', msg, opts),
            error: (msg, opts) => show('error', msg, opts),
            warning: (msg, opts) => show('warning', msg, opts),
            info: (msg, opts) => show('info', msg, opts)
        };
    })();

    function confirmAction(title, text, confirmText) {
        return new Promise(function (resolve) {
            if (window.Swal) {
                Swal.fire({
                    title: title || '¿Está seguro?',
                    text: text || 'Esta acción no se puede deshacer.',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: confirmText || 'Sí, confirmar',
                    cancelButtonText: 'Cancelar',
                    confirmButtonColor: '#4f46e5',
                    cancelButtonColor: '#6b7280',
                    reverseButtons: true
                }).then(function (r) { resolve(r.isConfirmed); });
            } else {
                resolve(confirm(title || '¿Está seguro?'));
            }
        });
    }

    // Password toggle (any view with [data-toggle-password])
    function initPasswordToggles() {
        document.querySelectorAll('[data-toggle-password]').forEach(function (btn) {
            if (btn.dataset.bound) return;
            btn.dataset.bound = '1';
            btn.addEventListener('click', function () {
                const input = document.getElementById(btn.getAttribute('data-toggle-password'));
                if (!input) return;
                const icon = btn.querySelector('[data-lucide]');
                const isPwd = input.type === 'password';
                input.type = isPwd ? 'text' : 'password';
                if (icon) {
                    icon.setAttribute('data-lucide', isPwd ? 'eye-off' : 'eye');
                    if (window.lucide) window.lucide.createIcons();
                }
            });
        });
    }

    // Password strength meter
    function initPasswordStrength() {
        const input = document.getElementById('password-input');
        if (!input || input.dataset.bound) return;
        input.dataset.bound = '1';
        input.addEventListener('input', function () {
            const val = this.value;
            const checks = [
                val.length >= 8,
                /[A-Z]/.test(val),
                /[a-z]/.test(val),
                /[0-9]/.test(val),
                /[^A-Za-z0-9]/.test(val)
            ];
            const score = checks.filter(Boolean).length;
            const bar = document.getElementById('strength-bar');
            const text = document.getElementById('strength-text');
            if (bar && text) {
                const colors = ['bg-red-500', 'bg-red-500', 'bg-yellow-500', 'bg-blue-500', 'bg-emerald-500'];
                const labels = ['Muy débil', 'Débil', 'Media', 'Fuerte', 'Muy fuerte'];
                const idx = score === 0 ? 0 : score - 1;
                bar.className = 'h-full transition-all duration-300 rounded-full ' + colors[idx];
                bar.style.width = (score * 20) + '%';
                text.textContent = 'Fortaleza: ' + labels[idx];
            }
        });
    }

    // Toggle between Image and YouTube fields in post form
    function togglePostType(type) {
        const imgField = document.getElementById('image-field');
        const ytField = document.getElementById('youtube-field');
        if (!imgField || !ytField) return;
        if (type === 1) {
            imgField.classList.remove('hidden');
            ytField.classList.add('hidden');
            const ytInput = ytField.querySelector('input');
            if (ytInput) ytInput.value = '';
        } else {
            imgField.classList.add('hidden');
            ytField.classList.remove('hidden');
            const fileInput = imgField.querySelector('input[type="file"]');
            if (fileInput) fileInput.value = '';
        }
    }

    function toggleEditPostType(type) {
        const imageField = document.getElementById('image-field-edit');
        const youtubeField = document.getElementById('youtube-field-edit');
        if (!imageField || !youtubeField) return;

        imageField.classList.toggle('hidden', type !== 1);
        youtubeField.classList.toggle('hidden', type === 1);
        if (window.lucide) window.lucide.createIcons();
    }

    function initPostTypeToggles() {
        document.querySelectorAll('[data-post-type]').forEach(function (input) {
            if (input.dataset.bound) return;
            input.dataset.bound = '1';
            input.addEventListener('change', function () {
                togglePostType(parseInt(input.dataset.postType, 10));
            });
        });

        document.querySelectorAll('[data-edit-post-type]').forEach(function (input) {
            if (input.dataset.bound) return;
            input.dataset.bound = '1';
            input.addEventListener('change', function () {
                toggleEditPostType(parseInt(input.dataset.editPostType, 10));
            });
        });
    }

    // Image preview in create-post form
    function initImagePreview() {
        const fileInput = document.querySelector('input[name="CreatePost.ImageFile"]');
        const preview = document.getElementById('image-preview');
        if (fileInput && preview && !fileInput.dataset.bound) {
            fileInput.dataset.bound = '1';
            fileInput.addEventListener('change', function () {
                if (this.files && this.files[0]) {
                    preview.src = URL.createObjectURL(this.files[0]);
                    preview.classList.remove('hidden');
                } else {
                    preview.classList.add('hidden');
                }
            });
        }
    }

    // Char counter on post textarea
    function initCharCounter() {
        const content = document.getElementById('post-content');
        const counter = document.getElementById('char-count');
        if (content && counter && !content.dataset.bound) {
            content.dataset.bound = '1';
            counter.textContent = content.value.length;
            content.addEventListener('input', function () {
                counter.textContent = this.value.length;
            });
        }
    }

    function initYouTubePreview() {
        var input = document.getElementById('youtube-url-input');
        if (!input || input.dataset.bound) return;
        input.dataset.bound = '1';

        input.addEventListener('input', window.LinkUpPro.debounce(function () {
            var url = this.value.trim();
            var videoId = window.LinkUpPro.extractYouTubeId(url);
            var preview = document.getElementById('youtube-preview');
            var thumbnail = document.getElementById('youtube-thumbnail');
            var title = document.getElementById('youtube-video-title');
            var error = document.getElementById('youtube-error');

            if (videoId) {
                var thumbUrl = 'https://img.youtube.com/vi/' + videoId + '/hqdefault.jpg';
                thumbnail.src = thumbUrl;
                title.textContent = 'YouTube ID: ' + videoId;
                preview.classList.remove('hidden');
                error.classList.add('hidden');
            } else if (url.length > 0) {
                preview.classList.add('hidden');
                error.classList.remove('hidden');
            } else {
                preview.classList.add('hidden');
                error.classList.add('hidden');
            }
        }, 500));
    }

    // Register wizard (multi-step)
    function initRegisterWizard() {
        const form = document.getElementById('register-form');
        if (!form) return;
        const stepper = document.getElementById('register-stepper');
        if (!stepper) return;

        let currentStep = 1;
        const totalSteps = 4;
        const panels = form.querySelectorAll('[data-step-panel]');
        const indicators = stepper.querySelectorAll('.step-indicator');
        const lines = stepper.querySelectorAll('.step-line');
        const btnPrev = document.getElementById('btn-prev');
        const btnNext = document.getElementById('btn-next');
        const btnSubmit = document.getElementById('btn-submit');

        // Phone input mask for step 2
        function initPhoneMask(container) {
            container.querySelectorAll('[data-phone-mask]').forEach(input => {
                input.addEventListener('input', function () {
                    var val = this.value.replace(/\D/g, '').slice(0, 10);
                    var formatted = '';
                    if (val.length > 0) formatted = val.slice(0, 3);
                    if (val.length > 3) formatted += '-' + val.slice(3, 6);
                    if (val.length > 6) formatted += '-' + val.slice(6, 10);
                    this.value = formatted;
                });
            });
        }

        function updateButtonVisibility(step) {
            btnPrev.style.display = step === 1 ? 'none' : 'inline-flex';
            btnNext.style.display = step === totalSteps ? 'none' : 'inline-flex';
            btnSubmit.style.display = step !== totalSteps ? 'none' : 'inline-flex';
        }

        function showStep(step) {
            currentStep = step;
            panels.forEach(p => {
                p.classList.toggle('hidden', parseInt(p.dataset.stepPanel) !== step);
            });
            indicators.forEach((ind, i) => {
                const idx = i + 1;
                const numEl = ind.querySelector('.step-num');
                const iconEl = ind.querySelector('.completed-icon');
                if (idx < step) {
                    ind.className = 'step-indicator flex items-center justify-center w-9 h-9 rounded-full text-sm font-black transition-all bg-indigo-600 text-white';
                    if (numEl) numEl.classList.add('hidden');
                    if (iconEl) iconEl.classList.remove('hidden');
                } else if (idx === step) {
                    ind.className = 'step-indicator flex items-center justify-center w-9 h-9 rounded-full text-sm font-black transition-all bg-gradient-to-br from-indigo-600 to-violet-600 text-white shadow-lg shadow-indigo-200';
                    if (numEl) numEl.classList.remove('hidden');
                    if (iconEl) iconEl.classList.add('hidden');
                } else {
                    ind.className = 'step-indicator flex items-center justify-center w-9 h-9 rounded-full text-sm font-black transition-all bg-gray-100 dark:bg-gray-700 text-gray-400';
                    if (numEl) numEl.classList.remove('hidden');
                    if (iconEl) iconEl.classList.add('hidden');
                }
            });
            lines.forEach((line, i) => {
                line.classList.toggle('bg-indigo-500', i + 1 < step);
                line.classList.toggle('bg-gray-200', i + 1 >= step);
                line.classList.toggle('dark:bg-gray-700', i + 1 >= step);
            });
            updateButtonVisibility(step);
            if (window.lucide) window.lucide.createIcons();
        }

        function validateStep(step) {
            const panel = form.querySelector(`[data-step-panel="${step}"]`);
            if (!panel) return true;
            let valid = true;
            let firstInvalid = null;
            panel.querySelectorAll('[data-step-input]').forEach(input => {
                if (input.type === 'file') {
                    if (!input.files || input.files.length === 0) {
                        valid = false;
                        if (!firstInvalid) firstInvalid = input;
                    }
                } else if (!input.value.trim()) {
                    valid = false;
                    if (!firstInvalid) firstInvalid = input;
                }
            });
            if (!valid) {
                Toast.warning(step === totalSteps
                    ? 'Debe seleccionar una foto de perfil para continuar.'
                    : 'Por favor complete todos los campos antes de continuar.');
                if (firstInvalid) firstInvalid.focus();
            }
            return valid;
        }

        btnNext && btnNext.addEventListener('click', function () {
            if (validateStep(currentStep) && currentStep < totalSteps) {
                showStep(currentStep + 1);
            }
        });
        btnPrev && btnPrev.addEventListener('click', function () {
            if (currentStep > 1) showStep(currentStep - 1);
        });
        form.addEventListener('submit', function () {
            btnPrev.disabled = true;
            btnNext.disabled = true;
            btnSubmit.disabled = true;
            btnSubmit.innerHTML = '<i data-lucide="loader-circle" class="w-4 h-4 animate-spin"></i> Registrando...';
            if (window.lucide) lucide.createIcons();
        });

        // Init phone mask on step 2 panel
        const step2 = form.querySelector('[data-step-panel="2"]');
        if (step2) initPhoneMask(step2);

        showStep(1);
    }

    // Profile photo drop zone + preview
    function initPhotoDropZone() {
        const zone = document.getElementById('photo-drop-zone');
        const input = document.getElementById('photo-input');
        const preview = document.getElementById('photo-preview');
        const zone_default = document.getElementById('photo-preview-zone');
        if (!zone || !input || !preview) return;

        zone.addEventListener('click', () => input.click());
        input.addEventListener('change', function () {
            if (this.files && this.files[0]) {
                preview.src = URL.createObjectURL(this.files[0]);
                preview.classList.remove('hidden');
                if (zone_default) zone_default.classList.add('hidden');
            }
        });
    }

    // Tab switching (FriendRequests + others)
    function initTabs() {
        document.querySelectorAll('[data-tabs]').forEach(function (container) {
            const buttons = container.querySelectorAll('.tab-btn');
            const contents = container.querySelectorAll('.tab-content');
            buttons.forEach(btn => {
                if (btn.dataset.bound) return;
                btn.dataset.bound = '1';
                btn.addEventListener('click', function () {
                    const tab = btn.dataset.tab;
                    buttons.forEach(b => {
                        b.classList.remove('bg-white', 'dark:bg-gray-800', 'shadow-sm', 'text-indigo-600', 'dark:text-indigo-400');
                        b.classList.add('text-gray-600', 'dark:text-gray-400');
                    });
                    btn.classList.add('bg-white', 'dark:bg-gray-800', 'shadow-sm', 'text-indigo-600', 'dark:text-indigo-400');
                    btn.classList.remove('text-gray-600', 'dark:text-gray-400');
                    contents.forEach(c => c.classList.add('hidden'));
                    const target = container.querySelector(`#tab-${tab}`);
                    if (target) target.classList.remove('hidden');
                });
            });
        });
    }

    // Sidebar (mobile drawer)
    function initSidebar() {
        const sidebar = document.getElementById('sidebar');
        const backdrop = document.getElementById('sidebar-backdrop');
        const openBtn = document.getElementById('open-sidebar-btn');
        const closeBtn = document.getElementById('close-sidebar-btn');
        if (!sidebar || sidebar.dataset.bound) return;
        sidebar.dataset.bound = '1';
        function toggle() {
            sidebar.classList.toggle('open');
            if (backdrop) backdrop.classList.toggle('visible');
        }
        if (openBtn) openBtn.addEventListener('click', toggle);
        if (closeBtn) closeBtn.addEventListener('click', toggle);
        if (backdrop) backdrop.addEventListener('click', toggle);
    }

    // User dropdown
    function closeAllDropdowns(except) {
        const pairs = [
            { btnId: 'user-menu-btn', ddId: 'user-dropdown' },
            { btnId: 'notification-menu-btn', ddId: 'notification-dropdown' }
        ];
        pairs.forEach(function (pair) {
            if (pair.btnId === except) return;
            var btn = document.getElementById(pair.btnId);
            var dd = document.getElementById(pair.ddId);
            if (dd && !dd.classList.contains('hidden')) {
                dd.classList.add('hidden');
                if (btn) btn.setAttribute('aria-expanded', 'false');
            }
        });
    }

    function initUserDropdown() {
        const btn = document.getElementById('user-menu-btn');
        const dd = document.getElementById('user-dropdown');
        if (!btn || !dd || btn.dataset.bound) return;
        btn.dataset.bound = '1';
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            const isOpen = dd.classList.contains('hidden');
            if (isOpen) closeAllDropdowns('user-menu-btn');
            dd.classList.toggle('hidden', !isOpen);
            btn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        });
        document.addEventListener('click', function (e) {
            if (!btn.contains(e.target)) {
                dd.classList.add('hidden');
                btn.setAttribute('aria-expanded', 'false');
            }
        });
    }

    function initNotificationDropdown() {
        const btn = document.getElementById('notification-menu-btn');
        const dd = document.getElementById('notification-dropdown');
        const content = document.getElementById('notification-dropdown-content');
        if (!btn || !dd || !content || btn.dataset.bound) return;

        btn.dataset.bound = '1';
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            const shouldOpen = dd.classList.contains('hidden');

            if (!shouldOpen) {
                // Close
                dd.classList.add('hidden');
                btn.setAttribute('aria-expanded', 'false');
                return;
            }

            closeAllDropdowns('notification-menu-btn');

            if (content.dataset.loaded) {
                // Already loaded → show immediately
                dd.classList.remove('hidden');
                btn.setAttribute('aria-expanded', 'true');
                return;
            }

            // First open → load content BEFORE showing to avoid animation stutter
            const url = btn.getAttribute('data-notification-url');
            if (!url) return;

            fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                .then(function (response) { return response.ok ? response.text() : Promise.reject(); })
                .then(function (html) {
                    content.innerHTML = html;
                    content.dataset.loaded = '1';
                    // Show AFTER content is fully loaded → animation plays cleanly
                    dd.classList.remove('hidden');
                    btn.setAttribute('aria-expanded', 'true');
                    if (window.lucide) window.lucide.createIcons();
                })
                .catch(function () {
                    content.innerHTML = '<div class="px-4 py-8 text-center text-sm text-red-600 dark:text-red-400">No se pudieron cargar las notificaciones.</div>';
                    dd.classList.remove('hidden');
                    btn.setAttribute('aria-expanded', 'true');
                });
        });

        dd.addEventListener('click', function (e) { e.stopPropagation(); });
        document.addEventListener('click', function () {
            dd.classList.add('hidden');
            btn.setAttribute('aria-expanded', 'false');
        });
    }

    // SweetAlert confirm on delete forms
    function initConfirmForms() {
        // Handled via global delegation below
    }

    // Delegate: confirm forms (works for dynamically added elements)
    document.addEventListener('submit', function (e) {
        var form = e.target.closest('form[data-confirm]');
        if (!form || form.dataset.confirmBound) return;
        form.dataset.confirmBound = '1';
        e.preventDefault();
        const title = form.dataset.confirm || '¿Está seguro?';
        const text = form.dataset.confirmText || 'Esta acción no se puede deshacer.';
        confirmAction(title, text).then(function (ok) {
            delete form.dataset.confirmBound;
            if (ok) form.submit();
        });
    });

    function getRequestVerificationToken() {
        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    function submitDynamicPost(url, values) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = url;

        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = getRequestVerificationToken();
        form.appendChild(tokenInput);

        Object.keys(values || {}).forEach(function (key) {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = key;
            input.value = values[key];
            form.appendChild(input);
        });

        document.body.appendChild(form);
        form.submit();
    }

    function rollbackReaction(article, likeBtn, dislikeBtn, prevLikeCount, prevDislikeCount, prevLikeActive, prevDislikeActive, message) {
        if (!article) return;
        article.querySelector('.like-count').textContent = prevLikeCount;
        article.querySelector('.dislike-count').textContent = prevDislikeCount;
        [likeBtn, dislikeBtn].forEach(function (r) {
            if (!r) return;
            r.classList.remove('bg-rose-50', 'dark:bg-rose-900/30', 'text-rose-600', 'dark:text-rose-400', 'bg-indigo-50', 'dark:bg-indigo-900/30', 'text-indigo-600', 'dark:text-indigo-400');
            r.classList.add('text-gray-500', 'dark:text-gray-400');
            var icon = r.querySelector('[data-lucide]');
            if (icon) icon.classList.remove('fill-current');
            r.setAttribute('aria-pressed', 'false');
        });
        if (prevLikeActive && likeBtn) {
            likeBtn.classList.add('bg-rose-50', 'dark:bg-rose-900/30', 'text-rose-600', 'dark:text-rose-400');
            likeBtn.classList.remove('text-gray-500', 'dark:text-gray-400');
            var icon = likeBtn.querySelector('[data-lucide]');
            if (icon) icon.classList.add('fill-current');
            likeBtn.setAttribute('aria-pressed', 'true');
        }
        if (prevDislikeActive && dislikeBtn) {
            dislikeBtn.classList.add('bg-indigo-50', 'dark:bg-indigo-900/30', 'text-indigo-600', 'dark:text-indigo-400');
            dislikeBtn.classList.remove('text-gray-500', 'dark:text-gray-400');
            var icon = dislikeBtn.querySelector('[data-lucide]');
            if (icon) icon.classList.add('fill-current');
            dislikeBtn.setAttribute('aria-pressed', 'true');
        }
        if (window.lucide) window.lucide.createIcons();
        if (message) Toast.error(message);
    }

    function initReactionButtons() {
        document.querySelectorAll('.reaction-btn').forEach(function (btn) {
            if (btn.dataset.bound) return;
            btn.dataset.bound = '1';
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                if (btn.dataset.pending === '1') return;
                btn.dataset.pending = '1';

                var postId = btn.dataset.postId;
                var btnType = parseInt(btn.dataset.reaction, 10);
                if (btnType === 0) return;
                var article = document.querySelector('article[data-post-id="' + postId + '"]');
                if (!article) { btn.dataset.pending = '0'; return; }

                // Save previous state for rollback
                var likeBtn = article.querySelector('.reaction-btn[data-reaction="1"]');
                var dislikeBtn = article.querySelector('.reaction-btn[data-reaction="2"]');
                var prevLikeActive = likeBtn ? likeBtn.classList.contains('bg-rose-50') : false;
                var prevDislikeActive = dislikeBtn ? dislikeBtn.classList.contains('bg-indigo-50') : false;
                var prevLikeCount = parseInt(article.querySelector('.like-count')?.textContent || '0');
                var prevDislikeCount = parseInt(article.querySelector('.dislike-count')?.textContent || '0');

                // Toggle: if clicking the active reaction, remove it (reactionType=0)
                var isActive = (btnType === 1 && prevLikeActive) || (btnType === 2 && prevDislikeActive);
                var reaction = isActive ? 0 : btnType;

                // OPTIMISTIC UI: calculate updated counts
                var updatedLikeCount = prevLikeCount;
                var updatedDislikeCount = prevDislikeCount;

                if (reaction === 1) {
                    if (!prevLikeActive) { updatedLikeCount++; }
                    if (prevDislikeActive) { updatedDislikeCount--; }
                } else if (reaction === 2) {
                    if (!prevDislikeActive) { updatedDislikeCount++; }
                    if (prevLikeActive) { updatedLikeCount--; }
                } else if (reaction === 0) {
                    if (prevLikeActive) { updatedLikeCount--; }
                    if (prevDislikeActive) { updatedDislikeCount--; }
                }

                // Reset both buttons to inactive state
                [likeBtn, dislikeBtn].forEach(function (r) {
                    if (!r) return;
                    r.classList.remove('bg-rose-50', 'dark:bg-rose-900/30', 'text-rose-600', 'dark:text-rose-400', 'bg-indigo-50', 'dark:bg-indigo-900/30', 'text-indigo-600', 'dark:text-indigo-400');
                    r.classList.add('text-gray-500', 'dark:text-gray-400');
                    var icon = r.querySelector('[data-lucide]');
                    if (icon) icon.classList.remove('fill-current');
                    r.setAttribute('aria-pressed', 'false');
                });

                // Apply active state to the clicked reaction
                if (!isActive) {
                    if (reaction === 1 && likeBtn) {
                        likeBtn.classList.add('bg-rose-50', 'dark:bg-rose-900/30', 'text-rose-600', 'dark:text-rose-400');
                        likeBtn.classList.remove('text-gray-500', 'dark:text-gray-400');
                        var icon = likeBtn.querySelector('[data-lucide]');
                        if (icon) icon.classList.add('fill-current');
                        likeBtn.setAttribute('aria-pressed', 'true');
                        // Bounce animation
                        likeBtn.classList.add('bounce');
                        setTimeout(function () { likeBtn.classList.remove('bounce'); }, 350);
                    } else if (reaction === 2 && dislikeBtn) {
                        dislikeBtn.classList.add('bg-indigo-50', 'dark:bg-indigo-900/30', 'text-indigo-600', 'dark:text-indigo-400');
                        dislikeBtn.classList.remove('text-gray-500', 'dark:text-gray-400');
                        var icon = dislikeBtn.querySelector('[data-lucide]');
                        if (icon) icon.classList.add('fill-current');
                        dislikeBtn.setAttribute('aria-pressed', 'true');
                        // Bounce animation
                        dislikeBtn.classList.add('bounce');
                        setTimeout(function () { dislikeBtn.classList.remove('bounce'); }, 350);
                    }
                }

                // Animate counters
                var likeCountEl = article.querySelector('.like-count');
                var dislikeCountEl = article.querySelector('.dislike-count');
                if (likeCountEl) {
                    likeCountEl.textContent = updatedLikeCount;
                    likeCountEl.classList.add('count-bounce');
                    setTimeout(function () { likeCountEl.classList.remove('count-bounce'); }, 300);
                }
                if (dislikeCountEl) {
                    dislikeCountEl.textContent = updatedDislikeCount;
                    dislikeCountEl.classList.add('count-bounce');
                    setTimeout(function () { dislikeCountEl.classList.remove('count-bounce'); }, 300);
                }

                if (window.lucide) window.lucide.createIcons();

                // Send POST async
                var token = getRequestVerificationToken();
                fetch('/Posts/ReactPost?postId=' + encodeURIComponent(postId) + '&reactionType=' + encodeURIComponent(reaction), {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': token
                    }
                })
                    .then(function (response) { return response.json(); })
                    .then(function (data) {
                        if (data.success && article) {
                            // Confirm with server counts
                            if (article.querySelector('.like-count')) article.querySelector('.like-count').textContent = data.likes;
                            if (article.querySelector('.dislike-count')) article.querySelector('.dislike-count').textContent = data.dislikes;
                        } else {
                            rollbackReaction(article, likeBtn, dislikeBtn, prevLikeCount, prevDislikeCount, prevLikeActive, prevDislikeActive, data.message || 'No se pudo procesar la reacción.');
                        }
                    })
                    .catch(function () {
                        rollbackReaction(article, likeBtn, dislikeBtn, prevLikeCount, prevDislikeCount, prevLikeActive, prevDislikeActive, 'Error de conexión. La reacción no se guardó.');
                    })
                    .finally(function () {
                        btn.dataset.pending = '0';
                    });
            });
        });
    }

    function initPostFilters() {
        var toggle = document.getElementById('toggle-advanced-filters');
        var panel = document.getElementById('advanced-filters');
        if (!toggle || !panel) return;

        // Check URL params: open panel if advanced filters (not preset) are active
        var params = new URLSearchParams(window.location.search);
        var hasPreset = params.has('preset');
        var hasAdvanced = !hasPreset && (
            params.has('FromDate') || params.has('ToDate') || params.has('FriendId')
            || (params.get('ContentType') && params.get('ContentType') !== '')
        );
        if (hasAdvanced) {
            panel.classList.remove('closed');
            toggle.setAttribute('aria-expanded', 'true');
            toggle.classList.add('text-indigo-600', 'dark:text-indigo-400');
        }

        // Toggle advanced filters
        toggle.addEventListener('click', function () {
            var isClosed = panel.classList.toggle('closed');
            toggle.setAttribute('aria-expanded', !isClosed);
            toggle.classList.toggle('text-indigo-600', !isClosed);
            toggle.classList.toggle('dark:text-indigo-400', !isClosed);
            if (window.lucide) window.lucide.createIcons();
        });

        // Delegate: preset/quick filter buttons (AJAX)
        document.querySelector('#filter-form').addEventListener('click', async function (e) {
            var presetBtn = e.target.closest('[data-preset]');
            var contentTypeBtn = e.target.closest('[data-content-type]');
            if (!presetBtn && !contentTypeBtn) return;

            var form = document.getElementById('filter-form');
            var actionUrl = form.dataset.ajaxFilter;
            if (!actionUrl) return;

            var params = new URLSearchParams();
            if (presetBtn) params.set('Preset', presetBtn.dataset.preset);
            if (contentTypeBtn) params.set('ContentType', contentTypeBtn.dataset.contentType);

            var container = document.getElementById('posts-container');
            if (!container) return;

            // Show skeleton
            container.innerHTML = '<div class="flex justify-center py-12"><div class="w-8 h-8 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div></div>';

            try {
                var resp = await fetch(actionUrl + '?' + params.toString(), {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });
                if (resp.ok) {
                    var html = await resp.text();
                    if (html) {
                        container.innerHTML = html;
                        // Update URL
                        var url = new URL(window.location);
                        // Remove existing filter params and set new ones
                        ['FromDate', 'ToDate', 'ContentType', 'EditedOnly', 'SearchText', 'preset', 'FriendId'].forEach(function (k) { url.searchParams.delete(k); });
                        if (presetBtn) url.searchParams.set('preset', presetBtn.dataset.preset);
                        if (contentTypeBtn) url.searchParams.set('ContentType', contentTypeBtn.dataset.contentType);
                        window.history.pushState({}, '', url);
                        // Re-init components
                        initReactionButtons();
                        initComments();
                        initConfirmForms();
                        if (window.lucide) window.lucide.createIcons();
                    }
                }
            } catch (e) { console.error('Error applying filter', e); }
        });
    }

    function initInfiniteScroll() {
        var sentinel = document.getElementById('load-more-sentinel');
        if (!sentinel || sentinel.dataset.bound) return;
        sentinel.dataset.bound = '1';

        var container = document.querySelector(sentinel.dataset.container || '#posts-container');
        var loadUrl = sentinel.dataset.loadUrl || '';
        var loading = false;

        var observer = new IntersectionObserver(async function (entries) {
            if (!entries[0].isIntersecting || loading) return;
            loading = true;

            // Show skeletons
            var skeletonsHtml = '';
            for (var i = 0; i < 3; i++) {
                var skeleton = document.getElementById('post-skeleton-template');
                if (skeleton) skeletonsHtml += skeleton.innerHTML;
            }
            if (skeletonsHtml) {
                var wrapper = document.createElement('div');
                wrapper.id = 'skeletons';
                wrapper.innerHTML = skeletonsHtml;
                container.appendChild(wrapper);
            }

            try {
                var params = new URLSearchParams(window.location.search);
                params.set('page', (parseInt(params.get('page') || '1') + 1).toString());
                var resp = await fetch(loadUrl + '?' + params.toString(), {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                var skeletonsEl = document.getElementById('skeletons');
                if (skeletonsEl) skeletonsEl.remove();

                if (resp.ok) {
                    var html = await resp.text();
                    if (html) {
                        container.insertAdjacentHTML('beforeend', html);
                        initReactionButtons();
                        initComments();
                        initPostCards();
                        if (window.lucide) window.lucide.createIcons();
                    }
                }
            } catch (e) {
                var skeletonsEl = document.getElementById('skeletons');
                if (skeletonsEl) skeletonsEl.remove();
                Toast.error('Error al cargar más publicaciones.');
            } finally {
                loading = false;
            }
        }, { rootMargin: '300px' });

        observer.observe(sentinel);
    }

    function initPostCards() {
        // YouTube thumbnail → click to load iframe
        document.querySelectorAll('.youtube-thumbnail').forEach(function (el) {
            if (el.dataset.bound) return;
            el.dataset.bound = '1';
            el.addEventListener('click', function () {
                var embedUrl = el.dataset.embedUrl;
                if (!embedUrl) return;
                el.innerHTML = '<iframe src="' + embedUrl + '" class="w-full h-full" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; fullscreen; playsinline" allowfullscreen referrerpolicy="strict-origin-when-cross-origin" title="YouTube video player"></iframe>';
            });
        });

        // "Read more" for long posts
        document.querySelectorAll('.show-more-btn').forEach(function (btn) {
            if (btn.dataset.bound) return;
            btn.dataset.bound = '1';
            var textEl = btn.previousElementSibling.querySelector('.post-content-text');
            if (!textEl) return;
            var full = textEl.textContent;
            var truncated = full.length > 300 ? full.slice(0, 297) + '...' : full;
            textEl.textContent = truncated;
            btn.addEventListener('click', function () {
                var expanded = textEl.textContent !== full;
                textEl.textContent = expanded ? truncated : full;
                btn.textContent = expanded ? 'Ver más' : 'Ver menos';
            });
        });
    }

    function initComments() {
        document.querySelectorAll('[data-toggle-comments]').forEach(function (btn) {
            if (btn.dataset.bound) return;
            btn.dataset.bound = '1';
            btn.addEventListener('click', function () {
                const comments = document.getElementById('comments-' + btn.dataset.toggleComments);
                if (comments) comments.classList.toggle('hidden');
                if (window.lucide) window.lucide.createIcons();
            });
        });

        document.querySelectorAll('[data-comment-form]').forEach(function (form) {
            if (form.dataset.bound) return;
            form.dataset.bound = '1';

            const textarea = form.querySelector('textarea[name="Content"]');
            const counter = form.querySelector('.char-count');

            if (textarea && counter) {
                var updateCount = function () {
                    counter.textContent = textarea.value.length + '/500';
                    counter.style.color = textarea.value.length > 450 ? '#ef4444' : (textarea.value.length > 400 ? '#f59e0b' : '');
                };
                textarea.addEventListener('input', updateCount);
                updateCount();
            }

            form.addEventListener('submit', function (e) {
                if (!textarea || textarea.value.trim()) return;

                e.preventDefault();
                textarea.focus();
                Toast.warning('Debe ingresar el contenido del comentario.');
            });
        });

        // Delegate: reply form toggle (works for dynamically added elements)
        document.addEventListener('click', async function (e) {
            var btn = e.target.closest('[data-toggle-reply]');
            if (!btn) return;
            e.preventDefault();

            // Close all other open reply forms before opening this one
            document.querySelectorAll('.reply-form:not(.hidden)').forEach(function (f) {
                if (f.id !== 'reply-form-' + btn.dataset.toggleReply) {
                    f.classList.add('hidden');
                }
            });

            var form = document.getElementById('reply-form-' + btn.dataset.toggleReply);
            if (!form) return;

            // Lazy-load the form content on first click
            if (!form.dataset.formLoaded) {
                var postId = form.dataset.postId;
                if (postId) {
                    try {
                        var resp = await fetch('/Posts/ReplyForm?parentCommentId=' + encodeURIComponent(btn.dataset.toggleReply) + '&postId=' + encodeURIComponent(postId), {
                            headers: { 'X-Requested-With': 'XMLHttpRequest' }
                        });
                        if (resp.ok) {
                            form.insertAdjacentHTML('beforeend', await resp.text());
                            form.dataset.formLoaded = 'true';
                            initComments();
                        }
                    } catch (e) { console.error('Error loading reply form', e); }
                }
            }

            form.classList.toggle('hidden');
            if (!form.classList.contains('hidden')) {
                var ctx = form.querySelector('.reply-context');
                if (ctx) {
                    ctx.textContent = 'Respondiendo a ' + (btn.dataset.replyToName || '');
                    ctx.classList.remove('hidden');
                }
                // Focus the textarea
                var ta = form.querySelector('textarea');
                if (ta) setTimeout(function () { ta.focus(); }, 100);
            }
            if (window.lucide) window.lucide.createIcons();
        });

        // Delegate: close reply form button
        document.addEventListener('click', function (e) {
            var closeBtn = e.target.closest('[data-close-reply-form]');
            if (!closeBtn) return;
            var form = closeBtn.closest('.reply-form');
            if (form) form.classList.add('hidden');
        });

        // Delegate: load more replies (for static + dynamically added links)
        document.addEventListener('click', async function (e) {
            var link = e.target.closest('[data-load-replies]');
            if (!link) return;
            e.preventDefault();

            var parentId = link.dataset.loadReplies;
            var page = parseInt(link.dataset.nextPage) || 1;
            var depth = parseInt(link.dataset.depth) || 0;

            try {
                var resp = await fetch('/Posts/GetReplies?parentCommentId=' + encodeURIComponent(parentId) + '&page=' + encodeURIComponent(page) + '&depth=' + encodeURIComponent(depth), {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });
                if (resp.ok) {
                    var html = await resp.text();
                    if (html) {
                        var container = link.closest('.comment-replies, .replies-container') || link.parentElement;
                        link.remove();
                        container.insertAdjacentHTML('beforeend', html);
                        if (window.lucide) window.lucide.createIcons();
                    }
                }
            } catch (err) { console.error('Error loading replies', err); }
        });

        // Delegate: edit comment (works for dynamically added elements)
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-edit-comment]');
            if (!btn) return;

            var commentThread = btn.closest('.comment-thread');
            if (!commentThread) return;
            var contentEl = commentThread.querySelector('.comment-content');
            var editForm = commentThread.querySelector('.edit-comment-form');

            if (editForm) {
                editForm.classList.toggle('hidden');
                if (!editForm.classList.contains('hidden')) {
                    var editTextarea = editForm.querySelector('textarea');
                    if (editTextarea) {
                        editTextarea.value = btn.dataset.editCommentContent || '';
                        editTextarea.focus();
                    }
                }
                if (contentEl) contentEl.classList.toggle('hidden');
            } else {
                var originalContent = btn.dataset.editCommentContent || '';
                var div = document.createElement('div');
                div.className = 'edit-comment-form mt-1 flex flex-col gap-1';
                div.innerHTML = '<textarea class="w-full px-3 py-2 bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" maxlength="500">' + window.LinkUpPro.escapeHtml(originalContent) + '</textarea>' +
                    '<div class="flex items-center gap-2">' +
                    '<button type="button" class="px-3 py-1.5 bg-indigo-600 text-white text-xs font-bold rounded-xl hover:bg-indigo-700 save-edit">Guardar</button>' +
                    '<button type="button" class="px-3 py-1.5 bg-gray-100 dark:bg-gray-600 text-gray-700 dark:text-gray-300 text-xs font-bold rounded-xl hover:bg-gray-200 dark:hover:bg-gray-500 cancel-edit">Cancelar</button>' +
                    '<span class="text-[10px] text-gray-400 char-count">' + originalContent.length + '/500</span>' +
                    '</div>';
                if (contentEl) contentEl.parentNode.insertBefore(div, contentEl.nextSibling);
                if (contentEl) contentEl.classList.add('hidden');

                var textarea = div.querySelector('textarea');
                var charCount = div.querySelector('.char-count');
                if (textarea && charCount) {
                    textarea.addEventListener('input', function () { charCount.textContent = textarea.value.length + '/500'; });
                    textarea.focus();
                }

                div.querySelector('.save-edit').addEventListener('click', function () {
                    var newContent = textarea.value.trim();
                    if (!newContent) { Toast.warning('El comentario no puede estar vacío.'); return; }
                    if (newContent === btn.dataset.editCommentContent) {
                        div.classList.add('hidden');
                        if (contentEl) contentEl.classList.remove('hidden');
                        return;
                    }
                    submitDynamicPost('/Posts/EditComment?id=' + encodeURIComponent(btn.dataset.editComment), { content: newContent });
                });

                div.querySelector('.cancel-edit').addEventListener('click', function () {
                    div.classList.add('hidden');
                    if (contentEl) contentEl.classList.remove('hidden');
                });
            }
        });
    }

    function initBattleshipBoards() {
        const attackBoard = document.getElementById('attack-board');
        if (attackBoard && !attackBoard.dataset.bound) {
            attackBoard.dataset.bound = '1';
            attackBoard.querySelectorAll('.battleship-cell:not(:disabled)').forEach(function (cell) {
                cell.addEventListener('click', function () {
                    if (cell.disabled || cell.classList.contains('hit') || cell.classList.contains('miss') || cell.classList.contains('sunk')) return;

                    const x = cell.dataset.x;
                    const y = cell.dataset.y;
                    const action = attackBoard.dataset.attackUrl + '&targetX=' + encodeURIComponent(x) + '&targetY=' + encodeURIComponent(y);
                    submitDynamicPost(action, {});
                });
            });
        }
    }

    function initCreateGameSelection() {
        const form = document.querySelector('[data-create-game-form]');
        if (!form || form.dataset.bound) return;
        form.dataset.bound = '1';

        form.querySelectorAll('input[name="SelectedOpponentId"]').forEach(function (radio) {
            radio.addEventListener('change', function () {
                const btn = document.getElementById('start-btn');
                const info = document.getElementById('selection-info');
                if (!radio.checked || !btn) return;

                btn.disabled = false;
                btn.classList.remove('opacity-50', 'cursor-not-allowed');
                btn.classList.add('hover:from-indigo-700', 'hover:to-violet-700');

                const label = radio.closest('label').querySelector('.opponent-name-text');
                if (label && info) info.innerHTML = 'Seleccionado: <strong>' + label.textContent + '</strong>';
            });
        });
    }

    function initCommonFriendsButtons() {
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-common-friends-id]');
            if (!btn) return;

            var friendId = btn.dataset.commonFriendsId;
            var friendName = btn.dataset.commonFriendsName || 'este usuario';

            fetch('/Friends/CommonFriends?id=' + encodeURIComponent(friendId), {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
                .then(function (response) { return response.text(); })
                .then(function (html) {
                    if (window.Swal) {
                        Swal.fire({
                            title: 'Amigos en común con ' + friendName,
                            html: html,
                            showConfirmButton: false,
                            showCloseButton: true,
                            width: '500px',
                            customClass: { popup: 'animate__animated animate__zoomIn animate__faster' }
                        });
                        setTimeout(function () { if (window.lucide) window.lucide.createIcons(); }, 50);
                    }
                })
                .catch(function () { Toast.error('No se pudieron cargar los amigos en común.'); });
        });
    }

    function initUserCardSelection() {
        var cards = document.querySelectorAll('.user-card');
        if (!cards.length) return;

        cards.forEach(function (card) {
            if (card.dataset.bound) return;
            card.dataset.bound = '1';

            card.addEventListener('click', function () {
                // Toggle selection (single select)
                cards.forEach(function (c) { c.dataset.selected = 'false'; });
                this.dataset.selected = 'true';

                // Update hidden input
                var userIdInput = document.getElementById('SelectedUserId');
                if (userIdInput) userIdInput.value = this.dataset.userId;

                // Show and enable send button
                var btn = document.getElementById('send-btn');
                var bar = document.getElementById('send-action-bar');
                var info = document.getElementById('selection-info');

                if (bar) bar.style.display = 'flex';
                if (btn) {
                    btn.disabled = false;
                    btn.classList.remove('opacity-50', 'cursor-not-allowed');
                }
                if (info) {
                    var name = this.dataset.userName || 'usuario';
                    var username = this.dataset.userUsername || '';
                    info.innerHTML = 'Seleccionado: <strong>' + name + '</strong> @' + username;
                }
            });

            // Keyboard support
            card.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    this.click();
                }
            });
        });
    }

    function initFriendRequestSearch() {
        var searchInput = document.getElementById('search-input');
        var loadingEl = document.getElementById('search-loading');
        var resultsEl = document.getElementById('search-results');
        var clearBtn = document.getElementById('clear-search');
        var searchTimeout;
        var currentAbortController = null;

        if (!searchInput) return;

        function showSkeleton() {
            if (loadingEl) loadingEl.classList.remove('hidden');
        }

        function hideSkeleton() {
            if (loadingEl) loadingEl.classList.add('hidden');
        }

        async function performSearch(term) {
            // Cancel previous request if still pending
            if (currentAbortController) currentAbortController.abort();
            currentAbortController = new AbortController();

            showSkeleton();

            try {
                var response = await fetch('/FriendRequests/SearchUsers?search=' + encodeURIComponent(term), {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    signal: currentAbortController.signal
                });

                if (response.ok) {
                    var html = await response.text();
                    if (html) {
                        resultsEl.innerHTML = html;
                        // Re-init Lucide icons and card selection for new DOM
                        if (window.lucide) window.lucide.createIcons();
                        if (window.LinkUpPro) window.LinkUpPro.initUserCardSelection();
                        // Reset selection
                        document.getElementById('SelectedUserId').value = '';
                        var bar = document.getElementById('send-action-bar');
                        if (bar) bar.style.display = 'none';
                    }
                }
            } catch (e) {
                if (e.name === 'AbortError') return; // Aborted, ignore
                console.error('Search error:', e);
            } finally {
                hideSkeleton();
                currentAbortController = null;
            }
        }

        function triggerSearch(term) {
            if (!term || term.length < 2) {
                // Clear results and show empty state
                if (loadingEl) loadingEl.classList.add('hidden');
                // Show initial empty state
                fetch('/FriendRequests/SearchUsers?search=', {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                })
                    .then(function (r) { return r.ok ? r.text() : Promise.reject(); })
                    .then(function (html) {
                        resultsEl.innerHTML = html;
                        if (window.lucide) window.lucide.createIcons();
                    })
                    .catch(function () { /* silent */ });
                return;
            }

            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                performSearch(term);
            }, 300);
        }

        // Debounced input handler
        searchInput.addEventListener('input', function () {
            var term = this.value.trim();
            if (term.length < 2 && term.length > 0) return; // wait for more input
            triggerSearch(term);
        });

        // Enter key triggers immediate search
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                var term = this.value.trim();
                if (term.length >= 2) {
                    clearTimeout(searchTimeout);
                    performSearch(term);
                }
            }
        });

        // Clear button
        if (clearBtn) {
            clearBtn.addEventListener('click', function (e) {
                e.preventDefault();
                searchInput.value = '';
                triggerSearch('');
                searchInput.focus();
            });
        }
    }

    // Auto-init everything on DOMContentLoaded
    function initAll() {
        Theme.bindToggle();
        initPasswordToggles();
        initPasswordStrength();
        initPostTypeToggles();
        initImagePreview();
        initCharCounter();
        initYouTubePreview();
        initRegisterWizard();
        initPhotoDropZone();
        initTabs();
        initSidebar();
        initNotificationDropdown();
        initUserDropdown();
        initConfirmForms();
        initReactionButtons();
        initComments();
        initPostCards();
        initPostFilters();
        initInfiniteScroll();
        initBattleshipBoards();
        initCreateGameSelection();
        initCommonFriendsButtons();
        initUserCardSelection();
        initFriendRequestSearch();
        if (window.lucide) window.lucide.createIcons();
    }

    // Export
    function escapeHtml(str) {
        if (!str) return '';
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(str));
        return div.innerHTML;
    }

    window.LinkUpPro = {
        formatDate, formatTimeAgo, debounce, extractYouTubeId,
        coordsToIndex, indexToCoords, getShipCells, isValidPlacement,
        buildYouTubeEmbedUrl,
        confirmAction,
        togglePostType,
        toggleEditPostType,
        escapeHtml,
        toast: Toast,
        theme: Theme,
        initUserCardSelection,
        initFriendRequestSearch,
        init: initAll,
        initAll
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAll);
    } else {
        initAll();
    }
})(window);
