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

document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("btnExportCsvExec")?.addEventListener("click", () => downloadFile("/export/csv?type=executive", "Fraud_Executive_Report.csv"));
    document.getElementById("btnExportPdfExec")?.addEventListener("click", () => downloadFile("/export/pdf?type=executive", "Fraud_Executive_Report.pdf"));
    document.getElementById("btnExportCsvOps")?.addEventListener("click", () => downloadFile("/export/csv?type=operations", "Fraud_Operations_Report.csv"));
    document.getElementById("btnExportPdfOps")?.addEventListener("click", () => downloadFile("/export/pdf?type=operations", "Fraud_Operations_Report.pdf"));
    document.getElementById("btnExportCsvAudit")?.addEventListener("click", () => downloadFile("/export/csv?type=audit", "Fraud_Audit_Report.csv"));
    document.getElementById("btnExportPdfAudit")?.addEventListener("click", () => downloadFile("/export/pdf?type=audit", "Fraud_Audit_Report.pdf"));
});

async function downloadFile(path, filename) {
    const response = await fetch(`${API_BASE_URL}${path}`, {
        headers: { "Authorization": `Bearer ${token}` }
    });

    if (response.ok) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
    } else {
        alert("Failed to download report.");
    }
}
