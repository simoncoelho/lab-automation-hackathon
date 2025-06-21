import asyncio, os
from agents import Agent, Runner
from agents.mcp.server import MCPServerStdio   # stdio • also see MCPServerSse / StreamableHTTP

async def main():
    # 2-a) connect to the joke MCP server –
    #      The context-manager handles startup, shutdown, and automatic tracing
    async with MCPServerStdio(
        name="local_jokes",
        params={
            "command": "python",
            "args": ["mcp_servers/joke_server.py"],  # path relative to cwd
        },
        cache_tools_list=True,          # skip list_tools() on every run (optional) :contentReference[oaicite:0]{index=0}
    ) as joke_server:

        # 2-b) define an Agent that knows about that server’s tools
        assistant = Agent(
            name="Joke Assistant",
            instructions=(
                "You are a witty assistant. "
                "Whenever it helps, call the Chuck Norris joke tool to spice things up."
            ),
            mcp_servers=[joke_server],  # this is all it takes :contentReference[oaicite:1]{index=1}
        )

        # 2-c) run the agent loop once
        result = await Runner.run(
            assistant,
            input="Tell me a joke about unit testing."
        )
        print("\nFINAL ANSWER ↴\n", result.final_output)

if __name__ == "__main__":
    asyncio.run(main())