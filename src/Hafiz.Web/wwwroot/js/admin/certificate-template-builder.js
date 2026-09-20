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
    cornerDecorations: true,
    watermarkEnabled: true,
    watermarkIcon: 'bx-book-open',
    watermarkOpacity: 0.04,
  };

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

  // Sample certificate used by the live preview only.
  const SAMPLE = {
    StudentName: 'أحمد محمد العبدالله',
    GenderedStudent: 'الطالب',
    GenderedCompleted: 'أتمّ',
    InstituteName: 'مركز تحفيظ القرآن الكريم',
    ClassName: 'حلقة الإمام البخاري',
    TeacherName: 'الأستاذ عبد الرحمن',
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
    HijriDate: '1447/03/12 هـ',
    GregorianDate: '2026/09/20 م',
    IssueDate: '1447/03/12 هـ الموافق 2026/09/20 م',
  };

  const SAMPLE_EXAM_RESULT = 'بتقدير: ممتاز ومبارك';

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
    next.sections = Array.isArray(next.sections) ? next.sections : [];
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

  document.querySelectorAll('.preset-btn').forEach((btn) => {
    btn.addEventListener('click', () => {
      Object.assign(config.theme, JSON.parse(btn.dataset.preset));
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
      btn.classList.toggle('active', JSON.parse(btn.dataset.preset).preset === config.theme.preset);
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
    return Object.keys(SAMPLE).reduce(
      (result, key) => result.replace(new RegExp(`\\{${key}\\}`, 'gi'), SAMPLE[key]),
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
    sheet.style.padding = `${margins.top}mm ${margins.right}mm ${margins.bottom}mm ${margins.left}mm`;

    const corners = `
      <div class="cert-corner cert-corner-tl"></div>
      <div class="cert-corner cert-corner-tr"></div>
      <div class="cert-corner cert-corner-bl"></div>
      <div class="cert-corner cert-corner-br"></div>`;

    const watermark = `<div class="cert-watermark" style="opacity:${theme.watermarkOpacity}"><i class="bx ${escapeAttr(theme.watermarkIcon)}"></i></div>`;

    sheet.innerHTML = `
      <div class="cert-outer-border" style="border-style:${escapeAttr(theme.borderStyle)};border-width:${Number(theme.borderWidth) || 0}px;border-color:${escapeAttr(theme.borderColor)}">
        <div class="cert-inner-border">
          ${theme.cornerDecorations ? corners : ''}
          ${theme.watermarkEnabled ? watermark : ''}
          ${sections.map((section) => renderSection(section.type)).join('')}
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
    const showLogo = sectionConfig('header', 'showInstituteLogo', true);
    const showName = sectionConfig('header', 'showInstituteName', true);
    const showSub = sectionConfig('header', 'showSubInstitute', true);
    const showBasmalah = sectionConfig('header', 'showBasmalah', true);
    const showNumber = sectionConfig('header', 'showCertificateNumber', true);
    const subText = sectionConfig('header', 'subInstituteText', `حلقة: ${SAMPLE.ClassName}`);

    const side =
      showLogo || showName || showSub
        ? `<div class="cert-header-side">
             ${showLogo ? '<div class="cert-logo cert-logo-placeholder">شعار</div>' : ''}
             <div>
               ${showName ? `<h2 class="cert-institute-title">${escapeHtml(SAMPLE.InstituteName)}</h2>` : ''}
               ${showSub ? `<div class="cert-sub-institute">${escapeHtml(resolve(subText))}</div>` : ''}
             </div>
           </div>`
        : '';

    return `<div class="cert-header">
      ${side}
      ${showBasmalah ? '<div class="cert-basmalah">بِسْمِ اللَّـهِ الرَّحْمَـٰنِ الرَّحِيمِ</div>' : ''}
      ${showNumber ? `<div class="cert-number-box"><span class="cert-serial">${SAMPLE.CertificateNumber}</span></div>` : ''}
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
    const intro = resolve(sectionConfig('statement', 'text', 'تشهد إدارة المركز بأن {GenderedStudent} قد أتمّ بحمد الله وتوفيقه.'));
    const showStudent = sectionConfig('statement', 'showStudentName', true);
    const showSubject = sectionConfig('statement', 'showSubjectName', true);
    const showAuthor = sectionConfig('statement', 'showAuthor', true);
    const showScope = sectionConfig('statement', 'showScopeDetails', true);
    const closing = config.tokens?.closingText;

    const box =
      showSubject || showAuthor || showScope
        ? `<div class="cert-achievement-box">
             ${showSubject ? `<span class="cert-subject-name">${escapeHtml(SAMPLE.SubjectName)}</span>` : ''}
             ${showAuthor ? `<div class="cert-author-name">${escapeHtml(SAMPLE.AuthorName)}</div>` : ''}
             ${showScope ? `<div class="cert-scope-text">${escapeHtml(SAMPLE.ScopeDetails)}</div>` : ''}
           </div>`
        : '';

    return `<div class="cert-body-content">
      <p class="cert-intro-text">${escapeHtml(intro)}</p>
      ${showStudent ? `<div class="cert-student-name">${escapeHtml(SAMPLE.StudentName)}</div>` : ''}
      ${box}
      ${closing ? `<p class="cert-closing-text">${escapeHtml(resolve(closing))}</p>` : ''}
    </div>`;
  }

  function renderScope() {
    const items = [];
    if (sectionConfig('quranScope', 'showFromJuz', true)) items.push(['من الجزء', SAMPLE.FromJuz]);
    if (sectionConfig('quranScope', 'showToJuz', true)) items.push(['إلى الجزء', SAMPLE.ToJuz]);
    if (sectionConfig('quranScope', 'showJuzCount', true)) items.push(['عدد الأجزاء', SAMPLE.JuzCount]);
    if (sectionConfig('quranScope', 'showCompletionPercentage', true)) items.push(['نسبة الإنجاز', `${SAMPLE.CompletionPercentage}%`]);

    const grid = items.length
      ? `<div class="cert-scope-grid">${items
          .map(
            ([label, value]) =>
              `<div class="cert-scope-item"><span class="cert-scope-label">${escapeHtml(label)}</span><span class="cert-scope-value">${escapeHtml(value)}</span></div>`,
          )
          .join('')}</div>`
      : '';

    const riwayah = sectionConfig('quranScope', 'showRiwayah', true)
      ? `<div class="cert-riwayah">${escapeHtml(resolve(sectionConfig('quranScope', 'riwayahText', SAMPLE.Riwayah)))}</div>`
      : '';

    return `<div class="cert-scope">${grid}${riwayah}</div>`;
  }

  function renderEvaluation() {
    const showScore = sectionConfig('evaluation', 'showScore', true);
    const showRating = sectionConfig('evaluation', 'showRating', true);
    const showExam = sectionConfig('evaluation', 'showExamResult', true);

    const badge = showExam
      ? `<div class="cert-rating-badge"><i class='bx bxs-star cert-rating-star'></i><span>${escapeHtml(SAMPLE_EXAM_RESULT)}</span></div>`
      : '';

    const rows =
      showScore || showRating
        ? `<div class="cert-evaluation">
             ${showScore ? `<div class="cert-evaluation-item"><span class="cert-evaluation-label">الدرجة</span><span class="cert-evaluation-value">${SAMPLE.Score}</span></div>` : ''}
             ${showRating ? `<div class="cert-evaluation-item"><span class="cert-evaluation-label">التقدير</span><span class="cert-evaluation-value">${escapeHtml(SAMPLE.Rating)}</span></div>` : ''}
           </div>`
        : '';

    return `${badge}${rows}`;
  }

  function renderSignatures() {
    const columns = signatureColumns().length ? signatureColumns() : DEFAULT_SIGNATURE_COLUMNS;
    const html = columns
      .map((column) => {
        if (column.type === 'seal') {
          return `<div class="cert-seal-box">
            ${column.showSeal ? '<div class="cert-official-seal"><i class="bx bxs-check-shield"></i></div>' : ''}
            ${column.showDate ? `<div class="cert-date-text">صدرت بتاريخ: ${escapeHtml(SAMPLE.IssueDate)}</div>` : ''}
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

  // Scales the sheet down to the width of the preview panel.
  function fitPreview(widthMm, heightMm) {
    if (!previewStage || !previewScaler) return;
    const sheet = previewHost.firstElementChild;
    // Content may run past the paper height, so the stage grows to whatever is actually rendered.
    const sheetHeight = Math.max(heightMm * MM_TO_PX, sheet ? sheet.offsetHeight : 0);
    const sheetWidth = widthMm * MM_TO_PX;
    const scale = Math.min(1, previewStage.clientWidth / sheetWidth);
    previewScaler.style.width = `${sheetWidth}px`;
    previewScaler.style.transform = `scale(${scale})`;
    previewScaler.style.transformOrigin = 'top right';
    previewStage.style.height = `${sheetHeight * scale}px`;
  }

  let lastPaper = { width: 297, height: 210 };

  let resizeTimer;
  window.addEventListener('resize', () => {
    clearTimeout(resizeTimer);
    resizeTimer = setTimeout(() => fitPreview(lastPaper.width, lastPaper.height), 120);
  });

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
