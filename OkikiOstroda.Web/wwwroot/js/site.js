// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", function () {
    const calendar = document.getElementById("availability-calendar");

    if (!calendar || typeof flatpickr === "undefined") {
        return;
    }

    const startInput = document.getElementById("StartDate");
    const endInput = document.getElementById("EndDate");
    const priceCalculation = document.getElementById("price-calculation");
    const bookedRanges = JSON.parse(calendar.dataset.bookedRanges || "[]");
    const updatePriceCalculation = function () {
        if (!priceCalculation || !startInput || !endInput || !startInput.value || !endInput.value) {
            return null;
        }

        const startDate = new Date(startInput.value);
        const endDate = new Date(endInput.value);
        const nights = Math.round((endDate - startDate) / 86400000);
        const baseRate = Number(priceCalculation.dataset.baseRate || 500);
        const discountRate = Number(priceCalculation.dataset.discountRate || 400);
        const discountStartsAfter = Number(priceCalculation.dataset.discountStartsAfter || 2);

        if (!Number.isFinite(nights) || nights <= 0) {
            return null;
        }

        const fullRateNights = Math.min(nights, discountStartsAfter);
        const discountedNights = Math.max(nights - discountStartsAfter, 0);
        const total = (fullRateNights * baseRate) + (discountedNights * discountRate);

        return total;
    };

    const updatePriceDisplay = function () {
        if (!priceCalculation) {
            return;
        }

        const total = updatePriceCalculation();
        priceCalculation.textContent = total === null ? "Wybierz termin, aby zobaczyć wycenę pobytu." : `${total} zł`;
    };

    flatpickr(calendar, {
        mode: "range",
        inline: true,
        minDate: "today",
        dateFormat: "Y-m-d",
        locale: flatpickr.l10ns.pl,
        disable: bookedRanges,
        defaultDate: startInput && endInput ? [startInput.value, endInput.value] : null,
        onChange: function (selectedDates, dateStr, instance) {
            if (!startInput || !endInput) {
                return;
            }

            if (selectedDates.length === 0) {
                startInput.value = "";
                endInput.value = "";
                updatePriceDisplay();
                return;
            }

            startInput.value = instance.formatDate(selectedDates[0], "Y-m-d");
            endInput.value = selectedDates.length > 1 ? instance.formatDate(selectedDates[1], "Y-m-d") : "";
            updatePriceDisplay();
        }
    });

    updatePriceDisplay();
});
