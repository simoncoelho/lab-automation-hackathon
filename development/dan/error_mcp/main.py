from error_mcp import ErrorMCP
from opentrons_handler import opentrons_handler
from byonoy_handler import byonoy_handler
from cephla_handler import cephla_handler

mcp = ErrorMCP()
mcp.register_handler("opentrons", opentrons_handler)
mcp.register_handler("byonoy", byonoy_handler)
mcp.register_handler("cephla", cephla_handler)

# Simulate some error messages
print(mcp.handle_error("opentrons", "Aspirate volume too low on channel A2"))
print(mcp.handle_error("byonoy", "Plate not detected"))
print(mcp.handle_error("cephla", "Focus error during scan"))
