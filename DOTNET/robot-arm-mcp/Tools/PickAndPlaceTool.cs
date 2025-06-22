using ModelContextProtocol.Server;
using robot_arm_mcp;
using robot_arm_mcp.Helpers;
using System.ComponentModel;
using System.Net.NetworkInformation;

[McpServerToolType]
public static class PickAndPlaceTool
{
    [McpServerTool, Description("Opens gripper, Moves to a safe location, Picks from Location A, Closes the gripper, Places at Location B, Opens the Gripper")]
    public static string PickAndPlace(RobotController controller, JointPosition jointALocation, JointPosition jointBLocation)
    {
        // Move to safe location
        controller.OpenGripper();
        controller.MoveToSafeLocation();
        controller.MoveToJointPositions(jointALocation);
        controller.MoveToSafeLocation();
        controller.MoveToJointPositions(jointBLocation);


        return "Robot moved plate.";
    }

    [McpServerTool, Description("Gets the current joint positions for the robot.")]
    public static JointPosition GetCurrentJointPositions()
    {
        Console.WriteLine("Getting joint positions....");

        return new JointPosition("0", "0", "0", "0", "0");
    }



}