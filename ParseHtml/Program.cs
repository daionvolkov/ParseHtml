using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;
using ParseHtml;

class Program
{
    static async Task Main(string[] args)
    {
        string filePath = AppConstants.HtmlFilePath;
        string apiUrl = AppConstants.ApiUrl;
        string outputFilePath = @"/Users/daniilvolkov/Documents/Projects/Test/ParseHtml/output.json"; 

        var processor = new MessageProcessor(filePath, apiUrl, outputFilePath);

        await processor.ProcessMessagesAsync();
    }
}
