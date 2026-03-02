let fromJuz = document.getElementById('FromJuz');
let toJuz = document.getElementById('ToJuz');
let fromSurah = document.getElementById('FromSurah');
let toSurah = document.getElementById('ToSurah');
let fromAyah = document.getElementById('FromAyah');
let toAyah = document.getElementById('ToAyah');
let assignmentType = document.getElementById('Type');
//if the user picked from juz should enter to juz number
fromJuz.addEventListener('change', function () {
  if (fromJuz.value) toJuz.required = true;
  else toJuz.required = false;
});

//if the user picked from surah should enter to surah number and only shows the suras after the from surah
fromSurah.addEventListener('change', function () {
  if (fromSurah.value) toSurah.required = true;
  else toSurah.required = false;
  //show only the suras after the from surah
  let fromSurahNumber = parseInt(fromSurah.value) - 1;
  for (let i = 0; i < toSurah.options.length; i++) {
    let option = toSurah.options[i];
    if (parseInt(option.value) <= fromSurahNumber) {
      option.style.display = 'none';
    } else {
      option.style.display = 'block';
    }
  }
});
//if the user picked from ayah should enter to ayah number and only shows the ayahs after the from ayah
fromAyah.addEventListener('change', function () {
  if (fromAyah.value) toAyah.required = true;
  else toAyah.required = false;
});
