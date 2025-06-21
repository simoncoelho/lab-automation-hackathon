import asyncio, os
from agents import Agent, Runner
from agents.mcp.server import MCPServerStdio   # stdio • also see MCPServerSse / StreamableHTTP

async def main():
    arm_server = MCPServerStdio(
            name="arm",
            params={
                "command": "python",
                "args": ["mcp_servers/arm.py"],
            },
            cache_tools_list=True,
        )
    ot_server = MCPServerStdio(
            name="ot",
            params={
                "command": "python",
                "args": ["mcp_servers/ot.py"],
            },
            cache_tools_list=True,
        )
    async with arm_server as arm_server, ot_server as ot_server:
        lab_manager = Agent(
            name="Lab Manager",
            instructions=(
                "You are a lab manager. "
                "Whenever it helps, call the arm and ot tools to help the user."
            ),
            mcp_servers=[arm_server, ot_server],
        )

        result = await Runner.run(
            lab_manager,
            input="Move a plate from slot 1 to slot 2 and run a pcr process."
        )
        print("\nFINAL ANSWER ↴\n", result.final_output)

if __name__ == "__main__":
    asyncio.run(main())