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
    loadCases();

    document.getElementById("btnFilter")?.addEventListener("click", loadCases);
    document.getElementById("btnReset")?.addEventListener("click", resetFilters);
});

async function loadSummaryStats() {
    const stats = await authFetch("/cases/summary-stats");
    if (stats) {
        document.getElementById("kpiOpenCases").textContent = stats.openCasesCount ?? 0;
        document.getElementById("kpiCriticalCases").textContent = stats.criticalCasesCount ?? 0;
        document.getElementById("kpiSlaBreaches").textContent = stats.slaBreachesCount ?? 0;
        document.getElementById("kpiClosedToday").textContent = stats.closedTodayCount ?? 0;
    }
}

async function loadCases() {
    const search = document.getElementById("searchInput").value.trim();
    const priority = document.getElementById("priorityFilter").value;
    const status = document.getElementById("statusFilter").value;

    const params = new URLSearchParams({ page: 1, pageSize: 20 });
    if (search) params.append("q", search);
    if (priority) params.append("priority", priority);
    if (status) params.append("status", status);

    const tbody = document.getElementById("casesTableBody");
    tbody.innerHTML = '<tr><td colspan="11" class="text-muted text-center py-4"><span class="spinner-border spinner-border-sm me-2"></span>Loading fraud cases...</td></tr>';

    const result = await authFetch(`/cases?${params.toString()}`);

    if (!result || !result.items || result.items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="11" class="text-muted text-center py-4">No fraud investigation cases found.</td></tr>';
        return;
    }

    tbody.innerHTML = result.items.map(c => `
        <tr>
          <td><a href="case-details.html?id=${c.caseID}" class="fw-bold text-decoration-none">${escapeHtml(c.caseNumber)}</a></td>
          <td>
            <div class="fw-bold">${escapeHtml(c.caseTitle)}</div>
            <small class="text-muted">${escapeHtml(c.caseType)}</small>
          </td>
          <td>${escapeHtml(c.customerName || 'N/A')}</td>
          <td><span class="badge ${c.priority === 'Critical' ? 'bg-danger' : c.priority === 'High' ? 'bg-warning text-dark' : 'bg-secondary'}">${escapeHtml(c.priority)}</span></td>
          <td><span class="badge bg-light text-dark border">${escapeHtml(c.severity)}</span></td>
          <td><span class="badge ${c.status === 'Open' ? 'bg-primary' : c.status === 'Escalated' ? 'bg-danger' : 'bg-success'}">${escapeHtml(c.status)}</span></td>
          <td><span class="badge ${c.slaStatus === 'Breached' ? 'bg-danger' : c.slaStatus === 'NearBreach' ? 'bg-warning text-dark' : 'bg-success'}">${escapeHtml(c.slaStatus)}</span></td>
          <td>${escapeHtml(c.assignedAnalystName || 'Unassigned')}</td>
          <td><span class="badge bg-secondary">${c.alertsCount} Alerts</span></td>
          <td>${c.ageHours}h</td>
          <td class="text-end">
            <a href="case-details.html?id=${c.caseID}" class="btn btn-sm btn-outline-secondary"><i class="bi bi-search"></i> View Case</a>
          </td>
        </tr>
    `).join("");
}

function resetFilters() {
    document.getElementById("searchInput").value = "";
    document.getElementById("priorityFilter").value = "";
    document.getElementById("statusFilter").value = "";
    loadCases();
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
