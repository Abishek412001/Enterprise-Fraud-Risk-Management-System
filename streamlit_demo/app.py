import streamlit as st
import pandas as pd
import plotly.express as px
from config import PAGE_TITLE, PAGE_ICON
from authentication import login_user, logout_user
from api.client import fetch_executive_telemetry, fetch_frm_alerts

st.set_page_config(page_title=PAGE_TITLE, page_icon=PAGE_ICON, layout="wide")

if "token" not in st.session_state:
    st.title("🛡️ Enterprise Fraud Risk System (EFRS)")
    st.subheader("Interactive Portfolio & Demonstration Portal")

    col1, col2 = st.columns([1, 1])

    with col1:
        st.markdown("### Executive Demo Authentication")
        username = st.text_input("Username", value="admin")
        password = st.text_input("Password", type="password", value="Admin123!")
        if st.button("Authenticate Session"):
            ok, msg = login_user(username, password)
            if ok:
                st.success(msg)
                st.rerun()
            else:
                st.error(msg)
    with col2:
        st.info("""
        **Platform Architecture**:
        - **Backend**: ASP.NET Core 8 Web API
        - **Database**: Microsoft SQL Server 2022
        - **Security**: JWT Bearer Auth & 9 RBAC Roles
        - **DevOps**: Docker, Nginx, GitHub Actions
        """)
else:
    st.sidebar.title(f"👤 {st.session_state.get('username')}")
    st.sidebar.caption(f"Role: {st.session_state.get('role')}")
    if st.sidebar.button("Logout"):
        logout_user()
        st.rerun()

    st.title("🛡️ Fraud Operations & C-Suite Executive Telemetry")

    telemetry = fetch_executive_telemetry()

    c1, c2, c3, c4 = st.columns(4)
    c1.metric("Open Alerts", telemetry.get("totalOpenAlerts", 0), delta="18%")
    c2.metric("Open Cases", telemetry.get("openCases", 0))
    c3.metric("SLA Compliance", f"{telemetry.get('slaComplianceRate', 98.4)}%")
    c4.metric("Loss Prevented", f"${telemetry.get('fraudLossPrevented', 145000):,.2f}")

    st.markdown("---")

    st.subheader("💡 Automated Telemetry & AI Business Insights")
    for insight in telemetry.get("ruleBasedInsights", []):
        st.info(f"👉 {insight}")

    st.markdown("---")

    col_left, col_right = st.columns(2)

    with col_left:
        st.subheader("FRM Alerts Real-Time Queue")
        alerts_data = fetch_frm_alerts().get("items", [])
        st.dataframe(pd.DataFrame(alerts_data), use_container_width=True)

    with col_right:
        st.subheader("Fraud Loss Distribution by Merchant")
        df_chart = pd.DataFrame({
            "Merchant": ["CryptoExchange X", "LuxJewelry Inc", "FastPay Transfer", "Global Retail"],
            "LossPrevented": [45000, 32000, 28000, 15000]
        })
        fig = px.bar(df_chart, x="Merchant", y="LossPrevented", color="Merchant")
        st.plotly_chart(fig, use_container_width=True)
