def opentrons_handler(error_msg: str) -> dict:
    """
    Simple Opentrons error handler
    """
    error_msg = error_msg.lower()
    if "aspirate" in error_msg:
        return {"status": "retry", "action": "repeat_aspirate"}
    elif "no pipette" in error_msg:
        return {"status": "halt", "message": "Check pipette mount or calibration"}
    else:
        return {"status": "log", "message": error_msg}
