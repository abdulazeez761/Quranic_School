/**
 * ============================================================================
 * Quran Range Computation Service (AlQuran Cloud API)
 * ============================================================================
 * Computes destination Quran range (toSurah, toAyah) from starting point and amount,
 * supporting Pages (0), Ayahs (1), and Juz (2).
 * Includes an in-memory cache to minimize network calls and ensure high performance.
 * ============================================================================
 */

// In-memory cache for Quran API responses (ayahs, pages, and juz)
const _quranApiCache = new Map();

/**
 * Helper to fetch data from AlQuran Cloud API with caching and error handling
 * @param {string} endpoint - e.g. 'ayah/2:255', 'page/22', 'juz/2'
 * @returns {Promise<any>}
 */
async function fetchQuranApi(endpoint) {
  if (_quranApiCache.has(endpoint)) {
    return _quranApiCache.get(endpoint);
  }

  const response = await fetch(`https://api.alquran.cloud/v1/${endpoint}`);
  if (!response.ok) {
    throw new Error(`فشل الاتصال بخادم القرآن: ${response.status}`);
  }

  const json = await response.json();
  if (json.code !== 200 || !json.data) {
    throw new Error(json?.data || 'استجابة غير صحيحة من الخادم');
  }

  _quranApiCache.set(endpoint, json.data);
  return json.data;
}

/**
 * Computes target by number of Ayahs
 * @param {number} startGlobalAyah - Global ayah index (1 - 6236)
 * @param {number} amount - Number of ayahs to advance
 * @returns {Promise<{toSurah: number, toAyah: number}>}
 */
async function calculateTargetByAyahs(startGlobalAyah, amount) {
  const count = Math.max(1, Math.round(amount));
  const targetGlobal = Math.min(6236, startGlobalAyah + count - 1);

  const ayahData = await fetchQuranApi(`ayah/${targetGlobal}`);
  return {
    toSurah: ayahData.surah.number,
    toAyah: ayahData.numberInSurah,
  };
}

/**
 * Computes target by number of Pages
 * @param {number} startPage - Starting page (1 - 604)
 * @param {number} amount - Number of pages (supports decimals like 0.5, 1, 1.5, 2)
 * @returns {Promise<{toSurah: number, toAyah: number}>}
 */
async function calculateTargetByPages(startPage, amount) {
  const pageCount = Math.max(1, Math.ceil(amount));
  const targetPage = Math.min(604, startPage + pageCount - 1);

  const pageData = await fetchQuranApi(`page/${targetPage}`);
  const ayahs = pageData.ayahs;
  const lastAyah = ayahs[ayahs.length - 1];

  return {
    toSurah: lastAyah.surah.number,
    toAyah: lastAyah.numberInSurah,
  };
}

/**
 * Computes target by number of Juz
 * @param {number} startJuz - Starting Juz (1 - 30)
 * @param {number} amount - Number of Juz
 * @returns {Promise<{toSurah: number, toAyah: number}>}
 */
async function calculateTargetByJuz(startJuz, amount) {
  const juzCount = Math.max(1, Math.ceil(amount));
  const targetJuz = Math.min(30, startJuz + juzCount - 1);

  const juzData = await fetchQuranApi(`juz/${targetJuz}`);
  const ayahs = juzData.ayahs;
  const lastAyah = ayahs[ayahs.length - 1];

  return {
    toSurah: lastAyah.surah.number,
    toAyah: lastAyah.numberInSurah,
  };
}

/**
 * Main calculation entrypoint:
 * Calculates destination (toSurah, toAyah) given start position, amount, and unit.
 *
 * @param {number} startSurahId - Starting Surah ID (1 - 114)
 * @param {number} startAyah    - Starting Ayah number within startSurahId
 * @param {number} amount       - Amount to memorize/revise
 * @param {number} amountUnit   - 0: Pages | 1: Ayahs | 2: Juz
 * @returns {Promise<{toSurah: number, toAyah: number}>}
 */
async function computeTargetRange(startSurahId, startAyah, amount, amountUnit) {
  const validSurah = Math.min(114, Math.max(1, parseInt(startSurahId, 10) || 1));
  const validAyah = Math.max(1, parseInt(startAyah, 10) || 1);
  const validAmount = Math.max(0.1, parseFloat(amount) || 1.0);
  const unit = parseInt(amountUnit, 10) || 0;

  try {
    // 1. Fetch starting position metadata (global ayah number, page, juz)
    const startData = await fetchQuranApi(`ayah/${validSurah}:${validAyah}`);

    // 2. Dispatch to specific calculator based on unit
    switch (unit) {
      case 1: // Ayahs (آيات)
        return await calculateTargetByAyahs(startData.number, validAmount);

      case 2: // Juz (أجزاء)
        return await calculateTargetByJuz(startData.juz, validAmount);

      case 0: // Pages (صفحات)
      default:
        return await calculateTargetByPages(startData.page, validAmount);
    }
  } catch (error) {
    console.error('خطأ أثناء حساب نطاق الورد عبر خادم القرآن:', error);
    // Fallback: return start position if network fails
    return { toSurah: validSurah, toAyah: validAyah };
  }
}
