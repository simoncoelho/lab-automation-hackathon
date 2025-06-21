def cephla_handler(error_msg: str) -> dict:
    """
    Example Cephla imaging error handler
    """
    error_msg = error_msg.lower()
    if "camera not found" in error_msg:
        return {"status": "halt", "message": "Check camera connection"}
    elif "focus error" in error_msg:
        return {"status": "adjust", "action": "refocus"}
    else:
        return {"status": "log", "message": error_msg}
