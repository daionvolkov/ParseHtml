using System.Text;

namespace ParseHtml.Service;

public class JsonService
{

    public JsonService() { }

    public void SendJsonToApi(string jsonData)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Put, AppConstants.ApiUrl)
                {
                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                };

                HttpResponseMessage response = client.Send(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to send data: {response.StatusCode}");

                    string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult(); 
                    Console.WriteLine($"Response Body: {responseBody}");
                }
                else
                {
                    Console.WriteLine("Data sent successfully.");
                }
            }
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"Request error: {httpEx.ToString()}");
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.ToString()}");
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
