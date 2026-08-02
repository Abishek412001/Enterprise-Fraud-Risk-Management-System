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

// Sidebar toggle
document.getElementById("sidebarToggle")?.addEventListener("click", () => {
    document.getElementById("sidebar")?.classList.toggle("open");
});

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

document.addEventListener("DOMContentLoaded", () => {
    loadSummaryStats();
    loadSentinelAlerts();

    document.getElementById("btnFilter")?.addEventListener("click", loadSentinelAlerts);
    document.getElementById("btnReset")?.addEventListener("click", resetFilters);
});

async function loadSummaryStats() {
    const stats = await authFetch("/sentinelalerts/summary-stats");
    if (stats) {
        document.getElementById("kpiOpenIncidents").textContent = stats.openIncidentsCount ?? 0;
        document.getElementById("kpiCriticalIncidents").textContent = stats.criticalIncidentsCount ?? 0;
        document.getElementById("kpiThreatIntel").textContent = stats.activeThreatIndicatorsCount ?? 0;
        document.getElementById("kpiEventsToday").textContent = stats.securityEventsTodayCount ?? 0;
    }
}

async function loadSentinelAlerts() {
    const search = document.getElementById("searchInput").value.trim();
    const severity = document.getElementById("severityFilter").value;
    const status = document.getElementById("statusFilter").value;

    const params = new URLSearchParams({ page: 1, pageSize: 20 });
    if (search) params.append("q", search);
    if (severity) params.append("severity", severity);
    if (status) params.append("status", status);

    const tbody = document.getElementById("sentinelAlertsTableBody");
    tbody.innerHTML = '<tr><td colspan="11" class="text-muted text-center py-4"><span class="spinner-border spinner-border-sm me-2"></span>Loading SIEM alerts...</td></tr>';

    const result = await authFetch(`/sentinelalerts?${params.toString()}`);

    if (!result || !result.items || result.items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="11" class="text-muted text-center py-4">No Sentinel SIEM alerts found.</td></tr>';
        return;
    }

    tbody.innerHTML = result.items.map(a => `
        <tr>
          <td><a href="incidentdetails.html?id=${a.incidentID || 1}" class="fw-bold text-decoration-none">${escapeHtml(a.alertNumber)}</a></td>
          <td>${escapeHtml(a.alertName)}</td>
          <td><span class="badge bg-light text-dark border">${escapeHtml(a.alertCategory)}</span></td>
          <td><span class="badge bg-info text-dark">${escapeHtml(a.alertSource)}</span></td>
          <td>${escapeHtml(a.customerName || 'N/A')}</td>
          <td><code>${escapeHtml(a.ipAddress)}</code> (${escapeHtml(a.country)})</td>
          <td><span class="fw-bold ${a.riskScore >= 80 ? 'text-danger' : 'text-warning'}">${a.riskScore}</span></td>
          <td><span class="badge ${a.severity === 'Critical' ? 'bg-danger' : 'bg-warning text-dark'}">${escapeHtml(a.severity)}</span></td>
          <td><span class="badge ${a.status === 'Open' ? 'bg-primary' : 'bg-success'}">${escapeHtml(a.status)}</span></td>
          <td class="small text-muted">${new Date(a.createdDate).toLocaleString()}</td>
          <td class="text-end">
            <a href="incidentdetails.html?id=${a.incidentID || 1}" class="btn btn-sm btn-outline-secondary"><i class="bi bi-search"></i> View Incident</a>
          </td>
        </tr>
    `).join("");
}

function resetFilters() {
    document.getElementById("searchInput").value = "";
    document.getElementById("severityFilter").value = "";
    document.getElementById("statusFilter").value = "";
    loadSentinelAlerts();
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
