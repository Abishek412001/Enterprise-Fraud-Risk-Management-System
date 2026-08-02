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
    if (exec) {
        document.getElementById("execOpenAlerts").textContent = exec.totalOpenAlerts ?? 0;
        document.getElementById("execOpenCases").textContent = exec.openCases ?? 0;
    }

    renderDistributionChart();
});

function renderDistributionChart() {
    const ctx = document.getElementById("execDistributionChart")?.getContext("2d");
    if (!ctx) return;

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['FRM Alerts', 'ATO Alerts', 'Sentinel SIEM', 'Escalated Cases'],
            datasets: [{
                label: 'Volume',
                data: [65, 34, 18, 12],
                backgroundColor: ['#1c3faa', '#d64545', '#e6a23c', '#16a37a']
            }]
        },
        options: { responsive: true, maintainAspectRatio: false }
    });
}
