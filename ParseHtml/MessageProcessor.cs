using System.Text;
using HtmlAgilityPack;
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

        string? lastDateValue = null;
        int messageCount = 1;
        var jsonObjects = new List<string>();


        if (textNodes != null)
        {
            for (int i = 0; i < textNodes.Count; i++)
            {
                string? currentDateValue = null;

                if (dateNodes != null && i < dateNodes.Count)
                {
                    currentDateValue = dateNodes[i].GetAttributeValue("title", null);
                    lastDateValue = currentDateValue ?? lastDateValue;
                }

                currentDateValue = currentDateValue ?? lastDateValue;
                string textContent = textNodes[i].InnerText.Trim();

                string? base64Image = null;
                if (photoNodes != null && i < photoNodes.Count)
                {
                    string photoHref = photoNodes[i].GetAttributeValue("href", null);
                    if (!string.IsNullOrEmpty(photoHref))
                    {
                        string photoPath = Path.Combine(Path.GetDirectoryName(_filePath), photoHref);
                        if (File.Exists(photoPath))
                        {
                            base64Image = ConvertImageToBase64(photoPath);
                        }
                    }
                }

                if (base64Image != null && string.IsNullOrWhiteSpace(textContent))
                {
                    textContent = "Картинка";
                }

                var data = new
                {
                    Token = AppConstants.token,
                    UserId = AppConstants.userId,
                    Title = $"{AppConstants.title} {messageCount + i}",
                    TargetLanguageId = AppConstants.targetLanguageId,
                    WrittenLanguageId = AppConstants.writtenLanguageId,
                    CategoryId = AppConstants.categoryId,
                    PublishDate = currentDateValue,
                    Content = textContent,
                    Picture = base64Image,
                    MaterialType = AppConstants.materialType
                };
 
                //string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                jsonObjects.Add(jsonData);
                
                Console.WriteLine("Сформированный JSON:");
                Console.WriteLine(jsonData);

                //await SendJsonToApi(jsonData);
            }
        }
        else
        {
            Console.WriteLine("Не удалось найти элементы с текстом.");
        }
        SaveJsonToFile(jsonObjects);

    }


    private async Task SendJsonToApi(string jsonData)
    {
        using (var client = new HttpClient())
        {
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(_apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Данные успешно отправлены на API.");
            }
            else
            {
                Console.WriteLine($"Ошибка при отправке данных: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
    }


    private string ConvertImageToBase64(string imagePath)
    {
        byte[] imageBytes = File.ReadAllBytes(imagePath);
        string base64String = Convert.ToBase64String(imageBytes);

        string extension = Path.GetExtension(imagePath).ToLowerInvariant();
        string mimeType = extension switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            _ => throw new NotSupportedException($"Unsupported image format: {extension}")
        };

        return $"{mimeType};base64,{base64String}";
    }


    private void SaveJsonToFile(IEnumerable<string> jsonObjects)
    {
        try
        {
            File.WriteAllLines(_outputFilePath, jsonObjects);
            Console.WriteLine($"JSON объекты успешно сохранены в файл: {_outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при сохранении JSON объектов в файл: {ex.Message}");
        }
    }
}
