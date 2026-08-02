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

let currentPage = 1;
const pageSize = 15;

document.addEventListener("DOMContentLoaded", () => {
    loadSummaryStats();
    loadAtoAlerts(1);

    document.getElementById("btnFilter")?.addEventListener("click", () => loadAtoAlerts(1));
    document.getElementById("btnReset")?.addEventListener("click", resetFilters);
});

async function loadSummaryStats() {
    const stats = await authFetch("/atoalerts/summary-stats");
    if (stats) {
        document.getElementById("kpiOpenAto").textContent = stats.openAtoAlerts ?? 0;
        document.getElementById("kpiHighRiskLogins").textContent = stats.highRiskLoginsToday ?? 0;
        document.getElementById("kpiFailedLogins").textContent = stats.failedLoginsToday ?? 0;
        document.getElementById("kpiSuspiciousDev").textContent = stats.suspiciousDevicesCount ?? 0;
    }
}

async function loadAtoAlerts(page = 1) {
    currentPage = page;
    const search = document.getElementById("searchInput").value.trim();
    const status = document.getElementById("statusFilter").value;
    const priority = document.getElementById("priorityFilter").value;

    const params = new URLSearchParams({
        page: currentPage,
        pageSize: pageSize
    });

    if (search) params.append("q", search);
    if (status) params.append("status", status);
    if (priority) params.append("priority", priority);

    const tbody = document.getElementById("atoAlertsTableBody");
    tbody.innerHTML = '<tr><td colspan="11" class="text-muted text-center py-4"><span class="spinner-border spinner-border-sm me-2"></span>Loading ATO alerts...</td></tr>';

    const result = await authFetch(`/atoalerts?${params.toString()}`);

    if (!result || !result.items || result.items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="11" class="text-muted text-center py-4">No ATO alerts match the criteria.</td></tr>';
        document.getElementById("paginationInfo").textContent = "Showing 0 of 0 alerts";
        document.getElementById("paginationControls").innerHTML = "";
        return;
    }

    tbody.innerHTML = result.items.map(a => `
        <tr>
          <td><a href="atodetails.html?id=${a.atoAlertID}" class="fw-bold text-decoration-none">${escapeHtml(a.atoAlertNumber)}</a></td>
          <td>${escapeHtml(a.customerName || 'N/A')}</td>
          <td><code>${escapeHtml(a.ipAddress || 'N/A')}</code></td>
          <td>${escapeHtml(a.country || 'N/A')}</td>
          <td class="small">${escapeHtml(a.browser)} / ${escapeHtml(a.operatingSystem)}</td>
          <td><span class="badge bg-light text-dark border">${escapeHtml(a.alertType)}</span></td>
          <td><span class="fw-bold ${a.riskScore >= 75 ? 'text-danger' : 'text-warning'}">${a.riskScore}</span></td>
          <td><span class="badge ${a.priority === 'Critical' ? 'bg-danger' : 'bg-warning text-dark'}">${escapeHtml(a.priority)}</span></td>
          <td><span class="badge ${a.status === 'Open' ? 'bg-danger' : 'bg-success'}">${escapeHtml(a.status)}</span></td>
          <td class="small text-muted">${new Date(a.createdDate).toLocaleString()}</td>
          <td class="text-end">
            <a href="atodetails.html?id=${a.atoAlertID}" class="btn btn-sm btn-outline-secondary"><i class="bi bi-search"></i> Investigate</a>
          </td>
        </tr>
    `).join("");

    renderPagination(result.totalCount, result.page, result.pageSize);
}

function resetFilters() {
    document.getElementById("searchInput").value = "";
    document.getElementById("statusFilter").value = "";
    document.getElementById("priorityFilter").value = "";
    loadAtoAlerts(1);
}

function renderPagination(totalCount, page, pageSize) {
    const start = (page - 1) * pageSize + 1;
    const end = Math.min(page * pageSize, totalCount);
    document.getElementById("paginationInfo").textContent = `Showing ${start}-${end} of ${totalCount} alerts`;

    const totalPages = Math.ceil(totalCount / pageSize);
    const controls = document.getElementById("paginationControls");

    let html = `
        <li class="page-item ${page === 1 ? 'disabled' : ''}">
            <button class="page-link" onclick="loadAtoAlerts(${page - 1})">Previous</button>
        </li>
    `;

    for (let p = 1; p <= totalPages; p++) {
        if (p === 1 || p === totalPages || (p >= page - 2 && p <= page + 2)) {
            html += `
                <li class="page-item ${p === page ? 'active' : ''}">
                    <button class="page-link" onclick="loadAtoAlerts(${p})">${p}</button>
                </li>
            `;
        }
    }

    html += `
        <li class="page-item ${page === totalPages ? 'disabled' : ''}">
            <button class="page-link" onclick="loadAtoAlerts(${page + 1})">Next</button>
        </li>
    `;

    controls.innerHTML = html;
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
