using System;
using System.Collections.Generic;
using System.Text;
using Khonshu.Interfaces;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;

namespace Khonshu.ObamaGemini
{
    public  sealed  class ObamaApi : IObamaAi
    {
        
        private GenerativeModel model;
        private Content? ct;
        private String modelTest = Model.Gemini31ProPreviewCustomTools;

        private SafetySetting safetySettings = new SafetySetting
        {
            Category = HarmCategory.HarmCategoryHarassment,
            Threshold = HarmBlockThreshold.BlockNone
        };
        private List<SafetySetting> listSetting = new List<SafetySetting>();
        private IObamaAi obamaAiImplementation;


        public ObamaApi(String apikeys, String? content = null, String? ModelNeiro = null)
        {
            listSetting.Add(safetySettings);

            if (ModelNeiro != null)
            {
                modelTest = ModelNeiro;
            }

            if(content != null)
            {
                ct = new Content(content);
            }

            var googleAi = new GoogleAI(apikeys);
            model = googleAi.GenerativeModel(model:modelTest, safetySettings:listSetting, systemInstruction:ct);

        }
        




        public async Task<String> Generate(String promt)
        {
            var response = await model.GenerateContent(promt).ConfigureAwait(false);
            if (response == null || response.Candidates == null || response.Candidates.Count == 0)
            {
                return "No content generated.";
            }
            else
            {
                return response.Text!.ToString();
            }

        }

        public async IAsyncEnumerable<String> StreamGenerate(String promt)
        {
            var stream = model.GenerateContentStream(promt);

            await foreach(var i in stream)
            {
                if(i != null)
                {
                    yield return i.Text!.ToString();
                }
            }

        }

        public async Task<String> ImageMessageAnalizeAsync(Byte[] ImageBytes, String prompt1)
        {
            if (ImageBytes == null || ImageBytes.Length == 0)
                throw new ArgumentException("Массив байтов пуст.");

            String mimeType = ImageHelper.GetMimeType(ImageBytes);

            String base64STR = Convert.ToBase64String(ImageBytes);



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
                Contents = new List<Content> { cts},
                SafetySettings = listSetting
            };

            if(ct != null)
            {
                request.SystemInstruction = ct;
            }


            var response = await model.GenerateContent(request);
            if (response == null || response.Candidates == null || response.Candidates.Count == 0)
            {
                return "No content generated.";
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
                SystemInstruction = (ct is not null) ? ct : new Content()

            };

            String? errorMessage = null;

            GenerateContentResponse? response;
            try
            {
                response = await model.GenerateContent(request);
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


            if (ct != null)
            {
                request.SystemInstruction = ct;
            }

            String? errorMessage = null;

            IAsyncEnumerable<GenerateContentResponse>? stream;

            try
            {
                stream = model.GenerateContentStream(request);
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
