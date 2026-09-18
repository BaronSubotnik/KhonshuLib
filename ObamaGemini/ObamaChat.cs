using Json.Schema;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using System;
using System.Collections.Generic;
using System.Text;
using Khonshu.Interfaces;

namespace Khonshu.ObamaGemini
{
    public sealed class ObamaChat : IObamaAi
    {
        private readonly GenerativeModel model;
        private readonly ChatSession chat;
        private Content sysInstruct;
        private readonly String TestModel = Model.Gemini31FlashImagePreview;

        private SafetySetting safetySettings = new SafetySetting
        {
            Category = HarmCategory.HarmCategoryHarassment,
            Threshold = HarmBlockThreshold.BlockNone
        };
        private List<SafetySetting> listSetting = new List<SafetySetting>();

        public ObamaChat(String apikeys, String? sysInstruct = null, String? ModelNeiro = null)
        {
            if (sysInstruct != null)
            {
                listSetting.Add(safetySettings);
            }

            if (ModelNeiro != null)
            {
                TestModel = ModelNeiro;
            }



            if (sysInstruct != null)
            {
                this.sysInstruct = new Content(sysInstruct);
            }

            
            var googleAi = new GoogleAI(apikeys);

            model = googleAi.GenerativeModel(model: TestModel, safetySettings: listSetting, systemInstruction: this.sysInstruct);

            chat = model.StartChat();
            return;
            

        }
        
        [Obsolete("Новые Универсальные методы!")]
        public async Task<String> SendMesage(String prompt)
        {
            String errorMessage = null!;
            if (String.IsNullOrWhiteSpace(prompt))
            {
                return "пусто";
            }

            GenerateContentResponse? result;
            try
            {
               result = await chat.SendMessage(prompt);
            }
            catch(Exception ex)
            {
                errorMessage = $"error {ex.InnerException} сказала: {ex.Message} full error:{ex}";
                result = null;
            }

            if(errorMessage != null || result == null)
            {
                return $"No content generated. Error {((errorMessage != null) ? errorMessage : String.Empty)}";
            }

            return result.Text!;


        }
        
        
        [Obsolete("Новые Универсальные методы!")]
        public async IAsyncEnumerable<String> SendMesageStream(String prompt)
        {
            if (String.IsNullOrWhiteSpace(prompt))
            {
                yield return "пусто";
                yield break;
            }
            String errorMessage = null;
            IAsyncEnumerable<GenerateContentResponse>? stream;

            try
            {
                stream = chat.SendMessageStream(prompt);
            }
            catch (Exception ex)
            {
                errorMessage = $"error {ex.InnerException} сказала: {ex.Message} full error:{ex}";
                stream = null;
            }


            if(errorMessage != null || stream == null)
            {
               yield return $"No content generated. Error {((errorMessage != null) ? errorMessage : String.Empty)}";
            }


            if(stream is not null)
            {
                await foreach (var i in stream)
                {
                    yield return i.Text!;
                }
            }
        }

        [Obsolete("Новые Универсальные методы!")]
        public async Task<String> ImageMessageAnalizeAsync(Byte[] ImageBytes, String prompt1)
        {
            if (ImageBytes == null || ImageBytes.Length == 0)
                throw new ArgumentException("Массив байтов пуст.");

            String mimeType = ImageHelper.GetMimeType(ImageBytes);

            String base64STR = Convert.ToBase64String(ImageBytes);

            String? errorMessage = null;

            var cts = new Content
            {
                Role = Role.User,
                Parts = new List<IPart>
                {
                    new Part { Text = prompt1 },
                    
                    new InlineData
                    {
                        MimeType = mimeType,
                        Data = base64STR,
                    }

                }

            };

            var request = new GenerateContentRequest
            {
                Contents = new List<Content> { cts },
                SafetySettings = listSetting
            };

            if (sysInstruct != null)
            {
                request.SystemInstruction = sysInstruct;
            }
            GenerateContentResponse? response;
            try
            {
                response = await chat.SendMessage(request);
            }
            catch(Exception ex)
            {
                errorMessage = $"error {ex.InnerException} сказала: {ex.Message} full error:{ex}";
                response = null;
            }


            
            if (response == null || response.Candidates == null || response.Candidates.Count == 0 || errorMessage != null)
            {
                return $"No content generated. Error {((errorMessage !=null)? errorMessage : String.Empty)}";
            }

            return response.Text!;


        }

        public async Task<String> UniversalSendMessageAsync(String textPrompt, Byte[]? bytesFromImage = null)
        {
            String? mimeType = null;
            String? base64Image = null;

            Content contentForAi = null!;

            if(bytesFromImage is not null)
            {
                mimeType = bytesFromImage.GetImageMimeType();
                base64Image = Convert.ToBase64String(bytesFromImage);

                contentForAi = new Content
                {
                    Role = Role.User,
                    Parts = new List<IPart>
                    {
                        new Part
                        {
                            Text = textPrompt
                        },

                        new InlineData
                        {
                            MimeType = mimeType,
                            Data = base64Image,
                        }

                    }
                };
            }
            else
            {
                contentForAi = new Content
                {
                    Role = Role.User,
                    Parts = new List<IPart>
                    {
                        new Part
                        {
                            Text = textPrompt
                        },

                    },
                    
                };
            }

            var request = new GenerateContentRequest
            {
                Contents = new List<Content> { contentForAi },
                SafetySettings = listSetting,
                SystemInstruction = (sysInstruct is not null) ? sysInstruct : new Content()

            };

            String? errorMessage = null;

            GenerateContentResponse? response;

            try
            {
                response = await chat.SendMessage(request);
            }
            catch (Exception ex)
            {
                errorMessage = $"error {ex.InnerException} сказала: {ex.Message} full error:{ex}";
                response = null;
            }



            if (response == null || response.Candidates == null || response.Candidates.Count == 0 || errorMessage != null)
            {
                return $"No content generated. Error {((errorMessage != null) ? errorMessage : String.Empty)}";
            }

            return response.Text!;
        }
        
        

        public async IAsyncEnumerable<String> UniversalSendMessageStreamAsync(String textPrompt, Byte[]? bytesFromImage = null)
        {
            String? mimeType = null;
            String? base64Image = null;

            Content contentForAi = null!;

            if (bytesFromImage is not null)
            {
                mimeType = bytesFromImage.GetImageMimeType();
                base64Image = Convert.ToBase64String(bytesFromImage);

                contentForAi = new Content
                {
                    Role = Role.User,
                    Parts = new List<IPart>
                    {
                        new Part
                        {
                            Text = textPrompt
                        },

                        new InlineData
                        {
                            MimeType = mimeType,
                            Data = base64Image,
                        }

                    }
                };
            }
            else
            {
                contentForAi = new Content
                {
                    Role = Role.User,
                    Parts = new List<IPart>
                    {
                        new Part
                        {
                            Text = textPrompt
                        },

                    }
                };
            }

            var request = new GenerateContentRequest
            {
                Contents = new List<Content> { contentForAi },
                SafetySettings = listSetting
            };


            if (sysInstruct != null)
            {
                request.SystemInstruction = sysInstruct;
            }

            String? errorMessage = null;

            IAsyncEnumerable<GenerateContentResponse>? stream;

            try
            {
                stream = chat.SendMessageStream(request);
            }
            catch (Exception ex)
            {
                errorMessage = $"error {ex.InnerException} сказала: {ex.Message} full error:{ex}";
                stream = null;
            }


            if (errorMessage != null || stream == null)
            {
                yield return $"No content generated. Error {((errorMessage != null) ? errorMessage : String.Empty)}";
                yield break;
            }


            if (stream is not null)
            {
                await foreach (var i in stream)
                {
                    yield return i.Text!;
                }
            }






        }

        
    }
}
