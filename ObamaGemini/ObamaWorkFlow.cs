using System.Buffers;
using System.Text;
using Microsoft.Agents.AI.Workflows;
using Khonshu.Enums;
using Khonshu.Interfaces;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Khonshu.ObamaGemini;

public sealed class ObamaWorkFlow : IObamaAi, IHistoryAi
{
    private readonly Workflow workFlow;
    private AIAgent mainWorkFlowAi;
    private AgentSession? session = null;
    private InMemoryChatHistoryProvider? provider = null;

    private ObamaWorkFlow(ObamaAgentWorkMode agetnsMode,  params IObamaAgent[] yourAgents)
    {
        if (yourAgents.Length < 2)
            throw new ArgumentException(message: "Надо больше двух агентов!");
        
        AIAgent[] currentAgents = new AIAgent[yourAgents.Length];
        for (Int32 i = 0; i < yourAgents.Length; i++)
        {
            currentAgents[i] = yourAgents[i].GetAgent();
        }

        switch (agetnsMode)
        {
            case ObamaAgentWorkMode.Sequential:
                workFlow = AgentWorkflowBuilder.BuildSequential(currentAgents);
                break;
            case ObamaAgentWorkMode.Parallel:
                workFlow = AgentWorkflowBuilder.BuildConcurrent(currentAgents);
                break;
            case ObamaAgentWorkMode.Handoff:
                var handoffBuilder = AgentWorkflowBuilder.CreateHandoffBuilderWith(currentAgents[0]);

                for (Int32 i = 0; i < currentAgents.Length; i++)
                {
                    var sourceAgent =  currentAgents[i];
                    var otherAgents = currentAgents.Where((_,index) =>  index != i);
                    handoffBuilder = handoffBuilder.WithHandoffs(sourceAgent, otherAgents);
                }
                workFlow = handoffBuilder.Build();
               break;
            
            case ObamaAgentWorkMode.GroupChat:

                workFlow = AgentWorkflowBuilder.CreateGroupChatBuilderWith(с => new RoundRobinGroupChatManager(с)
                {
                    MaximumIterationCount = 10
                }).AddParticipants(currentAgents)
                    .Build();
                break;
            
            case  ObamaAgentWorkMode.MagneticPower:
                workFlow = AgentWorkflowBuilder.CreateMagenticBuilderWith(currentAgents[0])
                    .WithMaxStalls(3)
                    .AddParticipants(currentAgents.Where((_, index) => index != 0))
                    .Build();
                
                break;
        }

        if (workFlow is not null) mainWorkFlowAi = workFlow.AsAIAgent();
    }

    public static async Task<ObamaWorkFlow> CreateWorkTableAsync(ObamaAgentWorkMode mode, params IObamaAgent[] yourAgents)
    {
        var  creating =  new ObamaWorkFlow(mode, yourAgents);
        await creating.CreateSessionAsync();
        creating.InitMemoryService();
        return creating;
    }

    private void InitMemoryService()
    {
        provider = mainWorkFlowAi.GetService<InMemoryChatHistoryProvider>();
    }
    
    

    public void SetWorkFlow(Workflow newWorkFlow)
    {
        mainWorkFlowAi =  newWorkFlow.AsAIAgent();
    }

    private async Task CreateSessionAsync()
    {
        session ??= await mainWorkFlowAi.CreateSessionAsync();
    }
    
    private ChatMessage GetUniversalMessageUser(String userPrompt, Byte[]? bytesFromImagePrompt = null)
    {
        var finalMessage = new ChatMessage(){Role = ChatRole.User };
        
        finalMessage.Contents.Add(new TextContent(userPrompt));
        
        if(bytesFromImagePrompt is not null)
            finalMessage.Contents.Add(new DataContent(new ReadOnlyMemory<Byte>(bytesFromImagePrompt), bytesFromImagePrompt.GetImageMimeType()));
        
        return finalMessage;
    }

    public async Task<String> UniversalSendMessageAsync(String textPrompt, Byte[]? bytesFromImage = null)
    {
        if (String.IsNullOrWhiteSpace(textPrompt))
            return "Пустое сообщение!";
        
        session ??= await mainWorkFlowAi.CreateSessionAsync();
        
        var response = await mainWorkFlowAi.RunAsync(GetUniversalMessageUser(textPrompt, bytesFromImage),session);
        return FixAiResponse(response.Text);


    }

    public async IAsyncEnumerable<String> UniversalSendMessageStreamAsync(String textPrompt, Byte[]? bytesFromImage = null)
    {
        if (String.IsNullOrWhiteSpace(textPrompt))
        {
            yield return "Пустое сообщение!";
            yield break;
        }
        session ??= await mainWorkFlowAi.CreateSessionAsync();

        StringBuilder sb = new StringBuilder();
        await foreach (var message in mainWorkFlowAi.RunStreamingAsync(GetUniversalMessageUser(textPrompt, bytesFromImage), session))
        {
            sb.Append(message.Text);
            yield return message.Text;
        }
        
        ReadOnlySpan<Char> finalMessage = $"[FINAL]{FixAiResponse(sb)}";
        yield return finalMessage.ToString();

    }
    private String FixAiResponse(StringBuilder sb)
    {
        Char[] rentCharArrayForFix = ArrayPool<Char>.Shared.Rent(sb.Length);
        try
        {
            sb.CopyTo(0, rentCharArrayForFix, 0, sb.Length);
            ReadOnlySpan<Char> textForFix = rentCharArrayForFix.AsSpan(0, sb.Length);

            return FixAiResponse(textForFix);
        }
        finally
        {
            ArrayPool<Char>.Shared.Return(rentCharArrayForFix, clearArray:true);
        }
        
    }

    private String FixAiResponse(ReadOnlySpan<Char> aiSpanMessage)
    {
        if (aiSpanMessage.IsWhiteSpace() || aiSpanMessage.IsEmpty)
            return "пусто";
        
        ReadOnlySpan<Char> openPattern = "<thought>";
        ReadOnlySpan<Char> closedPattern = "</thought>";
        ReadOnlySpan<Char> dropPattern = "<|channel>thought\n<channel|>";

        while (aiSpanMessage.Contains(openPattern, StringComparison.Ordinal))
        {

            Int32 closeIndex = aiSpanMessage.IndexOf(closedPattern, StringComparison.Ordinal);
            if (closeIndex != -1)
            {
                aiSpanMessage = aiSpanMessage.Slice(start: closeIndex + closedPattern.Length).TrimStart();
            }
            else
            {
                break;
            }

        }

        if (aiSpanMessage.Contains(dropPattern, StringComparison.Ordinal))
        {
            Int32 closeIndex = aiSpanMessage.IndexOf(dropPattern, StringComparison.Ordinal);
            if (closeIndex != -1)
            {
                aiSpanMessage = aiSpanMessage.Slice(start: closeIndex + dropPattern.Length).TrimStart();
            }
        }

        return aiSpanMessage.ToString();

        
 
    }

    private List<ChatMessage>? GetMessagesInProvider()
    {
        return provider?.GetMessages(session);
    }

    public void AddHistory(List<ChatMessage> newHistory)
    {
        var messages =  GetMessagesInProvider();
        messages?.AddRange(newHistory);
    }

    public void ClearHistory()
    {
        var messages =  GetMessagesInProvider();
        messages?.Clear();
    }

    public List<ChatMessage> GetHistory()
    {
        var messages = GetMessagesInProvider();
        return messages ?? [];
    }

    public void SetHistory(List<ChatMessage> history)
    {
        var messages = GetMessagesInProvider();
        if (messages is null)
            return;

        messages.Clear();
        messages.AddRange(history);
    }
}