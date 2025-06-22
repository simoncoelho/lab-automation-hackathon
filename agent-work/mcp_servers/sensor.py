from mcp.server.fastmcp import FastMCP
import sys

mcp = FastMCP("Sensor")

@mcp.tool()
def get_sensor_data(slot: str) -> dict:
    """Get sensor data.  The plate must be in the specified slot on the device."""
    log_message = f"Getting sensor data from slot {slot}"
    ui_message = f"Retrieved sensor data from slot {slot}"
    print(log_message, file=sys.stderr, flush=True)
    return {"status": "success", "data": "beep boop", "message": ui_message}

@mcp.tool()
def get_sensor_id() -> str:
    """Get the id of the sensor device."""
    return "sensor"

@mcp.tool()
def get_sensor_slots() -> list[str]:
    """Get the slots of the device."""
    return ["a", "b"]

if __name__ == "__main__":
    mcp.run()