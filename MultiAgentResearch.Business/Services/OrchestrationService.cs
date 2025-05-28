using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MultiAgentResearch.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace MultiAgentResearch.Business.Services
{
    public class OrchestrationService
    {
        public static string OpenAIKey { get; set; }
        public async Task GroupChatWithHumanAsync(string input)
        {
            var services = new ServiceCollection();
            services.AddOpenAIChatCompletion(modelId: "gpt-4",
                    apiKey: OpenAIKey);

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var email = KernelPluginFactory.CreateFromType<EmailPlugin>();
            var lookup = KernelPluginFactory.CreateFromType<StaffLookupPlugin>();

            KernelPluginCollection plugins = new KernelPluginCollection();
            plugins.Add(email);
            plugins.Add(lookup);

            var kernel = new Kernel(serviceProvider, plugins);

            ChatCompletionAgent writer =
                new ChatCompletionAgent
                {
                    Name = "CopyWriter",
                    Description = "A copy writer",
                    Instructions = """
                    You are a copywriter with ten years of experience and are known for brevity and a dry humor.
                    The goal is to refine and decide on the single best copy as an expert in the field.
                    Only provide a single proposal per response.
                    You're laser focused on the goal at hand.
                    Don't waste time with chit chat.
                    Consider suggestions when refining an idea.
                    """,
                    Kernel = kernel,
                    Arguments = new KernelArguments(
                    new OpenAIPromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                        Temperature = 0,
                    }),
                };
            ChatCompletionAgent editor =
                new ChatCompletionAgent
                {
                    Name = "Reviewer",
                    Description = "An editor.",
                    Instructions = """
                        You are an art director who has opinions about copywriting born of a love for David Ogilvy.
                        The goal is to determine if the given copy is acceptable to print.
                        If so, state that it is approved.
                        If not, provide insight on how to refine suggested copy without example.
                        """,
                    Kernel = kernel,
                    Arguments = new KernelArguments(
                    new OpenAIPromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                        Temperature = 0,
                    }),
                };

            // Define the orchestration
#pragma warning disable CS0612 // Type or member is obsolete
            GroupChatOrchestration orchestration =
                new(
                    new CustomRoundRobinGroupChatManager()
                    {
                        MaximumInvocationCount = 5,
                        InteractiveCallback = InteractiveCallback
                    },
                    writer,
                    editor)
                {
                    //LoggerFactory = this.loggerFactory
                };
#pragma warning restore CS0612 // Type or member is obsolete

            // Start the runtime
            InProcessRuntime runtime = new();
            await runtime.StartAsync();

            // Run the orchestration
            Console.WriteLine($"\n# INPUT: {input}\n");
            OrchestrationResult<string> result = await orchestration.InvokeAsync(input, runtime);
            string text = await result.GetValueAsync(TimeSpan.FromSeconds(30));
            Console.WriteLine($"\n# RESULT: {text}");

            await runtime.RunUntilIdleAsync();
        }

        ValueTask<ChatMessageContent> InteractiveCallback()
        {
            ChatMessageContent input = new(AuthorRole.User, "I like it");
            Console.WriteLine($"\n# INPUT: {input.Content}\n");
            return ValueTask.FromResult(input);
        }

        /// <summary>
        /// Define a custom group chat manager that enables user input.
        /// </summary>
        /// <remarks>
        /// User input is achieved by overriding the default round robin manager
        /// to allow user input after the reviewer agent's message.
        /// </remarks>
        private sealed class CustomRoundRobinGroupChatManager : RoundRobinGroupChatManager
        {
            public override ValueTask<GroupChatManagerResult<bool>> ShouldRequestUserInput(ChatHistory history, CancellationToken cancellationToken = default)
            {
                string? lastAgent = history.LastOrDefault()?.AuthorName;

                Console.Write(lastAgent + ":" + " " + history.LastOrDefault()?.Content + "\n");

                if (lastAgent is null)
                {
                    return ValueTask.FromResult(new GroupChatManagerResult<bool>(false) { Reason = "No agents have spoken yet." });
                }

                if (lastAgent == "Reviewer")
                {
                    return ValueTask.FromResult(new GroupChatManagerResult<bool>(true) { Reason = "User input is needed after the reviewer's message." });
                }

                return ValueTask.FromResult(new GroupChatManagerResult<bool>(false) { Reason = "User input is not needed until the reviewer's message." });
            }
        }
    }
}

#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.