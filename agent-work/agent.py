import asyncio, os
from agents import Agent, Runner
from agents.mcp.server import MCPServerStdio   # stdio • also see MCPServerSse / StreamableHTTP
from agents import enable_verbose_stdout_logging

def create_mcp_servers():
    """Create and return all MCP servers for the lab automation system."""
    return [
        MCPServerStdio(
            name="arm",
            params={
                "command": "python",
                "args": ["mcp_servers/arm.py"],
            },
            cache_tools_list=True,
        ),
        MCPServerStdio(
            name="ot",
            params={
                "command": "python",
                "args": ["mcp_servers/ot.py"],
            },
            cache_tools_list=True,
        ),
        MCPServerStdio(
            name="sensor",
            params={
                "command": "python",
                "args": ["mcp_servers/sensor.py"],
            },
            cache_tools_list=True,
        ),
        MCPServerStdio(
            name="error",
            params={
                "command": "python",
                "args": ["mcp_servers/error.py"],
            },
            cache_tools_list=True,
        ),
        MCPServerStdio(
            name="lab_status",
            params={
                "command": "python",
                "args": ["mcp_servers/lab_status.py"],
            },
            cache_tools_list=True,
        ),
    ]

def get_lab_manager_instructions():
    """Get the instructions for the lab manager agent."""
    return """
* You are a lab manager. 
* Whenever it helps, call the tools to help the user. 
* Use the device id on the tools to identify the device.
* If you need to know the slots on a device, use the get_<device_id>_slots tool.
* If you need to know the id of a device, use the get_<device_id>_id tool.
* Verify the id and slots of the device before using the tools.
* Before operating on a plate, user get_plate_location to find out where the plate is.
* If you need to move a plate, use the move_plate tool and then set the new plate location using the set_plate_location tool.
* If you get an error, use the error tool to report the error using the notify_error tool and halt the process.
* If you don't have a tool to complete a necessary step, use the notify_error tool to report the error and halt the process. Do not be afraid to halt the process.
"""

def create_lab_manager(mcp_servers):
    """Create and return the lab manager agent with the given MCP servers."""
    return Agent(
        name="Lab Manager",
        instructions=get_lab_manager_instructions(),
        mcp_servers=mcp_servers,
    )

async def main():
    #enable_verbose_stdout_logging()
    servers = create_mcp_servers()
    
    async with servers[0] as arm_server, servers[1] as ot_server, servers[2] as sensor_server, servers[3] as error_server, servers[4] as lab_status_server:
        lab_manager = create_lab_manager([arm_server, ot_server, sensor_server, error_server, lab_status_server])

        # Ask for user input
        user_input = input("Enter your lab automation request: ")
        
        result = await Runner.run(
            lab_manager,
            input=user_input
        )
        print("\nFINAL ANSWER ↴\n", result.final_output)

if __name__ == "__main__":
    asyncio.run(main())