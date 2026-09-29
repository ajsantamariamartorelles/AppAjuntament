// estadistiques.js - Visualitzacions de la pàgina d'estadístiques municipals

window.estadistiquesCharts = window.estadistiquesCharts || {};
window.estadistiquesPreferences = window.estadistiquesPreferences || {};

window.estadistiquesPreferences.getRowsPerTable = function () {
    const value = localStorage.getItem("estadistiques.rowsPerTable");
    if (!value) {
        return null;
    }

    const parsed = parseInt(value, 10);
    return Number.isNaN(parsed) ? null : parsed;
};

window.estadistiquesPreferences.setRowsPerTable = function (value) {
    localStorage.setItem("estadistiques.rowsPerTable", String(value));
};

window.renderPopulationDashboard = function (payload) {
    if (!payload) {
        return;
    }

    destroyPopulationCharts();

    renderSexeChart(payload);
    renderComparativaChart(payload);
    renderPiramideChart(payload);
};

function destroyPopulationCharts() {
    const keys = ["sexe", "comparativa", "piramide", "thematic"];

    for (const key of keys) {
        const chart = window.estadistiquesCharts[key];
        if (chart) {
            chart.destroy();
            window.estadistiquesCharts[key] = null;
        }
    }
}

window.renderThematicChart = function (payload) {
    const canvas = document.getElementById("thematicChart");
    if (!canvas || !payload || !payload.labels || !payload.values || payload.labels.length === 0) {
        return;
    }

    const previous = window.estadistiquesCharts.thematic;
    if (previous) {
        previous.destroy();
        window.estadistiquesCharts.thematic = null;
    }

    window.estadistiquesCharts.thematic = new Chart(canvas, {
        type: "bar",
        data: {
            labels: payload.labels,
            datasets: [{
                label: "Municipi",
                data: payload.values,
                backgroundColor: "#1B998B"
            }]
        },
        options: {
            indexAxis: "y",
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                title: {
                    display: !!payload.title,
                    text: payload.title || ""
                }
            }
        }
    });
};

function renderSexeChart(payload) {
    const canvas = document.getElementById("populationSexChart");
    if (!canvas || !payload.municipi) {
        return;
    }

    const homes = payload.municipi.homes ?? 0;
    const dones = payload.municipi.dones ?? 0;

    window.estadistiquesCharts.sexe = new Chart(canvas, {
        type: "doughnut",
        data: {
            labels: ["Homes", "Dones"],
            datasets: [{
                data: [homes, dones],
                backgroundColor: ["#2E86AB", "#D81159"],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });
}

function renderComparativaChart(payload) {
    const canvas = document.getElementById("populationComparativeChart");
    if (!canvas) {
        return;
    }

    const labels = [payload.nomMunicipi || "Municipi", payload.nomComarca || "Comarca", payload.nomCatalunya || "Catalunya"];
    const values = [payload.municipi?.total ?? 0, payload.comarca?.total ?? 0, payload.catalunya?.total ?? 0];
    const baseMunicipi = values[0] > 0 ? values[0] : 1;

    window.estadistiquesCharts.comparativa = new Chart(canvas, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [{
                label: "Població total",
                data: values,
                backgroundColor: ["#0B6E4F", "#F4A259", "#3A86FF"]
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const value = Number(context.raw || 0);
                            const ratio = value / baseMunicipi;
                            return "Població: " + value.toLocaleString("ca-ES") + " (" + ratio.toFixed(1).replace(".", ",") + "x municipi)";
                        }
                    }
                },
                subtitle: {
                    display: true,
                    text: "Escala logarítmica per visualitzar diferències grans"
                }
            },
            scales: {
                y: {
                    type: "logarithmic",
                    min: 1,
                    ticks: {
                        callback: function (value) {
                            return Number(value).toLocaleString("ca-ES");
                        }
                    }
                }
            }
        }
    });
}

function renderPiramideChart(payload) {
    const canvas = document.getElementById("populationPyramidChart");
    if (!canvas || !payload.piramide || !payload.piramide.labels) {
        return;
    }

    const labels = payload.piramide.labels;
    const homes = (payload.piramide.homes || []).map(v => -(v ?? 0));
    const dones = payload.piramide.dones || [];

    window.estadistiquesCharts.piramide = new Chart(canvas, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [
                {
                    label: "Homes",
                    data: homes,
                    backgroundColor: "#2E86AB"
                },
                {
                    label: "Dones",
                    data: dones,
                    backgroundColor: "#D81159"
                }
            ]
        },
        options: {
            indexAxis: "y",
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: "bottom"
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const value = Math.abs(context.raw);
                            return context.dataset.label + ": " + value.toLocaleString("ca-ES");
                        }
                    }
                }
            },
            scales: {
                x: {
                    stacked: true,
                    ticks: {
                        callback: function (value) {
                            return Math.abs(Number(value)).toLocaleString("ca-ES");
                        }
                    }
                },
                y: {
                    stacked: true
                }
            }
        }
    });
}
