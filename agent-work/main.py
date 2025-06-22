import asyncio, os
from agents import Agent, Runner
from agents.mcp.server import MCPServerStdio   # stdio • also see MCPServerSse / StreamableHTTP
from agents import enable_verbose_stdout_logging

async def main():
    #enable_verbose_stdout_logging()
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
    sensor_server = MCPServerStdio(
            name="sensor",
            params={
                "command": "python",
                "args": ["mcp_servers/sensor.py"],
            },
            cache_tools_list=True,
        )
    error_server = MCPServerStdio(
            name="error",
            params={
                "command": "python",
                "args": ["mcp_servers/error.py"],
            },
            cache_tools_list=True,
        )
    async with arm_server as arm_server, ot_server as ot_server, sensor_server as sensor_server, error_server as error_server:
        lab_manager = Agent(
            name="Lab Manager",
            instructions=(
                "You are a lab manager. "
                "Whenever it helps, call the tools to help the user. "
                "Use the device id on the tools to identify the device."
                "If you need to know the slots on a device, use the device://<device_id>/slots resource."
                "If you need to know the id of a device, use the device://<device_id>/id resource."
                "Verify the id and slots of the device before using the tools."
            ),
            mcp_servers=[arm_server, ot_server, sensor_server, error_server],
        )

        result = await Runner.run(
            lab_manager,
            input="The plate is initially in slot 1 on the opentrons device.  Run a pcr process on the opentrons device, then move the plate to the sensor device and analyze the results."
            #input="What slots are available on the devices?"
        )
        print("\nFINAL ANSWER ↴\n", result.final_output)

if __name__ == "__main__":
    asyncio.run(main())