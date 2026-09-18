using System;
using System.Collections.Generic;
using System.Text;

namespace Khonshu.Interfaces
{
    public  interface IObamaAi
    {
        Task<String> UniversalSendMessageAsync(String textPrompt, Byte[]? bytesFromImage = null);
        IAsyncEnumerable<String> UniversalSendMessageStreamAsync(String textPrompt, Byte[]? bytesFromImage = null);
    }
}
