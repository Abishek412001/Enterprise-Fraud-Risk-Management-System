const API_BASE_URL = window.EFRS_API_BASE_URL || "https://localhost:5001/api";

document.getElementById("registerForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const username = document.getElementById("username").value.trim();
    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("password").value;
    const role = document.getElementById("role").value;

    const formError = document.getElementById("formError");
    const usernameError = document.getElementById("usernameError");
    const emailError = document.getElementById("emailError");
    const passwordError = document.getElementById("passwordError");
    [formError, usernameError, emailError, passwordError].forEach(el => el.textContent = "");

    let valid = true;
    if (username.length < 3) { usernameError.textContent = "Username must be at least 3 characters."; valid = false; }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) { emailError.textContent = "Enter a valid email address."; valid = false; }
    if (password.length < 8) { passwordError.textContent = "Password must be at least 8 characters."; valid = false; }
    if (!valid) return;

    setLoading(true);

    try {
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, email, password, role })
        });

        if (!response.ok) {
            const body = await safeJson(response);
            formError.textContent = body?.error || "Registration failed. Please try again.";
            setLoading(false);
            return;
        }

        window.location.href = "login.html";
    } catch (err) {
        formError.textContent = "Could not reach the server. Please try again.";
        setLoading(false);
    }
});

function setLoading(isLoading) {
    document.getElementById("registerBtn").disabled = isLoading;
    document.getElementById("registerBtnText").classList.toggle("d-none", isLoading);
    document.getElementById("registerSpinner").classList.toggle("d-none", !isLoading);
}

async function safeJson(response) {
    try { return await response.json(); } catch { return null; }
}
