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

let currentIncidentId = null;

document.addEventListener("DOMContentLoaded", () => {
    const urlParams = new URLSearchParams(window.location.search);
    currentIncidentId = parseInt(urlParams.get("id")) || 1;

    loadIncidentDetails(currentIncidentId);

    document.getElementById("btnActionAssign")?.addEventListener("click", promptAssign);
    document.getElementById("btnActionFreeze")?.addEventListener("click", freezeAccount);
    document.getElementById("btnActionClose")?.addEventListener("click", closeIncident);
});

async function loadIncidentDetails(id) {
    const data = await authFetch(`/incidents/${id}`);
    if (!data) {
        alert("Incident not found.");
        window.location.href = "sentinelalerts.html";
        return;
    }

    document.getElementById("incidentNumberHeader").textContent = data.incidentNumber;
    document.getElementById("incTitle").textContent = data.title;
    document.getElementById("incDesc").textContent = data.description || "Correlated SIEM Incident.";
    document.getElementById("incSeverityBadge").textContent = `${data.severity} Severity`;
    document.getElementById("incStatusBadge").textContent = data.status;
    document.getElementById("incAnalyst").textContent = data.assignedAnalystName || "Unassigned";
    document.getElementById("custEmail").textContent = data.customerEmail || "N/A";
    document.getElementById("createdTime").textContent = new Date(data.createdDate).toLocaleString();

    // Threat Intel Matches
    const threatTbody = document.getElementById("threatIntelTableBody");
    if (data.matchedThreatIndicators && data.matchedThreatIndicators.length > 0) {
        threatTbody.innerHTML = data.matchedThreatIndicators.map(t => `
            <tr>
              <td><code>${escapeHtml(t.indicatorValue)}</code></td>
              <td>${escapeHtml(t.indicatorType)}</td>
              <td><span class="badge bg-danger">${escapeHtml(t.threatLevel)}</span></td>
              <td><span class="badge bg-info text-dark">${escapeHtml(t.source)}</span></td>
            </tr>
        `).join("");
    } else {
        threatTbody.innerHTML = '<tr><td colspan="4" class="text-muted">No threat intel matches found.</td></tr>';
    }

    // Correlated Alerts
    const alertsTbody = document.getElementById("alertsTableBody");
    if (data.correlatedAlerts && data.correlatedAlerts.length > 0) {
        alertsTbody.innerHTML = data.correlatedAlerts.map(a => `
            <tr>
              <td><code>${escapeHtml(a.alertNumber)}</code></td>
              <td>${escapeHtml(a.alertName)}</td>
              <td><span class="badge bg-danger">${escapeHtml(a.severity)}</span></td>
              <td>${escapeHtml(a.alertSource)}</td>
            </tr>
        `).join("");
    } else {
        alertsTbody.innerHTML = '<tr><td colspan="4" class="text-muted">No correlated alerts.</td></tr>';
    }

    // Security Events
    const eventsTbody = document.getElementById("eventsTableBody");
    if (data.securityEvents && data.securityEvents.length > 0) {
        eventsTbody.innerHTML = data.securityEvents.map(e => `
            <tr>
              <td class="small text-muted">${new Date(e.eventTime).toLocaleTimeString()}</td>
              <td><span class="badge bg-light text-dark border">${escapeHtml(e.eventType)}</span></td>
              <td><code>${escapeHtml(e.ipAddress)}</code></td>
              <td><span class="badge bg-success">${escapeHtml(e.result)}</span></td>
            </tr>
        `).join("");
    } else {
        eventsTbody.innerHTML = '<tr><td colspan="4" class="text-muted">No raw security events found.</td></tr>';
    }
}

async function promptAssign() {
    const analystIdStr = prompt("Enter Analyst User ID to assign to this SIEM Incident:", "1");
    if (!analystIdStr) return;
    const analystId = parseInt(analystIdStr);
    if (!analystId) return;

    const res = await authFetch("/incidents/assign", {
        method: "POST",
        body: JSON.stringify({ incidentID: currentIncidentId, analystID: analystId })
    });

    if (res) loadIncidentDetails(currentIncidentId);
}

async function freezeAccount() {
    if (!confirm("Freeze account associated with this SIEM incident?")) return;
    alert("Account frozen.");
}

async function closeIncident() {
    if (!confirm("Are you sure you want to CLOSE this SIEM Security Incident?")) return;
    const res = await authFetch("/incidents/close", {
        method: "POST",
        body: JSON.stringify({ incidentID: currentIncidentId })
    });

    if (res) {
        alert("SIEM Incident closed successfully.");
        loadIncidentDetails(currentIncidentId);
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
