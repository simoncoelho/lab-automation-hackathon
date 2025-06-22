using ModelContextProtocol.Server;
using robot_arm_mcp;
using robot_arm_mcp.Helpers;
using System.ComponentModel;
using System.Net.NetworkInformation;
using Microsoft.Extensions.DependencyInjection;

[McpServerToolType]
public static class PickAndPlaceTool
{
    private static RobotController GetController(IServiceProvider? provider, RobotController? controller)
    {
        if (controller != null) return controller;
        if (provider != null)
        {
            var resolved = provider.GetService(typeof(RobotController)) as RobotController;
            if (resolved != null) return resolved;
        }
        throw new InvalidOperationException("RobotController could not be resolved.");
    }

    [McpServerTool, Description("Connect to the robot.")]
    public static string StartUpRobot(IServiceProvider provider, RobotController? controller = null)
    {
        var ctrl = GetController(provider, controller);
        ctrl.ConnectToRobot();
        return "Connected to robot.";
    }

    [McpServerTool, Description("Opens gripper, Moves to a safe location, Picks from Location A, Closes the gripper, Places at Location B, Opens the Gripper")]
    public static string PickAndPlace(IServiceProvider provider, JointPosition jointALocation, JointPosition jointBLocation, RobotController? controller = null)
    {
        var ctrl = GetController(provider, controller);
        ctrl.OpenGripper();
        ctrl.MoveToSafeLocation();
        ctrl.MoveToJointPositions(jointALocation);
        ctrl.MoveToSafeLocation();
        ctrl.MoveToJointPositions(jointBLocation);
        return "Robot moved plate.";
    }

    [McpServerTool, Description("Gets the current joint positions for the robot.")]
    public static JointPosition GetCurrentJointPositions(IServiceProvider provider, RobotController? controller = null)
    {
        var ctrl = GetController(provider, controller);
        // Replace with actual logic if available
        return new JointPosition("0", "0", "0", "0", "0");
    }

    [McpServerTool, Description("Open the gripper.")]
    public static string OpenGripper(IServiceProvider provider, RobotController? controller = null)
    {
        var ctrl = GetController(provider, controller);
        ctrl.OpenGripper();
        return "Gripper opened.";
    }

    [McpServerTool, Description("Close the gripper.")]
    public static string CloseGripper(IServiceProvider provider, RobotController? controller = null)
    {
        var ctrl = GetController(provider, controller);
        ctrl.CloseGripper();
        return "Gripper closed.";
    }

    [McpServerTool, Description("Move the robot to a safe location.")]
    public static string MoveToSafeLocation(IServiceProvider provider, RobotController? controller = null)
    {
        var ctrl = GetController(provider, controller);
        ctrl.MoveToSafeLocation();
        return "Robot moved to safe location.";
    }
}