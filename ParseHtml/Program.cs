using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;
using ParseHtml;

class Program
{
    static async Task Main(string[] args)
    {
        string filePath = AppConstants.htmlFilePath;
        string apiUrl = AppConstants.ApiUrl;
        

        var processor = new MessageProcessor(filePath, apiUrl, AppConstants.outputFilePath);

        await processor.ProcessMessagesAsync();
    }
}
