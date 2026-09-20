/**
 * ==========================================================================
 * Assign Matn Modal & Drawer Controller (Hafiz Platform)
 * Modular, Clean & High-Performance
 * - Auto-detects Program & Assigned Matn Title
 * - Dual Chapter Selection: Predefined List or Manual Entry
 * - Full-width 5-unit Selection & Inline Range Stepper
 * - Silent Auto-Date & Multi-Card Submission
 * ==========================================================================
 */

// Global State
window.matnModalState = {
    studentsList: [],
    currentIndex: -1,
    currentTab: 'all',
    designatedMatn: '',
    programName: ''
};

window.matnUnits = {
    memorization: 4,
    revision: 1,
    mudarasah: 1
};

window.matnRatings = {
    memorization: 0,
    revision: 0,
    mudarasah: 0
};

// ==========================================================================
// Matn Static Reference Data API Client & In-Memory Cache
// ==========================================================================
const matnMemoryCache = new Map();
let currentMatnFetchController = null;
let matnCatalogPreloaded = false;

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function normalizeArabicText(text) {
    if (!text) return '';
    return text
        .replace(/[\u064B-\u065F\u0670]/g, '')
        .replace(/[أإآٱ]/g, 'ا')
        .replace(/ة/g, 'ه')
        .replace(/[ىي]/g, 'ي')
        .replace(/[\u0600-\u061F\u06D6-\u06ED]/g, '')
        .replace(/[^\u0621-\u064A0-9a-zA-Z\s]/g, ' ')
        .replace(/\s+/g, ' ')
        .trim()
        .toLowerCase();
}

/**
 * Prefetch all static mutun catalog once on startup (lightweight < 4KB)
 */
async function prefetchMatnCatalog() {
    if (matnCatalogPreloaded) return;
    try {
        const res = await fetch('/api/matns', { headers: { 'Accept': 'application/json' } });
        if (res.ok) {
            const list = await res.json();
            if (Array.isArray(list)) {
                list.forEach(item => {
                    if (item.id) matnMemoryCache.set(item.id.toLowerCase(), item);
                    if (item.name) {
                        matnMemoryCache.set(item.name.toLowerCase(), item);
                        matnMemoryCache.set(normalizeArabicText(item.name), item);
                    }
                    if (Array.isArray(item.aliases)) {
                        item.aliases.forEach(a => matnMemoryCache.set(normalizeArabicText(a), item));
                    }
                });
                matnCatalogPreloaded = true;
            }
        }
    } catch (e) {
        console.warn('Could not preload matn catalog:', e);
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', prefetchMatnCatalog);
} else {
    prefetchMatnCatalog();
}

/**
 * Fetches Matn reference data with memory caching and AbortController to prevent race conditions
 */
async function fetchMatnReferenceData(identifier) {
    if (!identifier) return null;
    const cleanKey = identifier.trim();
    const normalizedKey = normalizeArabicText(cleanKey);

    // 1. Memory Cache Lookup (0ms, 0 Network calls)
    if (matnMemoryCache.has(cleanKey.toLowerCase())) {
        return matnMemoryCache.get(cleanKey.toLowerCase());
    }
    if (matnMemoryCache.has(normalizedKey)) {
        return matnMemoryCache.get(normalizedKey);
    }

    // Check fuzzy match in cache
    for (const [key, value] of matnMemoryCache.entries()) {
        if (normalizedKey.includes(key) || key.includes(normalizedKey)) {
            return value;
        }
    }

    // 2. Abort previous pending fetch to prevent race conditions (A -> B -> C)
    if (currentMatnFetchController) {
        currentMatnFetchController.abort();
    }
    currentMatnFetchController = new AbortController();

    try {
        const res = await fetch(`/api/matns/${encodeURIComponent(cleanKey)}`, {
            signal: currentMatnFetchController.signal,
            headers: { 'Accept': 'application/json' }
        });

        if (!res.ok) {
            if (res.status === 404) {
                // Return clean empty definition - never invent data!
                const emptyDef = {
                    id: cleanKey,
                    name: cleanKey,
                    chapters: [],
                    units: []
                };
                matnMemoryCache.set(cleanKey.toLowerCase(), emptyDef);
                matnMemoryCache.set(normalizedKey, emptyDef);
                return emptyDef;
            }
            throw new Error(`HTTP ${res.status}`);
        }

        const data = await res.json();
        data.chapters = Array.isArray(data.chapters) ? data.chapters : [];
        data.units = Array.isArray(data.units) ? data.units : [];

        // Store in cache
        if (data.id) matnMemoryCache.set(data.id.toLowerCase(), data);
        if (data.name) {
            matnMemoryCache.set(data.name.toLowerCase(), data);
            matnMemoryCache.set(normalizeArabicText(data.name), data);
        }
        matnMemoryCache.set(cleanKey.toLowerCase(), data);
        matnMemoryCache.set(normalizedKey, data);

        return data;
    } catch (err) {
        if (err.name === 'AbortError') {
            return null; // A newer request superseded this one
        }
        console.error('Error fetching matn reference data:', err);
        throw err;
    } finally {
        currentMatnFetchController = null;
    }
}

function initMatnStudentsList() {
    if (Array.isArray(window.allClassStudents) && window.allClassStudents.length > 0) {
        window.matnModalState.studentsList = window.allClassStudents.map(s => ({
            id: (s.id || s.userId || '').toString(),
            name: (s.name || s.fullName || '').trim(),
            initials: s.initials || ''
        }));
    } else {
        window.matnModalState.studentsList = [];
    }
}

function updateStudentHeaderOnly(index, fallbackId, fallbackName) {
    let stId = fallbackId;
    let stName = fallbackName;

    const list = window.matnModalState.studentsList;
    if (list.length > 0 && index >= 0 && index < list.length) {
        stId = list[index].id;
        stName = list[index].name;
    }

    const idInput = document.getElementById('matnStudentId');
    if (idInput) idInput.value = stId || '';

    const nameEl = document.getElementById('matnDrawerStudentName');
    if (nameEl) nameEl.textContent = stName || 'اسم الطالب';

    const totalCount = list.length || 1;
    const currentDisplay = (index >= 0 ? index + 1 : 1);
    const subEl = document.getElementById('matnDrawerSubtitle');
    if (subEl) subEl.textContent = `طالب ${currentDisplay} من ${totalCount}`;

    const initials = (stName || '').split(' ').filter(Boolean).slice(0, 2).map(n => n[0]).join('');
    const avatar = document.getElementById('matnDrawerAvatar');
    if (avatar) {
        avatar.innerHTML = initials ? `<span>${initials}</span>` : `<i class='bx bxs-book-reader'></i>`;
    }

    const prevBtn = document.getElementById('matnPrevBtn');
    const saveNextBtnText = document.getElementById('matnSaveNextBtnText');

    if (prevBtn) prevBtn.disabled = (index <= 0);
    if (saveNextBtnText) {
        saveNextBtnText.textContent = (index >= 0 && index < totalCount - 1) ? 'حفظ والتالي' : 'حفظ وإنهاء';
    }
}

function resetAllMatnRatings() {
    ['memorization', 'revision', 'mudarasah'].forEach(t => {
        const upToggle = document.getElementById(`matn-isUpcoming-${t}`);
        if (upToggle) upToggle.checked = false;
        toggleUpcomingMatn(t, false, true);

        window.matnRatings[t] = 0;
        const group = document.getElementById(`matn-ratingGroup-${t}`);
        if (group) {
            group.querySelectorAll('.rating-pill-btn').forEach(btn => {
                btn.classList.remove('is-selected');
            });
        }
        const badge = document.getElementById(`matn-ratingBadge-${t}`);
        if (badge) {
            badge.className = 'wird-rating-badge rate-none';
            badge.textContent = 'لم يُسمّع بعد';
        }
        const clearBtn = document.getElementById(`matn-ratingClear-${t}`);
        if (clearBtn) {
            clearBtn.style.display = 'none';
            clearBtn.classList.add('is-hidden');
        }
    });
}

function openAssignMatnModal(studentId, studentName, designatedMatn = '', preselectedMatnId = '') {
    initMatnStudentsList();

    const selectContainer = document.getElementById('matnStudentSelectContainer');
    const selectDropdown = document.getElementById('matnStudentSelectDropdown');
    const prevBtn = document.getElementById('matnPrevBtn');
    const saveNextBtnText = document.getElementById('matnSaveNextBtnText');
    const panel = document.getElementById('matnDrawerPanel');
    const backdrop = document.getElementById('matnDrawerBackdrop');

    const targetDesignated = (designatedMatn || panel?.dataset?.assignedMatn || window.classDesignatedMatn || '').trim();
    const programName = (panel?.dataset?.programName || '').trim();

    window.matnModalState.designatedMatn = targetDesignated;
    window.matnModalState.programName = programName;
    window.matnModalState.preselectedMatnId = preselectedMatnId;
    window.matnModalState.currentStudentId = studentId;

    const progBadgeText = document.getElementById('matnDrawerProgramBadgeText');
    if (progBadgeText && programName) progBadgeText.textContent = programName;

    const matnBadge = document.getElementById('matnDrawerMatnBadge');
    const matnBadgeText = document.getElementById('matnDrawerMatnBadgeText');
    if (matnBadgeText) {
        matnBadgeText.textContent = targetDesignated || 'المتن المقرر';
        if (matnBadge) matnBadge.style.display = targetDesignated ? 'inline-flex' : 'none';
    }

    if (!studentId) {
        window.matnModalState.currentIndex = -1;
        if (selectContainer && selectDropdown) {
            selectContainer.style.display = 'block';
            if (selectDropdown.options.length <= 1) {
                selectDropdown.innerHTML = '<option value="">-- اختر الطالب من الحلقة --</option>';
                const frag = document.createDocumentFragment();
                window.matnModalState.studentsList.forEach((st, idx) => {
                    const opt = document.createElement('option');
                    opt.value = st.id;
                    opt.textContent = `${idx + 1}. ${st.name}`;
                    frag.appendChild(opt);
                });
                selectDropdown.appendChild(frag);
            }
        }
        document.getElementById('matnStudentId').value = '';
        document.getElementById('matnDrawerStudentName').textContent = 'تسميع أوراد المتون';
        document.getElementById('matnDrawerSubtitle').textContent = `طلاب الحلقة: ${window.matnModalState.studentsList.length}`;
        document.getElementById('matnDrawerAvatar').innerHTML = `<i class='bx bxs-book-reader'></i>`;
        if (prevBtn) prevBtn.disabled = true;
        if (saveNextBtnText) saveNextBtnText.textContent = 'اعتماد ورصد التسميع';
    } else {
        if (selectContainer) selectContainer.style.display = 'none';
        const cleanId = studentId.toString();
        const foundIndex = window.matnModalState.studentsList.findIndex(s => s.id.toLowerCase() === cleanId.toLowerCase());
        window.matnModalState.currentIndex = foundIndex >= 0 ? foundIndex : 0;
        updateStudentHeaderOnly(window.matnModalState.currentIndex, studentId, studentName);
    }

    const allTabBtn = document.querySelector('#matnSegmentedNav .segmented-btn[data-tab="all"]');
    setMatnFilterTab('all', allTabBtn);

    // 1. ANIMATION-FIRST: Trigger slide transition immediately (< 1ms execution time)
    const isAlreadyOpen = panel && panel.classList.contains('is-open');
    if (panel && backdrop) {
        panel.style.visibility = 'visible';
        backdrop.style.visibility = 'visible';
        requestAnimationFrame(() => {
            panel.classList.add('is-open', 'active', 'mobile-open');
            backdrop.classList.add('is-open', 'active');
            document.body.style.overflow = 'hidden';
        });
    }

    // 2. Schedule Card Construction & Reset:
    const setupCards = () => {
        resetAllMatnRatings();
        populateMatnDropdown(studentId, preselectedMatnId, targetDesignated);
    };

    if (isAlreadyOpen) {
        setupCards();
    } else {
        // Run during slide animation to keep 60 FPS initial response
        setTimeout(setupCards, 120);
    }
}

function getStoredMatnId(studentId) {
    if (studentId) {
        const byStudent = localStorage.getItem('hafiz_matn_' + studentId);
        if (byStudent) return byStudent;
    }
    const globalSaved = localStorage.getItem('hafiz_last_matn');
    if (globalSaved) return globalSaved;

    const match = document.cookie.match(/(?:^|;\s*)hafiz_selected_matn=([^;]+)/);
    return match ? decodeURIComponent(match[1]) : null;
}

function saveSelectedMatn(studentId, matnId) {
    if (!matnId) return;
    if (studentId) {
        try { localStorage.setItem('hafiz_matn_' + studentId, matnId); } catch(e){}
    }
    try { localStorage.setItem('hafiz_last_matn', matnId); } catch(e){}
    document.cookie = `hafiz_selected_matn=${encodeURIComponent(matnId)}; path=/; max-age=31536000; SameSite=Lax`;
}

async function populateMatnDropdown(studentId, preselectedMatnId, preselectedMatnTitle) {
    const dropdown = document.getElementById('matnSelectDropdown');
    const hint = document.getElementById('matnSelectorHint');
    if (!dropdown) return;

    dropdown.innerHTML = '<option value="">جارٍ تحميل المتون المقررة...</option>';

    let matuns = [];
    if (studentId) {
        try {
            const res = await fetch(`/Teacher/StudentMatnProgress/ForStudent?studentId=${studentId}`);
            const json = await res.json();
            if (json && json.success && Array.isArray(json.data) && json.data.length > 0) {
                matuns = json.data;
            }
        } catch (e) {
            console.warn('Could not fetch student matuns dynamically:', e);
        }
    }

    if (matuns.length === 0 && window.currentStudentMatuns && Array.isArray(window.currentStudentMatuns) && window.currentStudentMatuns.length > 0) {
        matuns = window.currentStudentMatuns;
    }

    if (matuns.length === 0 && (preselectedMatnTitle || preselectedMatnId)) {
        matuns = [{ matnId: preselectedMatnId || '', matnTitle: preselectedMatnTitle || 'المتن المقرر' }];
    }

    if (matuns.length === 0) {
        dropdown.innerHTML = '<option value="">-- لا توجد متون مسندة لهذا الطالب --</option>';
        if (hint) hint.textContent = '';
        applyDesignatedMatnToCards(preselectedMatnTitle || 'المتون العلمية');
        return;
    }

    dropdown.innerHTML = '';
    const storedMatnId = getStoredMatnId(studentId);
    const targetMatnId = preselectedMatnId || storedMatnId;

    let selectedOption = null;

    matuns.forEach((m, idx) => {
        const opt = document.createElement('option');
        const mId = m.matnId || m.id || '';
        const mTitle = m.matnTitle || m.title || '';
        const mAuthor = m.matnAuthor || m.author || '';
        const mOrder = m.matnOrder || m.order || (idx + 1);

        opt.value = mId;
        opt.setAttribute('data-title', mTitle);
        opt.setAttribute('data-author', mAuthor);
        opt.textContent = `${mOrder ? '#' + mOrder + ' ' : ''}${mTitle} ${mAuthor ? '(' + mAuthor + ')' : ''}`;

        if (targetMatnId && mId.toLowerCase() === targetMatnId.toLowerCase()) {
            opt.selected = true;
            selectedOption = opt;
        } else if (!selectedOption && preselectedMatnTitle && mTitle.trim().toLowerCase() === preselectedMatnTitle.trim().toLowerCase()) {
            opt.selected = true;
            selectedOption = opt;
        }

        dropdown.appendChild(opt);
    });

    if (!selectedOption && dropdown.options.length > 0) {
        dropdown.options[0].selected = true;
        selectedOption = dropdown.options[0];
    }

    if (hint) {
        hint.textContent = `متوفر (${matuns.length}) متون مقررة`;
    }

    if (selectedOption) {
        onMatnDropdownChange(dropdown);
    }
}

function onMatnDropdownChange(selectEl) {
    if (!selectEl) return;
    const opt = selectEl.options[selectEl.selectedIndex];
    if (!opt) return;

    const matnId = opt.value;
    const matnTitle = opt.getAttribute('data-title') || opt.text;

    const hiddenId = document.getElementById('matnSelectedId');
    if (hiddenId) hiddenId.value = matnId;

    const badge = document.getElementById('matnDrawerMatnBadge');
    const badgeText = document.getElementById('matnDrawerMatnBadgeText');
    if (badgeText) {
        badgeText.textContent = matnTitle;
        if (badge) badge.style.display = 'inline-flex';
    }

    window.matnModalState.designatedMatn = matnTitle;
    window.matnModalState.selectedMatnId = matnId;

    // Save selection in localStorage and cookie
    saveSelectedMatn(window.matnModalState.currentStudentId, matnId);

    applyDesignatedMatnToCards(matnTitle, matnId);
}

async function applyDesignatedMatnToCards(designatedTitle, matnId = '') {
    const cardTypes = ['memorization', 'revision', 'mudarasah'];
    const finalMatnName = (designatedTitle || 'المتون العلمية').trim();
    const queryKey = (matnId || finalMatnName).trim();
    window.matnModalState.designatedMatn = finalMatnName;

    cardTypes.forEach(type => {
        const hiddenInput = document.getElementById(`matnTitle-${type}`);
        if (hiddenInput && hiddenInput.value !== finalMatnName) {
            hiddenInput.value = finalMatnName;
        }
    });

    // CACHING CHECK: If already rendered for this exact matn, skip re-render
    if (window.matnModalState.hasRenderedChapters &&
        window.matnModalState.lastRenderedMatnKey === queryKey) {
        return;
    }

    // 1. Lightweight loading state (no heavy animations or layout shifts)
    const normalizedKey = normalizeArabicText(queryKey);
    const isCached = matnMemoryCache.has(queryKey.toLowerCase()) || matnMemoryCache.has(normalizedKey);
    if (!isCached) {
        cardTypes.forEach(type => {
            const chapterSelect = document.getElementById(`matnChapterSelect-${type}`);
            if (chapterSelect) {
                chapterSelect.innerHTML = '<option value="" disabled selected>جارٍ تحميل الأبواب...</option>';
            }
            const unitGroup = document.getElementById(`matn-unitGroup-${type}`);
            if (unitGroup) {
                unitGroup.innerHTML = '<span class="text-muted small py-1 px-2"><i class="bx bx-loader-alt bx-spin me-1"></i> جارٍ تحميل الوحدات...</span>';
            }
        });
    }

    // 2. Fetch data (Memory cache -> Instant 0ms, or Network API with AbortController)
    let matnData = null;
    try {
        matnData = await fetchMatnReferenceData(queryKey);
    } catch (err) {
        console.error('Failed to load matn definition:', err);
        // Error state: Show gentle message without destroying existing inputs
        cardTypes.forEach(type => {
            const chapterSelect = document.getElementById(`matnChapterSelect-${type}`);
            if (chapterSelect) {
                chapterSelect.innerHTML = '<option value="" disabled selected>تعذر تحميل بيانات المتن، يرجى المحاولة مرة أخرى.</option>';
            }
            const unitGroup = document.getElementById(`matn-unitGroup-${type}`);
            if (unitGroup) {
                unitGroup.innerHTML = '<span class="text-danger small py-1 px-2"><i class="bx bx-error-circle me-1"></i> تعذر تحميل بيانات المتن، يرجى المحاولة مرة أخرى.</span>';
            }
        });
        return;
    }

    // If request was superseded by a newer selection, exit cleanly
    if (!matnData) return;

    const chapters = Array.isArray(matnData.chapters) ? matnData.chapters : [];
    const units = Array.isArray(matnData.units) ? matnData.units : [];

    // Build the chapters options HTML once
    let chaptersHtml = '';
    if (chapters.length > 0) {
        chaptersHtml = chapters.map((ch, idx) => {
            const chName = ch.name || ch;
            return `<option value="${escapeHtml(chName)}"${idx === 0 ? ' selected' : ''}>${escapeHtml(chName)}</option>`;
        }).join('');
    }

    cardTypes.forEach(type => {
        const chapterSelect = document.getElementById(`matnChapterSelect-${type}`);
        const chapterInput = document.getElementById(`matnChapterName-${type}`);
        const modeToggle = document.getElementById(`matnChapterModeToggle-${type}`);
        const unitGroup = document.getElementById(`matn-unitGroup-${type}`);

        // Update Chapters (with Empty State support)
        if (chapters.length > 0) {
            if (modeToggle) modeToggle.style.display = 'inline-flex';
            if (chapterSelect) chapterSelect.innerHTML = chaptersHtml;
            const firstChapter = chapters[0].name || chapters[0];
            if (chapterInput) chapterInput.value = firstChapter;
            setChapterInputMode(type, 'select');
        } else {
            // Rule 6 & 11: Empty State
            if (modeToggle) modeToggle.style.display = 'none';
            if (chapterSelect) {
                chapterSelect.innerHTML = '<option value="" disabled selected>لا يوجد أبواب مسجلة لهذا المتن</option>';
            }
            if (chapterInput) {
                chapterInput.value = '';
                chapterInput.placeholder = 'لا يوجد أبواب مسجلة لهذا المتن - اكتب الموضع يدوياً...';
            }
            setChapterInputMode(type, 'manual');
        }

        // Update Units (with Empty State support)
        if (unitGroup) {
            if (units.length > 0) {
                unitGroup.innerHTML = units.map((u, idx) => `
                    <label class="unit-radio-option ${idx === 0 ? 'is-selected' : ''}" id="matn-unitOpt-${type}-${u.number}"
                           onclick="setMatnUnit('${type}', ${u.number}, '${escapeHtml(u.name)}', '${escapeHtml(u.singular || u.name)}')">
                        <input type="radio" name="matn-unit-${type}" value="${u.number}" ${idx === 0 ? 'checked' : ''}>
                        <span>${escapeHtml(u.name)}</span>
                    </label>
                `).join('');

                const defaultUnit = units[0];
                setMatnUnit(type, defaultUnit.number, defaultUnit.name, defaultUnit.singular || defaultUnit.name);
            } else {
                // Empty State for units
                unitGroup.innerHTML = `<span class="text-muted small py-1 px-2"><i class='bx bx-info-circle me-1'></i> لا توجد وحدات مقررة مسجلة لهذا المتن</span>`;
                setMatnUnit(type, 1, 'أبيات', 'بيت');
            }
        }

        updateRangeTotalBadge(type);
    });

    window.matnModalState.lastRenderedMatnKey = queryKey;
    window.matnModalState.lastRenderedMatn = finalMatnName;
    window.matnModalState.hasRenderedChapters = true;
}

/**
 * Clean & explicit mode toggle: 'select' (dropdown) vs 'manual' (direct text typing)
 */
function setChapterInputMode(type, mode) {
    const selectWrap = document.getElementById(`matnChapterSelectWrap-${type}`);
    const inputWrap = document.getElementById(`matnChapterInputWrap-${type}`);
    const btnSelect = document.getElementById(`btnModeSelect-${type}`);
    const btnManual = document.getElementById(`btnModeManual-${type}`);
    const input = document.getElementById(`matnChapterName-${type}`);
    const select = document.getElementById(`matnChapterSelect-${type}`);

    if (mode === 'manual') {
        if (btnSelect) btnSelect.classList.remove('active');
        if (btnManual) btnManual.classList.add('active');
        if (selectWrap) selectWrap.style.display = 'none';
        if (inputWrap) inputWrap.style.display = 'block';
        if (input) {
            input.focus();
            if (select && input.value === select.value) {
                input.value = '';
            }
        }
    } else {
        if (btnSelect) btnSelect.classList.add('active');
        if (btnManual) btnManual.classList.remove('active');
        if (selectWrap) selectWrap.style.display = 'block';
        if (inputWrap) inputWrap.style.display = 'none';
        if (select && input) {
            input.value = select.value;
        }
    }
}

function onMatnChapterSelectChange(type, val) {
    const chapterInput = document.getElementById(`matnChapterName-${type}`);
    if (chapterInput) chapterInput.value = val;
}

function loadMatnStudentDataByIndex(index, fallbackId, fallbackName) {
    updateStudentHeaderOnly(index, fallbackId, fallbackName);
    resetAllMatnRatings();
}

function onMatnStudentDropdownChange(select) {
    const stId = select.value;
    const list = window.matnModalState.studentsList;
    const foundIndex = list.findIndex(s => s.id === stId);

    if (foundIndex >= 0) {
        window.matnModalState.currentIndex = foundIndex;
        loadMatnStudentDataByIndex(foundIndex, list[foundIndex].id, list[foundIndex].name);
    } else {
        const stName = select.options[select.selectedIndex]?.text || '';
        document.getElementById('matnStudentId').value = stId;
        document.getElementById('matnDrawerStudentName').textContent = stName || 'تسميع متن جديد';
        document.getElementById('matnDrawerAvatar').innerHTML = `<i class='bx bxs-book-reader'></i>`;
    }
}

function navigateMatnStudent(delta) {
    const list = window.matnModalState.studentsList;
    if (!list || list.length === 0) return;

    const newIndex = window.matnModalState.currentIndex + delta;
    if (newIndex >= 0 && newIndex < list.length) {
        window.matnModalState.currentIndex = newIndex;
        loadMatnStudentDataByIndex(newIndex, list[newIndex].id, list[newIndex].name);

        const avatar = document.getElementById('matnDrawerAvatar');
        if (avatar) {
            avatar.style.transform = 'scale(1.1)';
            setTimeout(() => avatar.style.transform = '', 200);
        }
    }
}

function closeAssignMatnDrawer() {
    const drawer = document.getElementById('matnDrawerPanel');
    const backdrop = document.getElementById('matnDrawerBackdrop');
    if (drawer) drawer.classList.remove('is-open', 'active', 'mobile-open');
    if (backdrop) backdrop.classList.remove('is-open', 'active');
    setTimeout(() => {
        if (!drawer?.classList.contains('active') && !drawer?.classList.contains('is-open')) {
            document.body.style.overflow = '';
            if (drawer) drawer.style.visibility = '';
            if (backdrop) backdrop.style.visibility = '';
        }
    }, 260);
}

function setMatnFilterTab(tabKey, btn) {
    window.matnModalState.currentTab = tabKey;
    const nav = document.getElementById('matnSegmentedNav');
    if (nav) nav.querySelectorAll('.segmented-btn').forEach(b => b.classList.remove('active'));
    if (btn) btn.classList.add('active');

    const boxes = document.querySelectorAll('#matnDrawerBody .drawer-wird-box');
    boxes.forEach(box => {
        const bType = box.dataset.type;
        box.style.display = (tabKey === 'all' || tabKey === bType) ? 'block' : 'none';
    });
}

function toggleMatnCard(type, isChecked) {
    const label = document.getElementById(`matn-wstatus-${type}`);
    const box = document.getElementById(`matn-wbox-${type}`);
    if (label) {
        label.textContent = isChecked ? 'مفعل' : 'معطل';
        label.style.color = isChecked ? '#16a34a' : '#94a3b8';
    }
    if (box) {
        box.classList.toggle('is-disabled', !isChecked);
    }
}

function setMatnUnit(type, val, unitName, unitTag) {
    window.matnUnits[type] = parseInt(val);

    const group = document.getElementById(`matn-unitGroup-${type}`);
    if (group) {
        group.querySelectorAll('.unit-radio-option').forEach(opt => {
            const radio = opt.querySelector('input[type="radio"]');
            const match = radio && parseInt(radio.value) === parseInt(val);
            opt.classList.toggle('is-selected', match);
            if (radio) radio.checked = match;
        });
    }

    const fromLabel = document.getElementById(`matn-fromLabel-${type}`);
    const toLabel = document.getElementById(`matn-toLabel-${type}`);
    if (fromLabel) fromLabel.textContent = `من (${unitTag}):`;
    if (toLabel) toLabel.textContent = `إلى (${unitTag}):`;

    updateAmountPresetChipsForUnit(type, parseInt(val));
    updateRangeTotalBadge(type);
}

function stepMatnAmount(type, delta) {
    const amountInput = document.getElementById(`matn-amount-${type}`);
    if (!amountInput) return;
    const currentVal = parseFloat(amountInput.value) || 1;
    const nextVal = Math.max(1, currentVal + delta);
    amountInput.value = nextVal;
    onMatnAmountInput(type, nextVal);
}

function setQuickAmount(type, val) {
    const amountInput = document.getElementById(`matn-amount-${type}`);
    if (!amountInput) return;
    amountInput.value = val;
    onMatnAmountInput(type, val);
}

function onMatnAmountInput(type, val) {
    const num = parseFloat(val) || 1;
    const fromInput = document.getElementById(`matn-fromNumber-${type}`);
    const toInput = document.getElementById(`matn-toNumber-${type}`);
    const fromVal = parseInt(fromInput?.value) || 1;

    if (toInput && num >= 1) {
        toInput.value = fromVal + Math.floor(num) - 1;
    }
    updateRangeTotalBadge(type);
}

function calculateMatnAmount(type) {
    const fromVal = parseInt(document.getElementById(`matn-fromNumber-${type}`)?.value) || 1;
    const toVal = parseInt(document.getElementById(`matn-toNumber-${type}`)?.value) || 1;
    const amountInput = document.getElementById(`matn-amount-${type}`);

    if (toVal >= fromVal && amountInput) {
        amountInput.value = (toVal - fromVal + 1);
    }
    updateRangeTotalBadge(type);
}

function getUnitDisplayTag(unitVal) {
    switch (parseInt(unitVal)) {
        case 5: return 'حديث';
        case 4: return 'باب';
        case 3: return 'صفحة';
        case 2: return 'سطر';
        default: return 'بيت';
    }
}

function getUnitPluralTag(unitVal, count) {
    const tag = getUnitDisplayTag(unitVal);
    if (tag === 'بيت') return count > 10 ? `${count} بيتاً` : count >= 3 ? `${count} أبيات` : count === 2 ? 'بيتان' : 'بيت واحد';
    if (tag === 'حديث') return count > 10 ? `${count} حديثاً` : count >= 3 ? `${count} أحاديث` : count === 2 ? 'حديثان' : 'حديث واحد';
    if (tag === 'باب') return count > 10 ? `${count} باباً` : count >= 3 ? `${count} أبواب` : count === 2 ? 'بابان' : 'باب واحد';
    if (tag === 'صفحة') return count > 10 ? `${count} صفحة` : count >= 3 ? `${count} صفحات` : count === 2 ? 'صفحتان' : 'صفحة واحدة';
    if (tag === 'سطر') return count > 10 ? `${count} سطراً` : count >= 3 ? `${count} أسطر` : count === 2 ? 'سطران' : 'سطر واحد';
    return `${count} ${tag}`;
}

function updateRangeTotalBadge(type) {
    const fromVal = parseInt(document.getElementById(`matn-fromNumber-${type}`)?.value) || 1;
    const toVal = parseInt(document.getElementById(`matn-toNumber-${type}`)?.value) || 1;
    const badge = document.getElementById(`matn-rangeBadge-${type}`);
    if (!badge) return;

    const count = Math.max(1, toVal - fromVal + 1);
    const unit = window.matnUnits[type] || 1;
    const text = getUnitPluralTag(unit, count);
    if (badge.textContent !== text) {
        badge.textContent = text;
    }
}

function updateAmountPresetChipsForUnit(type, unitVal) {
    const container = document.getElementById(`matn-amountChips-${type}`);
    if (!container) return;

    const unitStr = String(unitVal);
    if (container.dataset.renderedUnit === unitStr) {
        return;
    }
    container.dataset.renderedUnit = unitStr;

    const presets = (unitVal === 1) ? [1, 5, 10, 15] : [1, 2, 3, 5];
    const frag = document.createDocumentFragment();
    presets.forEach(p => {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'amount-chip';
        btn.textContent = p;
        btn.onclick = () => setQuickAmount(type, p);
        frag.appendChild(btn);
    });
    container.innerHTML = '';
    container.appendChild(frag);
}

function setMatnRating(type, status, grade, hintText) {
    const numStatus = parseInt(status) || 0;
    window.matnRatings[type] = numStatus;
    const group = document.getElementById(`matn-ratingGroup-${type}`);
    if (!group) return;

    group.querySelectorAll('.rating-pill-btn').forEach(btn => {
        btn.classList.toggle('is-selected', parseInt(btn.dataset.status) === numStatus);
    });

    const badge = document.getElementById(`matn-ratingBadge-${type}`);
    const clearBtn = document.getElementById(`matn-ratingClear-${type}`);
    if (badge) {
        if (numStatus > 0) {
            badge.className = `wird-rating-badge rate-${grade}`;
            badge.textContent = hintText || 'تم التقييم';
        } else {
            badge.className = 'wird-rating-badge rate-none';
            badge.textContent = 'لم يُسمّع بعد';
        }
    }
    const hasRating = numStatus > 0;
    if (clearBtn) {
        clearBtn.style.display = hasRating ? 'inline-flex' : 'none';
        clearBtn.classList.toggle('is-hidden', !hasRating);
    }
}

function clearMatnRating(type) {
    window.matnRatings[type] = 0;
    const group = document.getElementById(`matn-ratingGroup-${type}`);
    if (!group) return;

    group.querySelectorAll('.rating-pill-btn').forEach(btn => btn.classList.remove('is-selected'));

    const badge = document.getElementById(`matn-ratingBadge-${type}`);
    const clearBtn = document.getElementById(`matn-ratingClear-${type}`);
    if (badge) {
        badge.className = 'wird-rating-badge rate-none';
        badge.textContent = 'لم يُسمّع بعد';
    }
    if (clearBtn) {
        clearBtn.style.display = 'none';
        clearBtn.classList.add('is-hidden');
    }
}

/**
 * Handles toggling a card as an upcoming wird (ورد قادم)
 * An upcoming wird is scheduled for next session: unrated (status = 0) and not completed (isCompleted = false)
 */
function toggleUpcomingMatn(type, isUpcoming, isSilent = false) {
    const cardBox = document.getElementById(`matn-wbox-${type}`);
    const ratingGroup = document.getElementById(`matn-ratingGroup-${type}`);
    const ratingBadge = document.getElementById(`matn-ratingBadge-${type}`);
    const ratingClear = document.getElementById(`matn-ratingClear-${type}`);

    if (cardBox) cardBox.classList.toggle('is-upcoming-box', isUpcoming);
    if (ratingGroup) ratingGroup.classList.toggle('is-upcoming-mode', isUpcoming);

    if (isUpcoming) {
        // Upcoming wird cannot have completion status or grade - reset to 0 (notSet)
        window.matnRatings[type] = 0;

        if (ratingGroup) {
            ratingGroup.querySelectorAll('.rating-pill-btn').forEach(b => b.classList.remove('is-selected'));
        }

        if (ratingBadge) {
            ratingBadge.className = 'wird-rating-badge rate-upcoming';
            ratingBadge.innerHTML = "<i class='bx bx-calendar-star'></i> ورد قادم (مجدول ولم يُسمّع بعد)";
        }

        if (ratingClear) {
            ratingClear.style.display = 'none';
            ratingClear.classList.add('is-hidden');
        }

        // Ensure card is active so it gets saved
        const cardToggle = document.getElementById(`matn-wtoggle-${type}`);
        if (cardToggle && !cardToggle.checked) {
            cardToggle.checked = true;
            toggleMatnCard(type, true);
        }

        if (!isSilent && typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'info',
                title: 'تحديد كورد قادم',
                text: 'تم تحديد هذا الورد كورد قادم للجلسة القادمة (مجدول وغير مقيّم).',
                timer: 1500,
                showConfirmButton: false,
                toast: true,
                position: 'top-end'
            });
        }
    } else {
        // Return to active evaluated mode (default to excellent 5)
        if (!isSilent) {
            setMatnRating(type, 5, 'excellent', 'متقن تماماً');
        }
    }
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return '';
}

async function submitMatnAssignment(navigateNext = false) {
    const studentId = document.getElementById('matnStudentId')?.value;
    const classId = document.getElementById('matnClassId')?.value || getCookie('selectedClassId');

    if (!studentId || !classId) {
        Swal.fire({
            icon: 'warning',
            title: 'تنبيه',
            text: 'يرجى التأكد من تحديد الطالب والحلقة.',
            confirmButtonText: 'حسناً',
            customClass: { confirmButton: 'btn btn-primary' }
        });
        return;
    }

    const allTypes = [
        { key: 'memorization', perfVal: 1 },
        { key: 'revision', perfVal: 2 },
        { key: 'mudarasah', perfVal: 3 }
    ];

    const targetTypes = window.matnModalState.currentTab === 'all'
        ? allTypes
        : allTypes.filter(t => t.key === window.matnModalState.currentTab);

    const payloads = [];
    const nowIso = new Date().toISOString();

    const matnTitle = window.matnModalState.designatedMatn
        || document.getElementById('matnDrawerPanel')?.dataset?.assignedMatn
        || 'المتون العلمية';

    for (const t of targetTypes) {
        const toggle = document.getElementById(`matn-wtoggle-${t.key}`);
        if (!toggle || !toggle.checked) continue;

        const amountVal = document.getElementById(`matn-amount-${t.key}`)?.value;
        if (!amountVal || parseFloat(amountVal) <= 0) continue;

        // Resolve chapter value based on active mode (select vs manual)
        const btnManual = document.getElementById(`btnModeManual-${t.key}`);
        const modeToggle = document.getElementById(`matnChapterModeToggle-${t.key}`);
        const isManual = (btnManual && btnManual.classList.contains('active')) || (modeToggle && modeToggle.style.display === 'none');

        const chapterVal = isManual
            ? (document.getElementById(`matnChapterName-${t.key}`)?.value.trim() || '')
            : (document.getElementById(`matnChapterSelect-${t.key}`)?.value || '');

        const combinedChapterName = chapterVal ? `${matnTitle}: ${chapterVal}` : matnTitle;

        const isUpcoming = document.getElementById(`matn-isUpcoming-${t.key}`)?.checked || false;

        // When upcoming or unrated: status = 0 (AssignmentStatus.notSet / غير مقيم), and isCompleted = false (غير مكتمل)
        const parsedStatus = parseInt(window.matnRatings[t.key], 10);
        const finalStatus = isUpcoming ? 0 : (!isNaN(parsedStatus) ? parsedStatus : 0);
        const finalIsCompleted = !isUpcoming && finalStatus > 0;

        const selectedMatnId = document.getElementById('matnSelectedId')?.value || document.getElementById('matnSelectDropdown')?.value || null;

        payloads.push({
            studentId: studentId,
            classId: classId,
            matnId: selectedMatnId,
            performanceType: t.perfVal,
            unit: parseInt(window.matnUnits[t.key]) || 4,
            amount: parseFloat(amountVal),
            chapterName: combinedChapterName.substring(0, 150),
            fromNumber: parseInt(document.getElementById(`matn-fromNumber-${t.key}`)?.value) || null,
            toNumber: parseInt(document.getElementById(`matn-toNumber-${t.key}`)?.value) || null,
            status: finalStatus,
            isCompleted: finalIsCompleted,
            isUpcoming: isUpcoming,
            assignedDate: nowIso,
            note: document.getElementById(`matnNote-${t.key}`)?.value.trim() || null
        });
    }

    if (payloads.length === 0) {
        Swal.fire({
            icon: 'warning',
            title: 'تنبيه',
            text: 'يرجى تفعيل ورد واحد على الأقل وتحديد مقداره لإتمام الرصد.',
            confirmButtonText: 'حسناً',
            customClass: { confirmButton: 'btn btn-primary' }
        });
        return;
    }

    const activeBtn = navigateNext ? document.getElementById('matnSaveNextBtn') : document.getElementById('matnSaveOnlyBtn');
    const originalText = activeBtn ? activeBtn.innerHTML : '';
    if (activeBtn) {
        activeBtn.disabled = true;
        activeBtn.innerHTML = "<i class='bx bx-loader-alt bx-spin'></i> جاري الحفظ...";
    }

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        const promises = payloads.map(payload =>
            fetch('/Teacher/Student/AssignMatn', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(payload)
            }).then(r => r.json())
        );

        const results = await Promise.all(promises);
        const allOk = results.every(res => res && res.success !== false);

        if (allOk) {
            Swal.fire({
                icon: 'success',
                title: 'تم الاعتماد بنجاح',
                text: 'تم رصد وتثبيت أوراد المتن للطالب بنجاح.',
                timer: 1600,
                showConfirmButton: false,
                toast: true,
                position: 'top-end'
            });

            const list = window.matnModalState.studentsList;
            const canAdvance = navigateNext && list.length > 0 && window.matnModalState.currentIndex < list.length - 1;

            if (canAdvance) {
                navigateMatnStudent(1);
            } else {
                closeAssignMatnDrawer();
                setTimeout(() => { window.location.reload(); }, 1200);
            }
        } else {
            Swal.fire({
                icon: 'error',
                title: 'تعذر الحفظ',
                text: 'حدث خطأ في حفظ بعض الأوراد، يرجى المحاولة مرة أخرى.',
                confirmButtonText: 'حسناً'
            });
        }
    } catch (err) {
        console.error(err);
        Swal.fire({
            icon: 'error',
            title: 'خطأ في الاتصال',
            text: 'تعذر الاتصال بالخادم، يرجى المحاولة مرة أخرى.',
            confirmButtonText: 'حسناً'
        });
    } finally {
        if (activeBtn) {
            activeBtn.disabled = false;
            activeBtn.innerHTML = originalText;
        }
    }
}

// Global Keyboard Listener (Escape closes drawer)
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const drawer = document.getElementById('matnDrawerPanel');
        if (drawer && drawer.classList.contains('is-open')) {
            closeAssignMatnDrawer();
        }
    }
});

// Explicit Global Window Attachments
window.openAssignMatnModal = openAssignMatnModal;
window.closeAssignMatnDrawer = closeAssignMatnDrawer;
window.setChapterInputMode = setChapterInputMode;
window.onMatnChapterSelectChange = onMatnChapterSelectChange;
window.setMatnFilterTab = setMatnFilterTab;
window.toggleMatnCard = toggleMatnCard;
window.setMatnUnit = setMatnUnit;
window.stepMatnAmount = stepMatnAmount;
window.setQuickAmount = setQuickAmount;
window.onMatnAmountInput = onMatnAmountInput;
window.calculateMatnAmount = calculateMatnAmount;
window.setMatnRating = setMatnRating;
window.clearMatnRating = clearMatnRating;
window.submitMatnAssignment = submitMatnAssignment;
window.navigateMatnStudent = navigateMatnStudent;
window.onMatnStudentDropdownChange = onMatnStudentDropdownChange;
window.toggleUpcomingMatn = toggleUpcomingMatn;
window.onMatnDropdownChange = onMatnDropdownChange;
window.populateMatnDropdown = populateMatnDropdown;
