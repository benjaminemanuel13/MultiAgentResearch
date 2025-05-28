
using Microsoft.SemanticKernel.Agents.Runtime.Core;
using MultiAgentResearch.Business.Services;

OrchestrationService.OpenAIKey = Environment.GetEnvironmentVariable("OPENAIKEY");

var svc = new OrchestrationService();

string input = "Create a slogon for a new eletric SUV that is affordable and fun to drive.";
input += "And email the result using the EmailPlugin plugin to john doe from jane smith, set the subject and body accordingly.";

await svc.GroupChatWithHumanAsync(input);

Console.WriteLine("Done...  Press any key to exit.");
Console.ReadKey(true);