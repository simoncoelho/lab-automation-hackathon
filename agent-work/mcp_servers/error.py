from mcp.server.fastmcp import FastMCP

mcp = FastMCP("Error Reporter")

@mcp.tool()
def report_error(error_message: str) -> dict:
    """Report an error."""
    # TODO: Implement the logic to report the error
    print(f"Error: {error_message}")
    return {"status": "success"}

if __name__ == "__main__":
    mcp.run()