def byonoy_handler(error_msg: str) -> dict:
    """
    Example Byonoy error handler
    """
    error_msg = error_msg.lower()
    if "read failure" in error_msg:
        return {"status": "retry", "action": "retry_read"}
    elif "plate not detected" in error_msg:
        return {"status": "halt", "message": "Ensure plate is properly loaded"}
    else:
        return {"status": "log", "message": error_msg}
