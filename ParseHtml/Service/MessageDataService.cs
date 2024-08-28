using System.Globalization;
using Models;

namespace ParseHtml.Service;

public class MessageDataService
{
    public MessageData CreateDataObject(string date, string content, int messageCount)
    {
        if(string.IsNullOrEmpty(date)) {
            date = "01.01.2024 01:01:01 UTC+02:00";
        }
        
        string iso8601Date = "";
        string format = "dd.MM.yyyy HH:mm:ss 'UTC'K";
        
        try
        {
            DateTimeOffset parsedDate = DateTimeOffset.ParseExact(date, format, CultureInfo.InvariantCulture);
            iso8601Date = parsedDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
        catch(FormatException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    


        var learningMaterial = new LearningMaterial
        {
            UserId = AppConstants.userId,
            Title = $"{AppConstants.title} {messageCount}",
            TargetLanguageId = AppConstants.targetLanguageId,
            WrittenLanguageId = AppConstants.writtenLanguageId,
            CategoryId = AppConstants.categoryId,
            PublishDate = iso8601Date,
            Content = content,
            MaterialType = AppConstants.materialType,
            Tags = AppConstants.hashtag,
        };


        return new MessageData
        {
            Token = AppConstants.token,
            LearningMaterial = learningMaterial
            
        };
    }
}
