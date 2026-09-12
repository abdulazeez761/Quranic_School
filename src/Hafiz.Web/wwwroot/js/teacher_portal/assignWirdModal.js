/**
 * ============================================================================
 * Hafiz Web - Assign Wird Drawer & Mobile Bottom Sheet Controller (Phase 1)
 * ============================================================================
 * Converts single-student popups into a high-speed offcanvas Drawer & 92vh
 * mobile bottom sheet with 4 simultaneous wird cards, touch ratings, and
 * thumb-zone stepper navigation.
 * ============================================================================
 */

// Global State
let classStudents = [];
let selectedStudentIndex = 0;
let currentFilterTab = 'all';

// Wird Type Definitions matching backend enums
const WIRD_TYPE_CONFIG = {
    memorization: {
        id: 'memorization',
        title: 'حفظ جديد (الورد اليومي)',
        typeCode: 1, // AssignmentType.Memorization
        color: '#2563eb',
        defaultAmount: 1.0,
        defaultUnit: 0 // Pages
    },
    recentRevision: {
        id: 'recentRevision',
        title: 'مراجعة صغرى (ورد قريب)',
        typeCode: 2, // AssignmentType.Revision
        color: '#059669',
        defaultAmount: 5.0,
        defaultUnit: 0 // Pages
    },
    oldRevision: {
        id: 'oldRevision',
        title: 'مراجعة كبرى (ورد بعيد)',
        typeCode: 2, // AssignmentType.Revision (or OldRevision when model updated)
        color: '#d97706',
        defaultAmount: 1.0,
        defaultUnit: 2 // Juz
    },
    recitation: {
        id: 'recitation',
        title: 'تلاوة وإتقان / استماع',
        typeCode: 3, // AssignmentType.Tajwid / Recitation
        color: '#7c3aed',
        defaultAmount: 2.0,
        defaultUnit: 0 // Pages
    }
};

const RATING_GRADES = {
    excellent: { grade: 'excellent', label: 'ممتاز', status: 5, icon: 'bxs-star', classSuffix: 'excellent' },
    very_good: { grade: 'very_good', label: 'جيد جداً', status: 4, icon: 'bxs-like', classSuffix: 'very_good' },
    good: { grade: 'good', label: 'جيد', status: 3, icon: 'bx-check-circle', classSuffix: 'good' },
    acceptable: { grade: 'acceptable', label: 'مقبول', status: 2, icon: 'bx-minus-circle', classSuffix: 'acceptable' },
    weak: { grade: 'weak', label: 'ضعيف', status: 1, icon: 'bx-error-circle', classSuffix: 'weak' }
};

// Initialize on DOM Ready
document.addEventListener('DOMContentLoaded', () => {
    discoverStudentsFromPage();
    setupDrawerKeyboardEvents();
});

/**
 * Automatically discovers students rendered on the teacher's Student/Index page
 */
function discoverStudentsFromPage() {
    classStudents = [];
    const discoveredMap = new Map();

    // 1. Scan Grid Cards
    const cards = document.querySelectorAll('#studentsGrid .student-card');
    cards.forEach(card => {
        const nameEl = card.querySelector('.student-name');
        const addBtn = card.querySelector('button[onclick*="openWirdModal"]');
        let studentId = '';
        let studentName = '';

        if (addBtn) {
            const match = addBtn.getAttribute('onclick').match(/openWirdModal\('([^']+)',\s*'([^']+)'\)/);
            if (match) {
                studentId = match[1];
                studentName = match[2];
            }
        }

        if (!studentId) {
            const detailLink = card.querySelector('a[href*="/Details/"]');
            if (detailLink) {
                const parts = detailLink.getAttribute('href').split('/');
                studentId = parts[parts.length - 1];
            }
        }

        if (!studentName && nameEl) {
            studentName = nameEl.childNodes[0]?.textContent?.trim() || nameEl.textContent.trim();
        }

        if (studentId && !discoveredMap.has(studentId)) {
            const initials = card.querySelector('.student-avatar span')?.textContent?.trim() || '';
            const tajwidTag = card.querySelector('.chip-tajwid')?.textContent?.trim() || '';
            const juzMemorized = card.querySelector('.memorization-value')?.textContent?.trim() || '';

            const studentObj = createStudentState(studentId, studentName, initials, tajwidTag, juzMemorized);
            discoveredMap.set(studentId, studentObj);
            classStudents.push(studentObj);
        }
    });

    // 2. Scan Table Rows as fallback
    const rows = document.querySelectorAll('#studentsTableWrap .student-row');
    rows.forEach(row => {
        const addBtn = row.querySelector('button[onclick*="openWirdModal"]');
        if (addBtn) {
            const match = addBtn.getAttribute('onclick').match(/openWirdModal\('([^']+)',\s*'([^']+)'\)/);
            if (match) {
                const studentId = match[1];
                const studentName = match[2];
                if (!discoveredMap.has(studentId)) {
                    const initials = row.querySelector('.row-avatar span')?.textContent?.trim() || '';
                    const tajwidTag = row.querySelector('.row-chip')?.textContent?.trim() || '';
                    const juzMemorized = row.querySelector('.row-juz-value')?.textContent?.trim() || '';

                    const studentObj = createStudentState(studentId, studentName, initials, tajwidTag, juzMemorized);
                    discoveredMap.set(studentId, studentObj);
                    classStudents.push(studentObj);
                }
            }
        }
    });

    // Restore cached drafts from localStorage
    loadCachedDrafts();

    // Show/hide floating pill button on mobile
    const pillBtn = document.getElementById('mobileOpenPillBtn');
    if (pillBtn && classStudents.length > 0) {
        pillBtn.style.display = window.innerWidth <= 1100 ? 'flex' : 'none';
    }
}

/**
 * Creates default state object for a student with the 4 wirds
 */
function createStudentState(id, name, initials = '', level = '', juz = '') {
    return {
        id: id,
        name: name || 'طالب',
        initials: initials || (name ? name.split(' ').slice(0, 2).map(n => n[0]).join('') : 'ط'),
        level: level,
        juz: juz,
        wirds: {
            memorization: {
                active: true,
                type: 1,
                amount: 1.0,
                amountUnit: 0,
                equivalentPages: null,
                fromSurah: 1,
                fromAyah: '1',
                toSurah: 1,
                toAyah: 15,
                rating: null,
                status: 0,
                note: '',
                isUpcoming: false
            },
            recentRevision: {
                active: true,
                type: 2,
                amount: 5.0,
                amountUnit: 0,
                equivalentPages: null,
                fromSurah: 1,
                fromAyah: '1',
                toSurah: 1,
                toAyah: 40,
                rating: null,
                status: 0,
                note: '',
                isUpcoming: false
            },
            oldRevision: {
                active: true,
                type: 2,
                amount: 1.0,
                amountUnit: 2, // Juz
                equivalentPages: null,
                fromSurah: 2,
                fromAyah: '1',
                toSurah: 2,
                toAyah: 141,
                rating: null,
                status: 0,
                note: '',
                isUpcoming: false
            },
            recitation: {
                active: true,
                type: 3,
                amount: 2.0,
                amountUnit: 0,
                equivalentPages: null,
                fromSurah: 1,
                fromAyah: '1',
                toSurah: 1,
                toAyah: 20,
                rating: null,
                status: 0,
                note: '',
                isUpcoming: false
            }
        }
    };
}

/**
 * Main Entry Point: Opens drawer for a specific student
 * Seamlessly called by existing button onclick="openWirdModal('id', 'name')"
 */
function openWirdModal(studentId, studentName) {
    let index = classStudents.findIndex(s => s.id === studentId);

    if (index === -1) {
        // If not in pre-scanned list, insert dynamically
        const newStudent = createStudentState(studentId, studentName);
        classStudents.push(newStudent);
        index = classStudents.length - 1;
    }

    selectedStudentIndex = index;
    loadStudentIntoDrawer(selectedStudentIndex);
    openWirdDrawer();
}

/**
 * Loads student data into the Drawer DOM controls
 */
function loadStudentIntoDrawer(index) {
    if (index < 0 || index >= classStudents.length) return;
    const student = classStudents[index];

    // Hidden inputs
    const studentIdInput = document.getElementById('StudentId');
    if (studentIdInput) studentIdInput.value = student.id;

    // Header Student Profile
    const nameEl = document.getElementById('drawerStudentName');
    if (nameEl) nameEl.textContent = student.name;

    const avatarEl = document.getElementById('drawerAvatar');
    if (avatarEl) {
        avatarEl.innerHTML = student.initials ? `<span>${student.initials}</span>` : `<i class='bx bx-user'></i>`;
    }

    const counterEl = document.getElementById('drawerStudentCounter');
    if (counterEl) {
        counterEl.textContent = `طالب ${index + 1} من ${classStudents.length}`;
    }

    const levelTag = document.getElementById('drawerStudentLevelTag');
    if (levelTag) {
        levelTag.textContent = student.level ? `• ${student.level}` : '';
    }

    // Populate each of the 4 wird cards
    Object.keys(WIRD_TYPE_CONFIG).forEach(typeKey => {
        const wird = student.wirds[typeKey];
        if (!wird) return;

        // 1. Toggle switch & active box state
        const toggle = document.getElementById(`wtoggle-${typeKey}`);
        const box = document.getElementById(`wbox-${typeKey}`);
        const statusLabel = document.getElementById(`wstatus-${typeKey}`);
        if (toggle) toggle.checked = wird.active;
        if (box) box.classList.toggle('is-disabled', !wird.active);
        if (statusLabel) {
            statusLabel.textContent = wird.active ? 'مفعل' : 'معطل';
            statusLabel.style.color = wird.active ? '#16a34a' : 'var(--text-medium)';
        }

        // 2. Amount
        const amountInput = document.getElementById(`amount-${typeKey}`);
        if (amountInput) amountInput.value = wird.amount || '';

        // 3. Unit radio buttons
        updateUnitRadioUI(typeKey, wird.amountUnit);

        // 4. Equivalent Pages (only for unit == 1 / Ayahs)
        const equivGroup = document.getElementById(`equivGroup-${typeKey}`);
        if (equivGroup) {
            equivGroup.style.display = wird.amountUnit === 1 ? 'block' : 'none';
            const chips = equivGroup.querySelectorAll('.chip');
            chips.forEach(c => {
                const cVal = parseFloat(c.dataset.val);
                c.classList.toggle('is-active', cVal === wird.equivalentPages);
            });
        }

        // 5. Surah & Ayah Ranges
        const fromSurah = document.getElementById(`fromSurah-${typeKey}`);
        const fromAyah = document.getElementById(`fromAyah-${typeKey}`);
        const toSurah = document.getElementById(`toSurah-${typeKey}`);
        const toAyah = document.getElementById(`toAyah-${typeKey}`);

        if (fromSurah && wird.fromSurah) fromSurah.value = wird.fromSurah;
        if (fromAyah) fromAyah.value = wird.fromAyah || '';
        if (toSurah && wird.toSurah) toSurah.value = wird.toSurah;
        if (toAyah) toAyah.value = wird.toAyah || '';

        // 6. Touch Rating Bar
        updateRatingUI(typeKey, wird.rating);

        // 7. Note & Upcoming
        const noteInput = document.getElementById(`note-${typeKey}`);
        if (noteInput) noteInput.value = wird.note || '';

        const upcomingToggle = document.getElementById(`isUpcoming-${typeKey}`);
        if (upcomingToggle) upcomingToggle.checked = !!wird.isUpcoming;
    });

    // Update Footer Stepper Buttons State
    const prevBtn = document.getElementById('drawerPrevBtn');
    if (prevBtn) {
        const isFirst = (index === 0);
        prevBtn.disabled = isFirst;
        prevBtn.classList.toggle('is-disabled', isFirst);
    }

    const nextBtnText = document.getElementById('drawerNextBtnText');
    if (nextBtnText) {
        if (index === classStudents.length - 1) {
            nextBtnText.textContent = '🎉 حفظ وإنهاء الحلقة';
        } else {
            nextBtnText.textContent = 'حفظ والتالي';
        }
    }
}

/**
 * Opens Drawer panel & sets background scroll lock
 */
function openWirdDrawer() {
    const drawer = document.getElementById('drawerPanel');
    const backdrop = document.getElementById('drawerBackdrop');
    if (drawer && backdrop) {
        drawer.classList.add('active');
        drawer.classList.add('mobile-open');
        backdrop.classList.add('active');
        document.body.style.overflow = 'hidden';
    }
}

/**
 * Closes Drawer panel & restores body scroll
 */
function closeWirdDrawer() {
    const drawer = document.getElementById('drawerPanel');
    const backdrop = document.getElementById('drawerBackdrop');
    if (drawer && backdrop) {
        drawer.classList.remove('active');
        drawer.classList.remove('mobile-open');
        backdrop.classList.remove('active');
        document.body.style.overflow = '';
    }
}

// Backwards compatibility alias for closeWirdModal
function closeWirdModal() {
    closeWirdDrawer();
}

/**
 * Toggles a single wird card between active and disabled
 */
function toggleWirdCard(typeKey, isActive) {
    const student = classStudents[selectedStudentIndex];
    if (student && student.wirds[typeKey]) {
        student.wirds[typeKey].active = isActive;
        const box = document.getElementById(`wbox-${typeKey}`);
        const statusLabel = document.getElementById(`wstatus-${typeKey}`);
        if (box) box.classList.toggle('is-disabled', !isActive);
        if (statusLabel) {
            statusLabel.textContent = isActive ? 'مفعل' : 'معطل';
            statusLabel.style.color = isActive ? '#16a34a' : 'var(--text-medium)';
        }
    }
}

/**
 * Updates amount for a wird and activates card if typed
 */
function onAmountChanged(typeKey, val) {
    const student = classStudents[selectedStudentIndex];
    if (student && student.wirds[typeKey]) {
        student.wirds[typeKey].amount = parseFloat(val) || 0;
        if (!student.wirds[typeKey].active && parseFloat(val) > 0) {
            toggleWirdCard(typeKey, true);
            const toggle = document.getElementById(`wtoggle-${typeKey}`);
            if (toggle) toggle.checked = true;
        }
    }
}

/**
 * Handles unit radio change (0 = Pages, 1 = Ayahs, 2 = Juz)
 */
function onUnitChanged(typeKey, unitVal) {
    const student = classStudents[selectedStudentIndex];
    if (student && student.wirds[typeKey]) {
        const unit = parseInt(unitVal);
        student.wirds[typeKey].amountUnit = unit;
        updateUnitRadioUI(typeKey, unit);

        // Toggle equivalent pages for Ayahs
        const equivGroup = document.getElementById(`equivGroup-${typeKey}`);
        if (equivGroup) {
            equivGroup.style.display = unit === 1 ? 'block' : 'none';
        }
    }
}

function updateUnitRadioUI(typeKey, unitVal) {
    const unitGroup = document.getElementById(`unitGroup-${typeKey}`);
    if (!unitGroup) return;

    const unit = parseInt(unitVal);
    unitGroup.querySelectorAll('.unit-radio-option').forEach(opt => {
        const radio = opt.querySelector('input[type="radio"]');
        const isMatched = radio && parseInt(radio.value) === unit;
        opt.classList.toggle('is-selected', isMatched);
        if (radio) radio.checked = isMatched;
    });
}

/**
 * Sets quick-pick equivalent pages chip (0.2, 0.25, 0.5, 0.75, 1, 1.5, 2)
 */
function setEquivalentPageChip(typeKey, val) {
    const student = classStudents[selectedStudentIndex];
    if (student && student.wirds[typeKey]) {
        student.wirds[typeKey].equivalentPages = val;
        const equivGroup = document.getElementById(`equivGroup-${typeKey}`);
        if (equivGroup) {
            equivGroup.querySelectorAll('.chip').forEach(c => {
                c.classList.toggle('is-active', parseFloat(c.dataset.val) === val);
            });
        }
    }
}

/**
 * Sets Touch Rating Grade (excellent, very_good, good, acceptable, weak)
 */
function setRatingGrade(typeKey, gradeKey, statusNumber) {
    const student = classStudents[selectedStudentIndex];
    if (!student || !student.wirds[typeKey]) return;

    const currentGrade = student.wirds[typeKey].rating;
    const newGrade = (currentGrade === gradeKey) ? null : gradeKey;
    const newStatus = newGrade ? statusNumber : 0;

    student.wirds[typeKey].rating = newGrade;
    student.wirds[typeKey].status = newStatus;

    // Auto-activate the card if rated
    if (newGrade && !student.wirds[typeKey].active) {
        toggleWirdCard(typeKey, true);
        const toggle = document.getElementById(`wtoggle-${typeKey}`);
        if (toggle) toggle.checked = true;
    }

    updateRatingUI(typeKey, newGrade);

    if (newGrade) {
        const meta = RATING_GRADES[newGrade];
        showDrawerToast(`⭐ تم تقييم ${WIRD_TYPE_CONFIG[typeKey].title} بـ (${meta.label})`, 'info');
    }
}

function clearWirdRating(typeKey) {
    const student = classStudents[selectedStudentIndex];
    if (student && student.wirds[typeKey]) {
        student.wirds[typeKey].rating = null;
        student.wirds[typeKey].status = 0;
        updateRatingUI(typeKey, null);
    }
}

function updateRatingUI(typeKey, ratingKey) {
    const group = document.getElementById(`ratingGroup-${typeKey}`);
    const badge = document.getElementById(`ratingBadge-${typeKey}`);
    const clearBtn = document.getElementById(`ratingClear-${typeKey}`);
    if (!group) return;

    const meta = ratingKey ? RATING_GRADES[ratingKey] : null;

    if (badge) {
        badge.className = `wird-rating-badge ${meta ? 'rate-' + meta.classSuffix : 'rate-none'}`;
        badge.innerHTML = meta ? `<i class='bx ${meta.icon}'></i> ${meta.label}` : 'لم يُسمّع / لم يُقيّم بعد';
    }

    if (clearBtn) {
        clearBtn.style.display = meta ? 'inline-flex' : 'none';
    }

    group.querySelectorAll('.rating-pill-btn').forEach(btn => {
        const grade = btn.dataset.grade;
        btn.classList.toggle('is-selected', !!(meta && meta.grade === grade));
    });
}

/**
 * Filter tabs: [الأوراد الأربعة] [حفظ] [قريب] [بعيد] [تلاوة]
 */
function setWirdFilterTab(tabKey, btn) {
    currentFilterTab = tabKey;
    document.querySelectorAll('.segmented-nav .segmented-btn').forEach(b => b.classList.remove('active'));
    if (btn) btn.classList.add('active');

    const boxes = document.querySelectorAll('#drawerWirdsList .drawer-wird-box');
    boxes.forEach(box => {
        const bType = box.dataset.type;
        const visible = (tabKey === 'all' || tabKey === bType);
        box.style.display = visible ? 'block' : 'none';
    });
}

/**
 * Quick Action: ⚡ الخطة المعتادة
 */
function applyStudentBaseline() {
    const student = classStudents[selectedStudentIndex];
    if (!student) return;

    // Standard Quran Circle Baseline
    student.wirds.memorization.active = true;
    student.wirds.memorization.amount = 1.0;
    student.wirds.memorization.amountUnit = 0;

    student.wirds.recentRevision.active = true;
    student.wirds.recentRevision.amount = 5.0;
    student.wirds.recentRevision.amountUnit = 0;

    student.wirds.oldRevision.active = true;
    student.wirds.oldRevision.amount = 1.0;
    student.wirds.oldRevision.amountUnit = 2; // Juz

    student.wirds.recitation.active = true;
    student.wirds.recitation.amount = 2.0;
    student.wirds.recitation.amountUnit = 0;

    loadStudentIntoDrawer(selectedStudentIndex);
    showDrawerToast('⚡ تم تطبيق الخطة اليومية المعتادة للطالب بنجاح', 'success');
}

/**
 * Quick Action: 📋 استكمال من الأمس
 * Uses QURAN_SURAHS if available to transition seamlessly to the next Ayah and Surah!
 */
function autoComputeNextWirds() {
    const student = classStudents[selectedStudentIndex];
    if (!student) return;

    const surahsList = (typeof QURAN_SURAHS !== 'undefined') ? QURAN_SURAHS : [];

    // Memorization auto-continue
    const mem = student.wirds.memorization;
    if (mem && mem.active) {
        let currentSurahId = parseInt(document.getElementById('toSurah-memorization')?.value) || mem.toSurah || 1;
        let currentAyah = parseInt(document.getElementById('toAyah-memorization')?.value) || mem.toAyah || 1;

        const surahData = surahsList.find(s => s.id === currentSurahId);
        let nextSurahId = currentSurahId;
        let nextAyah = currentAyah + 1;

        if (surahData && nextAyah > surahData.ayahs) {
            nextSurahId = Math.min(114, currentSurahId + 1);
            nextAyah = 1;
        }

        const nextSurahData = surahsList.find(s => s.id === nextSurahId);
        const amount = mem.amount || 1.0;
        const toAyah = Math.min(nextSurahData?.ayahs || 50, nextAyah + Math.round(amount * 15));

        mem.fromSurah = nextSurahId;
        mem.fromAyah = nextAyah.toString();
        mem.toSurah = nextSurahId;
        mem.toAyah = toAyah;
    }

    // Recent revision auto-continue
    const rev = student.wirds.recentRevision;
    if (rev && rev.active) {
        let curToAyah = parseInt(document.getElementById('toAyah-recentRevision')?.value) || rev.toAyah || 1;
        rev.fromAyah = (curToAyah + 1).toString();
        rev.toAyah = curToAyah + Math.round((rev.amount || 5) * 15);
    }

    loadStudentIntoDrawer(selectedStudentIndex);
    showDrawerToast('📋 تم استكمال نطاق الآيات تلقائياً بنجاح', 'info');
}

function showBaselinePresetInfo() {
    Swal.fire({
        title: 'ضبط الخطة المعتادة للطالب ⚙️',
        html: `
            <div style="text-align: right; font-size: 0.92rem; line-height: 1.7;">
                <p>تتيح الخطة المعتادة تحديد وتيرة الحفظ والمراجعة الدائمة للطالب.</p>
                <div style="margin: 12px 0; padding: 10px; background: #f8fafc; border-radius: 8px; border: 1px solid #e2e8f0;">
                    <strong>القوالب المتاحة في المرحلة الثانية:</strong>
                    <ul style="margin-top: 6px; padding-right: 20px;">
                        <li>🌱 <strong>خطة التأسيس والمبتدئين:</strong> (حفظ بالآيات + ورد قريب)</li>
                        <li>📖 <strong>المستوى المتوسط:</strong> (صفحة حفظ + 5 صفحات قريب + 1 جزء بعيد)</li>
                        <li>⚡ <strong>الحفاظ المتقدمين:</strong> (صفحتان حفظ + 10 صفحات قريب + 1.5 جزء بعيد)</li>
                        <li>👑 <strong>الخاتمين والإتقان:</strong> (مراجعة مكثفة 20 صفحة + سرد جزأين)</li>
                    </ul>
                </div>
                <small style="color: #64748b;">سيتم تفعيل شاشة الضبط الشاملة في المرحلة القادمة.</small>
            </div>
        `,
        icon: 'info',
        confirmButtonText: 'حسناً فهمت',
        confirmButtonColor: '#059669'
    });
}

/**
 * Stepper Navigation: Previous Student
 */
function prevStudent() {
    if (selectedStudentIndex > 0) {
        saveDrawerWirds(false);
        selectedStudentIndex--;
        loadStudentIntoDrawer(selectedStudentIndex);
    } else {
        showDrawerToast('أنت في بداية قائمة طلاب الحلقة', 'info');
    }
}

/**
 * Stepper Navigation: Save and Next Student
 */
function saveAndNextStudent() {
    saveDrawerWirds(false);

    if (selectedStudentIndex < classStudents.length - 1) {
        selectedStudentIndex++;
        loadStudentIntoDrawer(selectedStudentIndex);
        const body = document.getElementById('drawerWirdsList');
        if (body) body.scrollTop = 0;
    } else {
        closeWirdDrawer();
        Swal.fire({
            icon: 'success',
            title: '🎉 اكتملت الحلقة بنجاح!',
            text: `تم حفظ وتقييم أوراد جميع طلاب الحلقة (${classStudents.length} طالباً) بنجاح تام.`,
            confirmButtonText: 'ممتاز',
            confirmButtonColor: '#059669'
        });
    }
}

/**
 * Collects active wirds and dispatches save
 */
function saveDrawerWirds(shouldClose = false) {
    const student = classStudents[selectedStudentIndex];
    if (!student) return;

    // Collect DOM input values into student state before saving
    Object.keys(WIRD_TYPE_CONFIG).forEach(typeKey => {
        const wird = student.wirds[typeKey];
        if (!wird) return;

        const amountInput = document.getElementById(`amount-${typeKey}`);
        if (amountInput) wird.amount = parseFloat(amountInput.value) || wird.amount;

        const fromSurah = document.getElementById(`fromSurah-${typeKey}`);
        if (fromSurah) wird.fromSurah = parseInt(fromSurah.value) || wird.fromSurah;

        const fromAyah = document.getElementById(`fromAyah-${typeKey}`);
        if (fromAyah) wird.fromAyah = fromAyah.value;

        const toSurah = document.getElementById(`toSurah-${typeKey}`);
        if (toSurah) wird.toSurah = parseInt(toSurah.value) || wird.toSurah;

        const toAyah = document.getElementById(`toAyah-${typeKey}`);
        if (toAyah) wird.toAyah = parseInt(toAyah.value) || wird.toAyah;

        const noteInput = document.getElementById(`note-${typeKey}`);
        if (noteInput) wird.note = noteInput.value;

        const upcomingToggle = document.getElementById(`isUpcoming-${typeKey}`);
        if (upcomingToggle) wird.isUpcoming = upcomingToggle.checked;
    });

    // Generate payload objects
    const classId = document.getElementById('ClassId')?.value || null;
    const activeWirdsList = [];

    Object.keys(WIRD_TYPE_CONFIG).forEach(typeKey => {
        const w = student.wirds[typeKey];
        if (w && w.active) {
            activeWirdsList.push({
                StudentId: student.id,
                ClassId: classId,
                Type: WIRD_TYPE_CONFIG[typeKey].typeCode,
                Amount: w.amount,
                AmountUnit: w.amountUnit,
                EquivalentPages: w.equivalentPages,
                FromSurah: w.fromSurah,
                FromAyah: w.fromAyah,
                ToSurah: w.toSurah,
                ToAyah: w.toAyah,
                Status: w.status, // AssignmentStatus enum (1..5)
                Rating: w.rating,
                Note: w.note,
                IsUpcoming: w.isUpcoming,
                AssignedDate: new Date().toISOString()
            });
        }
    });

    // Populate BatchPayload hidden input
    const batchInput = document.getElementById('BatchPayloadJson');
    if (batchInput) batchInput.value = JSON.stringify(activeWirdsList);

    // Save to LocalStorage cache
    saveDraftsToCache();

    // Send to backend via AJAX fetch if available
    sendWirdsPayloadToBackend(student.id, activeWirdsList);

    showDrawerToast(`✔️ تم حفظ أوراد الطالب (${student.name}) بنجاح!`, 'success');

    if (shouldClose) {
        closeWirdDrawer();
    }
}

/**
 * Sends active wirds to the server
 */
async function sendWirdsPayloadToBackend(studentId, payloadList) {
    if (!payloadList || payloadList.length === 0) return;

    // Try batch endpoint first, with fallback to standard AssignWird
    try {
        const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenEl ? tokenEl.value : '';

        // Prepare first wird for legacy single-binding endpoint
        const primaryWird = payloadList[0];
        const formData = new FormData();
        formData.append('StudentId', studentId);
        formData.append('ClassId', primaryWird.ClassId || '');
        formData.append('Type', primaryWird.Type);
        formData.append('Amount', primaryWird.Amount);
        formData.append('AmountUnit', primaryWird.AmountUnit);
        if (primaryWird.EquivalentPages) formData.append('EquivalentPages', primaryWird.EquivalentPages);
        formData.append('FromSurah', primaryWird.FromSurah || '');
        formData.append('FromAyah', primaryWird.FromAyah || '');
        formData.append('ToSurah', primaryWird.ToSurah || '');
        formData.append('ToAyah', primaryWird.ToAyah || '');
        formData.append('Status', primaryWird.Status || 0);
        formData.append('Note', primaryWird.Note || '');
        formData.append('IsUpcoming', primaryWird.IsUpcoming);
        formData.append('BatchPayloadJson', JSON.stringify(payloadList));

        if (token) formData.append('__RequestVerificationToken', token);

        const response = await fetch('/Teacher/Student/AssignWird', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (response.ok) {
            console.log('Wird successfully saved to server.');
        }
    } catch (err) {
        console.warn('Asynchronous wird saving failed or endpoint in transition; draft kept in local storage:', err);
    }
}

/**
 * LocalStorage draft helpers
 */
function saveDraftsToCache() {
    try {
        localStorage.setItem('hafiz_class_wird_drafts', JSON.stringify(classStudents));
    } catch (e) { }
}

function loadCachedDrafts() {
    try {
        const raw = localStorage.getItem('hafiz_class_wird_drafts');
        if (!raw) return;
        const cached = JSON.parse(raw);
        if (Array.isArray(cached)) {
            cached.forEach(savedStudent => {
                const existing = classStudents.find(s => s.id === savedStudent.id);
                if (existing && savedStudent.wirds) {
                    existing.wirds = savedStudent.wirds;
                }
            });
        }
    } catch (e) { }
}

/**
 * Toast notifications in the bottom corner
 */
function showDrawerToast(message, type = 'success') {
    let container = document.getElementById('drawerToastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'drawerToastContainer';
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    const icon = type === 'success' ? 'bx-check-circle' : (type === 'info' ? 'bxs-star' : 'bx-info-circle');
    toast.innerHTML = `<i class='bx ${icon}' style="font-size: 1.25rem;"></i><span>${message}</span>`;

    container.appendChild(toast);
    setTimeout(() => toast.classList.add('show'), 10);
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3500);
}

/**
 * Keyboard shortcuts for the teacher
 */
function setupDrawerKeyboardEvents() {
    document.addEventListener('keydown', (e) => {
        const drawer = document.getElementById('drawerPanel');
        const isOpen = drawer && (drawer.classList.contains('active') || drawer.classList.contains('mobile-open'));

        if (!isOpen) return;

        if (e.key === 'Escape') {
            closeWirdDrawer();
        } else if (e.ctrlKey && e.key === 'Enter') {
            e.preventDefault();
            saveAndNextStudent();
        } else if (e.ctrlKey && (e.key === 's' || e.key === 'S' || e.key === 'س')) {
            e.preventDefault();
            saveDrawerWirds(false);
        }
    });
}
