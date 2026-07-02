// سلوك صفحة تقرير الأوراد: بحث فوري داخل جدول التفاصيل + طباعة/تصدير PDF.
(function () {
    "use strict";

    var searchInput = document.getElementById("wrSearch");
    var noMatch = document.getElementById("wrNoMatch");
    var printBtn = document.getElementById("wrPrintBtn");

    function rows() {
        return Array.prototype.slice.call(
            document.querySelectorAll(".wr-detail-row")
        );
    }

    function applySearch() {
        var term = (searchInput.value || "").trim().toLowerCase();
        var visible = 0;

        rows().forEach(function (row) {
            var haystack = (row.dataset.search || "").toLowerCase();
            var show = !term || haystack.indexOf(term) !== -1;
            row.style.display = show ? "" : "none";
            if (show) visible++;
        });

        if (noMatch) noMatch.style.display = visible === 0 ? "block" : "none";
    }

    if (searchInput) {
        searchInput.addEventListener("input", applySearch);
    }

    if (printBtn) {
        printBtn.addEventListener("click", function () {
            window.print();
        });
    }
})();
