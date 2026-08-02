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
    loadUsers();

    document.getElementById("btnAssignRole")?.addEventListener("click", async () => {
        const userId = prompt("Enter User ID:");
        if (!userId) return;
        const role = prompt("Enter Role (Administrator | FRM Manager | Senior Fraud Analyst | Fraud Analyst | Auditor):", "Senior Fraud Analyst");
        if (!role) return;

        const res = await authFetch("/roles/assign", {
            method: "POST",
            body: JSON.stringify({ userId: parseInt(userId), roleName: role })
        });

        if (res) {
            alert("Role assigned successfully!");
            loadUsers();
        }
    });
});

async function loadUsers() {
    const tbody = document.getElementById("usersTableBody");
    tbody.innerHTML = '<tr><td colspan="5" class="text-muted text-center py-4">Loading users...</td></tr>';

    tbody.innerHTML = `
        <tr>
          <td>1</td>
          <td class="fw-bold">admin</td>
          <td>admin@fraudrisk.com</td>
          <td><span class="badge bg-danger">Administrator</span></td>
          <td class="small text-muted">${new Date().toLocaleDateString()}</td>
        </tr>
        <tr>
          <td>2</td>
          <td class="fw-bold">analyst1</td>
          <td>analyst1@fraudrisk.com</td>
          <td><span class="badge bg-primary">Fraud Analyst</span></td>
          <td class="small text-muted">${new Date().toLocaleDateString()}</td>
        </tr>
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
