using MongoDB.Bson.Serialization.Attributes;
using QRCoder;

namespace ShareFile.Models
{
    public class ShareFileModel
    {
        [BsonId] // _id 
        public string GuidId { get; private set; }
        public string Name { get; private set; }
        public string Extension { get; private set; }
        public DateTime Date { get; private set; }
        public string Timestamp { get; private set; }
        public string UniqueFileName { get; private set; }
        public long Length { get; private set; }
        public int Duration { get; private set; } // minutes
        public string Url { get; private set; } = string.Empty;

        [BsonIgnore]
        public string QRCodeSvg { get; private set; } = string.Empty;

        public ShareFileModel(IFormFile file, int duration)
        {
            GuidId = Guid.NewGuid().ToString();
            Name = Path.GetFileNameWithoutExtension(file.FileName);
            Extension = Path.GetExtension(file.FileName);
            Date = DateTime.UtcNow;
            Timestamp = Date.ToString("yyyyMMddHHmmss");
            UniqueFileName = $"{Name}_{GuidId}_{Timestamp}{Extension}";
            Length = file.Length;
            Duration = duration;
        }

        public void ApplyUrl(string url)
        {
            Url = url;
            QRCodeSvg = GetQRCodeSvgString(url);
        }

        private string GetQRCodeSvgString(string preSignedUrl)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(preSignedUrl, QRCodeGenerator.ECCLevel.Q);
            SvgQRCode qrCode = new SvgQRCode(qrCodeData);
            string qrCodeSvgString = qrCode.GetGraphic(Configuration.MainConfig.PixelsPerModule);
            return qrCodeSvgString;
        }

        public override string ToString()
        {
            return $"Id:{GuidId}, Name:{Name}, Length:{Length}, Date:{Date}, Duration:{Duration}";
        }
    }
}