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
    const stats = await authFetch("/investigation/summary-stats");
    if (stats) {
        document.getElementById("kpiUnderInv").textContent = stats.customersUnderInvestigationCount ?? 0;
        document.getElementById("kpiFrozen").textContent = stats.accountsFrozenCount ?? 0;
        document.getElementById("kpiCardsSusp").textContent = stats.cardsSuspendedCount ?? 0;
        document.getElementById("kpiDevBlocked").textContent = stats.devicesBlockedCount ?? 0;
    }

    const tbody = document.getElementById("queueTableBody");
    tbody.innerHTML = `
        <tr>
          <td>#1001</td>
          <td>John Doe (ID: 1)</td>
          <td>Analyst User</td>
          <td>${new Date().toLocaleTimeString()}</td>
          <td><span class="badge bg-danger">Active Investigation</span></td>
          <td class="text-end">
            <a href="investigation.html?customerId=1" class="btn btn-sm btn-outline-primary"><i class="bi bi-search"></i> Open Workspace</a>
          </td>
        </tr>
    `;
});
