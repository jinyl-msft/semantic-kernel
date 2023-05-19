// Copyright (c) Microsoft. All rights reserved.

namespace CEDeviceChat
{
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.CoreSkills;
    using Microsoft.SemanticKernel.Orchestration;
    
    internal class SKPlannerDemo
    {
        public static void RunAsync()
        {
            var kernel = Kernel.Builder.Build();

            // Azure OpenAI
            kernel.Config.AddAzureTextCompletionService(
            "davinci-azure",                     // Alias used by the kernel
            "Azure OpenAI Deployment Name",          // Azure OpenAI Deployment Name
            "Azure OpenAI Endpoint", // Azure OpenAI Endpoint
            "Azure OpenAI Key"        // Azure OpenAI Key
        );

            // Alternative using OpenAI
            // kernel.Config.AddOpenAITextCompletionService("davinci-openai",
            //     "text-davinci-003",               // OpenAI Model name
            //     "...your OpenAI API Key..."       // OpenAI API Key
            // );

            // Create skills needed for the plan
            var brainstormTemplate = @"
            Must: brainstorm ideas and create a list.
            Must: use a numbered list.
            Must: only one list.
            Must: end list with ##END##
            Should: no more than 5 items.
            Should: at least 3 items.
            Topic: {{$INPUT}}";                ;

            var brainstormSkill = kernel.CreateSemanticFunction(brainstormTemplate, "Brainstorm", "BrainstormSkill", maxTokens: 2000);

            var xmlStoryTemplate = @"
            Tell a story about {{$input}} using XML format:
            ONLY USE XML TAGS IN THIS LIST: 
            [XML ELEMENT TAG LIST]
            time
            place
            characters
            plot
            [END LIST]

            EMIT WELL FORMED XML ALWAYS. .";

            var storytellerSkill = kernel.CreateSemanticFunction(xmlStoryTemplate, "XmlStoryteller", "StorytellerSkill", maxTokens: 2000);

            var ask = @"I want to know some famous characters of cartoon movies and write a story about all of them.";

            // Create a plan with available skills
            /*var planner = kernel.ImportSkill(new PlannerSkill(kernel));
            var plan = await kernel.RunAsync(ask, planner["CreatePlan"]);

            Console.WriteLine("The plan is:\n");
            Console.WriteLine(plan.Variables.ToPlan().PlanString + "\n");

            int step = 1;
            int maxSteps = 10;
            while (!plan.Variables.ToPlan().IsComplete && step < maxSteps)
            {
                var results = await kernel.RunAsync(plan.Variables, planner["ExecutePlan"]);
                if (results.Variables.ToPlan().IsSuccessful)
                {
                    Console.WriteLine($"Step {step} - Plan execution results:");
                    Console.WriteLine(results.Variables.ToPlan().PlanString);

                    if (results.Variables.ToPlan().IsComplete)
                    {
                        Console.WriteLine($"Step {step} - Plan completed!\n");
                        Console.WriteLine(results.Variables.ToPlan().Result);
                        break;
                    }
                }
                else
                {
                    Console.WriteLine($"Step {step} - Plan execution failed:");
                    Console.WriteLine(results.Variables.ToPlan().Result);
                    break;
                }

                plan = results;
                step++;
                Console.WriteLine("");
            }*/
        }
    }
}
