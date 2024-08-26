using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;
using ParseHtml;

class Program
{
    static async Task Main(string[] args)
    {
        string filePath = AppConstants.htmlFilePath;
        
        

        var processor = new MessageProcessor(filePath, AppConstants.outputFilePath);

        await processor.ProcessMessagesAsync();
    }
}
