(() => {
  const configArea = document.getElementById('configJson');
  const previewHost = document.getElementById('certificatePreview');
  const previewStage = document.getElementById('previewStage');
  const previewScaler = document.getElementById('previewScaler');
  if (!configArea || !previewHost) return;

  const MM_TO_PX = 96 / 25.4;

  const DEFAULT_THEME = {
    preset: 'classic_gold',
    primaryColor: '#C59B27',
    secondaryColor: '#064E3B',
    accentColor: '#DFBA69',
    textColor: '#1C1917',
    backgroundColor: '#FDFBF7',
    borderColor: '#C59B27',
    borderStyle: 'double',
    borderWidth: 3,
    cornerStyle: 'auto',
    cornerSize: 56,
    frameStyle: 'lines',
    backgroundPattern: 'none',
    backgroundPatternColor: 'primary',
    backgroundPatternOpacity: 0.06,
    cornerDecorations: true,
    watermarkEnabled: true,
    watermarkIcon: 'bx-book-open',
    watermarkOpacity: 0.04,
  };

  // Whitelists: these values reach class names and mask lookups, so nothing else may pass through.
  const CORNER_STYLES = ['none', 'simple', 'arabesque', 'girih', 'floral', 'medallion'];
  const FRAME_STYLES = ['lines', 'ornate'];
  const BACKGROUND_PATTERNS = ['none', 'paper', 'grid', 'arabesque', 'geometric', 'damask', 'stars'];

  const LEGACY_CORNER_SIZE = 38;

  // Normalises one stored value for a whitelist lookup. Distinct from normalize() further down,
  // which normalises the whole config object — the two names must stay apart.
  // Stored JSON is untrusted and may not be cased the way the pickers write it, so every value is
  // trimmed and lower-cased before matching. The renderer normalises identically — that is what
  // keeps the preview and the printed sheet in agreement.
  const normalizeKey = (value, fallback) => {
    const text = String(value ?? '').trim().toLowerCase();
    return text === '' ? fallback : text;
  };

  // Mirrors the resolution in _CertificateDynamic.cshtml so preview and print agree.
  function resolveCornerStyle(theme) {
    const requested = normalizeKey(theme.cornerStyle, 'auto');
    // "auto" is every template saved before CornerStyle existed: fall back to the legacy flag.
    if (requested === 'auto') return theme.cornerDecorations === false ? 'none' : 'simple';
    // An unrecognised value falls back to the legacy flag, not to a fixed style, so it cannot
    // draw corners the renderer would omit.
    return CORNER_STYLES.includes(requested)
      ? requested
      : theme.cornerDecorations === false
        ? 'none'
        : 'simple';
  }

  function resolveCornerSize(theme) {
    // Legacy templates keep the original 38px L; an explicit style opts into the size control.
    if (normalizeKey(theme.cornerStyle, 'auto') === 'auto') return LEGACY_CORNER_SIZE;
    const size = Number(theme.cornerSize);
    return Number.isFinite(size) ? Math.min(140, Math.max(24, size)) : 56;
  }

  const resolveFrameStyle = (theme) => {
    const requested = normalizeKey(theme.frameStyle, 'lines');
    return FRAME_STYLES.includes(requested) ? requested : 'lines';
  };

  function resolveBackground(theme) {
    const requested = normalizeKey(theme.backgroundPattern, 'none');
    const pattern = BACKGROUND_PATTERNS.includes(requested) ? requested : 'none';
    const rawOpacity = Number(theme.backgroundPatternOpacity);
    const opacity = Math.min(0.15, Math.max(0, isNaN(rawOpacity) ? 0.05 : rawOpacity));
    const color =
      { secondary: theme.secondaryColor, accent: theme.accentColor }[
        normalizeKey(theme.backgroundPatternColor, 'primary')
      ] || theme.primaryColor;
    return { pattern, opacity, color, visible: pattern !== 'none' && opacity > 0 };
  }

  const DEFAULT_TYPOGRAPHY = {
    titleFont: 'Reem Kufi',
    bodyFont: 'Cairo',
    arabicFont: 'Amiri',
    titleSize: '2.1rem',
    bodySize: '1.28rem',
    studentNameSize: '2.6rem',
    lineHeight: 1.8,
  };

  const DEFAULT_LAYOUT = {
    pageSize: 'A4',
    orientation: 'landscape',
    margins: { top: 10, right: 12, bottom: 10, left: 12 },
  };

  const DEFAULT_SIGNATURE_COLUMNS = [
    { title: 'معلم الحلقة', nameField: '{TeacherName}', subtitle: 'التوقيع والاعتماد', type: 'signature' },
    { title: 'الختم والتاريخ', nameField: '', subtitle: '', type: 'seal', showSeal: true, showDate: true },
    { title: 'إدارة المركز', nameField: '{DirectorName}', subtitle: 'الختم الرسمي', type: 'signature' },
  ];

  // Sample datasets used by the live preview for different certificate types and student genders.
  const SAMPLE_DATASETS = {
    quran_male: {
      StudentName: 'أحمد محمد العبدالله',
      GenderedStudent: 'الطالب',
      GenderedCompleted: 'أتمّ',
      InstituteName: 'مركز تحفيظ القرآن الكريم',
      ClassName: 'حلقة الإمام عاصم',
      TeacherName: 'الشيخ عبد الرحمن السعدي',
      DirectorName: 'إدارة الشؤون التعليمية',
      CertificateNumber: 'QRN-ABC-00042',
      CertificateTitle: 'شهادة إتمام وتفوق قرآني',
      SubjectName: 'حفظ (5) أجزاء من كتاب الله تعالى',
      AuthorName: 'برواية حفص عن عاصم من طريق الشاطبية',
      FromJuz: '1',
      ToJuz: '5',
      JuzCount: '5',
      CompletionPercentage: '17',
      Riwayah: 'برواية حفص عن عاصم من طريق الشاطبية',
      ScopeDetails: 'من الجزء 1 إلى الجزء 5',
      Score: '98',
      Rating: 'ممتاز',
      ExamResult: 'بتقدير: ممتاز ومبارك',
      HijriDate: '1447/03/12 هـ',
      GregorianDate: '2026/09/20 م',
      IssueDate: '1447/03/12 هـ الموافق 2026/09/20 م',
    },
    quran_female: {
      StudentName: 'فاطمة بنت أحمد الزهراء',
      GenderedStudent: 'الطالبة',
      GenderedCompleted: 'أتمّت',
      InstituteName: 'دار القرآن والحديث النسائية',
      ClassName: 'حلقة أم المؤمنين خديجة',
      TeacherName: 'المعلمة مريم الصالح',
      DirectorName: 'الإدارة النسائية',
      CertificateNumber: 'QRN-FEM-00108',
      CertificateTitle: 'شهادة إتقان وتفوق قرآني',
      SubjectName: 'حفظ (10) أجزاء من كتاب الله تعالى',
      AuthorName: 'برواية حفص عن عاصم من طريق الشاطبية',
      FromJuz: '1',
      ToJuz: '10',
      JuzCount: '10',
      CompletionPercentage: '33',
      Riwayah: 'برواية حفص عن عاصم من طريق الشاطبية',
      ScopeDetails: 'من الجزء 1 إلى الجزء 10',
      Score: '100',
      Rating: 'ممتاز مرتفع',
      ExamResult: 'بتقدير: ممتاز مرتفع ومبارك',
      HijriDate: '1447/03/15 هـ',
      GregorianDate: '2026/09/23 م',
      IssueDate: '1447/03/15 هـ الموافق 2026/09/23 م',
    },
    matn_male: {
      StudentName: 'عمر بن خالد الفاسي',
      GenderedStudent: 'الطالب',
      GenderedCompleted: 'أتمّ',
      InstituteName: 'معهد التأصيل والمتون العلمية',
      ClassName: 'حلقة الإتقان في التجويد',
      TeacherName: 'الشيخ إبراهيم المقرئ',
      DirectorName: 'إدارة المعهد العلمية',
      CertificateNumber: 'MTN-TKH-00015',
      CertificateTitle: 'شهادة إتقان وضبط متن علمي',
      SubjectName: 'متن تحفة الأطفال والغلمان في تجويد القرآن',
      AuthorName: 'للإمام سليمان الجمزوري رحمه الله',
      FromJuz: '1',
      ToJuz: '1',
      JuzCount: '61 بيتاً',
      CompletionPercentage: '100',
      Riwayah: 'حفظاً وضبطاً وإتقاناً مع شرح الأصول',
      ScopeDetails: 'كامل منظومة تحفة الأطفال (61 بيتاً)',
      Score: '99',
      Rating: 'ممتاز',
      ExamResult: 'بتقدير: إتقان تام وضبط مبارك',
      HijriDate: '1447/03/18 هـ',
      GregorianDate: '2026/09/26 م',
      IssueDate: '1447/03/18 هـ الموافق 2026/09/26 م',
    },
    sanad_male: {
      StudentName: 'عبد الله بن يحيى الشنقيطي',
      GenderedStudent: 'الطالب المجاز',
      GenderedCompleted: 'أتمّ',
      InstituteName: 'مقرأة الإسناد والقراءات العشر',
      ClassName: 'مجلس الإسناد العالي',
      TeacherName: 'المسند الشيخ عبد العزيز المكي',
      DirectorName: 'عمادة المقارئ القرآنية',
      CertificateNumber: 'SND-QUR-00003',
      CertificateTitle: 'إجازة بالسند المتصل في القرآن الكريم',
      SubjectName: 'ختم القرآن الكريم كاملاً غيباً عن ظهر قلب',
      AuthorName: 'برواية ورش عن نافع المدني من طريق الأزرق',
      FromJuz: '1',
      ToJuz: '30',
      JuzCount: '30 جزءاً',
      CompletionPercentage: '100',
      Riwayah: 'برواية ورش عن نافع من طريق الأزرق بسند متصل',
      ScopeDetails: 'من سورة الفاتحة إلى سورة الناس قراءة وإقراءً',
      Score: '100',
      Rating: 'إجازة مسندة',
      ExamResult: 'أجيز بالسند المتصل إلى رسول الله ﷺ',
      HijriDate: '1447/03/20 هـ',
      GregorianDate: '2026/09/28 م',
      IssueDate: '1447/03/20 هـ الموافق 2026/09/28 م',
    },
    general_appreciation: {
      StudentName: 'محمد بن سالم الغامدي',
      GenderedStudent: 'المكرم',
      GenderedCompleted: 'أتمّ',
      InstituteName: 'مركز التميز والأنشطة القرآنية',
      ClassName: 'المسابقة القرآنية السنوية',
      TeacherName: 'لجنة التحكيم والمسابقات',
      DirectorName: 'مدير عام المركز',
      CertificateNumber: 'GEN-APR-00088',
      CertificateTitle: 'شهادة شكر وتقدير وتميز',
      SubjectName: 'المشاركة الفاعلة والتفوق في فعاليات الملتقى القرآني',
      AuthorName: 'المركز العام للأنشطة الطلابية',
      FromJuz: '-',
      ToJuz: '-',
      JuzCount: '-',
      CompletionPercentage: '100',
      Riwayah: '-',
      ScopeDetails: 'المشاركة المتميزة في برامج خدمة القرآن الكريم',
      Score: '97',
      Rating: 'تميز شرفي',
      ExamResult: 'مع وافر الشكر والتقدير والدعاء بمزيد من التوفيق',
      HijriDate: '1447/03/22 هـ',
      GregorianDate: '2026/09/30 م',
      IssueDate: '1447/03/22 هـ الموافق 2026/09/30 م',
    }
  };

  let currentSampleKey = 'quran_male';
  const getSample = () => SAMPLE_DATASETS[currentSampleKey] || SAMPLE_DATASETS.quran_male;

  let config;
  let parseFailed = false;
  config = readConfig();

  /* ---------------------------------------------------------------- config */

  function readConfig() {
    let parsed;
    try {
      parsed = JSON.parse(configArea.value || '{}');
    } catch {
      // Never rewrite a stored configuration we could not read.
      parseFailed = true;
      parsed = {};
    }
    return normalize(parsed);
  }

  function normalize(raw) {
    const next = raw && typeof raw === 'object' ? raw : {};
    next.theme = { ...DEFAULT_THEME, ...(next.theme || {}) };
    next.typography = { ...DEFAULT_TYPOGRAPHY, ...(next.typography || {}) };
    next.layout = {
      ...DEFAULT_LAYOUT,
      ...(next.layout || {}),
      margins: { ...DEFAULT_LAYOUT.margins, ...((next.layout || {}).margins || {}) },
    };
    next.tokens = { closingText: '', customFields: {}, ...(next.tokens || {}) };
    // A stored section that is not an object would throw while being defaulted, which would take
    // the whole preview down with it, so anything that is not a section is dropped here.
    next.sections = Array.isArray(next.sections)
      ? next.sections.filter((section) => section && typeof section === 'object')
      : [];
    next.sections.forEach((section, index) => {
      section.config = section.config || {};
      if (typeof section.order !== 'number') section.order = index + 1;
      if (typeof section.enabled !== 'boolean') section.enabled = true;
    });
    return next;
  }

  const at = (obj, path) => path.split('.').reduce((node, key) => (node == null ? node : node[key]), obj);

  function setAt(obj, path, value) {
    const keys = path.split('.');
    const last = keys.pop();
    const target = keys.reduce((node, key) => (node[key] = node[key] || {}), obj);
    target[last] = value;
  }

  function sectionOf(type, create = true) {
    const found = config.sections.find((s) => s.type === type);
    if (found || !create) return found;
    const created = { type, enabled: true, order: nextOrder(), config: {} };
    config.sections.push(created);
    // Orders follow the cards the admin sees, so a section enabled later lands where its card sits.
    const cards = [...document.querySelectorAll('#sectionList [data-section-card]')];
    if (cards.length) {
      config.sections.forEach((section) => {
        const index = cards.findIndex((card) => card.dataset.sectionCard === section.type);
        if (index >= 0) section.order = index + 1;
      });
    }
    return created;
  }

  const nextOrder = () => config.sections.reduce((max, s) => Math.max(max, s.order || 0), 0) + 1;

  // The signature editor shows the built-in columns until the admin edits one, so browsing never adds a section.
  const signatureColumns = () => {
    const columns = sectionOf('signatures', false)?.config?.columns;
    return Array.isArray(columns) ? columns : DEFAULT_SIGNATURE_COLUMNS;
  };

  function ensureSignatureSection() {
    if (sectionOf('signatures', false)) return;
    sectionOf('signatures').config.columns = DEFAULT_SIGNATURE_COLUMNS.map((column) => ({ ...column }));
  }

  function write() {
    configArea.value = JSON.stringify(config, null, 2);
    configArea.classList.remove('is-invalid');
    renderPreview();
  }

  /* -------------------------------------------------------------- controls */

  function controlValue(input, path, cast) {
    const raw = at(config, path);
    if (input.type === 'checkbox') return typeof raw === 'boolean' ? raw : raw !== 'false';
    if (raw == null || raw === '') return input.dataset.default ?? '';
    return cast === 'rem' ? parseFloat(raw) : raw;
  }

  function applyControl(input, path, cast) {
    if (input.type === 'checkbox') {
      setAt(config, path, input.checked);
    } else if (input.value === '' && cast) {
      // Keep the previous value instead of writing an unusable empty size/number.
      return;
    } else if (cast === 'rem') {
      setAt(config, path, `${input.value}rem`);
    } else if (cast === 'number') {
      setAt(config, path, Number(input.value));
    } else {
      setAt(config, path, input.value);
    }
    write();
  }

  document.querySelectorAll('[data-config]').forEach((input) => {
    const path = input.dataset.config;
    const cast = input.dataset.cast;
    input.addEventListener('change', () => applyControl(input, path, cast));
    if (input.type !== 'checkbox' && input.tagName !== 'SELECT') {
      input.addEventListener('input', () => applyControl(input, path, cast));
    }
  });

  document.querySelectorAll('[data-section]').forEach((input) => {
    input.addEventListener('change', () => {
      sectionOf(input.dataset.section).enabled = input.checked;
      write();
    });
  });

  document.querySelectorAll('[data-section-config]').forEach((input) => {
    const [type, key] = input.dataset.sectionConfig.split(':');
    input.addEventListener('change', () => {
      const section = sectionOf(type);
      if (input.type === 'checkbox') section.config[key] = input.checked;
      else if (input.dataset.cast === 'number') section.config[key] = Number(input.value);
      else section.config[key] = input.value;
      write();
    });
    if (input.tagName === 'TEXTAREA' || input.type === 'text') {
      input.addEventListener('input', () => {
        const section = sectionOf(type);
        section.config[key] = input.value;
        write();
      });
    }
  });

  // The preset payload is reviewable markup, so it is parsed defensively: one broken attribute must
  // never throw its way out of init and leave the preview blank.
  function presetOf(btn) {
    try {
      const preset = JSON.parse(btn.dataset.preset);
      return preset && typeof preset === 'object' ? preset : null;
    } catch {
      return null;
    }
  }

  document.querySelectorAll('.preset-btn').forEach((btn) => {
    btn.addEventListener('click', () => {
      const preset = presetOf(btn);
      // A malformed data-preset must not stop the builder from applying the rest of the page.
      if (!preset) return;
      Object.assign(config.theme, preset);
      syncControls();
      write();
    });
  });

  // The three ornament pickers are button groups, so they write their key by hand.
  document.querySelectorAll('[data-corner-style]').forEach((btn) => {
    btn.addEventListener('click', () => {
      config.theme.cornerStyle = btn.dataset.cornerStyle;
      // Keep the legacy flag coherent so anything still reading it agrees with the picker.
      config.theme.cornerDecorations = btn.dataset.cornerStyle !== 'none';
      syncControls();
      write();
    });
  });

  document.querySelectorAll('[data-frame-style]').forEach((btn) => {
    btn.addEventListener('click', () => {
      config.theme.frameStyle = btn.dataset.frameStyle;
      syncControls();
      write();
    });
  });

  document.querySelectorAll('[data-bg-pattern]').forEach((btn) => {
    btn.addEventListener('click', () => {
      config.theme.backgroundPattern = btn.dataset.bgPattern;
      syncControls();
      write();
    });
  });

  // A template still on "auto" renders the fixed legacy size, so nudging the size control
  // would do nothing. Pin it to whatever "auto" currently resolves to, then the size applies.
  document.querySelectorAll('[data-config="theme.cornerSize"]').forEach((input) => {
    input.addEventListener('change', () => {
      if (normalizeKey(config.theme.cornerStyle, 'auto') !== 'auto') return;
      config.theme.cornerStyle = resolveCornerStyle(config.theme);
      config.theme.cornerDecorations = config.theme.cornerStyle !== 'none';
      syncControls();
      write();
    });
  });

  // Reordering swaps the order values of neighbouring sections.
  document.querySelectorAll('[data-move]').forEach((btn) => {
    btn.addEventListener('click', () => {
      const type = btn.dataset.moveTarget;
      const ordered = [...config.sections].sort((a, b) => a.order - b.order);
      const index = ordered.findIndex((s) => s.type === type);
      const target = btn.dataset.move === 'up' ? index - 1 : index + 1;
      if (index < 0 || target < 0 || target >= ordered.length) return;
      const a = ordered[index];
      const b = ordered[target];
      [a.order, b.order] = [b.order, a.order];
      ordered.sort((x, y) => x.order - y.order);
      ordered.forEach((section, i) => (section.order = i + 1));
      reorderSectionCards(ordered);
      write();
    });
  });

  function reorderSectionCards(ordered) {
    const list = document.getElementById('sectionList');
    if (!list) return;
    const cards = [...list.querySelectorAll('[data-section-card]')];
    const orderedTypes = ordered.map((section) => section.type);
    // Sections that are not part of the configuration yet stay at the end of the list.
    const untouched = cards.filter((card) => !orderedTypes.includes(card.dataset.sectionCard));
    [...orderedTypes.map((type) => cards.find((card) => card.dataset.sectionCard === type)), ...untouched]
      .filter(Boolean)
      .forEach((card) => list.appendChild(card));
  }

  // Token palette inserts into the field the author last touched.
  let lastField = null;
  document.addEventListener('focusin', (event) => {
    const field = event.target;
    if (field.matches('[data-config], [data-section-config], textarea, input[type="text"]')) lastField = field;
  });

  document.querySelectorAll('[data-token]').forEach((btn) => {
    btn.addEventListener('click', () => {
      if (!lastField || lastField.type === 'checkbox') return;
      const token = btn.dataset.token;
      const start = lastField.selectionStart ?? lastField.value.length;
      const end = lastField.selectionEnd ?? start;
      lastField.value = lastField.value.slice(0, start) + token + lastField.value.slice(end);
      lastField.selectionStart = lastField.selectionEnd = start + token.length;
      lastField.dispatchEvent(new Event('input', { bubbles: true }));
      lastField.focus();
    });
  });

  /* ---------------------------------------------------- signature columns */

  const columnsHost = document.getElementById('signatureColumns');

  function renderSignatureEditor() {
    if (!columnsHost) return;
    const columns = signatureColumns();
    columnsHost.innerHTML = columns
      .map(
        (column, index) => `
        <div class="sig-row" data-sig-index="${index}">
          <input data-sig-field="title" value="${escapeAttr(column.title || '')}" placeholder="العنوان (مثال: معلم الحلقة)" />
          <input data-sig-field="nameField" value="${escapeAttr(column.nameField || '')}" placeholder="الحقل ({TeacherName})" />
          <input data-sig-field="subtitle" value="${escapeAttr(column.subtitle || '')}" placeholder="السطر أسفل الخط" />
          <select data-sig-field="type">
            <option value="signature"${column.type === 'seal' ? '' : ' selected'}>توقيع</option>
            <option value="seal"${column.type === 'seal' ? ' selected' : ''}>ختم وتاريخ</option>
          </select>
          <button type="button" class="icon-btn danger" data-sig-remove title="حذف العمود"><i class='bx bx-trash'></i></button>
        </div>`,
      )
      .join('');

    columnsHost.querySelectorAll('.sig-row').forEach((row) => {
      const index = Number(row.dataset.sigIndex);
      row.querySelectorAll('[data-sig-field]').forEach((field) => {
        field.addEventListener('input', () => {
          ensureSignatureSection();
          const column = signatureColumns()[index];
          if (!column) return;
          column[field.dataset.sigField] = field.value;
          if (field.dataset.sigField === 'type') {
            column.showSeal = field.value === 'seal';
            column.showDate = field.value === 'seal';
          }
          write();
        });
      });
      row.querySelector('[data-sig-remove]').addEventListener('click', () => {
        ensureSignatureSection();
        const section = sectionOf('signatures');
        section.config.columns = signatureColumns().filter((_, i) => i !== index);
        renderSignatureEditor();
        write();
      });
    });
  }

  document.getElementById('addSignatureColumn')?.addEventListener('click', () => {
    ensureSignatureSection();
    const section = sectionOf('signatures');
    section.config.columns = [
      ...signatureColumns(),
      { title: 'عمود جديد', nameField: '', subtitle: '', type: 'signature' },
    ];
    renderSignatureEditor();
    write();
  });

  /* ------------------------------------------------------------- sync back */

  function syncControls() {
    document.querySelectorAll('[data-config]').forEach((input) => {
      const value = controlValue(input, input.dataset.config, input.dataset.cast);
      if (input.type === 'checkbox') input.checked = Boolean(value);
      else input.value = value;
    });

    document.querySelectorAll('[data-section]').forEach((input) => {
      const section = sectionOf(input.dataset.section, false);
      input.checked = section ? section.enabled !== false : false;
    });

    document.querySelectorAll('[data-section-config]').forEach((input) => {
      const [type, key] = input.dataset.sectionConfig.split(':');
      const section = sectionOf(type, false);
      const raw = section?.config?.[key];
      if (input.type === 'checkbox') {
        input.checked = typeof raw === 'boolean' ? raw : input.dataset.default !== 'false';
      } else if (raw == null || raw === '') {
        input.value = input.dataset.default ?? '';
      } else {
        input.value = input.dataset.cast === 'number' || input.dataset.cast === 'rem' ? parseFloat(raw) : raw;
      }
    });

    document.querySelectorAll('.preset-btn').forEach((btn) => {
      const preset = presetOf(btn);
      btn.classList.toggle('active', Boolean(preset) && preset.preset === config.theme.preset);
    });

    const activeCorner = resolveCornerStyle(config.theme);
    document.querySelectorAll('[data-corner-style]').forEach((btn) => {
      btn.classList.toggle('active', btn.dataset.cornerStyle === activeCorner);
    });

    const activeFrame = resolveFrameStyle(config.theme);
    document.querySelectorAll('[data-frame-style]').forEach((btn) => {
      btn.classList.toggle('active', btn.dataset.frameStyle === activeFrame);
    });

    const activePattern = resolveBackground(config.theme).pattern;
    document.querySelectorAll('[data-bg-pattern]').forEach((btn) => {
      btn.classList.toggle('active', btn.dataset.bgPattern === activePattern);
    });
  }

  configArea.addEventListener('change', () => {
    try {
      config = normalize(JSON.parse(configArea.value));
      configArea.classList.remove('is-invalid');
      syncControls();
      renderSignatureEditor();
      renderPreview();
    } catch {
      configArea.classList.add('is-invalid');
    }
  });

  // A half-edited JSON blob must never be posted.
  document.getElementById('certificateBuilderForm')?.addEventListener('submit', (event) => {
    try {
      JSON.parse(configArea.value);
    } catch {
      event.preventDefault();
      configArea.classList.add('is-invalid');
      configArea.closest('details')?.setAttribute('open', 'open');
      alert('إعدادات JSON غير صالحة، صحّحها قبل الحفظ.');
    }
  });

  /* -------------------------------------------------------------- preview */

  function escapeAttr(value) {
    return String(value).replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;');
  }

  function escapeHtml(value) {
    return String(value).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  }

  function resolve(text) {
    if (!text) return '';
    const sample = getSample();
    return Object.keys(sample).reduce(
      (result, key) => result.replace(new RegExp(`\\{${key}\\}`, 'gi'), sample[key]),
      String(text),
    );
  }

  const sectionConfig = (type, key, fallback) => {
    const section = sectionOf(type, false);
    const value = section?.config?.[key];
    return value === undefined || value === null || value === '' ? fallback : value;
  };

  function parseColor(hex) {
    const value = String(hex || '').trim().replace('#', '');
    const full = value.length === 3 ? value.split('').map((c) => c + c).join('') : value;
    if (!/^[0-9a-f]{6}$/i.test(full)) return [197, 155, 39];
    return [0, 2, 4].map((i) => parseInt(full.slice(i, i + 2), 16));
  }

  const toHex = (rgb) =>
    '#' + rgb.map((c) => Math.max(0, Math.min(255, Math.round(c))).toString(16).padStart(2, '0')).join('');

  const darken = (hex, factor) => toHex(parseColor(hex).map((c) => c * factor));

  const mix = (hex, other, ratio) =>
    toHex(
      parseColor(hex).map((c, i) => c * (1 - ratio) + parseColor(other)[i] * ratio),
    );

  // Stand-in for the real QR image so the preview shows the final layout.
  const QR_PLACEHOLDER =
    'data:image/svg+xml;utf8,' +
    encodeURIComponent(
      `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 21 21" fill="#111"><rect width="21" height="21" fill="#fff"/>` +
        `<path d="M0 0h7v7H0zm1 1v5h5V1zM14 0h7v7h-7zm1 1v5h5V1zM0 14h7v7H0zm1 1v5h5v-5zM8 0h1v1H8zM10 0h1v2h-1zM8 2h2v1H8zM9 3h1v1H9zM8 4h1v2H8zM10 5h1v1h-1zM9 7h1v1H9zM0 8h1v1H0zM2 8h2v1H2zM5 8h1v2H5zM1 10h1v1H1zM3 10h1v1H3zM0 12h2v1H0zM4 12h1v1H4zM6 8h1v1H6zM8 8h3v1H8zM12 8h1v2h-1zM14 8h2v1h-2zM17 8h1v1h-1zM19 8h2v1h-2zM9 10h2v1H9zM13 10h2v1h-2zM16 10h1v1h-1zM18 10h3v1h-3zM8 12h1v1H8zM10 12h3v1h-3zM15 12h1v1h-1zM17 12h2v1h-2zM8 14h1v1H8zM10 14h2v1h-2zM13 14h1v2h-1zM15 14h2v1h-2zM18 14h1v1h-1zM9 16h2v1H9zM12 16h1v1h-1zM14 16h1v1h-1zM16 16h1v2h-1zM19 16h2v1h-2zM8 18h1v1H8zM10 18h2v1h-2zM13 18h2v1h-2zM17 18h1v1h-1zM20 18h1v1h-1zM9 20h1v1H9zM11 20h2v1h-2zM15 20h2v1h-2zM18 20h1v1h-1z"/></svg>`,
    );

  function renderPreview() {
    const theme = config.theme;
    const typography = config.typography;
    const layout = config.layout;
    const margins = layout.margins;

    const portrait = layout.orientation === 'portrait';
    const letter = layout.pageSize === 'Letter';
    const width = portrait ? (letter ? 216 : 210) : letter ? 279 : 297;
    const height = portrait ? (letter ? 279 : 297) : letter ? 216 : 210;

    const sections = config.sections
      .filter((section) => section.enabled !== false)
      .sort((a, b) => (a.order || 0) - (b.order || 0));

    const cornerStyle = resolveCornerStyle(theme);
    const cornerClasses =
      cornerStyle === 'none' || cornerStyle === 'simple'
        ? ''
        : `cert-corners--masked cert-corners--${cornerStyle}`;
    const ornate = resolveFrameStyle(theme) === 'ornate';
    const background = resolveBackground(theme);

    const sheet = document.createElement('div');
    sheet.className = 'certificate-sheet';
    sheet.style.setProperty('--cert-paper-w', `${width}mm`);
    sheet.style.setProperty('--cert-paper-h', `${height}mm`);
    sheet.style.setProperty('--cert-bg-parchment', theme.backgroundColor);
    sheet.style.setProperty('--cert-gold-1', theme.accentColor);
    sheet.style.setProperty('--cert-gold-2', theme.primaryColor);
    sheet.style.setProperty('--cert-gold-3', darken(theme.accentColor, 0.72));
    sheet.style.setProperty('--cert-gold-dark', darken(theme.primaryColor, 0.62));
    sheet.style.setProperty('--cert-emerald-dark', theme.secondaryColor);
    sheet.style.setProperty('--cert-emerald-primary', theme.secondaryColor);
    sheet.style.setProperty('--cert-blue-dark', theme.secondaryColor);
    sheet.style.setProperty('--cert-text-dark', theme.textColor);
    sheet.style.setProperty('--cert-text-muted', mix(theme.textColor, theme.backgroundColor, 0.42));
    sheet.style.setProperty('--tpl-title-font', `'${typography.titleFont}'`);
    sheet.style.setProperty('--tpl-body-font', `'${typography.bodyFont}'`);
    sheet.style.setProperty('--tpl-arabic-font', `'${typography.arabicFont}'`);
    sheet.style.setProperty('--tpl-title-size', typography.titleSize);
    sheet.style.setProperty('--tpl-body-size', typography.bodySize);
    sheet.style.setProperty('--tpl-student-size', typography.studentNameSize);
    sheet.style.setProperty('--tpl-line-height', typography.lineHeight);
    sheet.style.setProperty('--cert-corner-size', `${resolveCornerSize(theme)}px`);
    sheet.style.setProperty('--cert-pattern-color', background.color);
    sheet.style.setProperty('--cert-pattern-opacity', background.opacity);
    sheet.style.padding = `${margins.top}mm ${margins.right}mm ${margins.bottom}mm ${margins.left}mm`;

    const corners = `
      <div class="cert-corner cert-corner-tl"></div>
      <div class="cert-corner cert-corner-tr"></div>
      <div class="cert-corner cert-corner-bl"></div>
      <div class="cert-corner cert-corner-br"></div>`;

    const watermark = `<div class="cert-watermark" style="opacity:${theme.watermarkOpacity}"><i class="bx ${escapeAttr(theme.watermarkIcon)}"></i></div>`;

    const band = `
      <div class="cert-frame-band">
        <div class="cert-band-edge cert-band-edge--top"></div>
        <div class="cert-band-edge cert-band-edge--bottom"></div>
        <div class="cert-band-edge cert-band-edge--left"></div>
        <div class="cert-band-edge cert-band-edge--right"></div>
        <div class="cert-band-corner cert-band-corner--tl"></div>
        <div class="cert-band-corner cert-band-corner--tr"></div>
        <div class="cert-band-corner cert-band-corner--bl"></div>
        <div class="cert-band-corner cert-band-corner--br"></div>
      </div>`;

    const patternLayer = background.visible
      ? `<div class="cert-bg-pattern cert-pattern--${background.pattern}"></div>`
      : '';

    const innerClasses = ['cert-inner-border', cornerClasses].filter(Boolean).join(' ');

    const defaultSectionTypes = ['header', 'title', 'verse', 'statement', 'evaluation', 'signatures'];
    const sectionMarkup = sections.length
      ? sections.map((section) => renderSection(section.type)).join('')
      : defaultSectionTypes.map((type) => renderSection(type)).join('');

    sheet.innerHTML = `
      <div class="cert-outer-border${ornate ? ' cert-outer-border--ornate' : ''}" style="border-style:${escapeAttr(theme.borderStyle)};border-width:${Number(theme.borderWidth) || 0}px;border-color:${escapeAttr(theme.borderColor)}">
        ${ornate ? band : ''}
        <div class="${innerClasses}">
          ${patternLayer}
          ${cornerStyle === 'none' ? '' : corners}
          ${theme.watermarkEnabled ? watermark : ''}
          ${sectionMarkup}
        </div>
      </div>`;

    previewHost.replaceChildren(sheet);
    lastPaper = { width, height };
    fitPreview(width, height);
  }

  function renderSection(type) {
    switch (type) {
      case 'header':
        return renderHeader();
      case 'title':
        return renderTitle();
      case 'verse':
        return `<div class="cert-verse">${escapeHtml(resolve(sectionConfig('verse', 'text', '﴿ إِنَّ هَٰذَا الْقُرْآنَ يَهْدِي لِلَّتِي هِيَ أَقْوَمُ ﴾')))}</div>`;
      case 'statement':
        return renderStatement();
      case 'quranScope':
        return renderScope();
      case 'evaluation':
        return renderEvaluation();
      case 'signatures':
        return renderSignatures();
      case 'qrVerification':
        return renderQr();
      default:
        return '';
    }
  }

  function renderHeader() {
    const sample = getSample();
    const showLogo = sectionConfig('header', 'showInstituteLogo', true);
    const showName = sectionConfig('header', 'showInstituteName', true);
    const showSub = sectionConfig('header', 'showSubInstitute', true);
    const showBasmalah = sectionConfig('header', 'showBasmalah', true);
    const showNumber = sectionConfig('header', 'showCertificateNumber', true);
    const subText = sectionConfig('header', 'subInstituteText', `حلقة: ${sample.ClassName}`);

    const side =
      showLogo || showName || showSub
        ? `<div class="cert-header-side">
             ${showLogo ? '<div class="cert-logo cert-logo-placeholder">شعار</div>' : ''}
             <div>
               ${showName ? `<h2 class="cert-institute-title">${escapeHtml(sample.InstituteName)}</h2>` : ''}
               ${showSub ? `<div class="cert-sub-institute">${escapeHtml(resolve(subText))}</div>` : ''}
             </div>
           </div>`
        : '';

    return `<div class="cert-header">
      ${side}
      ${showBasmalah ? '<div class="cert-basmalah">بِسْمِ اللَّـهِ الرَّحْمَـٰنِ الرَّحِيمِ</div>' : ''}
      ${showNumber ? `<div class="cert-number-box"><span class="cert-serial">${sample.CertificateNumber}</span></div>` : ''}
    </div>`;
  }

  function renderTitle() {
    const text = resolve(sectionConfig('title', 'text', '{CertificateTitle}'));
    const decorators = sectionConfig('title', 'showDecorators', true);
    const underline = sectionConfig('title', 'showUnderline', true);
    return `<div class="cert-title-container${decorators ? '' : ' no-decorators'}">
      <h1 class="cert-main-title">${escapeHtml(text)}</h1>
      ${underline ? '<div class="cert-title-line"></div>' : ''}
    </div>`;
  }

  function renderStatement() {
    const sample = getSample();
    const intro = resolve(sectionConfig('statement', 'text', 'تشهد إدارة المركز بأن {GenderedStudent} قد أتمّ بحمد الله وتوفيقه.'));
    const showStudent = sectionConfig('statement', 'showStudentName', true);
    const showSubject = sectionConfig('statement', 'showSubjectName', true);
    const showAuthor = sectionConfig('statement', 'showAuthor', true);
    const showScope = sectionConfig('statement', 'showScopeDetails', true);
    const closing = config.tokens?.closingText;

    const box =
      showSubject || showAuthor || showScope
        ? `<div class="cert-achievement-box">
             ${showSubject ? `<span class="cert-subject-name">${escapeHtml(sample.SubjectName)}</span>` : ''}
             ${showAuthor ? `<div class="cert-author-name">${escapeHtml(sample.AuthorName)}</div>` : ''}
             ${showScope ? `<div class="cert-scope-text">${escapeHtml(sample.ScopeDetails)}</div>` : ''}
           </div>`
        : '';

    return `<div class="cert-body-content">
      <p class="cert-intro-text">${escapeHtml(intro)}</p>
      ${showStudent ? `<div class="cert-student-name">${escapeHtml(sample.StudentName)}</div>` : ''}
      ${box}
      ${closing ? `<p class="cert-closing-text">${escapeHtml(resolve(closing))}</p>` : ''}
    </div>`;
  }

  function renderScope() {
    const sample = getSample();
    const items = [];
    if (sectionConfig('quranScope', 'showFromJuz', true)) items.push(['من الجزء', sample.FromJuz]);
    if (sectionConfig('quranScope', 'showToJuz', true)) items.push(['إلى الجزء', sample.ToJuz]);
    if (sectionConfig('quranScope', 'showJuzCount', true)) items.push(['عدد الأجزاء', sample.JuzCount]);
    if (sectionConfig('quranScope', 'showCompletionPercentage', true)) items.push(['نسبة الإنجاز', `${sample.CompletionPercentage}%`]);

    const grid = items.length
      ? `<div class="cert-scope-grid">${items
          .map(
            ([label, value]) =>
              `<div class="cert-scope-item"><span class="cert-scope-label">${escapeHtml(label)}</span><span class="cert-scope-value">${escapeHtml(value)}</span></div>`,
          )
          .join('')}</div>`
      : '';

    const riwayah = sectionConfig('quranScope', 'showRiwayah', true)
      ? `<div class="cert-riwayah">${escapeHtml(resolve(sectionConfig('quranScope', 'riwayahText', sample.Riwayah)))}</div>`
      : '';

    return `<div class="cert-scope">${grid}${riwayah}</div>`;
  }

  function renderEvaluation() {
    const sample = getSample();
    const showScore = sectionConfig('evaluation', 'showScore', true);
    const showRating = sectionConfig('evaluation', 'showRating', true);
    const showExam = sectionConfig('evaluation', 'showExamResult', true);

    const badge = showExam
      ? `<div class="cert-rating-badge"><i class='bx bxs-star cert-rating-star'></i><span>${escapeHtml(sample.ExamResult || 'بتقدير: ممتاز ومبارك')}</span></div>`
      : '';

    const rows =
      showScore || showRating
        ? `<div class="cert-evaluation">
             ${showScore ? `<div class="cert-evaluation-item"><span class="cert-evaluation-label">الدرجة</span><span class="cert-evaluation-value">${sample.Score}</span></div>` : ''}
             ${showRating ? `<div class="cert-evaluation-item"><span class="cert-evaluation-label">التقدير</span><span class="cert-evaluation-value">${escapeHtml(sample.Rating)}</span></div>` : ''}
           </div>`
        : '';

    return `${badge}${rows}`;
  }

  function renderSignatures() {
    const sample = getSample();
    const columns = signatureColumns().length ? signatureColumns() : DEFAULT_SIGNATURE_COLUMNS;
    const html = columns
      .map((column) => {
        if (column.type === 'seal') {
          return `<div class="cert-seal-box">
            ${column.showSeal ? '<div class="cert-official-seal"><i class="bx bxs-check-shield"></i></div>' : ''}
            ${column.showDate ? `<div class="cert-date-text">صدرت بتاريخ: ${escapeHtml(sample.IssueDate)}</div>` : ''}
          </div>`;
        }
        return `<div class="cert-sign-col">
          ${column.title ? `<div class="cert-sign-title">${escapeHtml(column.title)}</div>` : ''}
          <div class="cert-sign-name">${escapeHtml(resolve(column.nameField || ''))}</div>
          <div class="cert-sign-line"></div>
          ${column.subtitle ? `<small class="text-muted">${escapeHtml(column.subtitle)}</small>` : ''}
        </div>`;
      })
      .join('');
    return `<div class="cert-footer">${html}</div>`;
  }

  function renderQr() {
    const size = Number(sectionConfig('qrVerification', 'size', 80)) || 80;
    const showLabel = sectionConfig('qrVerification', 'showLabel', true);
    const label = resolve(sectionConfig('qrVerification', 'labelText', 'رمز التحقق الإلكتروني'));
    return `<div class="cert-qr">
      <img src="${QR_PLACEHOLDER}" alt="${escapeAttr(label)}" width="${size}" height="${size}" />
      ${showLabel ? `<div class="cert-qr-label">${escapeHtml(label)}</div>` : ''}
    </div>`;
  }

  // Scales the sheet down to the width of the preview panel with optional zoom.
  let zoomFactor = 1.0;

  function fitPreview(widthMm, heightMm) {
    if (!previewStage || !previewScaler) return;
    const sheet = previewHost.firstElementChild;
    const sheetHeight = Math.max(heightMm * MM_TO_PX, sheet ? sheet.offsetHeight : 0);
    const sheetWidth = widthMm * MM_TO_PX;
    const baseScale = Math.min(1, previewStage.clientWidth / sheetWidth);
    const scale = baseScale * zoomFactor;
    previewScaler.style.width = `${sheetWidth}px`;
    previewScaler.style.transform = `scale(${scale})`;
    previewScaler.style.transformOrigin = 'top center';
    previewStage.style.height = `${sheetHeight * scale}px`;

    const zoomEl = document.getElementById('zoomLevel');
    if (zoomEl) zoomEl.textContent = `${Math.round(zoomFactor * 100)}%`;
  }

  let lastPaper = { width: 297, height: 210 };

  const refit = () => fitPreview(lastPaper.width, lastPaper.height);

  // Zoom controls
  document.getElementById('btnZoomIn')?.addEventListener('click', () => {
    zoomFactor = Math.min(2.0, Math.round((zoomFactor + 0.15) * 100) / 100);
    refit();
  });
  document.getElementById('btnZoomOut')?.addEventListener('click', () => {
    zoomFactor = Math.max(0.4, Math.round((zoomFactor - 0.15) * 100) / 100);
    refit();
  });
  document.getElementById('btnZoomReset')?.addEventListener('click', () => {
    zoomFactor = 1.0;
    refit();
  });

  // Sample dataset dropdown
  document.getElementById('sampleDataSelect')?.addEventListener('change', (e) => {
    currentSampleKey = e.target.value;
    renderPreview();
  });

  // Print test
  document.getElementById('btnPrintTest')?.addEventListener('click', () => {
    window.print();
  });

  let resizeTimer;
  window.addEventListener('resize', () => {
    clearTimeout(resizeTimer);
    resizeTimer = setTimeout(refit, 120);
  });

  if (window.ResizeObserver && previewStage) {
    let lastWidth = null;
    new ResizeObserver(([entry]) => {
      const width = entry.contentRect.width;
      if (lastWidth !== null && Math.abs(width - lastWidth) < 1) return;
      lastWidth = width;
      refit();
    }).observe(previewStage);
  }

  // Version loader APIs for history inspection and restoration
  const originalInitialJson = configArea.value;

  window.loadVersionConfigIntoBuilder = function(configJson, versionNumber) {
    try {
      configArea.value = configJson;
      config = normalize(JSON.parse(configJson));
      configArea.classList.remove('is-invalid');
      syncControls();
      renderSignatureEditor();
      renderPreview();

      const banner = document.getElementById('versionNoticeBanner');
      const label = document.getElementById('loadedVersionLabel');
      if (banner && label) {
        label.textContent = `النسخة ${versionNumber}`;
        banner.style.display = 'flex';
        banner.style.setProperty('display', 'flex', 'important');
      }
      return true;
    } catch (e) {
      console.error('Failed to load version config:', e);
      return false;
    }
  };

  window.resetToOriginalConfig = function() {
    configArea.value = originalInitialJson;
    config = normalize(JSON.parse(originalInitialJson));
    configArea.classList.remove('is-invalid');
    syncControls();
    renderSignatureEditor();
    renderPreview();

    const banner = document.getElementById('versionNoticeBanner');
    if (banner) {
      banner.style.display = 'none';
      banner.style.setProperty('display', 'none', 'important');
    }
  };

  /* ----------------------------------------------------------------- init */

  syncControls();
  renderSignatureEditor();
  if (parseFailed) {
    configArea.classList.add('is-invalid');
    renderPreview();
  } else {
    write();
  }
})();

