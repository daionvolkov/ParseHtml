using System.Text;

namespace ParseHtml.Service;

public class JsonService
{

    public JsonService() { }

    public async Task SendJsonToApi(string jsonData)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
               
                var response = await client.PutAsync(AppConstants.ApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to send data: {response.StatusCode}");
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Body: {responseBody}");
                }
                else
                {
                    Console.WriteLine("Data sent successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
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
