const API_BASE_URL = window.EFRS_API_BASE_URL || "https://localhost:5001/api";

const token = localStorage.getItem("efrs_token");
if (!token) {
    window.location.href = "login.html";
}

document.getElementById("navUsername").textContent = localStorage.getItem("efrs_username") || "User";

document.getElementById("logoutLink")?.addEventListener("click", (e) => {
    e.preventDefault();
    localStorage.removeItem("efrs_token");
    localStorage.removeItem("efrs_username");
    localStorage.removeItem("efrs_role");
    window.location.href = "login.html";
});

// Theme toggle
const themeToggle = document.getElementById("themeToggle");
const savedTheme = localStorage.getItem("efrs_theme") || "light";
document.body.setAttribute("data-theme", savedTheme);
updateThemeIcon(savedTheme);

themeToggle?.addEventListener("click", () => {
    const current = document.body.getAttribute("data-theme");
    const next = current === "light" ? "dark" : "light";
    document.body.setAttribute("data-theme", next);
    localStorage.setItem("efrs_theme", next);
    updateThemeIcon(next);
});

function updateThemeIcon(theme) {
    if (themeToggle) {
        themeToggle.innerHTML = theme === "light"
            ? '<i class="bi bi-moon-fill"></i>'
            : '<i class="bi bi-sun-fill"></i>';
    }
}

// Authenticated Fetch Helper
async function authFetch(path, options = {}) {
    const defaultHeaders = {
        "Authorization": `Bearer ${token}`,
        "Content-Type": "application/json"
    };

    const response = await fetch(`${API_BASE_URL}${path}`, {
        ...options,
        headers: { ...defaultHeaders, ...options.headers }
    });

    if (response.status === 401) {
        localStorage.removeItem("efrs_token");
        window.location.href = "login.html";
        return null;
    }

    if (!response.ok) return null;
    if (response.status === 204) return true;
    return response.json();
}

document.addEventListener("DOMContentLoaded", async () => {
    const exec = await authFetch("/executive");
    if (exec && exec.ruleBasedInsights) {
        const ul = document.getElementById("insightsList");
        ul.innerHTML = exec.ruleBasedInsights.map(i => `<li>${escapeHtml(i)}</li>`).join("");
    }

    renderMerchantChart();
    renderCountryChart();
});

function renderMerchantChart() {
    const ctx = document.getElementById("merchantChart")?.getContext("2d");
    if (!ctx) return;

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['CryptoExchange X', 'LuxJewelry Inc', 'FastPay Transfer', 'Global Retail', 'Electronics Hub'],
            datasets: [{
                label: 'Fraud Volume ($)',
                data: [45000, 32000, 28000, 15000, 9000],
                backgroundColor: '#d64545'
            }]
        },
        options: { responsive: true, maintainAspectRatio: false }
    });
}

function renderCountryChart() {
    const ctx = document.getElementById("countryChart")?.getContext("2d");
    if (!ctx) return;

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['USA', 'UK', 'Brazil', 'Nigeria', 'Others'],
            datasets: [{
                data: [40, 25, 15, 12, 8],
                backgroundColor: ['#1c3faa', '#e6a23c', '#d64545', '#16a37a', '#6c757d']
            }]
        },
        options: { responsive: true, maintainAspectRatio: false }
    });
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
