const API_BASE_URL = window.EFRS_API_BASE_URL || "https://localhost:5001/api";

document.getElementById("loginForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const username = document.getElementById("username").value.trim();
    const password = document.getElementById("password").value;
    const formError = document.getElementById("formError");
    const usernameError = document.getElementById("usernameError");
    const passwordError = document.getElementById("passwordError");

    formError.textContent = "";
    usernameError.textContent = "";
    passwordError.textContent = "";

    let valid = true;
    if (!username) { usernameError.textContent = "Username is required."; valid = false; }
    if (!password) { passwordError.textContent = "Password is required."; valid = false; }
    if (!valid) return;

    setLoading(true);

    try {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, password })
        });

        if (!response.ok) {
            const body = await safeJson(response);
            formError.textContent = body?.error || "Invalid username or password.";
            setLoading(false);
            return;
        }

        const data = await response.json();
        localStorage.setItem("efrs_token", data.token);
        localStorage.setItem("efrs_username", data.username);
        localStorage.setItem("efrs_role", data.role);

        window.location.href = "dashboard.html";
    } catch (err) {
        formError.textContent = "Could not reach the server. Please try again.";
        setLoading(false);
    }
});

function setLoading(isLoading) {
    document.getElementById("loginBtn").disabled = isLoading;
    document.getElementById("loginBtnText").classList.toggle("d-none", isLoading);
    document.getElementById("loginSpinner").classList.toggle("d-none", !isLoading);
}

async function safeJson(response) {
    try { return await response.json(); } catch { return null; }
}
