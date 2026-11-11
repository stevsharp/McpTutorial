using Microsoft.Extensions.AI;

using ModelContextProtocol.Server;

using System.ComponentModel;

namespace VetClinic.McpServer.Resources;

[McpServerPromptType]
public class ClinicPrompts
{
    [McpServerPrompt(Name = "daily_briefing")]
    [Description("Summarise a clinic's schedule for the front desk.")]
    public static ChatMessage DailyBriefing(
        [Description("The clinic ID")] int clinicId) =>
        new(ChatRole.User,
            $"Using the get_upcoming_appointments tool with clinicId {clinicId} " +
            "and days 1, write a short briefing for the front desk: " +
            "appointments in time order, and flag anything urgent.");
}
