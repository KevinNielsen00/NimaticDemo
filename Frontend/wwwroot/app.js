window.themeManager = {
    apply: function (enabled) {
        document.documentElement.classList.toggle("dark-mode", enabled);
        document.body.classList.toggle("dark-mode", enabled);

        document.documentElement.setAttribute(
            "data-bs-theme",
            enabled ? "dark" : "light"
        );
    },

    setDarkMode: function (enabled) {
        localStorage.setItem("darkMode", enabled ? "true" : "false");
        this.apply(enabled);
    },

    loadDarkMode: function () {
        const enabled = localStorage.getItem("darkMode") === "true";
        this.apply(enabled);
        return enabled;
    }
};

window.themeManager.loadDarkMode();

document.addEventListener("DOMContentLoaded", function () {
    window.themeManager.loadDarkMode();
});

if (window.Blazor) {
    Blazor.addEventListener?.("enhancedload", function () {
        window.themeManager.loadDarkMode();
    });
}

window.timeZoneManager = {
    setTimeZone: function (timeZone) {
        localStorage.setItem("timeZone", timeZone);
    },

    loadTimeZone: function () {
        return localStorage.getItem("timeZone") || "Europe/Copenhagen";
    },

    formatDateTime: function (utcDateString) {
        const timeZone = localStorage.getItem("timeZone") || "Europe/Copenhagen";

        return new Date(utcDateString).toLocaleString("da-DK", {
            timeZone: timeZone,
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit"
        });
    }
};