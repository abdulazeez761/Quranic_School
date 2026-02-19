var container = document.getElementById('navigatorContainer');
var navigators = container.getElementsByTagName('li');

for (let i = 0; i < navigators.length; i++) {
  navigators[i].addEventListener('click', function () {
    var currentActive = document.querySelector('#navigatorContainer li.active');
    if (currentActive) currentActive.classList.remove('active');
    this.classList.add('active');
  });
}
