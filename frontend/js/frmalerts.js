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

// Global State
let currentPage = 1;
const pageSize = 15;
let assignModalInstance = null;

document.addEventListener("DOMContentLoaded", () => {
    assignModalInstance = new bootstrap.Modal(document.getElementById("assignModal"));
    loadSummaryStats();
    loadAlerts(1);

    document.getElementById("btnFilter")?.addEventListener("click", () => loadAlerts(1));
    document.getElementById("btnReset")?.addEventListener("click", resetFilters);
    document.getElementById("btnConfirmAssign")?.addEventListener("click", submitAssign);
});

async function loadSummaryStats() {
    const stats = await authFetch("/frmalerts/summary-stats");
    if (stats) {
        document.getElementById("kpiOpenAlerts").textContent = stats.openAlerts ?? 0;
        document.getElementById("kpiCriticalAlerts").textContent = stats.criticalAlerts ?? 0;
        document.getElementById("kpiAssignedAlerts").textContent = stats.assignedAlerts ?? 0;
        document.getElementById("kpiAvgAge").textContent = `${stats.averageAlertAgeHours ?? 0} hrs`;
    }
}

async function loadAlerts(page = 1) {
    currentPage = page;
    const search = document.getElementById("searchInput").value.trim();
    const status = document.getElementById("statusFilter").value;
    const priority = document.getElementById("priorityFilter").value;
    const severity = document.getElementById("severityFilter").value;

    const params = new URLSearchParams({
        page: currentPage,
        pageSize: pageSize
    });

    if (search) params.append("q", search);
    if (status) params.append("status", status);
    if (priority) params.append("priority", priority);
    if (severity) params.append("severity", severity);

    const tbody = document.getElementById("frmAlertsTableBody");
    tbody.innerHTML = '<tr><td colspan="11" class="text-muted text-center py-4"><span class="spinner-border spinner-border-sm me-2"></span>Loading alerts...</td></tr>';

    const result = await authFetch(`/frmalerts?${params.toString()}`);

    if (!result || !result.items || result.items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="11" class="text-muted text-center py-4">No FRM alerts match the criteria.</td></tr>';
        document.getElementById("paginationInfo").textContent = "Showing 0 of 0 alerts";
        document.getElementById("paginationControls").innerHTML = "";
        return;
    }

    tbody.innerHTML = result.items.map(a => `
        <tr>
          <td><a href="alertdetails.html?id=${a.alertID}" class="fw-bold text-decoration-none">${escapeHtml(a.alertNumber)}</a></td>
          <td>${escapeHtml(a.customerName || 'N/A')}</td>
          <td><code>${escapeHtml(a.accountNumber || 'N/A')}</code></td>
          <td><span class="badge bg-light text-dark border">${escapeHtml(a.alertType)}</span></td>
          <td><span class="badge ${getPriorityBadgeClass(a.priority)}">${escapeHtml(a.priority)}</span></td>
          <td><span class="badge ${getSeverityBadgeClass(a.severity)}">${escapeHtml(a.severity)}</span></td>
          <td>
            <div class="d-flex align-items-center gap-1">
              <span class="fw-bold ${a.riskScore >= 80 ? 'text-danger' : a.riskScore >= 50 ? 'text-warning' : 'text-success'}">${a.riskScore}</span>
              <div class="progress flex-fill ms-1" style="height:5px;">
                <div class="progress-bar ${a.riskScore >= 80 ? 'bg-danger' : a.riskScore >= 50 ? 'bg-warning' : 'bg-success'}" style="width: ${a.riskScore}%"></div>
              </div>
            </div>
          </td>
          <td>${a.assignedAnalystName ? `<span class="badge bg-secondary"><i class="bi bi-person me-1"></i>${escapeHtml(a.assignedAnalystName)}</span>` : '<span class="text-muted small">Unassigned</span>'}</td>
          <td><span class="badge ${getStatusBadgeClass(a.status)}">${escapeHtml(a.status)}</span></td>
          <td class="small text-muted">${new Date(a.createdDate).toLocaleString()}</td>
          <td class="text-end">
            <div class="btn-group btn-group-sm">
              <a href="alertdetails.html?id=${a.alertID}" class="btn btn-outline-secondary" title="Investigate"><i class="bi bi-search"></i> View</a>
              <button class="btn btn-outline-primary" onclick="openAssignModal(${a.alertID}, ${a.assignedAnalystID || 1})" title="Assign"><i class="bi bi-person-plus"></i></button>
            </div>
          </td>
        </tr>
    `).join("");

    renderPagination(result.totalCount, result.page, result.pageSize);
}

function resetFilters() {
    document.getElementById("searchInput").value = "";
    document.getElementById("statusFilter").value = "";
    document.getElementById("priorityFilter").value = "";
    document.getElementById("severityFilter").value = "";
    loadAlerts(1);
}

function openAssignModal(alertId, analystId) {
    document.getElementById("assignAlertId").value = alertId;
    document.getElementById("analystIdInput").value = analystId || 1;
    assignModalInstance.show();
}

async function submitAssign() {
    const alertId = parseInt(document.getElementById("assignAlertId").value);
    const analystId = parseInt(document.getElementById("analystIdInput").value);

    if (!alertId || !analystId) return;

    const res = await authFetch("/frmalerts/assign", {
        method: "POST",
        body: JSON.stringify({ alertID: alertId, analystID: analystId })
    });

    if (res) {
        assignModalInstance.hide();
        loadSummaryStats();
        loadAlerts(currentPage);
    }
}

function renderPagination(totalCount, page, pageSize) {
    const start = (page - 1) * pageSize + 1;
    const end = Math.min(page * pageSize, totalCount);
    document.getElementById("paginationInfo").textContent = `Showing ${start}-${end} of ${totalCount} alerts`;

    const totalPages = Math.ceil(totalCount / pageSize);
    const controls = document.getElementById("paginationControls");

    let html = `
        <li class="page-item ${page === 1 ? 'disabled' : ''}">
            <button class="page-link" onclick="loadAlerts(${page - 1})">Previous</button>
        </li>
    `;

    for (let p = 1; p <= totalPages; p++) {
        if (p === 1 || p === totalPages || (p >= page - 2 && p <= page + 2)) {
            html += `
                <li class="page-item ${p === page ? 'active' : ''}">
                    <button class="page-link" onclick="loadAlerts(${p})">${p}</button>
                </li>
            `;
        }
    }

    html += `
        <li class="page-item ${page === totalPages ? 'disabled' : ''}">
            <button class="page-link" onclick="loadAlerts(${page + 1})">Next</button>
        </li>
    `;

    controls.innerHTML = html;
}

function getPriorityBadgeClass(p) {
    switch (p) {
        case 'Critical': return 'bg-danger';
        case 'High': return 'bg-warning text-dark';
        case 'Medium': return 'bg-info text-dark';
        default: return 'bg-secondary';
    }
}

function getSeverityBadgeClass(s) {
    switch (s) {
        case 'Critical': return 'bg-danger';
        case 'High': return 'bg-warning text-dark';
        case 'Medium': return 'bg-info text-dark';
        default: return 'bg-light text-dark border';
    }
}

function getStatusBadgeClass(s) {
    switch (s) {
        case 'Open': return 'bg-primary';
        case 'InProgress': return 'bg-info text-dark';
        case 'Escalated': return 'bg-danger';
        case 'Closed': return 'bg-success';
        case 'FalsePositive': return 'bg-secondary';
        default: return 'bg-light text-dark';
    }
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
