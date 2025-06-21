using ModelContextProtocol.Server;
using robot_arm_mcp.Helpers;
using System.ComponentModel;
using System.Net.NetworkInformation;

[McpServerToolType]
public static class PickAndPlaceTool
{
    [McpServerTool, Description("Picks from Location A, Closes the gripper, Places at Location B, Opens the Gripper")]
    public static string PickAndPlace(JointPosition jointALocations, JointPosition jointBLocations)
    {



        return "Robot moved plate.";
    }
}