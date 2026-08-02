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
    const tbody = document.getElementById("globalTimelineBody");
    tbody.innerHTML = '<tr><td colspan="5" class="text-muted text-center py-4"><span class="spinner-border spinner-border-sm me-2"></span>Loading audit stream...</td></tr>';

    const caseData = await authFetch("/cases/1");
    if (caseData && caseData.timeline) {
        tbody.innerHTML = caseData.timeline.map(t => `
            <tr>
              <td class="small text-muted">${new Date(t.timestamp).toLocaleString()}</td>
              <td><a href="case-details.html?id=${caseData.caseID}" class="fw-bold">${caseData.caseNumber}</a></td>
              <td><span class="badge bg-info text-dark">${escapeHtml(t.action)}</span></td>
              <td>${escapeHtml(t.actionByName || 'System')}</td>
              <td class="small">${escapeHtml(t.details || '')}</td>
            </tr>
        `).join("");
    } else {
        tbody.innerHTML = '<tr><td colspan="5" class="text-muted text-center py-4">No global audit activity logged.</td></tr>';
    }
});

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
