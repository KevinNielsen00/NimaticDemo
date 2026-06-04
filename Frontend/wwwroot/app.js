window.themeManager = {
    setDarkMode: function (enabled) {
        document.body.classList.toggle("dark-mode", enabled);
        localStorage.setItem("darkMode", enabled ? "true" : "false");
    },

    loadDarkMode: function () {
        const enabled = localStorage.getItem("darkMode") === "true";
        document.body.classList.toggle("dark-mode", enabled);
        return enabled;
    }
};