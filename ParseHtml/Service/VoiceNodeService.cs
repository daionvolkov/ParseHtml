using HtmlAgilityPack;
using Newtonsoft.Json;

namespace ParseHtml.Service
{
    public class VoiceNodeService
    {
        private readonly string _filePath = AppConstants.htmlFilePath;
        private readonly MessageDataService _messageDataService;
        private readonly JsonService _jsonService;
        private readonly MediaConverService _mediaConverService;


        public VoiceNodeService()
        {
            _jsonService = new JsonService();
            _messageDataService = new MessageDataService();
            _mediaConverService = new MediaConverService();
        }

        public void ProcessVoiceNode(HtmlNode bodyNode, string? lastDateValue, List<string> jsonObjects, ref int messageCount)
        {
            var voiceNode = bodyNode.SelectSingleNode(".//a[contains(@class, 'media_voice_message')]");

            if (voiceNode != null)
            {
                string voiceHref = voiceNode.GetAttributeValue("href", null);
                if (!string.IsNullOrEmpty(voiceHref))
                {
                    var jsonData = ProcessVoiceNode(voiceNode, lastDateValue, messageCount, voiceHref);
                    if (jsonData != null) jsonObjects.Add(jsonData);
                    messageCount++;
                }
            }
        }

        private string? ProcessVoiceNode(HtmlNode voiceNode, string? date, int messageCount, string mediaPath)
        {
            string? directoryPath = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = Directory.GetCurrentDirectory();
            }

            string fullVoicePath = Path.Combine(directoryPath, mediaPath);

            if (File.Exists(fullVoicePath))
            {
                string base64Voice = _mediaConverService.ConvertMediaToBase64(fullVoicePath, "audio");

                string audioHtml = $"<audio controls><source src=\"{base64Voice}\" type=\"audio/ogg\"></audio>";

                var dateContent = _messageDataService.CreateDataObject(date, audioHtml, messageCount);
                string jsonData = JsonConvert.SerializeObject(dateContent, Formatting.Indented);
                Console.WriteLine("Formed JSON:");

                _jsonService.SendJsonToApi(jsonData);
                return jsonData;
            }
            return null;
        }
    }
}
