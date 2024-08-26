using System.Text;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using Models;
using Newtonsoft.Json;
using ParseHtml.Service;

namespace ParseHtml;

public class MessageProcessor
{
    private readonly string _filePath;
    private readonly string _outputFilePath;
    private readonly MessageDataService _messageDataSerivce;
    private readonly JsonService _jsonService;


    public MessageProcessor(string filePath, string outputFilePath)
    {
        _filePath = filePath;
        _outputFilePath = outputFilePath;
        _messageDataSerivce = new MessageDataService();
        _jsonService = new JsonService();
    }

    public async Task ProcessMessagesAsync()
    {
        var doc = new HtmlDocument();
        doc.Load(_filePath);

        var dateNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'pull_right date details')]");
        var textNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'text')]");
        var photoNodes = doc.DocumentNode.SelectNodes("//a[contains(@class, 'photo_wrap clearfix pull_left')]");
        var voiceNodes = doc.DocumentNode.SelectNodes("//a[contains(@class, 'media_voice_message')]");

        if (dateNodes == null)
        {
            Console.WriteLine("Failed to find items with dates.");
            return;
        }

        string? lastDateValue = null;
        var jsonObjects = new List<string>();
        int messageCount = 1;

        for (int i = 0; i < dateNodes.Count; i++)
        {
            string currentDateValue = dateNodes[i].GetAttributeValue("title", null);
            lastDateValue = currentDateValue ?? lastDateValue;

            if (textNodes != null && i < textNodes.Count)
            {
                var jsonData = ProcessTextNode(textNodes[i], lastDateValue, messageCount);
                if (jsonData != null) jsonObjects.Add(jsonData);
                messageCount ++;
            }

            if (photoNodes != null && i < photoNodes.Count)
            {
                var jsonData = ProcessPhotoNode(photoNodes[i], lastDateValue, messageCount);
                if (jsonData != null) jsonObjects.Add(jsonData);
                messageCount ++;
            }

            if (voiceNodes != null && i < voiceNodes.Count)
            {
                var jsonData = ProcessVoiceNode(voiceNodes[i], lastDateValue, messageCount);
                if (jsonData != null) jsonObjects.Add(jsonData);
                messageCount ++;
            }
        }
        _jsonService.SaveJsonToFile(jsonObjects, _outputFilePath);
    }


    private string? ProcessTextNode(HtmlNode textNode, string? date, int messageCount)
    {
        string textContent = textNode.InnerText.Trim();
        if (!string.IsNullOrWhiteSpace(textContent))
        {
            var dataContent = _messageDataSerivce.CreateDataObject(date, textContent, messageCount);

            string jsonData = JsonConvert.SerializeObject(dataContent, Formatting.Indented);
            Console.WriteLine("Formed JSON:");
            
            //Console.WriteLine(jsonData);
            //_jsonService.SendJsonToApi(jsonData);
            return jsonData;
        }
        return null;
    }

    private string? ProcessPhotoNode(HtmlNode photoNode, string? date, int messageCount)
    {
        string photoHref = photoNode.GetAttributeValue("href", null);
        if (!string.IsNullOrEmpty(photoHref))
        {
            string? directoryPath = Path.GetDirectoryName(_filePath);


            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = Directory.GetCurrentDirectory();
            }
            string photoPath = Path.Combine(directoryPath, photoHref);


            if (File.Exists(photoPath))
            {
                string base64Image = ConvertMediaToBase64(photoPath, "image");

                var dateContent = _messageDataSerivce.CreateDataObject(date, base64Image, messageCount);
            
                string jsonData = JsonConvert.SerializeObject(dateContent, Formatting.Indented);
                Console.WriteLine("Formed JSON:");

                //Console.WriteLine(jsonData);
                //_jsonService.SendJsonToApi(jsonData);
                return jsonData;
            }
        }
        return null;
    }

    private string? ProcessVoiceNode(HtmlNode voiceNode, string? date, int messageCount)
    {
        string voiceHref = voiceNode.GetAttributeValue("href", null);
        if (!string.IsNullOrEmpty(voiceHref))
        {
            string? directoryPath = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = Directory.GetCurrentDirectory();
            }
            string voicePath = Path.Combine(directoryPath, voiceHref);
            if (File.Exists(voicePath))
            {
                string base64Voice = ConvertMediaToBase64(voicePath, "audio");

                var dateContent = _messageDataSerivce.CreateDataObject(date, base64Voice, messageCount);

                string jsonData = JsonConvert.SerializeObject(dateContent, Formatting.Indented);
                Console.WriteLine("Formed JSON:");
                
                //Console.WriteLine(jsonData);
                //_jsonService.SendJsonToApi(jsonData);
                return jsonData;
            }
        }
        return null;
    }
    
    

    private string ConvertMediaToBase64(string mediaPath, string mediaType)
    {
        byte[] mediaBytes = File.ReadAllBytes(mediaPath);
        string base64String = Convert.ToBase64String(mediaBytes);

        string extension = Path.GetExtension(mediaPath).ToLowerInvariant();
        string mimeType = mediaType switch
        {
            "image" => extension switch
            {
                ".png" => "data:image/png;base64,",
                ".jpg" => "data:image/jpeg;base64,",
                ".jpeg" => "data:image/jpeg;base64,",
                _ => throw new NotSupportedException($"Unsupported image format: {extension}")
            },
            "audio" => extension switch
            {
                ".ogg" => "data:audio/ogg;base64,",
                ".mp3" => "data:audio/mpeg;base64,",
                _ => throw new NotSupportedException($"Unsupported audio format: {extension}")
            },
            _ => throw new NotSupportedException($"Unsupported media type: {mediaType}")
        };

        return $"{mimeType}{base64String}";
    }
}

