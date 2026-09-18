using Khonshu.Interfaces;

namespace Khonshu.ObamaGemini;

public abstract class Obama
{
    protected IObamaAi ai;

    protected Obama(IObamaAi ai)
    {
        this.ai = ai;
    }

    protected virtual Boolean TrySetHistory(List<Microsoft.Extensions.AI.ChatMessage> newMessages)
    {
        if (ai is IHistoryAi sotorageHistory)
        {
            sotorageHistory.SetHistory(newMessages);
            return true;
        }

        return false;
    }

    protected virtual List<Microsoft.Extensions.AI.ChatMessage>? GetHistory()
    {
        if (ai is IHistoryAi storageHistory)
            return storageHistory.GetHistory();
        else
            return null;
    }

    protected virtual Boolean TryClearHistory()
    {
        if (ai is IHistoryAi storageHistory)
        {
            storageHistory.ClearHistory();
            return true;
        }

        return false;
    }

    protected virtual Boolean TryAddHistory(List<Microsoft.Extensions.AI.ChatMessage> newMessages)
    {
        if (ai is IHistoryAi storageHistory)
        {
            storageHistory.AddHistory(newMessages);
            return true;
        }

        return false;
    }
    
    

    public Type GetCurrentAiType()
    {
        return ai.GetType();
    }

    protected  async Task<String> SendMessageForAiAsync(String prompt, Byte[]? imagePrompt)
    {
        return await ai.UniversalSendMessageAsync(prompt, imagePrompt);
    }

    protected  async IAsyncEnumerable<String> SendMessageForAiStream(String prompt, Byte[]? imagePrompt)
    {
        await foreach (var i in ai.UniversalSendMessageStreamAsync(prompt, imagePrompt))
        {
            yield return i;
        }
    }
}