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