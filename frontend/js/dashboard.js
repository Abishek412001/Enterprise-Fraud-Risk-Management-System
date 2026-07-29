const API_BASE_URL = window.EFRS_API_BASE_URL || "https://localhost:5001/api";

// ---- Auth guard ----
const token = localStorage.getItem("efrs_token");
if (!token) {
    window.location.href = "login.html";
}
document.getElementById("navUsername").textContent = localStorage.getItem("efrs_username") || "User";

document.getElementById("logoutLink").addEventListener("click", (e) => {
    e.preventDefault();
    localStorage.removeItem("efrs_token");
    localStorage.removeItem("efrs_username");
    localStorage.removeItem("efrs_role");
    window.location.href = "login.html";
});

// ---- Sidebar toggle (mobile) ----
document.getElementById("sidebarToggle")?.addEventListener("click", () => {
    document.getElementById("sidebar").classList.toggle("open");
});

// ---- Theme toggle ----
const themeToggle = document.getElementById("themeToggle");
const savedTheme = localStorage.getItem("efrs_theme") || "light";
document.body.setAttribute("data-theme", savedTheme);
updateThemeIcon(savedTheme);

themeToggle.addEventListener("click", () => {
    const current = document.body.getAttribute("data-theme");
    const next = current === "light" ? "dark" : "light";
    document.body.setAttribute("data-theme", next);
    localStorage.setItem("efrs_theme", next);
    updateThemeIcon(next);
});

function updateThemeIcon(theme) {
    themeToggle.innerHTML = theme === "light"
        ? '<i class="bi bi-moon-fill"></i>'
        : '<i class="bi bi-sun-fill"></i>';
}

// ---- Authenticated fetch helper ----
async function authFetch(path) {
    const response = await fetch(`${API_BASE_URL}${path}`, {
        headers: { Authorization: `Bearer ${token}` }
    });
    if (response.status === 401) {
        localStorage.removeItem("efrs_token");
        window.location.href = "login.html";
        return null;
    }
    if (!response.ok) return null;
    return response.json();
}

// ---- Populate dashboard stats & tables ----
(async function loadDashboard() {
    const customers = await authFetch("/customers?page=1&pageSize=1");
    if (customers) document.getElementById("statCustomers").textContent = customers.totalCount ?? "--";

    // NOTE: /accounts, /transactions, /fraudalerts summary endpoints follow the
    // same controller -> service -> repository pattern as CustomersController
    // and are wired up the same way once those modules are added.
    const recentTxBody = document.getElementById("recentTransactionsBody");
    const recentAlertsBody = document.getElementById("recentAlertsBody");

    if (recentTxBody) {
        recentTxBody.innerHTML = '<tr><td colspan="6" class="text-muted text-center">No data source connected yet</td></tr>';
    }
    if (recentAlertsBody) {
        recentAlertsBody.innerHTML = '<tr><td colspan="6" class="text-muted text-center">No data source connected yet</td></tr>';
    }
})();
