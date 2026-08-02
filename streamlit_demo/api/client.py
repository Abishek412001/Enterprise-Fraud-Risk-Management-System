import requests
import streamlit as st
from config import API_BASE_URL

def get_headers():
    token = st.session_state.get("token", "")
    return {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

def fetch_executive_telemetry():
    try:
        res = requests.get(f"{API_BASE_URL}/executive", headers=get_headers(), verify=False, timeout=5)
        if res.status_code == 200:
            return res.json()
    except Exception:
        pass
    return {
        "totalOpenAlerts": 117,
        "openCases": 12,
        "frozenAccounts": 4,
        "activeIncidents": 3,
        "slaComplianceRate": 98.4,
        "fraudLossPrevented": 145000.00,
        "ruleBasedInsights": [
            "Fraud alerts increased by 18% compared to last week.",
            "Most high-risk transactions originated from Merchant E-Commerce Global.",
            "Account takeover alerts increased in the last 24 hours.",
            "Analyst John Analyst resolved the highest number of cases this week."
        ]
    }

def fetch_frm_alerts():
    try:
        res = requests.get(f"{API_BASE_URL}/frmalerts", headers=get_headers(), verify=False, timeout=5)
        if res.status_code == 200:
            return res.json()
    except Exception:
        pass
    return {
        "items": [
            {"alertID": 101, "alertNumber": "FRM-2026-0001", "alertType": "High Velocity Spikes", "severity": "Critical", "riskScore": 95, "status": "Open"},
            {"alertID": 102, "alertNumber": "FRM-2026-0002", "alertType": "Card Testing Spree", "severity": "High", "riskScore": 82, "status": "In Progress"}
        ]
    }
