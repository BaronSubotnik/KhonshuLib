using System;
using System.Collections.Generic;
using System.Text;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using Khonshu;
using Khonshu.Constants;
using Khonshu.Interfaces;

namespace Khonshu.ObamaGemini
{
    public sealed class OpenObama : IObamaAi
    {
        private readonly OpenAIClientOptions optionsAi = null!;
        private readonly Uri serverUrl = null!;
        
        private ApiKeyCredential apiKeyCredential;
        private String modelAi;
        private SystemChatMessage? systemInstruction;

        private readonly ChatClient chat = null!;

        private  List<ChatMessage> storageForHistory = new();

        public OpenObama(String? systemInstruct = null)
        {
            serverUrl = new(CommonKhonshuConst.Endponit.LlmStudioLocalServerV1);
            optionsAi = new OpenAIClientOptions() { Endpoint = serverUrl  };
            apiKeyCredential = new("Are_You_Sure");
            modelAi = CommonKhonshuConst.AiModels.Gemma4Qat12B;

            if(systemInstruct is not null)
            {
                systemInstruction = new(systemInstruct);
                storageForHistory.Clear();
                storageForHistory.Add(systemInstruction);
            }
                


            chat = new(modelAi, apiKeyCredential, optionsAi);
        }


        public OpenObama(Uri serverUrl, ApiKeyCredential apiKey, String modelAi, String? systemInstruct = null)
        {
            this.serverUrl = serverUrl;
            optionsAi = new OpenAIClientOptions() { Endpoint = serverUrl };
            this.apiKeyCredential = apiKey;
            this.modelAi = modelAi;

            if (systemInstruct is not null)
            {
                systemInstruction = new SystemChatMessage(systemInstruct);
                storageForHistory.Clear();
                storageForHistory.Add(systemInstruction);
            }
                

            chat = new(modelAi, apiKeyCredential, optionsAi);
        }

        public async Task<String> SendMessageAsync(String promptForAi)
        {
            
            storageForHistory.Add(new UserChatMessage
                (
                    ChatMessageContentPart.CreateTextPart(promptForAi)
                ));
            
            String Respone;

            try
            {
                ChatCompletion complite = await chat.CompleteChatAsync(storageForHistory);
                Respone = complite.Content[^1].Text ;

                storageForHistory.Add(new AssistantChatMessage(Respone));
            }
            catch (Exception ex)
            {
                Respone = $"error {ex.InnerException} сказала: {ex.Message} full error:{ex}";
            }

            return Respone;
        }

        public void ClearChatHistory()
        {
            storageForHistory.Clear();
            if(systemInstruction is not null)
            {
                storageForHistory.Add (systemInstruction);
            }
        }

        public void SetSystemPrompt(String prompt)
        {
            systemInstruction = new(prompt);
            storageForHistory.Clear();
            storageForHistory.Add(systemInstruction);
        }

        public void LoadChatHistory(List<ChatMessage> history)
        {
            storageForHistory = history;
        }
            


        public async Task<String> ImageAnalyzeMessageSendAsync(Byte[] imageFromPromt, String promptForAi)
        {
            BinaryData image = BinaryData.FromBytes(imageFromPromt);


            storageForHistory.Add(new UserChatMessage
                (
                    ChatMessageContentPart.CreateTextPart(promptForAi),
                    ChatMessageContentPart.CreateImagePart(imageBytes: image, imageBytesMediaType: imageFromPromt.GetImageMimeType())
                    ));
            String Response;
            try
            {
                ChatCompletion complite = await chat.CompleteChatAsync(storageForHistory);
                Response = complite.Content[^1].Text;

                storageForHistory.Add(new AssistantChatMessage(Response));
            }
            catch (Exception ex)
            {
                Response = $"error {ex.InnerException} сказала: {ex.Message} full error:{ex}";
            }
            return Response;
        }

        public async Task<String> UniversalSendMessageAsync(String yourPrompt, Byte[]? yourImageBytesBuffer = null)
        {
            
            String Response;
            if (yourImageBytesBuffer is not null)
            {
                BinaryData binaryImage = BinaryData.FromBytes(yourImageBytesBuffer);

                storageForHistory.Add(new UserChatMessage
                    (
                        ChatMessageContentPart.CreateTextPart(yourPrompt),
                        ChatMessageContentPart.CreateImagePart(binaryImage, yourImageBytesBuffer.GetImageMimeType())
                        ));
            }
            else
            {
                storageForHistory.Add(new UserChatMessage
                    (
                        ChatMessageContentPart.CreateTextPart(yourPrompt)
                        ));
            }


            try
            {
                ChatCompletion compiteon = await chat.CompleteChatAsync(storageForHistory);
                Response = compiteon.Content[^1].Text;
                storageForHistory.Add(new AssistantChatMessage(Response));
            }
            catch(Exception ex)
            {
                Response = $"error {ex.InnerException} сказала: {ex.Message} full error:{ex}";
            }
            return Response;
        }

        public async IAsyncEnumerable<String> UniversalSendMessageStreamAsync(String yourPrompt, Byte[]? imageBytesBuffer = null)
        {
            
            
            if (imageBytesBuffer is not null)
            {
                BinaryData binaryImage = BinaryData.FromBytes(imageBytesBuffer);
                
                storageForHistory.Add(new UserChatMessage
                    (
                        ChatMessageContentPart.CreateTextPart(yourPrompt),
                        ChatMessageContentPart.CreateImagePart(binaryImage, ImageHelper.GetMimeType(imageBytesBuffer))
                        ));
            }
            else
            {
                storageForHistory.Add(new UserChatMessage
                    (
                        ChatMessageContentPart.CreateTextPart(yourPrompt)
                        ));
            }

            
            StringBuilder fullResponse = new StringBuilder();
            try
            {
                var response = chat.CompleteChatStreamingAsync(storageForHistory);
                await foreach (StreamingChatCompletionUpdate update in response)
                {
                    if (update.ContentUpdate.Count > 0)
                    {
                        String chunk =  update.ContentUpdate[^1].Text;
                        fullResponse.Append(chunk);
                        yield return chunk;
                    }
                }
                storageForHistory.Add(new AssistantChatMessage(fullResponse.ToString()));
            }
            finally
            {
                
            }
        }
    }
}
