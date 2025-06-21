# joke_server.py  --  < 20 lines, pure Python >
import asyncio
import requests
from mcp.server.fastmcp import FastMCP

mcp = FastMCP("Joke Server")
# Add an addition tool
@mcp.tool()
def get_joke() -> dict:
    """Fetch a random joke from the public API."""
    text = requests.get("https://api.chucknorris.io/jokes/random").json()["value"]
    return {"joke": text}

if __name__ == "__main__":
    # one-line bootstrap to talk over STDIO
    mcp.run()