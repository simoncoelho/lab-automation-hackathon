from mcp.server.fastmcp import FastMCP
import sys

mcp = FastMCP("Robot Arm")

plate_locations = {
    "plates": {
        "1": {"device_id": "opentrons", "slot": "3"},
        "2": {"device_id": "opentrons", "slot": "4"},
    }
}

@mcp.tool()
def get_plate_location(plate_id: str) -> dict:
    """Get the location of a plate"""
    return plate_locations["plates"][plate_id]

@mcp.tool()
def set_plate_location(plate_id: str, device_id: str, slot: str) -> dict:
    """Set the location of a plate"""
    log_message = f"Setting plate {plate_id} to device {device_id} slot {slot}"
    ui_message = f"Set plate {plate_id} to device {device_id} slot {slot}"
    print(log_message, file=sys.stderr, flush=True)
    plate_locations["plates"][plate_id] = {"device_id": device_id, "slot": slot}
    return {"status": "success", "message": ui_message}

if __name__ == "__main__":
    mcp.run()