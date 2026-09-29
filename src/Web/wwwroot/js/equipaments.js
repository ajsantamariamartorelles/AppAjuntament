window.equipamentsMap = null;

window.inicialitzarMapaEquipaments = function (containerId) {
    if (window.equipamentsMap) {
        window.equipamentsMap.remove();
        window.equipamentsMap = null;
    }
    const container = document.getElementById(containerId);
    if (!container) return false;

    window.equipamentsMap = L.map(containerId).setView([41.5197, 2.2539], 14);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
        maxZoom: 19
    }).addTo(window.equipamentsMap);
    return true;
};

window.afegirMarkersEquipaments = function (equipaments) {
    if (!window.equipamentsMap) return;

    const icona = L.divIcon({
        html: '<div style="background:#0d6efd;width:32px;height:32px;border-radius:50% 50% 50% 0;transform:rotate(-45deg);border:3px solid white;box-shadow:0 2px 6px rgba(0,0,0,.4)"></div>',
        iconSize: [32, 32],
        iconAnchor: [16, 32],
        popupAnchor: [0, -34],
        className: ''
    });

    const bounds = [];

    equipaments.forEach(e => {
        const lat = parseFloat(e.latitud);
        const lng = parseFloat(e.longitud);
        if (!lat || !lng) return;

        bounds.push([lat, lng]);

        const web = e.web ? `<a href="${e.web}" target="_blank" class="btn btn-sm btn-outline-primary mt-2 w-100">Web</a>` : '';
        const hores = e.horari ? `<div class='text-muted small mt-1'><i class='fas fa-clock me-1'></i>${e.horari}</div>` : '';

        L.marker([lat, lng], { icon: icona })
            .addTo(window.equipamentsMap)
            .bindPopup(`
                <div style="min-width:180px">
                    <div class="fw-bold">${e.nom}</div>
                    <div class="text-muted small">${e.tipusInstalacio || ''}</div>
                    ${hores}
                    ${web}
                </div>
            `);
    });

    if (bounds.length > 0) {
        window.equipamentsMap.fitBounds(bounds, { padding: [40, 40] });
    }
};
