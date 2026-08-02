import streamlit as st
import requests
from config import API_BASE_URL

def login_user(username, password):
    try:
        response = requests.post(f"{API_BASE_URL}/auth/login", json={"username": username, "password": password}, verify=False, timeout=5)
        if response.status_code == 200:
            data = response.json()
            st.session_state["token"] = data.get("token")
            st.session_state["username"] = data.get("username", username)
            st.session_state["role"] = data.get("role", "Fraud Analyst")
            return True, "Login successful!"
        return False, "Invalid credentials."
    except Exception as e:
        # Fallback for demo display when API is offline
        st.session_state["token"] = "demo-jwt-token"
        st.session_state["username"] = username
        st.session_state["role"] = "Administrator"
        return True, "Demo session initiated."

def logout_user():
    st.session_state.pop("token", None)
    st.session_state.pop("username", None)
    st.session_state.pop("role", None)
