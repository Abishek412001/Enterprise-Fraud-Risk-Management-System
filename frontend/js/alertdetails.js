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

let currentAlertId = null;
let closeModalInstance = null;
let escalateModalInstance = null;

document.addEventListener("DOMContentLoaded", () => {
    closeModalInstance = new bootstrap.Modal(document.getElementById("closeModal"));
    escalateModalInstance = new bootstrap.Modal(document.getElementById("escalateModal"));

    const urlParams = new URLSearchParams(window.location.search);
    currentAlertId = parseInt(urlParams.get("id"));

    if (!currentAlertId) {
        alert("No valid alert ID provided.");
        window.location.href = "frmalerts.html";
        return;
    }

    loadAlertDetails(currentAlertId);

    document.getElementById("btnSubmitComment")?.addEventListener("click", submitComment);
    document.getElementById("btnActionClose")?.addEventListener("click", () => closeModalInstance.show());
    document.getElementById("btnConfirmClose")?.addEventListener("click", submitClose);
    document.getElementById("btnActionEscalate")?.addEventListener("click", () => escalateModalInstance.show());
    document.getElementById("btnConfirmEscalate")?.addEventListener("click", submitEscalate);
    document.getElementById("btnActionAssign")?.addEventListener("click", promptAssign);
    document.getElementById("btnActionFreeze")?.addEventListener("click", freezeAccount);
    document.getElementById("btnActionUnfreeze")?.addEventListener("click", unfreezeAccount);
});

async function loadAlertDetails(id) {
    const alertData = await authFetch(`/frmalerts/${id}`);
    if (!alertData) {
        alert("Alert not found or error loading data.");
        window.location.href = "frmalerts.html";
        return;
    }

    document.getElementById("alertNumberHeader").textContent = alertData.alertNumber;
    document.getElementById("alertStatusBadge").textContent = alertData.status;
    document.getElementById("alertStatusBadge").className = `badge fs-6 ${getStatusBadgeClass(alertData.status)}`;
    document.getElementById("alertPriorityBadge").textContent = alertData.priority;
    document.getElementById("alertPriorityBadge").className = `badge fs-6 ${getPriorityBadgeClass(alertData.priority)}`;
    document.getElementById("alertSeverityBadge").textContent = `Severity: ${alertData.severity}`;
    document.getElementById("alertRiskScoreText").textContent = alertData.riskScore;

    // Customer
    document.getElementById("custName").textContent = alertData.customerName || "N/A";
    document.getElementById("custEmail").textContent = alertData.customerEmail || "N/A";
    document.getElementById("custPhone").textContent = alertData.customerPhone || "N/A";
    document.getElementById("custNationalId").textContent = alertData.nationalIdNumber || "N/A";

    // Account
    document.getElementById("accNumber").textContent = alertData.accountNumber || "N/A";
    document.getElementById("accBalance").textContent = (alertData.accountBalance || 0).toFixed(2);
    document.getElementById("accStatus").textContent = alertData.accountStatus || "Active";
    document.getElementById("accStatus").className = `badge ${alertData.accountStatus === 'Frozen' ? 'bg-danger' : 'bg-success'}`;
    document.getElementById("assignedAnalyst").textContent = alertData.assignedAnalystName || "Unassigned";

    // Metadata
    document.getElementById("alertType").textContent = alertData.alertType || "N/A";
    document.getElementById("alertCategory").textContent = alertData.alertCategory || "N/A";
    document.getElementById("createdDate").textContent = new Date(alertData.createdDate).toLocaleString();
    document.getElementById("lastUpdated").textContent = new Date(alertData.lastUpdated).toLocaleString();

    // Cards
    const cardsTbody = document.getElementById("cardsTableBody");
    if (alertData.cards && alertData.cards.length > 0) {
        cardsTbody.innerHTML = alertData.cards.map(c => `
            <tr>
              <td><code>${escapeHtml(c.cardNumberMasked)}</code></td>
              <td>${escapeHtml(c.cardType)}</td>
              <td><span class="badge ${c.status === 'Blocked' ? 'bg-danger' : 'bg-success'}">${escapeHtml(c.status)}</span></td>
              <td>${new Date(c.expiryDate).toLocaleDateString()}</td>
            </tr>
        `).join("");
    } else {
        cardsTbody.innerHTML = '<tr><td colspan="4" class="text-muted">No linked cards found.</td></tr>';
    }

    // Timeline History
    const timelineContainer = document.getElementById("timelineContainer");
    if (alertData.history && alertData.history.length > 0) {
        timelineContainer.innerHTML = alertData.history.map(h => `
            <div class="border-start border-3 border-primary ps-3 py-1 mb-2">
              <div class="d-flex justify-content-between align-items-center">
                <strong class="small text-primary">${escapeHtml(h.action)}</strong>
                <span class="text-muted" style="font-size:0.75rem;">${new Date(h.timestamp).toLocaleString()}</span>
              </div>
              <div class="small text-dark">${escapeHtml(h.comments || 'No remarks')}</div>
              <div class="text-muted" style="font-size:0.75rem;">By: ${escapeHtml(h.actionByUsername)} ${h.oldStatus ? `(${h.oldStatus} ➔ ${h.newStatus})` : ''}</div>
            </div>
        `).join("");
    } else {
        timelineContainer.innerHTML = '<div class="text-muted">No timeline events recorded.</div>';
    }

    // Comments
    renderComments(alertData.comments);
}

function renderComments(comments) {
    const commentsContainer = document.getElementById("commentsContainer");
    if (comments && comments.length > 0) {
        commentsContainer.innerHTML = comments.map(c => `
            <div class="bg-light p-2 rounded mb-2 border">
              <div class="d-flex justify-content-between align-items-center mb-1">
                <strong class="small text-dark"><i class="bi bi-person me-1"></i>${escapeHtml(c.analystUsername)}</strong>
                <span class="text-muted" style="font-size:0.75rem;">${new Date(c.timestamp).toLocaleString()}</span>
              </div>
              <div class="small text-secondary">${escapeHtml(c.comment)}</div>
            </div>
        `).join("");
    } else {
        commentsContainer.innerHTML = '<div class="text-muted small">No investigation comments yet.</div>';
    }
}

async function submitComment() {
    const input = document.getElementById("newCommentInput");
    const comment = input.value.trim();
    if (!comment) return;

    const res = await authFetch("/frmalerts/comment", {
        method: "POST",
        body: JSON.stringify({ alertID: currentAlertId, comment: comment })
    });

    if (res) {
        input.value = "";
        loadAlertDetails(currentAlertId);
    }
}

async function promptAssign() {
    const analystIdStr = prompt("Enter Analyst User ID to assign to this alert:", "1");
    if (!analystIdStr) return;
    const analystId = parseInt(analystIdStr);
    if (!analystId) return;

    const res = await authFetch("/frmalerts/assign", {
        method: "POST",
        body: JSON.stringify({ alertID: currentAlertId, analystID: analystId })
    });

    if (res) {
        loadAlertDetails(currentAlertId);
    }
}

async function submitEscalate() {
    const reason = document.getElementById("escalateReason").value.trim();
    if (!reason) {
        alert("Please enter an escalation reason.");
        return;
    }

    const res = await authFetch("/frmalerts/escalate", {
        method: "POST",
        body: JSON.stringify({ alertID: currentAlertId, reason: reason })
    });

    if (res) {
        escalateModalInstance.hide();
        loadAlertDetails(currentAlertId);
    }
}

async function submitClose() {
    const resolution = document.getElementById("closeResolution").value;
    const notes = document.getElementById("closeNotes").value.trim();

    if (!notes) {
        alert("Please enter resolution notes.");
        return;
    }

    const res = await authFetch("/frmalerts/close", {
        method: "POST",
        body: JSON.stringify({ alertID: currentAlertId, resolution: resolution, resolutionNotes: notes })
    });

    if (res) {
        closeModalInstance.hide();
        loadAlertDetails(currentAlertId);
    }
}

async function freezeAccount() {
    if (!confirm("Are you sure you want to FREEZE this customer's account as part of this investigation?")) return;
    alert("Account freeze request executed.");
}

async function unfreezeAccount() {
    if (!confirm("Are you sure you want to UNFREEZE this customer's account?")) return;
    alert("Account unfreeze request executed.");
}

function getPriorityBadgeClass(p) {
    switch (p) {
        case 'Critical': return 'bg-danger';
        case 'High': return 'bg-warning text-dark';
        case 'Medium': return 'bg-info text-dark';
        default: return 'bg-secondary';
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
