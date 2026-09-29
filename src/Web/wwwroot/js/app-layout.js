window.appAjuntament = window.appAjuntament || {};

window.appAjuntament.isMobileViewport = function () {
    return window.matchMedia("(max-width: 640.98px)").matches;
};
