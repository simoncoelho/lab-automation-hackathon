using ModelContextProtocol.Server;
using MyRobotMCP;
using System;
using System.ComponentModel;
using System.Text.Json;

namespace MyRobotMCP;

[McpServerToolType]
public static class MonkeyTools
{
    [McpServerTool, Description("Get a list of monkeys.")]
    public static string GetMonkeys(MonkeyService monkeyService)
    {
        var monkeys = monkeyService.GetMonkeys().GetAwaiter().GetResult();
        return JsonSerializer.Serialize(monkeys);
    }

    [McpServerTool, Description("Get a monkey by name.")]
    public static string GetMonkey(MonkeyService monkeyService, [Description("The name of the monkey to get details for")] string name)
    {
        var monkey = monkeyService.GetMonkey(name).GetAwaiter().GetResult();
        return JsonSerializer.Serialize(monkey);
    }
}