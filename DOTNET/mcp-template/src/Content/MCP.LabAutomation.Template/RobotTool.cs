using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using robot_arm_mcp; // Adjust namespace as needed

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Initialize robot arm.")]
    public static string InitializeRobot(RobotController controller)
    {
        try
        {
            controller.InitializeRobotArm();
        }
        catch (Exception ex)
        {
            return $"Error initializing robot arm: {ex.Message}";
        }

        return "Robot arm initialized successfully.";
    }

    [McpServerTool, Description("Echoes the current full string.")]
    public static double[] GetCurrentPosition(RobotController controller)
    {
        return controller.GetCurrentPosition();
    }

    [McpServerTool, Description("Moves the robot arm to a specified position.")]
    public static string MoveToPosition(
        double j1,
        double j2,
        double j3,
        double j4,
        double speed,
        double accel,
        double extAxis,
        RobotController controller)
    {
        controller.MoveToPosition(j1, j2, j3, j4, speed, accel, MoveKind.Joint, extAxis);
        return $"Moved to position: J1={j1}, J2={j2}, J3={j3}, J4={j4}, Speed={speed}, Accel={accel}, ExtAxis={extAxis}" +
               $"Current Position: {JsonSerializer.Serialize(controller.GetCurrentPosition())}";
    }


    [McpServerTool, Description("Opens the gripper of the robot arm.")]
    public static string OpenGripper(RobotController controller)
    {
        try
        {
            controller.OpenGripper();
        }
        catch (Exception ex)
        {
            return $"Error opening gripper: {ex.Message}";
        }
        return "Opened gripper successfully.";
    }

    [McpServerTool, Description("Closes the gripper of the robot arm.")]
    public static string CloseGripper(RobotController controller)
    {
        try
        {
            controller.CloseGripper();
        }
        catch (Exception ex)
        {
            return $"Error closing gripper: {ex.Message}";
        }
        return "Closed gripper successfully.";
    }
}

