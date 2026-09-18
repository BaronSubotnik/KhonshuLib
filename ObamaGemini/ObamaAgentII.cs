using System.Buffers;
using System.Text;
using Khonshu.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;

namespace Khonshu.ObamaGemini;

public sealed class ObamaAgentII<TTools> : IObamaAi,IObamaAgent, IHistoryAi where TTools : class, IAgentTools
{
    private readonly AIAgent agent;
    private AgentSession? session = null;

    public ObamaAgentII(IChatClient client, String agentName, String? systemInstryction = null, TTools? tools = null, Single? temperature = null, Int32? maxOutputToken = null, Boolean isWebSearch = false)
    {
        var agentTools = tools?.GetTools().ToList() ?? new();
        if (isWebSearch)
        {
            agentTools.Add(new HostedWebSearchTool());
        }
        
        agent = new ChatClientAgent(
            client,
            new ChatClientAgentOptions()
            {
                Name = agentName,
                
                ChatOptions = new ChatOptions()
                {
                    Tools = agentTools,
                    AllowMultipleToolCalls =  true,
                    Instructions = systemInstryction ?? "ты полезный ии агент, используй методы",
                    Temperature = temperature,
                    MaxOutputTokens = maxOutputToken,
                }
            });
        
    }

    public static async Task<ObamaAgentII<TTools>> CreateAgentAsync(IChatClient client, String agentName, String? systemInstryction = null, TTools? tools = null)
    {
        var newAgent = new ObamaAgentII<TTools>(client, agentName, systemInstryction, tools);
        await newAgent.CreateSessionAsync();
        return newAgent;
    }

    public async Task CreateSessionAsync()
    {
        session ??= await agent.CreateSessionAsync();
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
            return "пусто";
        
        if(session is null)
            await CreateSessionAsync();


        var response = await agent.RunAsync(GetUniversalMessageUser(textPrompt, bytesFromImage), session);
        return FixAiResponse(response.Text);
    }

    public async IAsyncEnumerable<String> UniversalSendMessageStreamAsync(String textPrompt, Byte[]? bytesFromImage = null)
    {
        if (String.IsNullOrWhiteSpace(textPrompt))
        {
            yield return  "пусто";
            yield break;
        }
       
        
        if(session is null)
            await CreateSessionAsync();
        
        StringBuilder allTokenAiResponse = new StringBuilder();
        await foreach (var i in agent.RunStreamingAsync(GetUniversalMessageUser(textPrompt, bytesFromImage), session))
        {
            allTokenAiResponse.Append(i.Text);
            yield return i.Text;
        }

        ReadOnlySpan<Char> finalResultAiResponse = $"[FINAL]{FixAiResponse(allTokenAiResponse)}";
        yield return finalResultAiResponse.ToString();
        

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

    public void AddHistory(List<ChatMessage> newHistory)
    {
        var provider = agent.GetService<InMemoryChatHistoryProvider>();
        if (provider is not null)
        {
            var messages =  provider.GetMessages(session);
            messages.AddRange(newHistory);
        }
    }

    public void ClearHistory()
    {
        var provider = agent.GetService<InMemoryChatHistoryProvider>();
        if (provider is not null)
        {
            var messages =  provider.GetMessages(session);
            messages.Clear();
        }
    }

    public List<ChatMessage> GetHistory()
    {
        var provider = agent.GetService<InMemoryChatHistoryProvider>();
        if (provider is not null)
            return provider.GetMessages(session);

        return [];
    }

    public void SetHistory(List<ChatMessage> history)
    {
        var provider = agent.GetService<InMemoryChatHistoryProvider>();
        if (provider is not null)
        {
            var messages =  provider.GetMessages(session);
            messages.Clear();
            messages.AddRange(history);
        }
    }

    public AIAgent GetAgent()
    {
        return agent;
    }
}