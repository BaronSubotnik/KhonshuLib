using Microsoft.Extensions.AI;
namespace Khonshu.Interfaces;

public interface IHistoryAi
{
    void AddHistory(List<ChatMessage> newHistory);
    void ClearHistory();
    List<ChatMessage> GetHistory();
    void SetHistory(List<ChatMessage> history);
}