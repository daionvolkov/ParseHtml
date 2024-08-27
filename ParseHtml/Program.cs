using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;
using ParseHtml;

class Program
{
    static void Main()
    {
        string filePath = AppConstants.htmlFilePath;

        var processor = new MessageProcessor(filePath, AppConstants.outputFilePath);

        processor.ProcessMessages();
    }
}
