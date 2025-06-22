from mcp.server.fastmcp import FastMCP
import sys

mcp = FastMCP("Robot Arm")

@mcp.tool()
def move_plate(from_device_id: str, from_slot: str, to_device_id: str, to_slot: str) -> dict:
    """Move a plate from one slot on one device to another slot maybe on this device, maybe on another device."""
    print(f"Moving plate from device {from_device_id} slot {from_slot} to device {to_device_id} slot {to_slot}", file=sys.stderr, flush=True)
    return {"status": "success"}

if __name__ == "__main__":
    mcp.run()