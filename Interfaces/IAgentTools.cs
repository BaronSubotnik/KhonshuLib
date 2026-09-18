using Microsoft.Extensions.AI;

namespace Khonshu.Interfaces;

public interface IAgentTools
{
    IEnumerable<AITool> GetTools();
}