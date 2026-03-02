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
  console.log(
    'From Juz changed:',
    fromJuz.value,
    'To Juz required:',
    toJuz.required,
  );
});

//if the user picked from surah should enter to surah number
