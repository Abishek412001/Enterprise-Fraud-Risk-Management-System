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

let currentCustId = 1;

document.addEventListener("DOMContentLoaded", () => {
    const urlParams = new URLSearchParams(window.location.search);
    currentCustId = parseInt(urlParams.get("customerId")) || 1;
    document.getElementById("targetCustId").textContent = currentCustId;

    loadCustomerData(currentCustId);

    document.getElementById("btnFreeze")?.addEventListener("click", freezeAccount);
    document.getElementById("btnUnfreeze")?.addEventListener("click", unfreezeAccount);
    document.getElementById("btnSuspendCard")?.addEventListener("click", suspendCard);
    document.getElementById("btnBlockDevice")?.addEventListener("click", blockDevice);
    document.getElementById("btnCreateCase")?.addEventListener("click", () => window.location.href = "case-create.html");
    document.getElementById("btnPartnerContact")?.addEventListener("click", () => window.location.href = "partnercommunications.html");
    document.getElementById("btnSaveNotes")?.addEventListener("click", saveAnalystNotes);
});

async function loadCustomerData(id) {
    const data = await authFetch(`/investigation/customer360/${id}`);
    if (data) {
        document.getElementById("custName").textContent = data.fullName;
        document.getElementById("custEmail").textContent = data.email;
        document.getElementById("custPhone").textContent = data.phone;
        document.getElementById("custKyc").textContent = data.kycStatus;
        document.getElementById("custAml").textContent = data.amlRiskLevel;
        document.getElementById("custAccounts").textContent = data.totalAccounts;
        document.getElementById("custCards").textContent = data.totalCards;

        const statusBadge = document.getElementById("accountStatusBadge");
        if (data.isFrozen) {
            statusBadge.textContent = "ACCOUNT FROZEN";
            statusBadge.className = "badge fs-6 bg-danger";
        } else {
            statusBadge.textContent = "Account Normal";
            statusBadge.className = "badge fs-6 bg-success";
        }
    }
}

async function freezeAccount() {
    const reason = prompt("Enter reason for freezing account:", "High-risk ATO & Suspicious transaction velocity.");
    if (!reason) return;

    const res = await authFetch("/account/freeze", {
        method: "POST",
        body: JSON.stringify({ customerID: currentCustId, analystID: 1, reason: reason })
    });

    if (res) {
        alert("Account frozen.");
        loadCustomerData(currentCustId);
    }
}

async function unfreezeAccount() {
    const reason = prompt("Enter reason for unfreezing account:", "Customer identity verified.");
    if (!reason) return;

    const res = await authFetch("/account/unfreeze", {
        method: "POST",
        body: JSON.stringify({ customerID: currentCustId, analystID: 1, reason: reason })
    });

    if (res) {
        alert("Account unfrozen.");
        loadCustomerData(currentCustId);
    }
}

async function suspendCard() {
    const reason = prompt("Enter reason for suspending card:", "Card compromised in brute force attempt.");
    if (!reason) return;

    const res = await authFetch("/card/suspend", {
        method: "POST",
        body: JSON.stringify({ cardID: 1, analystID: 1, reason: reason })
    });

    if (res) alert("Card suspended.");
}

async function blockDevice() {
    const reason = prompt("Enter reason for blocking device:", "Malicious fingerprint hash.");
    if (!reason) return;

    const res = await authFetch("/device/block", {
        method: "POST",
        body: JSON.stringify({ deviceID: 1, analystID: 1, reason: reason })
    });

    if (res) alert("Device blocked.");
}

async function saveAnalystNotes() {
    const notes = document.getElementById("investigationNotes").value.trim();
    if (!notes) {
        alert("Please enter investigation notes.");
        return;
    }

    const res = await authFetch("/wca", {
        method: "POST",
        body: JSON.stringify({
            customerID: currentCustId,
            analystID: 1,
            actionType: "InvestigationNote",
            actionCategory: "AnalystDecision",
            actionDescription: "Analyst logged investigation decision and WCA evidence note.",
            comments: notes
        })
    });

    if (res) {
        alert("Analyst decision and WCA interaction logged successfully.");
        document.getElementById("investigationNotes").value = "";
    }
}
