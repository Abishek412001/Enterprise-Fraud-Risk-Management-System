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
    const tbody = document.getElementById("analystTableBody");
    const list = await authFetch("/performance");

    if (!list || list.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-muted text-center py-4">No analyst metrics records found.</td></tr>';
        return;
    }

    tbody.innerHTML = list.map(a => `
        <tr>
          <td class="fw-bold">${escapeHtml(a.analystName)}</td>
          <td>${a.assignedAlerts}</td>
          <td><span class="badge bg-success">${a.closedAlerts}</span></td>
          <td><span class="badge bg-warning text-dark">${a.openCases}</span></td>
          <td>${a.avgInvestigationMinutes} mins</td>
          <td><span class="badge bg-info text-dark">${a.slaComplianceRate}%</span></td>
          <td><div class="progress" style="height:15px;"><div class="progress-bar bg-primary" style="width:${a.workloadScore}%">${a.workloadScore}%</div></div></td>
        </tr>
    `).join("");
});

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
