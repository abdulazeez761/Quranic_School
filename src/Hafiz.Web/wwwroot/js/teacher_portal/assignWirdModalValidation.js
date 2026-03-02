let form = document.getElementById('wirdAssignmentForm');
let fromJuz = document.getElementById('FromJuz');
let toJuz = document.getElementById('ToJuz');
let fromSurah = document.getElementById('FromSurah');
let toSurah = document.getElementById('ToSurah');
let fromAyah = document.getElementById('FromAyah');
let toAyah = document.getElementById('ToAyah');
let assignmentType = document.getElementById('Type');
//if the user picked from juz should enter to juz number
fromJuz.addEventListener('change', function () {
  if (fromJuz.value) {
    toJuz.required = true;
    setRequired(toJuz, true);
  } else {
    toJuz.required = false;
    setRequired(toJuz, false);
  }
});

//if the user picked from surah should enter to surah number and only shows the suras after the from surah
fromSurah.addEventListener('change', function () {
  if (fromSurah.value) {
    toSurah.required = true;
    setRequired(toSurah, true);
  } else {
    toSurah.required = false;
    setRequired(toSurah, false);
  }
  //show only the suras after the from surah
  let fromSurahNumber = parseInt(fromSurah.value);
  for (let i = 0; i < toSurah.options.length; i++) {
    let option = toSurah.options[i];
    if (parseInt(option.value) < fromSurahNumber) {
      option.style.display = 'none';
    } else {
      option.style.display = 'block';
    }
  }
});
//if the user picked from ayah should enter to ayah number and only shows the ayahs after the from ayah
fromAyah.addEventListener('change', function () {
  if (fromAyah.value) {
    toAyah.required = true;
    setRequired(toAyah, true);
  } else {
    toAyah.required = false;
    setRequired(toAyah, false);
  }
});

// do not allow to send an empty form

function setRequired(field, isRequired) {
  field.required = isRequired;
  field
    .closest('.form-group')
    ?.querySelector('.form-label')
    ?.classList.toggle('required', isRequired);
}
