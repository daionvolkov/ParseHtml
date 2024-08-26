using Models;

namespace ParseHtml.Service;

public class MessageDataService
{
    public MessageData CreateDataObject(string? date, string content, int messageCount)
    {
        var contentData = new ContentData 
        {
            UserId = AppConstants.userId,
            Title = $"{AppConstants.title} {messageCount}",
            TargetLanguageId = AppConstants.targetLanguageId,
            WrittenLanguageId = AppConstants.writtenLanguageId,
            CategoryId = AppConstants.categoryId,
            PublishDate = date,
            Content = content,
            MaterialType = AppConstants.materialType
        };

        return new MessageData
        {
            Token = AppConstants.token, 
            ContentDatas = contentData
            
        };
    }
}
