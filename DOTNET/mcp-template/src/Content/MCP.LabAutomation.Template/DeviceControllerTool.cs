using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using MCP.LabAutomation; // Adjust namespace as needed

[McpServerToolType]
public static class DeviceControllerTool
{
    [McpServerTool, Description("Initialize device.")]
    public static string InitializeDevice(DeviceController controller)
    {
        try
        {
            controller.InitializeDevice();
        }
        catch (Exception ex)
        {
            return $"Error initializing robot arm: {ex.Message}";
        }

        return "Robot arm initialized successfully.";
    }

    [McpServerTool, Description("Moves the device to a specified position.")]
    public static string RunMeasurement(DeviceController controller, string measurementType)
    {
        try
        {
            controller.RunMeasurement(measurementType);
        }
        catch (Exception ex)
        {
            return $"Error running measurement: {ex.Message}";
        }
        return $"Measurement '{measurementType}' completed successfully.";
    }

    [McpServerTool, Description("Gets the current status of the device.")]
    public static string GetStatus(DeviceController controller)
    {
        return controller.GetDeviceStatus();
    }

    [McpServerTool, Description("Opens the door of the device.")]
    public static void OpenDoor(DeviceController controller)
    {
        controller.OpenDoor();
    }

    [McpServerTool, Description("Closes the door of the device.")]
    public static void CloseDoor(DeviceController controller)
    {
        controller.CloseDoor();
    }
}

