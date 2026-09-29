// Variables globals del mapa
let patrimoniMap = null;
let patrimoniMarkersGroup = null;

// Funcions del mapa disponibles globalment
window.inicialitzarMapa = function(lat, lng) {
    try {
        console.log('Inicialitzant mapa amb coordenades:', lat, lng);
        
        // Verificar que Leaflet està disponible
        if (typeof L === 'undefined') {
            console.error('Leaflet no està disponible');
            return false;
        }

        // Verificar que l'element del mapa existeix
        const mapElement = document.getElementById('patrimoni-map');
        if (!mapElement) {
            console.error('Element del mapa no trobat');
            return false;
        }

        // Netejar mapa anterior si existeix
        if (patrimoniMap) {
            patrimoniMap.remove();
            patrimoniMap = null;
            patrimoniMarkersGroup = null;
        }

        // Crear el mapa
        patrimoniMap = L.map('patrimoni-map').setView([lat, lng], 13);

        // Afegir tiles d'OpenStreetMap
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(patrimoniMap);

        // Crear grup de markers
        patrimoniMarkersGroup = L.layerGroup().addTo(patrimoniMap);
        
        console.log('Mapa inicialitzat correctament');
        return true;
    } catch (error) {
        console.error('Error inicialitzant mapa:', error);
        return false;
    }
};

window.navegarADetall = function(id) {
    console.log('Navegant a detall del patrimoni:', id);
    window.location.href = '/detall/' + id + '?from=mapa';
};

window.actualitzarMarkers = function(markersData) {
    try {
        console.log('=== actualitzarMarkers - Inici ===');
        console.log('  Nombre d\'elements rebuts:', markersData ? markersData.length : 0);
        console.log('  Tipus de dades:', typeof markersData);
        console.log('  És array?:', Array.isArray(markersData));
        
        if (markersData && markersData.length > 0) {
            console.log('  Primer element:', markersData[0]);
        }
        
        if (!patrimoniMap || !patrimoniMarkersGroup) {
            console.error('✗ Mapa no inicialitzat per actualitzar markers');
            console.log('  patrimoniMap:', patrimoniMap);
            console.log('  patrimoniMarkersGroup:', patrimoniMarkersGroup);
            return false;
        }

        console.log('✓ Mapa i grup de markers disponibles');

        // Netejar markers existents
        patrimoniMarkersGroup.clearLayers();
        console.log('✓ Markers anteriors netejats');

        // Si no hi ha dades, sortir
        if (!markersData || markersData.length === 0) {
            console.log('⚠ No hi ha markers per mostrar');
            return true;
        }

        console.log(`Processant ${markersData.length} markers...`);

        // Afegir nous markers
        let markersAfegits = 0;
        markersData.forEach(function(marker, index) {
            try {
                // Validar coordenades
                if (typeof marker.lat !== 'number' || typeof marker.lng !== 'number') {
                    console.error(`✗ Marker ${index + 1} (${marker.nom}): coordenades invàlides`, marker.lat, marker.lng);
                    return;
                }
                
                if (index < 3) {
                    console.log(`  Processant marker ${index + 1}: ${marker.nom} - [${marker.lat}, ${marker.lng}]`);
                }
                
                // Crear icona personalitzada
                const customIcon = L.divIcon({
                    className: 'custom-marker',
                    html: '<div style="background-color: ' + marker.color + '; width: 30px; height: 30px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 10px rgba(0,0,0,0.3); color: white; font-weight: bold; font-size: 12px; display: flex; align-items: center; justify-content: center;">' + 
                          (marker.tipus ? marker.tipus.charAt(0).toUpperCase() : '?') + 
                          '</div>',
                    iconSize: [30, 30],
                    iconAnchor: [15, 15]
                });

                // Crear marker
                const mapMarker = L.marker([marker.lat, marker.lng], { icon: customIcon });

                // Crear popup amb botó que crida a JavaScript
                const popupContent = `
                    <div style="max-width: 250px;">
                        <h6 style="margin: 0 0 10px 0; color: #333; font-weight: bold;">${marker.nom}</h6>
                        <p style="margin: 5px 0; color: #666; font-size: 0.9rem;">
                            <i class="bi bi-tag" style="color: ${marker.color}; margin-right: 5px;"></i>
                            ${marker.tipus}
                        </p>
                        ${marker.ubicacio ? `<p style="margin: 5px 0; color: #666; font-size: 0.9rem;">
                            <i class="bi bi-geo-alt" style="color: #666; margin-right: 5px;"></i>
                            ${marker.ubicacio}
                        </p>` : ''}
                        ${marker.estat ? `<p style="margin: 5px 0; color: #666; font-size: 0.9rem;">
                            <i class="bi bi-shield-check" style="color: #666; margin-right: 5px;"></i>
                            Estat: ${marker.estat}
                        </p>` : ''}
                        ${marker.descripcio ? `<p style="margin: 10px 0 5px 0; color: #666; font-size: 0.85rem; line-height: 1.3;">
                            ${marker.descripcio.substring(0, 100)}${marker.descripcio.length > 100 ? '...' : ''}
                        </p>` : ''}
                        <div style="text-align: center; margin-top: 15px;">
                            <button onclick="window.navegarADetall(${marker.id})" 
                               style="background: ${marker.color}; color: white; padding: 8px 15px; border: none; border-radius: 5px; font-size: 0.85rem; cursor: pointer;">
                                Veure detalls
                            </button>
                        </div>
                    </div>
                `;

                mapMarker.bindPopup(popupContent);
                patrimoniMarkersGroup.addLayer(mapMarker);
                markersAfegits++;
                
            } catch (markerError) {
                console.error(`✗ Error creant marker ${index + 1}:`, markerError);
            }
        });

        console.log(`✓ ${markersAfegits} markers afegits al mapa`);

        // Ajustar vista del mapa per mostrar tots els markers
        if (markersData.length > 0) {
            try {
                const group = new L.featureGroup(patrimoniMarkersGroup.getLayers());
                const bounds = group.getBounds();
                console.log('  Bounds del grup:', bounds);
                patrimoniMap.fitBounds(bounds.pad(0.1));
                console.log('✓ Vista del mapa ajustada');
            } catch (boundsError) {
                console.error('✗ Error ajustant vista del mapa:', boundsError);
            }
        }
        
        console.log('=== actualitzarMarkers - Fi (èxit) ===');
        return true;
    } catch (error) {
        console.error('✗✗ Error actualitzant markers:', error);
        console.error('Stack trace:', error.stack);
        console.log('=== actualitzarMarkers - Fi (error) ===');
        return false;
    }
};

window.centrarMapa = function() {
    try {
        if (patrimoniMap && patrimoniMarkersGroup) {
            const group = new L.featureGroup(patrimoniMarkersGroup.getLayers());
            if (group.getLayers().length > 0) {
                patrimoniMap.fitBounds(group.getBounds().pad(0.1));
                return true;
            }
        }
        return false;
    } catch (error) {
        console.error('Error centrant mapa:', error);
        return false;
    }
};

// Debug: Verificar que les funcions estan disponibles
console.log('Funcions del mapa carregades:', {
    inicialitzarMapa: typeof window.inicialitzarMapa,
    actualitzarMarkers: typeof window.actualitzarMarkers,
    centrarMapa: typeof window.centrarMapa,
    navegarADetall: typeof window.navegarADetall,
    leafletDisponible: typeof L !== 'undefined'
});