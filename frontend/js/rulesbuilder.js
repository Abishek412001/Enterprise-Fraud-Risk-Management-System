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
    loadRules();

    document.getElementById("ruleForm")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const code = document.getElementById("ruleCode").value.trim();
        const name = document.getElementById("ruleName").value.trim();
        const category = document.getElementById("ruleCategory").value;
        const condition = document.getElementById("ruleCondition").value.trim();

        const payload = {
            ruleCode: code,
            ruleName: name,
            category: category,
            conditionExpression: condition,
            riskScoreWeight: 30,
            actionToTake: "CreateAlert",
            priority: 1,
            isActive: true
        };

        const res = await authFetch("/rules", {
            method: "POST",
            body: JSON.stringify(payload)
        });

        if (res) {
            alert("Fraud rule deployed successfully!");
            document.getElementById("ruleCode").value = "";
            document.getElementById("ruleName").value = "";
            document.getElementById("ruleCondition").value = "";
            loadRules();
        }
    });
});

async function loadRules() {
    const tbody = document.getElementById("rulesTableBody");
    const rules = await authFetch("/rules");

    if (!rules || rules.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-muted text-center py-4">No active fraud rules configured.</td></tr>';
        return;
    }

    tbody.innerHTML = rules.map(r => `
        <tr>
          <td><code>${escapeHtml(r.ruleCode)}</code></td>
          <td class="fw-bold">${escapeHtml(r.ruleName)}</td>
          <td><span class="badge bg-secondary">${escapeHtml(r.category)}</span></td>
          <td><code>${escapeHtml(r.conditionExpression)}</code></td>
          <td><span class="badge bg-danger">+${r.riskScoreWeight}</span></td>
          <td><span class="badge bg-info text-dark">${escapeHtml(r.actionToTake)}</span></td>
          <td><span class="badge bg-success">Active</span></td>
        </tr>
    `).join("");
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
