using System.Text.Json.Serialization;

namespace QMS.Models
{
    public class VoiceMessageModel
    {
        public string TransactionId { get; set; }
        public string AudioData { get; set; }
    }
}
