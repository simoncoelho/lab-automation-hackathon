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
EMAIL_FROM = "dwelte@gmail.com"
EMAIL_PASSWORD = os.getenv("EMAIL_PASSWORD", "no_password")

@mcp.tool()
def notify_error(error_message: str, email_to: str = "dwelte@readysetpotato.com") -> dict:
    """Send an email notification if a system error occurs."""
    print(f"Sending error email to {email_to} with message: {error_message}", file=sys.stderr, flush=True)
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

        return {"status": "success", "message": f"Alert email sent to {email_to}."}
    except Exception as e:
        print(f"Error sending error email: {e}", file=sys.stderr, flush=True)
        return {"status": "failure", "message": str(e)}

if __name__ == "__main__":
    mcp.run()
