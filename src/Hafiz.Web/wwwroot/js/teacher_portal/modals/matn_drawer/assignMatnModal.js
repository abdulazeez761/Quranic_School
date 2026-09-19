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

// Chapters database for standard Mutun (API /Teacher/Matn/GetChapters can be plugged here in future)
const MATN_CHAPTERS_MAP = {
    "الآجرومية (ابن آجروم) - النحو": [
        "المقدمة والكلام وما يتألف منه", "باب الإعراب وعلاماته", "باب الأفعال وأحكامها",
        "باب مرفوعات الأسماء (الفاعل ونائبه)", "باب المبتدأ والخبر ونواسخهما",
        "باب منصوبات الأسماء (المفاعيل والحال والتمييز)", "باب الاستثناء ولا والمنادى",
        "باب مخفوضات الأسماء (بالحرف وبالإضافة)"
    ],
    "تحفة الأطفال (الجمزوري) - التجويد": [
        "المقدمة", "باب أحكام النون الساكنة والتنوين", "باب حكم النون والميم المشددتين",
        "باب أحكام الميم الساكنة", "باب حكم لام أل ولام الفعل",
        "باب في المِثْلين والمتقاربين والمتجانسين", "باب أقسام المد وأحكامه",
        "باب أقسام المد اللازم وأحكامه", "الخاتمة"
    ],
    "المقدمة الجزرية (ابن الجزري) - التجويد": [
        "المقدمة", "باب مخارج الحروف", "باب صفات الحروف", "باب التجويد واستعمال الحروف",
        "باب التفخيم والترقيق وأحكام الراءات", "باب اللامات وأحكام النون والميم",
        "باب المد والقصر", "باب معرفة الوقف والابتداء", "باب المقطوع والموصول والتاءات",
        "باب همز الوصل والخاتمة"
    ],
    "المنظومة البيقونية (البيقوني) - مصطلح الحديث": [
        "المقدمة والحديث الصحيح", "الحديث الحسن والضعيف", "المرفوع والمقطوع والمسند والمتصل",
        "المسلسل والعزيز والمشهور", "المعنعن والمبهم والعالي والنازل",
        "الموقوف والمقطوع والمرسل والغريب", "المدرج والمدبج والمتفق والمفترق",
        "المتروك والموضوع والخاتمة"
    ],
    "الأربعون النووية (النووي) - الحديث الشريف": [
        "الحديث 1: إنما الأعمال بالنيات", "الحديث 2: مراتب الدين والإسلام والإيمان والإحسان",
        "الحديث 3: بني الإسلام على خمس", "الحديث 4: خلق الإنسان وشقاوته وسعادته",
        "الحديث 5: من أحدث في أمرنا هذا ما ليس منه فهو رد", "الحديث 6: الحلال بيّن والحرام بيّن",
        "الحديث 7: الدين النصيحة", "الحديث 8: أمرت أن أقاتل الناس حتى يشهدوا",
        "الحديث 9: ما نهيتكم عنه فاجتنبوه", "الحديث 10: إن الله طيب لا يقبل إلا طيبا"
    ],
    "عمدة الأحكام (المقدسي) - أحاديث الأحكام": [
        "كتاب الطهارة", "كتاب الصلاة", "كتاب الجنائز", "كتاب الزكاة",
        "كتاب الصيام", "كتاب الحج", "كتاب البيوع", "كتاب النكاح والطلاق"
    ],
    "نخبة الفكر (ابن حجر) - مصطلح الحديث": [
        "تقسيم الخبر إلى متواتر وآحاد", "المشهور والعزيز والغريب",
        "المقبول: الصحيح والحسن", "المردود وأسباب الرد", "الجرح والتعديل والصفات"
    ],
    "الأصول الثلاثة (محمد بن عبد الوهاب) - العقيدة": [
        "المقدمة والمسائل الأربع", "الأصل الأول: معرفة العبد ربه وأنواع العبادة",
        "الأصل الثاني: معرفة دين الإسلام بمراتبه الثلاث", "الأصل الثالث: معرفة نبيكم محمد صلى الله عليه وسلم"
    ]
};

const MATN_ALIASES = [
    { key: "الأربعون النووية (النووي) - الحديث الشريف", terms: ["نوويه", "نووي", "اربعون", "حديث", "الحديث", "احاديث", "اربعين"] },
    { key: "الآجرومية (ابن آجروم) - النحو", terms: ["اجروميه", "اجروم", "نحو", "اعراب", "قواعد"] },
    { key: "تحفة الأطفال (الجمزوري) - التجويد", terms: ["تحفه", "اطفال", "جمزوري", "تجويد"] },
    { key: "المقدمة الجزرية (ابن الجزري) - التجويد", terms: ["جزريه", "جزري", "مقدمه جزريه", "مخارج"] },
    { key: "المنظومة البيقونية (البيقوني) - مصطلح الحديث", terms: ["بيقونيه", "بيقوني", "مصطلح"] },
    { key: "عمدة الأحكام (المقدسي) - أحاديث الأحكام", terms: ["عمده", "احكام", "مقدسي"] },
    { key: "نخبة الفكر (ابن حجر) - مصطلح الحديث", terms: ["نخبه", "فكر", "ابن حجر"] },
    { key: "الأصول الثلاثة (محمد بن عبد الوهاب) - العقيدة", terms: ["اصول", "ثلاثه", "عقيده", "توحيد"] }
];

const matnChaptersCache = new Map();

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

function findMatchingMatnChapters(designatedTitle, programName) {
    const combined = `${designatedTitle || ''} ${programName || ''}`.trim();
    if (matnChaptersCache.has(combined)) {
        return matnChaptersCache.get(combined);
    }

    const cleanTarget = normalizeArabicText(combined);
    if (!cleanTarget) {
        const res = { chapters: [], matchedKey: '' };
        matnChaptersCache.set(combined, res);
        return res;
    }

    for (const key of Object.keys(MATN_CHAPTERS_MAP)) {
        const cleanKey = normalizeArabicText(key);
        if (cleanTarget.includes(cleanKey) || cleanKey.includes(cleanTarget)) {
            const res = { chapters: MATN_CHAPTERS_MAP[key], matchedKey: key };
            matnChaptersCache.set(combined, res);
            return res;
        }
    }

    for (const alias of MATN_ALIASES) {
        for (const term of alias.terms) {
            if (cleanTarget.includes(normalizeArabicText(term))) {
                const res = { chapters: MATN_CHAPTERS_MAP[alias.key] || [], matchedKey: alias.key };
                matnChaptersCache.set(combined, res);
                return res;
            }
        }
    }

    const res = { chapters: [], matchedKey: '' };
    matnChaptersCache.set(combined, res);
    return res;
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

function openAssignMatnModal(studentId, studentName, designatedMatn = '') {
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
        applyDesignatedMatnToCards(targetDesignated);
    };

    if (isAlreadyOpen) {
        setupCards();
    } else {
        // Run during slide animation to keep 60 FPS initial response
        setTimeout(setupCards, 120);
    }
}

function applyDesignatedMatnToCards(designatedTitle) {
    const cardTypes = ['memorization', 'revision', 'mudarasah'];
    const finalMatnName = designatedTitle || 'المتون العلمية';
    const currentProg = window.matnModalState.programName || '';
    window.matnModalState.designatedMatn = finalMatnName;

    cardTypes.forEach(type => {
        const hiddenInput = document.getElementById(`matnTitle-${type}`);
        if (hiddenInput && hiddenInput.value !== finalMatnName) {
            hiddenInput.value = finalMatnName;
        }
    });

    // CACHING CHECK: If chapters are already rendered for this matn & program, skip rebuilding options & DOM!
    if (window.matnModalState.hasRenderedChapters &&
        window.matnModalState.lastRenderedMatn === finalMatnName &&
        window.matnModalState.lastRenderedProgram === currentProg) {
        // Fast path: Only sync current select values if needed
        cardTypes.forEach(type => {
            const chapterSelect = document.getElementById(`matnChapterSelect-${type}`);
            const chapterInput = document.getElementById(`matnChapterName-${type}`);
            if (chapterSelect && chapterInput && !chapterInput.value && chapterSelect.value) {
                chapterInput.value = chapterSelect.value;
            }
            updateRangeTotalBadge(type);
        });
        return;
    }

    const { chapters, matchedKey } = findMatchingMatnChapters(designatedTitle, currentProg);

    // Build the options HTML string once and reuse across cards (instead of 90 separate createElement & appendChild calls!)
    let chaptersHtml = '';
    if (chapters.length > 0) {
        chaptersHtml = chapters.map((ch, idx) =>
            `<option value="${escapeHtml(ch)}"${idx === 0 ? ' selected' : ''}>${escapeHtml(ch)}</option>`
        ).join('');
    }

    cardTypes.forEach(type => {
        const chapterSelect = document.getElementById(`matnChapterSelect-${type}`);
        const chapterInput = document.getElementById(`matnChapterName-${type}`);
        const modeToggle = document.getElementById(`matnChapterModeToggle-${type}`);

        if (chapters.length > 0) {
            if (modeToggle) modeToggle.style.display = 'inline-flex';
            if (chapterSelect) {
                chapterSelect.innerHTML = chaptersHtml;
            }
            if (chapterInput) chapterInput.value = chapters[0];
            setChapterInputMode(type, 'select');
        } else {
            if (modeToggle) modeToggle.style.display = 'none';
            if (chapterInput) {
                chapterInput.value = '';
                chapterInput.placeholder = 'اكتب اسم الباب أو موضع التسميع...';
            }
            setChapterInputMode(type, 'manual');
        }

        const refName = (matchedKey || designatedTitle || currentProg || '').toLowerCase();
        if (refName.includes('النووية') || refName.includes('الحديث') || refName.includes('أحاديث') || refName.includes('اربعون')) {
            setMatnUnit(type, 5, 'أحاديث', 'حديث');
        } else if (refName.includes('الآجرومية') || refName.includes('عمدة الأحكام') || refName.includes('الأصول') || refName.includes('اجروم')) {
            setMatnUnit(type, 4, 'أبواب', 'باب');
        } else {
            setMatnUnit(type, 1, 'أبيات', 'بيت');
        }

        updateRangeTotalBadge(type);
    });

    window.matnModalState.lastRenderedMatn = finalMatnName;
    window.matnModalState.lastRenderedProgram = currentProg;
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
    const todayDateStr = new Date().toISOString().split('T')[0];

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

        payloads.push({
            studentId: studentId,
            classId: classId,
            performanceType: t.perfVal,
            unit: parseInt(window.matnUnits[t.key]) || 4,
            amount: parseFloat(amountVal),
            chapterName: combinedChapterName.substring(0, 150),
            fromNumber: parseInt(document.getElementById(`matn-fromNumber-${t.key}`)?.value) || null,
            toNumber: parseInt(document.getElementById(`matn-toNumber-${t.key}`)?.value) || null,
            status: finalStatus,
            isCompleted: finalIsCompleted,
            isUpcoming: isUpcoming,
            assignedDate: todayDateStr,
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
