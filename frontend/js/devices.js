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

document.addEventListener("DOMContentLoaded", () => {
    loadDevices();

    document.getElementById("btnFilter")?.addEventListener("click", loadDevices);
});

async function loadDevices() {
    const filter = document.getElementById("statusFilter").value;
    const isBlocked = filter === "blocked" ? true : null;

    const tbody = document.getElementById("devicesTableBody");
    tbody.innerHTML = '<tr><td colspan="9" class="text-muted text-center py-4"><span class="spinner-border spinner-border-sm me-2"></span>Loading devices...</td></tr>';

    const result = await authFetch(`/devices?page=1&pageSize=20${isBlocked !== null ? `&isBlocked=${isBlocked}` : ''}`);

    if (!result || !result.items || result.items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="9" class="text-muted text-center py-4">No registered devices found.</td></tr>';
        return;
    }

    tbody.innerHTML = result.items.map(d => `
        <tr>
          <td>${d.deviceID}</td>
          <td>${escapeHtml(d.customerName || 'N/A')}</td>
          <td><code>${escapeHtml(d.deviceFingerprint)}</code></td>
          <td>${escapeHtml(d.browser)} / ${escapeHtml(d.operatingSystem)}</td>
          <td><code>${escapeHtml(d.ipAddress)}</code> (${escapeHtml(d.country)})</td>
          <td class="small text-muted">${new Date(d.firstSeen).toLocaleDateString()}</td>
          <td class="small text-muted">${new Date(d.lastSeen).toLocaleDateString()}</td>
          <td><span class="badge ${d.isBlocked ? 'bg-danger' : d.isTrusted ? 'bg-success' : 'bg-secondary'}">${d.isBlocked ? 'Blocked' : d.isTrusted ? 'Trusted' : 'Untrusted'}</span></td>
          <td class="text-end">
            <button class="btn btn-sm btn-outline-danger" onclick="toggleBlockDevice(${d.deviceID}, ${!d.isBlocked})">${d.isBlocked ? 'Unblock' : 'Block'}</button>
            <button class="btn btn-sm btn-outline-success" onclick="toggleTrustDevice(${d.deviceID}, ${!d.isTrusted})">${d.isTrusted ? 'Untrust' : 'Trust'}</button>
          </td>
        </tr>
    `).join("");
}

async function toggleBlockDevice(id, isBlocked) {
    const res = await authFetch(`/devices/${id}/block`, { method: "POST" });
    if (res) loadDevices();
}

async function toggleTrustDevice(id, isTrusted) {
    const res = await authFetch(`/devices/${id}/trust`, { method: "POST" });
    if (res) loadDevices();
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
