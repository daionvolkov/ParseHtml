using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    var jsonData = ProcessPhotoNode(photoAnchorNode, lastDateValue, messageCount, photoHref);
                    if (jsonData != null) jsonObjects.Add(jsonData);
                    messageCount++;
                }
            }
        }


        private string? ProcessPhotoNode(HtmlNode photoNode, string? date, int messageCount, string mediaPath)
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

                var dateContent = _messageDataService.CreateDataObject(date, base64Photo, messageCount);
                string jsonData = JsonConvert.SerializeObject(dateContent, Formatting.Indented);
                Console.WriteLine("Formed JSON:");

                _jsonService.SendJsonToApi(jsonData);
                return jsonData;
            }
            return null;
        }

    }
}
