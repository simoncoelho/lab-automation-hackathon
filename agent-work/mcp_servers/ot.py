from mcp.server.fastmcp import FastMCP

mcp = FastMCP("Opentrons Flex Robot")

@mcp.tool()
def run_pcr_process() -> dict:
    """Run an PCR process."""
    return {"status": "success"}

if __name__ == "__main__":
    mcp.run()