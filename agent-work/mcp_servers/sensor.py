from mcp.server.fastmcp import FastMCP

mcp = FastMCP("Sensor")

@mcp.tool()
def get_sensor_data() -> dict:
    """Get sensor data."""
    return {"status": "success", "data": "beep boop"}

if __name__ == "__main__":
    mcp.run()