# error_notifier_server.py
import os
import smtplib
from email.message import EmailMessage
from mcp.server.fastmcp import FastMCP
import sys
mcp = FastMCP("Error Notifier")

# === CONFIGURATION ===
SMTP_SERVER = "smtp.gmail.com"
SMTP_PORT = 587
EMAIL_TO = os.getenv("EMAIL_TO", "no_email")
EMAIL_FROM = os.getenv("EMAIL_FROM", "no_email")
EMAIL_PASSWORD = os.getenv("EMAIL_PASSWORD", "no_password")

@mcp.tool()
def notify_error(error_message: str, email_to: str = EMAIL_TO) -> dict:
    """Send an email notification if a system error occurs."""
    log_message = f"Sending error email to {email_to} with message: {error_message}"
    print(log_message, file=sys.stderr, flush=True)
    try:
        msg = EmailMessage()
        msg["Subject"] = "System Error Alert"
        msg["From"] = EMAIL_FROM
        msg["To"] = email_to
        msg.set_content(f"A system error occurred:\n\n{error_message}")

        with smtplib.SMTP(SMTP_SERVER, SMTP_PORT) as server:
            server.starttls()
            server.login(EMAIL_FROM, EMAIL_PASSWORD)
            server.send_message(msg)

        ui_message = f"Sent error email to {email_to}"
        return {"status": "success", "message": ui_message}
    except Exception as e:
        error_log = f"Error sending error email: {e}"
        print(error_log, file=sys.stderr, flush=True)
        ui_message = f"Failed to send error email: {str(e)}"
        return {"status": "failure", "message": ui_message}

if __name__ == "__main__":
    mcp.run()
