// helpers.js — Funcions JS genèriques per a l'aplicatiu

/**
 * Descarrega un fitxer encodoat en base64 al navegador de l'usuari.
 * @param {string} base64   Contingut del fitxer en base64
 * @param {string} fileName Nom del fitxer per la descàrrega
 * @param {string} mimeType MIME type del fitxer
 */
window.downloadBase64File = function (base64, fileName, mimeType) {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: mimeType });

    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

window.initTooltips = function () {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        if (!el._bsTooltip) {
            el._bsTooltip = new bootstrap.Tooltip(el);
        }
    });
};
