using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;


namespace Khonshu.ObamaGemini;

public sealed class InitAgentHelper
{
    private readonly OpenAIClient client;
    private readonly String modelName;
    
    public InitAgentHelper(Uri uriServer,String model, String? apikey = null)
    {
        modelName = model;
        var options = new OpenAIClientOptions(){Endpoint =  uriServer};
        client = new OpenAIClient(new ApiKeyCredential((apikey is not null)? apikey : "lm-studio"), options);
        
    }

    public IChatClient GetInitAgent()
    {
        return client.GetChatClient(modelName).AsIChatClient().AsBuilder()
            .UseFunctionInvocation() 
            .Build();
    }
}