window.initParliamentChart = function (labels, data, titolEix, titolGrafic) {
    if (typeof Chart === 'undefined') {
        console.error('Chart.js no està carregat');
        return;
    }

    const ctx = document.getElementById('parliamentChart');
    if (!ctx) {
        console.error('Element canvas parliamentChart no trobat');
        return;
    }

    // Destruir gràfic existent si existeix (Chart.js v3 API)
    const existingChart = Chart.getChart(ctx);
    if (existingChart) {
        existingChart.destroy();
    }

    const colors = [
        'rgba(255, 99, 132, 0.8)',
        'rgba(54, 162, 235, 0.8)',
        'rgba(255, 206, 86, 0.8)',
        'rgba(75, 192, 192, 0.8)',
        'rgba(153, 102, 255, 0.8)',
        'rgba(255, 159, 64, 0.8)',
        'rgba(199, 199, 199, 0.8)',
        'rgba(83, 102, 255, 0.8)',
        'rgba(255, 99, 255, 0.8)',
        'rgba(99, 255, 132, 0.8)'
    ];
    const borderColors = colors.map(c => c.replace('0.8', '1'));

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: titolEix,
                data: data,
                backgroundColor: colors.slice(0, data.length),
                borderColor: borderColors.slice(0, data.length),
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                title: {
                    display: true,
                    text: titolGrafic
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    ticks: { stepSize: 1 },
                    title: { display: true, text: titolEix }
                },
                y: {
                    title: { display: true, text: 'Partits' }
                }
            }
        }
    });
};
