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
    private readonly CheckData _checkData;
    private readonly TextNodeService _textNodeService;
    private readonly PhotoNodeService _photoNodeService;



    public MessageProcessor(string filePath, string outputFilePath)
    {
        _filePath = filePath;
        _outputFilePath = outputFilePath;
        _messageDataSerivce = new MessageDataService();
        _jsonService = new JsonService();
        _checkData = new CheckData();
        _textNodeService = new TextNodeService();
        _photoNodeService = new PhotoNodeService();
    }

    public async Task ProcessMessagesAsync()
    {
        var doc = new HtmlDocument();
        doc.Load(_filePath);

        // Находим все элементы <div class="body">
        var bodyNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'body')]");

        if (bodyNodes == null)
        {
            Console.WriteLine("Failed to find items with class 'body'.");
            return;
        }

        string? lastDateValue = null;
        var jsonObjects = new List<string>();
        int messageCount = 1;

        foreach (var bodyNode in bodyNodes)
        {
            var fromNameNode = bodyNode.SelectSingleNode(".//div[contains(@class, 'from_name')]");

  
            if (fromNameNode == null || fromNameNode.InnerText.Trim().Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                
                var dateNode = bodyNode.SelectSingleNode(".//div[contains(@class, 'pull_right date details')]");
                string currentDateValue = dateNode?.GetAttributeValue("title", null);
                lastDateValue = currentDateValue ?? lastDateValue;

                
                var textNode = bodyNode.SelectSingleNode(".//div[contains(@class, 'text')]");
                if (textNode != null)
                {
                    var jsonData = ProcessTextNode(textNode, lastDateValue, messageCount);
                    if (jsonData != null) jsonObjects.Add(jsonData);
                    messageCount++;
                }

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
                await Task.Delay(500);
            }
        }

        _jsonService.SaveJsonToFile(jsonObjects, _outputFilePath);
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
            string base64Voice = ConvertMediaToBase64(fullVoicePath, "audio");

            var dateContent = _messageDataSerivce.CreateDataObject(date, base64Voice, messageCount);
            string jsonData = JsonConvert.SerializeObject(dateContent, Formatting.Indented);
            Console.WriteLine("Formed JSON:");

       
            _jsonService.SendJsonToApi(jsonData);
            return jsonData;
        }
        return null;
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

            var dataContent = _messageDataSerivce.CreateDataObject(date, textContent, messageCount);
            string jsonData = JsonConvert.SerializeObject(dataContent, Formatting.Indented);
            Console.WriteLine("Formed JSON:");


            //Console.WriteLine(jsonData);
            _ = _jsonService.SendJsonToApi(jsonData);

            return jsonData;
        }
        return null;
    }



    public string ConvertMediaToBase64(string mediaPath, string mediaType)
    {
        if (string.IsNullOrWhiteSpace(mediaPath) || !File.Exists(mediaPath))
        {
            throw new FileNotFoundException("The media file does not exist.", mediaPath);
        }

        byte[] mediaBytes;
        try
        {
            mediaBytes = File.ReadAllBytes(mediaPath);
        }
        catch (Exception ex)
        {
            throw new IOException($"Error reading the media file: {ex.Message}", ex);
        }

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
            string base64Photo = ConvertMediaToBase64(fullPhotoPath, "image");

            var dateContent = _messageDataSerivce.CreateDataObject(date, base64Photo, messageCount);
            string jsonData = JsonConvert.SerializeObject(dateContent, Formatting.Indented);
            Console.WriteLine("Formed JSON:");

            _jsonService.SendJsonToApi(jsonData);
            return jsonData;
        }
        return null;
    }

}

