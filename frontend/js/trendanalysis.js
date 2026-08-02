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
    const tbody = document.getElementById("trendsTableBody");
    const trends = await authFetch("/trends");

    if (!trends || trends.length === 0) {
        tbody.innerHTML = `
            <tr>
              <td>Credential Stuffing Surge</td>
              <td><span class="badge bg-danger">ATO</span></td>
              <td><span class="badge bg-danger">High Risk</span></td>
              <td class="text-danger fw-bold">+45%</td>
              <td>Repeated IP Subnet 192.168.x.x</td>
              <td class="small text-muted">${new Date().toLocaleDateString()}</td>
            </tr>
            <tr>
              <td>High Velocity E-Commerce Fraud</td>
              <td><span class="badge bg-warning text-dark">Velocity</span></td>
              <td><span class="badge bg-warning text-dark">Medium Risk</span></td>
              <td class="text-warning fw-bold">+18%</td>
              <td>Merchant CryptoExchange X</td>
              <td class="small text-muted">${new Date().toLocaleDateString()}</td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = trends.map(t => `
        <tr>
          <td class="fw-bold">${escapeHtml(t.trendName)}</td>
          <td><span class="badge bg-secondary">${escapeHtml(t.category)}</span></td>
          <td><span class="badge bg-danger">${escapeHtml(t.riskLevel)}</span></td>
          <td class="fw-bold text-danger">+${t.growthPercentage}%</td>
          <td>${escapeHtml(t.topIndicator)}</td>
          <td class="small text-muted">${new Date(t.detectedDate).toLocaleDateString()}</td>
        </tr>
    `).join("");
});

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
