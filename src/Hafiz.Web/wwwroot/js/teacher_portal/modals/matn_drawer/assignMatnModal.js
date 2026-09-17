/**
 * ==========================================================================
 * Assign Matn Modal & Drawer Controller (Hafiz Platform)
 * Pure Modular JS - Fully Isolated Architecture
 * Supports Multi-card Performance (Memorization, Revision, Mudarasah)
 * Auto-detects Program & Designated Matn (No redundant card dropdown)
 * Ready for Chapters API Integration
 * ==========================================================================
 */

// Global State Management
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
    memorization: 5,
    revision: 5,
    mudarasah: 5
};

/**
 * =========================================================================================
 * ملاحظة معمارية بخصوص "الموضع من المتن" (Chapters/Topics API):
 * =========================================================================================
 * حالياً يتم جلب أبواب ومواضع المتون من الخريطة المحلية (MATN_CHAPTERS_MAP) أدناه لأن كيان
 * Matn في قاعدة البيانات يحوي حالياً TotalChapters فقط ولا يوجد جدول فرعي مستقل للأبواب.
 *
 * الاقتراح المعماري لواجهة برمجة التطبيقات (API Proposal) المقترحة للربط مستقبلاً:
 * 1. مسار الـ API المقترح:
 *    GET /Teacher/Matn/GetChapters?matnId={guid}&matnTitle={encodeURIComponent(title)}
 *    أو: GET /api/matn/{matnId}/chapters
 *
 * 2. هيكل الاستجابة المتوقع (Expected JSON Response):
 *    [
 *      { "id": 1, "order": 1, "title": "المقدمة والكلام وما يتألف منه", "versesCount": 15 },
 *      { "id": 2, "order": 2, "title": "باب الإعراب وعلاماته", "versesCount": 24 },
 *      { "id": 3, "order": 3, "title": "باب الأفعال وأحكامها", "versesCount": 30 }
 *    ]
 *
 * 3. دالة الاستدعاء عند توفر الـ API في الـ Backend:
 *    async function fetchMatnChaptersFromApi(matnTitle, matnId = null) {
 *        try {
 *            const url = matnId
 *                ? `/Teacher/Matn/GetChapters?matnId=${matnId}`
 *                : `/Teacher/Matn/GetChapters?matnTitle=${encodeURIComponent(matnTitle)}`;
 *            const res = await fetch(url);
 *            if (res.ok) {
 *                const data = await res.json();
 *                return data.map(item => typeof item === 'string' ? item : item.title);
 *            }
 *        } catch (e) {
 *            console.warn("تعذر جلب الأبواب من الـ API، سيتم الاعتماد على الخريطة الثابتة", e);
 *        }
 *        return MATN_CHAPTERS_MAP[matnTitle] || [];
 *    }
 * =========================================================================================
 */
const MATN_CHAPTERS_MAP = {
    "الآجرومية (ابن آجروم) - النحو": [
        "المقدمة والكلام وما يتألف منه",
        "باب الإعراب وعلاماته",
        "باب الأفعال وأحكامها",
        "باب مرفوعات الأسماء (الفاعل ونائبه)",
        "باب المبتدأ والخبر ونواسخهما",
        "باب منصوبات الأسماء (المفاعيل والحال والتمييز)",
        "باب الاستثناء ولا والمنادى",
        "باب مخفوضات الأسماء (بالحرف وبالإضافة)"
    ],
    "تحفة الأطفال (الجمزوري) - التجويد": [
        "المقدمة",
        "باب أحكام النون الساكنة والتنوين",
        "باب حكم النون والميم المشددتين",
        "باب أحكام الميم الساكنة",
        "باب حكم لام أل ولام الفعل",
        "باب في المِثْلين والمتقاربين والمتجانسين",
        "باب أقسام المد وأحكامه",
        "باب أقسام المد اللازم وأحكامه",
        "الخاتمة"
    ],
    "المقدمة الجزرية (ابن الجزري) - التجويد": [
        "المقدمة",
        "باب مخارج الحروف",
        "باب صفات الحروف",
        "باب التجويد واستعمال الحروف",
        "باب التفخيم والترقيق وأحكام الراءات",
        "باب اللامات وأحكام النون والميم",
        "باب المد والقصر",
        "باب معرفة الوقف والابتداء",
        "باب المقطوع والموصول والتاءات",
        "باب همز الوصل والخاتمة"
    ],
    "المنظومة البيقونية (البيقوني) - مصطلح الحديث": [
        "المقدمة والحديث الصحيح",
        "الحديث الحسن والضعيف",
        "المرفوع والمقطوع والمسند والمتصل",
        "المسلسل والعزيز والمشهور",
        "المعنعن والمبهم والعالي والنازل",
        "الموقوف والمقطوع والمرسل والغريب",
        "المدرج والمدبج والمتفق والمفترق",
        "المتروك والموضوع والخاتمة"
    ],
    "الأربعون النووية (النووي) - الحديث الشريف": [
        "الحديث 1: إنما الأعمال بالنيات",
        "الحديث 2: مراتب الدين والإسلام والإيمان والإحسان",
        "الحديث 3: بني الإسلام على خمس",
        "الحديث 4: خلق الإنسان وشقاوته وسعادته",
        "الحديث 5: من أحدث في أمرنا هذا ما ليس منه فهو رد",
        "الحديث 6: الحلال بيّن والحرام بيّن",
        "الحديث 7: الدين النصيحة",
        "الحديث 8: أمرت أن أقاتل الناس حتى يشهدوا",
        "الحديث 9: ما نهيتكم عنه فاجتنبوه",
        "الحديث 10: إن الله طيب لا يقبل إلا طيبا"
    ],
    "عمدة الأحكام (المقدسي) - أحاديث الأحكام": [
        "كتاب الطهارة",
        "كتاب الصلاة",
        "كتاب الجنائز",
        "كتاب الزكاة",
        "كتاب الصيام",
        "كتاب الحج",
        "كتاب البيوع",
        "كتاب النكاح والطلاق"
    ],
    "نخبة الفكر (ابن حجر) - مصطلح الحديث": [
        "تقسيم الخبر إلى متواتر وآحاد",
        "المشهور والعزيز والغريب",
        "المقبول: الصحيح والحسن",
        "المردود وأسباب الرد",
        "الجرح والتعديل والصفات"
    ],
    "الأصول الثلاثة (محمد بن عبد الوهاب) - العقيدة": [
        "المقدمة والمسائل الأربع",
        "الأصل الأول: معرفة العبد ربه وأنواع العبادة",
        "الأصل الثاني: معرفة دين الإسلام بمراتبه الثلاث",
        "الأصل الثالث: معرفة نبيكم محمد صلى الله عليه وسلم"
    ]
};

// Aliases and search terms to match Islamic study programs to their correct Matn
const MATN_ALIASES = [
    { key: "الأربعون النووية (النووي) - الحديث الشريف", terms: ["نوويه", "نووي", "اربعون", "حديث", "الحديث", "احاديث", "اربعين", "سنه", "السنه"] },
    { key: "الآجرومية (ابن آجروم) - النحو", terms: ["اجروميه", "اجروم", "نحو", "اعراب", "قواعد", "لغه"] },
    { key: "تحفة الأطفال (الجمزوري) - التجويد", terms: ["تحفه", "اطفال", "جمزوري", "تجويد", "نون"] },
    { key: "المقدمة الجزرية (ابن الجزري) - التجويد", terms: ["جزريه", "جزري", "مقدمه جزريه", "مخارج"] },
    { key: "المنظومة البيقونية (البيقوني) - مصطلح الحديث", terms: ["بيقونيه", "بيقوني", "مصطلح"] },
    { key: "عمدة الأحكام (المقدسي) - أحاديث الأحكام", terms: ["عمده", "احكام", "مقدسي"] },
    { key: "نخبة الفكر (ابن حجر) - مصطلح الحديث", terms: ["نخبه", "فكر", "ابن حجر", "عسقلاني"] },
    { key: "الأصول الثلاثة (محمد بن عبد الوهاب) - العقيدة", terms: ["اصول", "ثلاثه", "عقيده", "توحيد"] }
];

/**
 * Normalizes Arabic text for flexible, resilient search matching
 */
function normalizeArabicText(text) {
    if (!text) return '';
    return text
        .replace(/[\u064B-\u065F\u0670]/g, '') // Tashkeel / Harakat
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
 * Searches and resolves predefined Matn chapters based on program name or designated Matn
 */
function findMatchingMatnChapters(designatedTitle, programName) {
    const combined = `${designatedTitle || ''} ${programName || ''}`.trim();
    const cleanTarget = normalizeArabicText(combined);
    if (!cleanTarget) return { chapters: [], matchedKey: '' };

    // 1. Check exact key or substring inclusion
    for (const key of Object.keys(MATN_CHAPTERS_MAP)) {
        const cleanKey = normalizeArabicText(key);
        if (cleanTarget.includes(cleanKey) || cleanKey.includes(cleanTarget)) {
            return { chapters: MATN_CHAPTERS_MAP[key], matchedKey: key };
        }
    }

    // 2. Check alias terms match
    for (const alias of MATN_ALIASES) {
        for (const term of alias.terms) {
            const cleanTerm = normalizeArabicText(term);
            if (cleanTarget.includes(cleanTerm)) {
                return { chapters: MATN_CHAPTERS_MAP[alias.key] || [], matchedKey: alias.key };
            }
        }
    }

    return { chapters: [], matchedKey: '' };
}

/**
 * Initializes the student list from window.allClassStudents
 */
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

/**
 * Main entrance to open the Matn Assignment Drawer/Modal
 * @param {string} studentId
 * @param {string} studentName
 * @param {string} designatedMatn Optional designated Matn title from caller
 */
function openAssignMatnModal(studentId, studentName, designatedMatn = '') {
    initMatnStudentsList();

    const selectContainer = document.getElementById('matnStudentSelectContainer');
    const selectDropdown = document.getElementById('matnStudentSelectDropdown');
    const prevBtn = document.getElementById('matnPrevBtn');
    const saveNextBtnText = document.getElementById('matnSaveNextBtnText');
    const panel = document.getElementById('matnDrawerPanel');

    // 1. Resolve Designated Matn and Study Program Name
    const targetDesignated = (designatedMatn || panel?.dataset?.assignedMatn || window.classDesignatedMatn || '').trim();
    const programName = (panel?.dataset?.programName || '').trim();

    window.matnModalState.designatedMatn = targetDesignated;
    window.matnModalState.programName = programName;

    // 2. Update Header Badges (Program Badge + Matn Badge)
    const progBadgeText = document.getElementById('matnDrawerProgramBadgeText');
    if (progBadgeText && programName) {
        progBadgeText.textContent = programName;
    }

    const matnBadge = document.getElementById('matnDrawerMatnBadge');
    const matnBadgeText = document.getElementById('matnDrawerMatnBadgeText');
    if (matnBadgeText) {
        matnBadgeText.textContent = targetDesignated || 'المتن المقرر';
        if (matnBadge) {
            matnBadge.style.display = targetDesignated ? 'inline-flex' : 'none';
        }
    }

    // 3. Handle Student selection
    if (!studentId) {
        // Opened globally (without a specific student preselected)
        window.matnModalState.currentIndex = -1;
        if (selectContainer && selectDropdown) {
            selectContainer.style.display = 'block';
            selectDropdown.innerHTML = '<option value="">-- اختر الطالب من الحلقة --</option>';
            window.matnModalState.studentsList.forEach((st, idx) => {
                const opt = document.createElement('option');
                opt.value = st.id;
                opt.textContent = `${idx + 1}. ${st.name}`;
                selectDropdown.appendChild(opt);
            });
        }

        document.getElementById('matnStudentId').value = '';
        document.getElementById('matnDrawerStudentName').textContent = 'تسميع أوراد المتون';
        document.getElementById('matnDrawerSubtitle').textContent = `طلاب الحلقة: ${window.matnModalState.studentsList.length}`;
        document.getElementById('matnDrawerAvatar').innerHTML = `<i class='bx bxs-book-reader'></i>`;
        if (prevBtn) prevBtn.disabled = true;
        if (saveNextBtnText) saveNextBtnText.textContent = 'اعتماد ورصد التسميع';
    } else {
        // Opened for a specific student
        if (selectContainer) selectContainer.style.display = 'none';

        const cleanId = studentId.toString();
        const foundIndex = window.matnModalState.studentsList.findIndex(s => s.id.toLowerCase() === cleanId.toLowerCase());
        window.matnModalState.currentIndex = foundIndex >= 0 ? foundIndex : 0;

        loadMatnStudentDataByIndex(window.matnModalState.currentIndex, studentId, studentName);
    }

    // 4. Automatically populate chapters & set units based on program's designated Matn
    applyDesignatedMatnToCards(targetDesignated);

    // 5. Default to 'all' tab (displays ALL 3 types: memorization, revision, mudarasah)
    const allTabBtn = document.querySelector('#matnSegmentedNav .segmented-btn[data-tab="all"]');
    setMatnFilterTab('all', allTabBtn);

    // 6. Open drawer & backdrop
    const backdrop = document.getElementById('matnDrawerBackdrop');
    if (panel && backdrop) {
        panel.classList.add('is-open', 'active', 'mobile-open');
        backdrop.classList.add('is-open', 'active');
        document.body.style.overflow = 'hidden';
    }
}

/**
 * Automatically applies the program's designated Matn to all cards:
 * Resolves chapters cleanly without stacking controls, recommends appropriate units,
 * and sets up preset chips.
 */
function applyDesignatedMatnToCards(designatedTitle) {
    const cardTypes = ['memorization', 'revision', 'mudarasah'];
    const finalMatnName = designatedTitle || 'المتون العلمية';
    window.matnModalState.designatedMatn = finalMatnName;

    // Set hidden inputs
    cardTypes.forEach(type => {
        const hiddenInput = document.getElementById(`matnTitle-${type}`);
        if (hiddenInput) hiddenInput.value = finalMatnName;
    });

    const { chapters, matchedKey } = findMatchingMatnChapters(designatedTitle, window.matnModalState.programName);

    // Populate each card's chapter dropdown or custom input cleanly
    cardTypes.forEach(type => {
        const chapterSelect = document.getElementById(`matnChapterSelect-${type}`);
        const chapterInput = document.getElementById(`matnChapterName-${type}`);
        const selectWrap = document.getElementById(`matnChapterSelectWrap-${type}`);
        const inputWrap = document.getElementById(`matnChapterInputWrap-${type}`);
        const toggleBtn = document.getElementById(`matnChapterToggleBtn-${type}`);
        const returnBtn = document.getElementById(`matnReturnListBtn-${type}`);

        if (chapters.length > 0) {
            // Mode A: Predefined chapters exist
            if (chapterSelect) {
                chapterSelect.innerHTML = '';
                chapters.forEach((ch, idx) => {
                    const opt = document.createElement('option');
                    opt.value = ch;
                    opt.textContent = ch;
                    if (idx === 0) opt.selected = true;
                    chapterSelect.appendChild(opt);
                });

                // Add custom option at the bottom
                const customOpt = document.createElement('option');
                customOpt.value = '__custom__';
                customOpt.textContent = '✏️ كتابة موضع مخصص آخر...';
                chapterSelect.appendChild(customOpt);
            }

            if (chapterInput) chapterInput.value = chapters[0];

            // Show Select wrapper, Hide Custom Input wrapper
            if (selectWrap) selectWrap.style.display = 'block';
            if (inputWrap) inputWrap.style.display = 'none';
            if (toggleBtn) toggleBtn.style.display = 'inline-flex';
            if (returnBtn) returnBtn.style.display = 'inline-flex';
        } else {
            // Mode B: No predefined chapters -> HIDE SELECT COMPLETELY!
            if (selectWrap) selectWrap.style.display = 'none';
            if (inputWrap) inputWrap.style.display = 'block';
            if (chapterInput) {
                chapterInput.value = '';
                chapterInput.placeholder = 'اكتب اسم الباب أو موضع التسميع...';
            }
            if (toggleBtn) toggleBtn.style.display = 'none';
            if (returnBtn) returnBtn.style.display = 'none';
        }

        // Recommend appropriate unit for this Matn
        const refName = (matchedKey || designatedTitle || window.matnModalState.programName || '').toLowerCase();
        if (refName.includes('النووية') || refName.includes('الحديث') || refName.includes('أحاديث') || refName.includes('اربعون')) {
            setMatnUnit(type, 5, 'أحاديث', 'حديث');
        } else if (refName.includes('الآجرومية') || refName.includes('عمدة الأحكام') || refName.includes('الأصول') || refName.includes('اجروم')) {
            setMatnUnit(type, 4, 'أبواب', 'باب');
        } else {
            setMatnUnit(type, 1, 'أبيات', 'بيت');
        }

        // Update range summary badge
        updateRangeTotalBadge(type);
    });
}

/**
 * Switches cleanly to custom manual text input for chapters
 */
function toggleManualChapterEntry(type) {
    const selectWrap = document.getElementById(`matnChapterSelectWrap-${type}`);
    const inputWrap = document.getElementById(`matnChapterInputWrap-${type}`);
    const chapterInput = document.getElementById(`matnChapterName-${type}`);
    const chapterSelect = document.getElementById(`matnChapterSelect-${type}`);

    if (selectWrap) selectWrap.style.display = 'none';
    if (inputWrap) inputWrap.style.display = 'block';
    if (chapterSelect) chapterSelect.value = '__custom__';

    if (chapterInput) {
        chapterInput.focus();
        if (chapterSelect && chapterInput.value === chapterSelect.options[0]?.value) {
            chapterInput.value = '';
        }
    }
}

/**
 * Returns from manual text entry back to chapter dropdown list
 */
function returnToChapterList(type) {
    const selectWrap = document.getElementById(`matnChapterSelectWrap-${type}`);
    const inputWrap = document.getElementById(`matnChapterInputWrap-${type}`);
    const chapterSelect = document.getElementById(`matnChapterSelect-${type}`);
    const chapterInput = document.getElementById(`matnChapterName-${type}`);

    if (selectWrap) selectWrap.style.display = 'block';
    if (inputWrap) inputWrap.style.display = 'none';

    if (chapterSelect) {
        if (chapterSelect.value === '__custom__' && chapterSelect.options.length > 1) {
            chapterSelect.selectedIndex = 0;
        }
        if (chapterInput) {
            chapterInput.value = chapterSelect.value;
        }
    }
}

/**
 * Handles dropdown change event
 */
function onMatnChapterSelectChange(type, val) {
    const chapterInput = document.getElementById(`matnChapterName-${type}`);
    if (val === '__custom__') {
        toggleManualChapterEntry(type);
    } else {
        if (chapterInput) {
            chapterInput.value = val;
        }
    }
}

/**
 * Loads student information and updates subtitle & stepper buttons
 */
function loadMatnStudentDataByIndex(index, fallbackId, fallbackName) {
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

    // Initials Avatar Badge
    const initials = (stName || '').split(' ').filter(Boolean).slice(0, 2).map(n => n[0]).join('');
    const avatar = document.getElementById('matnDrawerAvatar');
    if (avatar) {
        avatar.innerHTML = initials ? `<span>${initials}</span>` : `<i class='bx bxs-book-reader'></i>`;
    }

    // Stepper buttons state
    const prevBtn = document.getElementById('matnPrevBtn');
    const saveNextBtnText = document.getElementById('matnSaveNextBtnText');

    if (prevBtn) {
        prevBtn.disabled = (index <= 0);
    }

    if (saveNextBtnText) {
        if (index >= 0 && index < totalCount - 1) {
            saveNextBtnText.textContent = 'حفظ والتالي';
        } else {
            saveNextBtnText.textContent = 'حفظ وإنهاء';
        }
    }
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
    if (drawer) {
        drawer.classList.remove('is-open', 'active', 'mobile-open');
    }
    if (backdrop) {
        backdrop.classList.remove('is-open', 'active');
    }
    setTimeout(() => {
        document.body.style.overflow = '';
    }, 260);
}

// Segmented Navigation: When 'all' is active, ALL 3 CARDS ARE DISPLAYED!
function setMatnFilterTab(tabKey, btn) {
    window.matnModalState.currentTab = tabKey;
    const nav = document.getElementById('matnSegmentedNav');
    if (nav) {
        nav.querySelectorAll('.segmented-btn').forEach(b => b.classList.remove('active'));
    }
    if (btn) btn.classList.add('active');

    const boxes = document.querySelectorAll('#matnDrawerBody .drawer-wird-box');
    boxes.forEach(box => {
        const bType = box.dataset.type;
        const visible = (tabKey === 'all' || tabKey === bType);
        box.style.display = visible ? 'block' : 'none';
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
        if (isChecked) {
            box.classList.remove('is-disabled');
        } else {
            box.classList.add('is-disabled');
        }
    }
}

/**
 * Sets the active measurement unit and updates labels, chips and range badge
 */
function setMatnUnit(type, val, unitName, unitTag) {
    window.matnUnits[type] = parseInt(val);

    const group = document.getElementById(`matn-unitGroup-${type}`);
    if (group) {
        group.querySelectorAll('.unit-radio-option').forEach(opt => {
            const radio = opt.querySelector('input[type="radio"]');
            if (radio && parseInt(radio.value) === parseInt(val)) {
                opt.classList.add('is-selected');
                radio.checked = true;
            } else {
                opt.classList.remove('is-selected');
                if (radio) radio.checked = false;
            }
        });
    }

    const fromLabel = document.getElementById(`matn-fromLabel-${type}`);
    const toLabel = document.getElementById(`matn-toLabel-${type}`);
    if (fromLabel) fromLabel.textContent = `من (${unitTag}):`;
    if (toLabel) toLabel.textContent = `إلى (${unitTag}):`;

    updateAmountPresetChipsForUnit(type, parseInt(val));
    updateRangeTotalBadge(type);
}

/**
 * Stepper increment/decrement for Amount
 */
function stepMatnAmount(type, delta) {
    const amountInput = document.getElementById(`matn-amount-${type}`);
    if (!amountInput) return;

    let currentVal = parseFloat(amountInput.value) || 1;
    let nextVal = Math.max(1, currentVal + delta);
    amountInput.value = nextVal;
    onMatnAmountInput(type, nextVal);
}

/**
 * Quick preset chip tap for Amount
 */
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

    if (toVal >= fromVal) {
        const calculated = (toVal - fromVal + 1);
        if (amountInput) amountInput.value = calculated;
    }
    updateRangeTotalBadge(type);
}

function getUnitDisplayTag(unitVal) {
    switch (parseInt(unitVal)) {
        case 5: return 'حديث';
        case 4: return 'باب';
        case 3: return 'صفحة';
        case 2: return 'سطر';
        case 1:
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
    badge.textContent = getUnitPluralTag(unit, count);
}

function updateAmountPresetChipsForUnit(type, unitVal) {
    const container = document.getElementById(`matn-amountChips-${type}`);
    if (!container) return;

    let presets = [1, 2, 3, 5];
    if (unitVal === 1) { // أبيات
        presets = [1, 5, 10, 15];
    } else if (unitVal === 4) { // أبواب
        presets = [1, 2, 3, 5];
    } else if (unitVal === 5) { // أحاديث
        presets = [1, 2, 3, 5];
    } else if (unitVal === 3) { // صفحات
        presets = [1, 2, 3, 5];
    }

    container.innerHTML = '';
    presets.forEach(p => {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'amount-chip';
        btn.textContent = p;
        btn.onclick = () => setQuickAmount(type, p);
        container.appendChild(btn);
    });
}

/**
 * Appends teacher feedback text cleanly into note input
 */
function appendMatnNote(type, noteText) {
    const input = document.getElementById(`matnNote-${type}`);
    if (!input) return;

    if (input.value.trim().length > 0) {
        if (!input.value.includes(noteText)) {
            input.value = `${input.value.trim()} - ${noteText}`;
        }
    } else {
        input.value = noteText;
    }

    input.focus();
    input.classList.add('highlight-pulse');
    setTimeout(() => input.classList.remove('highlight-pulse'), 500);
}

/**
 * Quick Action Bar: Marks all enabled cards as Excellent (5)
 */
function setAllMatnRatingsExcellent() {
    const cardTypes = ['memorization', 'revision', 'mudarasah'];
    cardTypes.forEach(type => {
        const toggle = document.getElementById(`matn-wtoggle-${type}`);
        if (toggle && toggle.checked) {
            setMatnRating(type, 5, 'excellent', 'متقن تماماً');
        }
    });

    Swal.fire({
        icon: 'success',
        title: 'تم رصد التقييم الممتاز',
        text: 'تم تحديد تقييم (ممتاز) لجميع الأوراد المفعلة بنجاح.',
        timer: 1200,
        showConfirmButton: false,
        toast: true,
        position: 'top-end'
    });
}

/**
 * Quick Action Bar: Automatically advances recitation range from previous end
 */
function autoContinueMatnRecitation(btn) {
    const cardTypes = ['memorization', 'revision', 'mudarasah'];
    let updatedCount = 0;

    cardTypes.forEach(type => {
        const toggle = document.getElementById(`matn-wtoggle-${type}`);
        if (!toggle || !toggle.checked) return;

        const fromInput = document.getElementById(`matn-fromNumber-${type}`);
        const toInput = document.getElementById(`matn-toNumber-${type}`);
        const amountInput = document.getElementById(`matn-amount-${type}`);

        if (fromInput && toInput) {
            const currentTo = parseInt(toInput.value) || 1;
            const amount = parseInt(amountInput?.value) || 1;

            const nextFrom = currentTo + 1;
            const nextTo = nextFrom + amount - 1;

            fromInput.value = nextFrom;
            toInput.value = nextTo;
            updateRangeTotalBadge(type);
            updatedCount++;
        }
    });

    if (btn) {
        btn.classList.add('btn-success-pulse');
        setTimeout(() => btn.classList.remove('btn-success-pulse'), 800);
    }

    Swal.fire({
        icon: 'info',
        title: 'تم استكمال النطاق',
        text: 'تم تقديم نطاق التسميع تلقائياً للأوراد المفعلة.',
        timer: 1300,
        showConfirmButton: false,
        toast: true,
        position: 'top-end'
    });
}

function setMatnRating(type, status, grade, hintText) {
    window.matnRatings[type] = status;
    const group = document.getElementById(`matn-ratingGroup-${type}`);
    if (!group) return;

    group.querySelectorAll('.rating-pill-btn').forEach(btn => {
        if (parseInt(btn.dataset.status) === parseInt(status)) {
            btn.classList.add('is-selected');
        } else {
            btn.classList.remove('is-selected');
        }
    });

    const badge = document.getElementById(`matn-ratingBadge-${type}`);
    const clearBtn = document.getElementById(`matn-ratingClear-${type}`);
    if (badge) {
        badge.className = `wird-rating-badge rate-${grade}`;
        badge.textContent = hintText || 'تم التقييم';
    }
    if (clearBtn) clearBtn.style.display = 'inline-flex';
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
    if (clearBtn) clearBtn.style.display = 'none';
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return '';
}

/**
 * Submits the Matn assignment(s) for the active card(s)
 * Handled with silent date (today's date) and automatic program Matn title
 */
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

    // All 3 Matn types: Memorization (1), Revision (2), Mudarasah (3)
    const allTypes = [
        { key: 'memorization', perfVal: 1 },
        { key: 'revision', perfVal: 2 },
        { key: 'mudarasah', perfVal: 3 }
    ];

    // Check all cards when tab is 'all', or only the active tab card
    const targetTypes = window.matnModalState.currentTab === 'all'
        ? allTypes
        : allTypes.filter(t => t.key === window.matnModalState.currentTab);

    const payloads = [];
    const todayDateStr = new Date().toISOString().split('T')[0];

    // Matn title is designated automatically from the Study Program
    const matnTitle = window.matnModalState.designatedMatn
        || document.getElementById('matnDrawerPanel')?.dataset?.assignedMatn
        || 'المتون العلمية';

    for (const t of targetTypes) {
        const toggle = document.getElementById(`matn-wtoggle-${t.key}`);
        if (!toggle || !toggle.checked) continue; // Skip disabled card

        const amountVal = document.getElementById(`matn-amount-${t.key}`)?.value;
        if (!amountVal || parseFloat(amountVal) <= 0) continue;

        const chapterSelectVal = document.getElementById(`matnChapterSelect-${t.key}`)?.value || '';
        const chapterCustom = document.getElementById(`matnChapterName-${t.key}`)?.value.trim() || '';
        const chapterVal = (chapterSelectVal === '__custom__' ? chapterCustom : chapterSelectVal) || chapterCustom;
        const combinedChapterName = chapterVal ? `${matnTitle}: ${chapterVal}` : matnTitle;

        payloads.push({
            studentId: studentId,
            classId: classId,
            performanceType: t.perfVal,
            unit: parseInt(window.matnUnits[t.key]) || 4,
            amount: parseFloat(amountVal),
            chapterName: combinedChapterName.substring(0, 150),
            fromNumber: parseInt(document.getElementById(`matn-fromNumber-${t.key}`)?.value) || null,
            toNumber: parseInt(document.getElementById(`matn-toNumber-${t.key}`)?.value) || null,
            status: parseInt(window.matnRatings[t.key]) || 5,
            isCompleted: true,
            isUpcoming: document.getElementById(`matn-isUpcoming-${t.key}`)?.checked || false,
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
                setTimeout(() => {
                    window.location.reload();
                }, 1200);
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
window.toggleManualChapterEntry = toggleManualChapterEntry;
window.returnToChapterList = returnToChapterList;
window.onMatnChapterSelectChange = onMatnChapterSelectChange;
window.setMatnFilterTab = setMatnFilterTab;
window.toggleMatnCard = toggleMatnCard;
window.setMatnUnit = setMatnUnit;
window.stepMatnAmount = stepMatnAmount;
window.setQuickAmount = setQuickAmount;
window.onMatnAmountInput = onMatnAmountInput;
window.calculateMatnAmount = calculateMatnAmount;
window.appendMatnNote = appendMatnNote;
window.setAllMatnRatingsExcellent = setAllMatnRatingsExcellent;
window.autoContinueMatnRecitation = autoContinueMatnRecitation;
window.setMatnRating = setMatnRating;
window.clearMatnRating = clearMatnRating;
window.submitMatnAssignment = submitMatnAssignment;
window.navigateMatnStudent = navigateMatnStudent;
window.onMatnStudentDropdownChange = onMatnStudentDropdownChange;
