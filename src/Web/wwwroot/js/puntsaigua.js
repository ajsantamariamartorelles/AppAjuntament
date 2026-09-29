let puntsAiguaMap = null;
let puntsAiguaMarkers = null;

window.inicialitzarMapaPuntsAigua = function (lat, lng) {
    try {
        if (typeof L === 'undefined') return false;
        const el = document.getElementById('punts-aigua-map');
        if (!el) return false;

        if (puntsAiguaMap) {
            puntsAiguaMap.remove();
            puntsAiguaMap = null;
        }

        puntsAiguaMap = L.map('punts-aigua-map').setView([lat, lng], 14);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(puntsAiguaMap);

        puntsAiguaMarkers = L.layerGroup().addTo(puntsAiguaMap);
        return true;
    } catch (e) {
        console.error('Error inicialitzant mapa punts aigua:', e);
        return false;
    }
};

window.afegirMarkersPuntsAigua = function (punts) {
    if (!puntsAiguaMap || !puntsAiguaMarkers) return;
    puntsAiguaMarkers.clearLayers();

    const iconColor = p => p.estat === 'En servei' ? '#e74c3c' : '#95a5a6';

    punts.forEach(p => {
        const icon = L.divIcon({
            className: '',
            html: `<div style="
                background:${iconColor(p)};
                width:32px;height:32px;border-radius:50% 50% 50% 0;
                transform:rotate(-45deg);border:3px solid white;
                box-shadow:0 2px 6px rgba(0,0,0,0.4);">
                <div style="transform:rotate(45deg);color:white;font-size:14px;
                    display:flex;align-items:center;justify-content:center;height:100%;">
                    💧
                </div>
            </div>`,
            iconSize: [32, 32],
            iconAnchor: [16, 32],
            popupAnchor: [0, -34]
        });

        const foto = p.foto1
            ? `<img src="${p.foto1}" style="width:100%;border-radius:6px;margin-top:8px;" onerror="this.style.display='none'">`
            : '';

        const estatBadge = p.estat === 'En servei'
            ? `<span style="background:#d4edda;color:#155724;padding:2px 8px;border-radius:10px;font-size:0.8rem;">${p.estat}</span>`
            : `<span style="background:#f8d7da;color:#721c24;padding:2px 8px;border-radius:10px;font-size:0.8rem;">${p.estat || 'Desconegut'}</span>`;

        const popup = `
            <div style="min-width:200px;max-width:260px;font-family:sans-serif;">
                <div style="font-weight:bold;font-size:1rem;margin-bottom:4px;">${p.codi}</div>
                <div style="color:#555;margin-bottom:6px;">${p.tipologia} · ${p.tipus}</div>
                <div style="margin-bottom:6px;"><i>📍</i> ${p.paratge}</div>
                <div>${estatBadge}</div>
                ${foto}
            </div>`;

        L.marker([p.lat, p.lng], { icon })
            .bindPopup(popup)
            .addTo(puntsAiguaMarkers);
    });

    if (punts.length > 0) {
        const bounds = L.latLngBounds(punts.map(p => [p.lat, p.lng]));
        puntsAiguaMap.fitBounds(bounds, { padding: [40, 40] });
    }
};
