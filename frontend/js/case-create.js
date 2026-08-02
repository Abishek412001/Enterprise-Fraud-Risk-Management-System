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
    document.getElementById("createCaseForm")?.addEventListener("submit", async (e) => {
        e.preventDefault();

        const customerId = parseInt(document.getElementById("customerSelect").value);
        const caseType = document.getElementById("caseType").value;
        const priority = document.getElementById("priority").value;
        const caseTitle = document.getElementById("caseTitle").value.trim();
        const caseDescription = document.getElementById("caseDescription").value.trim();

        const payload = {
            customerID: customerId,
            caseType: caseType,
            priority: priority,
            severity: priority,
            caseTitle: caseTitle,
            caseDescription: caseDescription
        };

        const res = await authFetch("/cases", {
            method: "POST",
            body: JSON.stringify(payload)
        });

        if (res && res.caseID) {
            alert(`Fraud Case ${res.caseNumber} created successfully!`);
            window.location.href = `case-details.html?id=${res.caseID}`;
        }
    });
});
