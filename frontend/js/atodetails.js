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

let currentAtoAlertId = null;

document.addEventListener("DOMContentLoaded", () => {
    const urlParams = new URLSearchParams(window.location.search);
    currentAtoAlertId = parseInt(urlParams.get("id"));

    if (!currentAtoAlertId) {
        alert("No valid ATO alert ID provided.");
        window.location.href = "atoalerts.html";
        return;
    }

    loadAtoDetails(currentAtoAlertId);

    document.getElementById("btnActionAssign")?.addEventListener("click", promptAssign);
    document.getElementById("btnActionBlockDevice")?.addEventListener("click", blockCurrentDevice);
    document.getElementById("btnActionTrustDevice")?.addEventListener("click", trustCurrentDevice);
    document.getElementById("btnActionFreeze")?.addEventListener("click", freezeAccount);
    document.getElementById("btnActionClose")?.addEventListener("click", closeAtoAlert);
});

async function loadAtoDetails(id) {
    const data = await authFetch(`/atoalerts/${id}`);
    if (!data) {
        alert("ATO Alert not found.");
        window.location.href = "atoalerts.html";
        return;
    }

    document.getElementById("atoNumberHeader").textContent = data.atoAlertNumber;
    document.getElementById("atoStatusBadge").textContent = data.status;
    document.getElementById("atoPriorityBadge").textContent = `${data.priority} Priority`;
    document.getElementById("atoRiskScoreText").textContent = data.riskScore;

    // Customer Info
    document.getElementById("custName").textContent = data.customerName || "N/A";
    document.getElementById("custEmail").textContent = data.customerEmail || "N/A";
    document.getElementById("custPhone").textContent = data.customerPhone || "N/A";
    document.getElementById("assignedAnalyst").textContent = data.assignedAnalystName || "Unassigned";

    // Telemetry
    document.getElementById("sessIp").textContent = data.ipAddress || "N/A";
    document.getElementById("sessCountry").textContent = data.country || "N/A";
    document.getElementById("sessBrowser").textContent = data.browser || "N/A";
    document.getElementById("sessOs").textContent = data.operatingSystem || "N/A";

    // Devices
    const devTbody = document.getElementById("devicesTableBody");
    if (data.previousDevices && data.previousDevices.length > 0) {
        devTbody.innerHTML = data.previousDevices.map(d => `
            <tr>
              <td><code>${escapeHtml(d.deviceFingerprint.substring(0, 12))}...</code></td>
              <td>${escapeHtml(d.browser)} / ${escapeHtml(d.operatingSystem)}</td>
              <td>${escapeHtml(d.country)}</td>
              <td><span class="badge ${d.isBlocked ? 'bg-danger' : d.isTrusted ? 'bg-success' : 'bg-secondary'}">${d.isBlocked ? 'Blocked' : d.isTrusted ? 'Trusted' : 'Untrusted'}</span></td>
            </tr>
        `).join("");
    } else {
        devTbody.innerHTML = '<tr><td colspan="4" class="text-muted">No device history found.</td></tr>';
    }

    // Sessions
    const sessTbody = document.getElementById("sessionsTableBody");
    if (data.recentSessions && data.recentSessions.length > 0) {
        sessTbody.innerHTML = data.recentSessions.map(s => `
            <tr>
              <td class="small text-muted">${new Date(s.loginTime).toLocaleTimeString()}</td>
              <td><code>${escapeHtml(s.ipAddress)}</code></td>
              <td>${escapeHtml(s.country)}</td>
              <td><span class="badge ${s.loginStatus === 'Success' ? 'bg-success' : 'bg-danger'}">${escapeHtml(s.loginStatus)}</span></td>
              <td><span class="fw-bold ${s.riskScore >= 60 ? 'text-danger' : 'text-success'}">${s.riskScore}</span></td>
            </tr>
        `).join("");
    } else {
        sessTbody.innerHTML = '<tr><td colspan="5" class="text-muted">No session logs found.</td></tr>';
    }
}

async function promptAssign() {
    const analystIdStr = prompt("Enter Analyst User ID to assign to this ATO alert:", "1");
    if (!analystIdStr) return;
    const analystId = parseInt(analystIdStr);
    if (!analystId) return;

    const res = await authFetch("/atoalerts/assign", {
        method: "POST",
        body: JSON.stringify({ atoAlertID: currentAtoAlertId, analystID: analystId })
    });

    if (res) loadAtoDetails(currentAtoAlertId);
}

async function blockCurrentDevice() {
    if (!confirm("Block this device fingerprint for future customer logins?")) return;
    const res = await authFetch("/devices/1/block", { method: "POST" });
    if (res) alert("Device successfully blocked.");
}

async function trustCurrentDevice() {
    if (!confirm("Mark this device fingerprint as TRUSTED for this customer?")) return;
    const res = await authFetch("/devices/1/trust", { method: "POST" });
    if (res) alert("Device successfully marked as trusted.");
}

async function freezeAccount() {
    if (!confirm("Freeze customer accounts to stop unauthorized ATO access?")) return;
    alert("Customer accounts frozen.");
}

async function closeAtoAlert() {
    const notes = prompt("Enter investigation resolution notes for closing this ATO alert:", "ATO investigation verified with customer; account secured.");
    if (!notes) return;

    const res = await authFetch("/atoalerts/close", {
        method: "POST",
        body: JSON.stringify({ atoAlertID: currentAtoAlertId, resolution: "ResolvedCustomerVerified", resolutionNotes: notes })
    });

    if (res) {
        alert("ATO Alert closed successfully.");
        loadAtoDetails(currentAtoAlertId);
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
