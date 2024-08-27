using HtmlAgilityPack;
using ParseHtml.Service;

namespace ParseHtml;

public class MessageProcessor
{
    private readonly string _filePath;
    private readonly string _outputFilePath;
    private readonly JsonService _jsonService;
    private readonly CheckData _checkData;
    private readonly TextNodeService _textNodeService;
    private readonly PhotoNodeService _photoNodeService;
    private readonly VoiceNodeService _voiceNodeService;



    public MessageProcessor(string filePath, string outputFilePath)
    {
        _filePath = filePath;
        _outputFilePath = outputFilePath;
        _jsonService = new JsonService();
        _checkData = new CheckData();
        _textNodeService = new TextNodeService();
        _photoNodeService = new PhotoNodeService();
        _voiceNodeService = new VoiceNodeService();
    }

    public void ProcessMessages()
    {
        var doc = new HtmlDocument();
        doc.Load(_filePath);

        var bodyNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'body')]");

        if (bodyNodes == null)
        {
            Console.WriteLine("Failed to find items with class 'body'.");
            return;
        }

        string? lastDateValue = null;
        var jsonObjects = new List<string>();
        int messageCount = 1;

        foreach (var bodyNode in bodyNodes)
        {

            if (_checkData.ShouldProcessNode(bodyNode))
            {
                lastDateValue = _checkData.GetDateFromBodyNode(bodyNode, lastDateValue);

                _textNodeService.ProcessTextNode(bodyNode, lastDateValue, jsonObjects, ref messageCount);
                _photoNodeService.ProcessPhotoNode(bodyNode, lastDateValue, jsonObjects, ref messageCount);
                _voiceNodeService.ProcessVoiceNode(bodyNode, lastDateValue, jsonObjects, ref messageCount);

                Thread.Sleep(500);
            }
            _jsonService.SaveJsonToFile(jsonObjects, _outputFilePath);
        }
    }

}

