using HtmlAgilityPack;
using Newtonsoft.Json;

namespace ParseHtml.Service
{
    public class PhotoNodeService
    {
        private readonly MessageDataService _messageDataService;
        private readonly JsonService _jsonService;
        private readonly MediaConverService _mediaConverService;
        private readonly string _filePath = AppConstants.htmlFilePath;

        public PhotoNodeService()
        {
            _jsonService = new JsonService();
            _messageDataService = new MessageDataService();
            _mediaConverService = new MediaConverService();
        }

        public void ProcessPhotoNode(HtmlNode bodyNode, string? lastDateValue, List<string> jsonObjects, ref int messageCount)
        {
            var photoAnchorNode = bodyNode.SelectSingleNode(".//a[contains(@class, 'photo_wrap clearfix pull_left')]");
            if (photoAnchorNode != null)
            {
                string photoHref = photoAnchorNode.GetAttributeValue("href", null);
                if (!string.IsNullOrEmpty(photoHref))
                {
                    var imgNode = photoAnchorNode.SelectSingleNode(".//img[contains(@class, 'photo')]");
                    if (imgNode != null)
                    {
                        string imgStyle = imgNode.GetAttributeValue("style", string.Empty);
                        var jsonData = ProcessPhotoNode(photoAnchorNode, lastDateValue, messageCount, photoHref, imgStyle);
                        if (jsonData != null) jsonObjects.Add(jsonData);
                        messageCount++;
                    }
                }
            }
        }
        private string? ProcessPhotoNode(HtmlNode photoNode, string? date, int messageCount, string mediaPath, string imgStyle)
        {
            string? directoryPath = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = Directory.GetCurrentDirectory();
            }
            string fullPhotoPath = Path.Combine(directoryPath, mediaPath);

            if (File.Exists(fullPhotoPath))
            {
                string base64Photo = _mediaConverService.ConvertMediaToBase64(fullPhotoPath, "image");

                string imgHtml = $"<img class=\"photo\" src=\"{base64Photo}\" style=\"{imgStyle}\" />";
                var dateContent = _messageDataService.CreateDataObject(date, imgHtml, messageCount);

                string jsonData = JsonConvert.SerializeObject(dateContent, Formatting.Indented);
                Console.WriteLine("Formed JSON:");

                _jsonService.SendJsonToApi(jsonData);
                return jsonData;
            }
            return null;
        }
    }
}
