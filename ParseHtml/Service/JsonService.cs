using System.Text;

namespace ParseHtml.Service;

public class JsonService
{

    public JsonService() {}

    public async void SendJsonToApi(string jsonData)
    {
        using (var client = new HttpClient())
        {
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(AppConstants.ApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("The data was successfully sent to the API");
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


}
