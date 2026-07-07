// Mobile student report list: one-open-at-a-time accordion behaviour.
(function () {
    var accordion = document.querySelector('[data-accordion]');
    if (!accordion) return;

    function setOpen(item, open) {
        var toggle = item.querySelector('[data-accordion-toggle]');
        item.classList.toggle('open', open);
        if (toggle) toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
    }

    accordion.addEventListener('click', function (e) {
        var toggle = e.target.closest('[data-accordion-toggle]');
        if (!toggle || !accordion.contains(toggle)) return;

        var item = toggle.closest('[data-accordion-item]');
        if (!item) return;

        var wasOpen = item.classList.contains('open');

        accordion.querySelectorAll('[data-accordion-item].open').forEach(function (other) {
            if (other !== item) setOpen(other, false);
        });

        setOpen(item, !wasOpen);
    });
})();
