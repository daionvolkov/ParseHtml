using HtmlAgilityPack;
using Newtonsoft.Json;

namespace ParseHtml.Service
{
    public class TextNodeService
    {
        private readonly MessageDataService _messageDataService;
        private readonly JsonService _jsonService;

        public TextNodeService()
        {
            _messageDataService = new MessageDataService();
            _jsonService = new JsonService();
        }

        public void ProcessTextNode(HtmlNode bodyNode, string? lastDateValue, List<string> jsonObjects, ref int messageCount)
        {
            var textNode = bodyNode.SelectSingleNode(".//div[contains(@class, 'text')]");
            if (textNode != null)
            {
                var jsonData = ProcessTextNode(textNode, lastDateValue, messageCount);
                if (jsonData != null) jsonObjects.Add(jsonData);
                messageCount++;
            }
        }



        private string? ProcessTextNode(HtmlNode textNode, string? date, int messageCount)
        {
            string textContent = textNode.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(textContent))
            {

                var dataContent = _messageDataService.CreateDataObject(date, textContent, messageCount);
                string jsonData = JsonConvert.SerializeObject(dataContent, Formatting.Indented);
                Console.WriteLine("Formed JSON:");


                //Console.WriteLine(jsonData);
                _ = _jsonService.SendJsonToApi(jsonData);
                
                return jsonData;
            }
            return null;
        }
    }
}
