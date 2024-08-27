using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseHtml.Service
{
    public class MediaConverService
    {
        public string ConvertMediaToBase64(string mediaPath, string mediaType)
        {
            if (string.IsNullOrWhiteSpace(mediaPath) || !File.Exists(mediaPath))
            {
                throw new FileNotFoundException("The media file does not exist.", mediaPath);
            }

            byte[] mediaBytes;
            try
            {
                mediaBytes = File.ReadAllBytes(mediaPath);
            }
            catch (Exception ex)
            {
                throw new IOException($"Error reading the media file: {ex.Message}", ex);
            }

            string base64String = Convert.ToBase64String(mediaBytes);

            string extension = Path.GetExtension(mediaPath).ToLowerInvariant();
            string mimeType = mediaType switch
            {
                "image" => extension switch
                {
                    ".png" => "data:image/png;base64,",
                    ".jpg" => "data:image/jpeg;base64,",
                    ".jpeg" => "data:image/jpeg;base64,",
                    _ => throw new NotSupportedException($"Unsupported image format: {extension}")
                },
                "audio" => extension switch
                {
                    ".ogg" => "data:audio/ogg;base64,",
                    ".mp3" => "data:audio/mpeg;base64,",
                    _ => throw new NotSupportedException($"Unsupported audio format: {extension}")
                },
                _ => throw new NotSupportedException($"Unsupported media type: {mediaType}")
            };

            return $"{mimeType}{base64String}";
        }
    }
}
