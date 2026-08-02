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
    const clusters = await authFetch("/aianalytics/clusters");
    if (clusters) {
        document.getElementById("clustersContainer").innerHTML = clusters.map(c => `
            <div class="p-3 border rounded d-flex justify-content-between align-items-center">
              <div>
                <strong class="d-block">${escapeHtml(c.clusterName)}</strong>
                <small class="text-muted">${c.customerCount} Customers assigned</small>
              </div>
              <span class="badge ${c.riskCategory === 'High' ? 'bg-danger' : c.riskCategory === 'Medium' ? 'bg-warning text-dark' : 'bg-success'}">${c.riskCategory} Risk</span>
            </div>
        `).join("");
    }

    const anomalies = await authFetch("/aianalytics/anomalies");
    if (anomalies) {
        document.getElementById("anomaliesBody").innerHTML = anomalies.map(a => `
            <tr>
              <td><code>${escapeHtml(a.entityID)}</code></td>
              <td>${escapeHtml(a.anomalyType)}</td>
              <td><span class="badge bg-danger">${(a.confidenceScore * 100).toFixed(0)}%</span></td>
            </tr>
        `).join("");
    }
});

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
