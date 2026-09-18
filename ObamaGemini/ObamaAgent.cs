using System.Buffers;
using System.Text;
using Khonshu.Interfaces;
using Microsoft.Extensions.AI;


namespace Khonshu.ObamaGemini;

[Obsolete("Используй ObamaAgentII, этот класс работает некоректно")]
public sealed class ObamaAgent<TTools> : IObamaAi, IHistoryAi  where TTools : class, IAgentTools
{
    private readonly IChatClient chatClient;
    private readonly List<ChatMessage> history = [];
    private readonly ChatOptions chatOptions;
    private readonly String? currentSystemInstryct;

    public ObamaAgent(IChatClient chatClient, String? systemInstruct = null, TTools? tools = null) 
    {
        this.chatClient = chatClient;
        currentSystemInstryct = systemInstruct;
        this.chatOptions = new ChatOptions()
        {
            Temperature = 0.4f,
            Reasoning = new ReasoningOptions()
            {
                Output = ReasoningOutput.Summary
            },
            Tools = tools?.GetTools().Cast<AITool>().ToList()
            
        };
        history.Add(new ChatMessage(ChatRole.System,
            systemInstruct ?? "Ты умный ии агент, обьясняй все кратко и по делу используй методы!"));

    }
    
    public List<ChatMessage> GetHistory() => history;
    public void SetHistory(List<ChatMessage> newHistory)
    {
        history.Clear();
        history.AddRange(newHistory);
    }

    public void ClearHistory()
    {
        history.Clear();
        history.Add(new ChatMessage(ChatRole.System,
            (!String.IsNullOrEmpty(currentSystemInstryct))
                ? currentSystemInstryct
                : "Ты умный ии агент, обьясняй все кратко и по делу используй методы!"));
    }

    

    public void AddHistory(List<ChatMessage> newHistory)
    {
        history.AddRange(newHistory);
    }
    
    
    public async Task<String> UniversalSendMessageAsync(String textPrompt, Byte[]? bytesFromImage = null)
    {
        if (String.IsNullOrWhiteSpace(textPrompt))
            return "Ничо не вижу!";

        history.Add(GetUniversalMessageUser(textPrompt, bytesFromImage));
        var options = chatOptions.Clone();
        
        var response = await chatClient.GetResponseAsync(history, options);
        String resulit = FixAiResponse(response.Text);
        
        history.Add(new ChatMessage(ChatRole.Assistant, resulit));
        
        return resulit;
    }

    public async IAsyncEnumerable<String> UniversalSendMessageStreamAsync(String textPrompt, Byte[]? bytesFromImage = null)
    {
        if (String.IsNullOrWhiteSpace(textPrompt))
        {
           yield return "Это пустое сообщение";
           yield break;
        }
        
        history.Add(GetUniversalMessageUser(textPrompt, bytesFromImage));
        
       
        var options = chatOptions.Clone();
        
        StringBuilder fullResult = new();
        await foreach (var i in chatClient.GetStreamingResponseAsync(GetUniversalMessageUser(textPrompt, bytesFromImage), options))
        {
            if(String.IsNullOrWhiteSpace(i.Text))
                continue;
            fullResult.Append(i.Text);
            yield return i.Text;
        }

        String finalResult = FixAiResponse(fullResult);
        yield return $"[FINAL]{finalResult}";
        
        //history.Add(new ChatMessage(ChatRole.Assistant, finalResult));
        //history.Add(new ChatMessage(ChatRole.Tool, finalResult));
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

    private ChatMessage GetUniversalMessageUser(String userPrompt, Byte[]? bytesFromImagePrompt = null)
    {
        var finalMessage = new ChatMessage(){Role = ChatRole.User };
        
        finalMessage.Contents.Add(new TextContent(userPrompt));
        
        if(bytesFromImagePrompt is not null)
            finalMessage.Contents.Add(new DataContent(new ReadOnlyMemory<Byte>(bytesFromImagePrompt), bytesFromImagePrompt.GetImageMimeType()));
        
        return finalMessage;
    }
}