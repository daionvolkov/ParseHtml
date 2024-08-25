using System.Text;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace ParseHtml;

public class MessageProcessor
{
    private readonly string _filePath;
    private readonly string _apiUrl;
    private readonly string _outputFilePath;


    public MessageProcessor(string filePath, string apiUrl, string outputFilePath)
    {
        _filePath = filePath;
        _apiUrl = apiUrl;
        _outputFilePath = outputFilePath;

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

        for (int i = 0; i < dateNodes.Count; i++)
        {
            string currentDateValue = dateNodes[i].GetAttributeValue("title", null);
            lastDateValue = currentDateValue ?? lastDateValue;

            if (textNodes != null && i < textNodes.Count)
            {
                var jsonData = ProcessTextNode(textNodes[i], lastDateValue);
                if (jsonData != null) jsonObjects.Add(jsonData);
            }

            if (photoNodes != null && i < photoNodes.Count)
            {
                var jsonData = ProcessPhotoNode(photoNodes[i], lastDateValue);
                if (jsonData != null) jsonObjects.Add(jsonData);
            }

            if (voiceNodes != null && i < voiceNodes.Count)
            {
                var jsonData = ProcessVoiceNode(voiceNodes[i], lastDateValue);
                if (jsonData != null) jsonObjects.Add(jsonData);
            }
        }

        SaveJsonToFile(jsonObjects, _outputFilePath);
    }


    private string? ProcessTextNode(HtmlNode textNode, string? date)
    {
        string textContent = textNode.InnerText.Trim();
        if (!string.IsNullOrWhiteSpace(textContent))
        {
            var data = new
            {
                Date = date,
                Content = textContent
            };

            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            Console.WriteLine("Formed JSON:");
            //Console.WriteLine(jsonData);

            //SendJsonToApi(jsonData);
            return jsonData;
        }
        return null;
    }

    private string? ProcessPhotoNode(HtmlNode photoNode, string? date)
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

                var data = new
                {
                    Date = date,
                    Content = base64Image
                };

                string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                Console.WriteLine("Formed JSON:");
                //Console.WriteLine(jsonData);

                //SendJsonToApi(jsonData);
                return jsonData;
            }
        }
        return null;
    }

    private string? ProcessVoiceNode(HtmlNode voiceNode, string? date)
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

                var data = new
                {
                    Date = date,
                    Content = base64Voice
                };

                string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                Console.WriteLine("Formed JSON:");
                //Console.WriteLine(jsonData);

                //SendJsonToApi(jsonData);
                return jsonData;
            }
        }
        return null;
    }
    private async void SendJsonToApi(string jsonData)
    {
        using (var client = new HttpClient())
        {
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(_apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("The data was successfully sent to the API.Failed to find items with dates.");
            }
            else
            {
                Console.WriteLine($"Error when sending data: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
    }

    private void SaveJsonToFile(IEnumerable<string> jsonObjects, string _outputFilePath)
    {
        if (string.IsNullOrEmpty(_outputFilePath))
        {
            Console.WriteLine("Error: The path for saving JSON objects is not specified.");
            return;
        }
        try
        {
            File.WriteAllLines(_outputFilePath, jsonObjects);
            Console.WriteLine($"JSON objects have been successfully saved to a file: {_outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error when saving JSON objects to a file: {ex.Message}");
        }
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


// Token = AppConstants.token,
//                     UserId = AppConstants.userId,
//                     Title = $"{AppConstants.title} {messageCount + i}",
//                     TargetLanguageId = AppConstants.targetLanguageId,
//                     WrittenLanguageId = AppConstants.writtenLanguageId,
//                     CategoryId = AppConstants.categoryId,
//                     PublishDate = currentDateValue,
//                     Content = contextContent,

//                     MaterialType = AppConstants.materialType