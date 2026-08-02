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

document.addEventListener("DOMContentLoaded", () => {
    loadCustomer360(1);

    document.getElementById("btnLoad360")?.addEventListener("click", () => {
        const id = parseInt(document.getElementById("custSelect").value);
        loadCustomer360(id);
    });
});

async function loadCustomer360(id) {
    const data = await authFetch(`/investigation/customer360/${id}`);
    const container = document.getElementById("c360Content");

    if (!data) {
        container.innerHTML = '<div class="text-muted">Customer 360 profile not found.</div>';
        return;
    }

    container.innerHTML = `
        <div class="col-md-4">
          <div class="p-3 border rounded h-100 bg-light">
            <h6 class="border-bottom pb-2">Profile & Identity</h6>
            <div><strong>Name:</strong> ${escapeHtml(data.fullName)}</div>
            <div><strong>Email:</strong> ${escapeHtml(data.email)}</div>
            <div><strong>Phone:</strong> ${escapeHtml(data.phone)}</div>
            <div><strong>KYC Status:</strong> <span class="badge bg-success">${escapeHtml(data.kycStatus)}</span></div>
            <div><strong>AML Risk:</strong> <span class="badge bg-info text-dark">${escapeHtml(data.amlRiskLevel)}</span></div>
            <div><strong>Account Status:</strong> <span class="badge ${data.isFrozen ? 'bg-danger' : 'bg-success'}">${data.isFrozen ? 'Frozen' : 'Active'}</span></div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="p-3 border rounded h-100 bg-light">
            <h6 class="border-bottom pb-2">Risk Scoring</h6>
            <div class="display-6 fw-bold ${data.currentRiskScore >= 70 ? 'text-danger' : 'text-warning'}">${data.currentRiskScore}/100</div>
            <div><strong>Risk Category:</strong> ${escapeHtml(data.riskCategory)}</div>
            <hr>
            <div><strong>Open Cases:</strong> ${data.openCasesCount}</div>
            <div><strong>Devices Registered:</strong> ${data.registeredDevicesCount}</div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="p-3 border rounded h-100 bg-light">
            <h6 class="border-bottom pb-2">Cross-Module Alerts Summary</h6>
            <div><strong>FRM Alerts:</strong> ${data.frmAlertsCount}</div>
            <div><strong>ATO Alerts:</strong> ${data.atoAlertsCount}</div>
            <div><strong>Sentinel SIEM Alerts:</strong> ${data.sentinelAlertsCount}</div>
            <div><strong>Total Transactions:</strong> ${data.totalTransactions}</div>
          </div>
        </div>
    `;
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
