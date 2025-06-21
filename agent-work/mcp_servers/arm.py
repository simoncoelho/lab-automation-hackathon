from mcp.server.fastmcp import FastMCP

mcp = FastMCP("Robot Arm")

@mcp.tool()
def move_plate(from_device: str, from_slot: str, to_device: str, to_slot: str) -> dict:
    """Move a plate from one slot to another."""
    return {"status": "success"}

if __name__ == "__main__":
    mcp.run()