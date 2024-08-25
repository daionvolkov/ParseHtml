using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace ParseHtml;

public class ProcessHelper
{
    private readonly string _filePath;
    private readonly string _apiUrl;
    private readonly string _outputFilePath;

    public ProcessHelper(string filePath, string apiUrl, string outputFilePath)
    {
        _filePath = filePath;
        _apiUrl = apiUrl;
        _outputFilePath = outputFilePath;
    }

    public ProcessHelper() { }

    public string? ProcessTextNode(HtmlNode textNode, string? date)
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

    public string? ProcessPhotoNode(HtmlNode photoNode, string? date)
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

    public string? ProcessVoiceNode(HtmlNode voiceNode, string? date)
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

    public void SaveJsonToFile(IEnumerable<string> jsonObjects, string _outputFilePath)
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