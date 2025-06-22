from mcp.server.fastmcp import FastMCP
import sys

mcp = FastMCP("Opentrons Flex Robot")

@mcp.tool()
def run_pcr_process(slot: str) -> dict:
    """Run an PCR process.  The plate must be in the specified slot on the device."""
    print(f"Running PCR process on slot {slot}", file=sys.stderr, flush=True)
    return {"status": "success"}

@mcp.tool()
def get_ot_id() -> str:
    """Get the id of the opentrons device."""
    return "opentrons"

@mcp.tool()
def get_ot_slots() -> list[str]:
    """Get the slots of the device."""
    return ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10"]


if __name__ == "__main__":
    mcp.run()