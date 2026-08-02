// Charts render against vw_DailyTransactions, FraudAlerts (monthly), and
// vw_MerchantRisk / country aggregates once those endpoints are added to the
// backend (see analytical_queries.sql for the matching T-SQL). Placeholder
// data below shows the intended shape.

const trendCtx = document.getElementById("transactionTrendChart");
if (trendCtx) {
    new Chart(trendCtx, {
        type: "line",
        data: {
            labels: [],
            datasets: [{
                label: "Transactions",
                data: [],
                borderColor: "#1c3faa",
                backgroundColor: "rgba(28,63,170,0.12)",
                fill: true,
                tension: 0.35
            }]
        },
        options: { responsive: true, plugins: { legend: { display: false } } }
    });
}

const monthlyFraudCtx = document.getElementById("monthlyFraudChart");
if (monthlyFraudCtx) {
    new Chart(monthlyFraudCtx, {
        type: "bar",
        data: {
            labels: [],
            datasets: [{ label: "Fraud Alerts", data: [], backgroundColor: "#d64545" }]
        },
        options: { responsive: true, plugins: { legend: { display: false } } }
    });
}

const countryFraudCtx = document.getElementById("countryFraudChart");
if (countryFraudCtx) {
    new Chart(countryFraudCtx, {
        type: "doughnut",
        data: {
            labels: [],
            datasets: [{ data: [], backgroundColor: ["#1c3faa", "#16a37a", "#e6a23c", "#d64545", "#7c1d1d"] }]
        },
        options: { responsive: true }
    });
}

const merchantFraudCtx = document.getElementById("merchantFraudChart");
if (merchantFraudCtx) {
    new Chart(merchantFraudCtx, {
        type: "bar",
        data: {
            labels: [],
            datasets: [{ label: "Flagged Transactions", data: [], backgroundColor: "#e6a23c" }]
        },
        options: { responsive: true, indexAxis: "y", plugins: { legend: { display: false } } }
    });
}
