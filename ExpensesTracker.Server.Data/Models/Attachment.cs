using Newtonsoft.Json;

namespace ExpensesTracker.Server.Data.Models
{
    public class Attachment
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("uploadedByUserId")] 
        public string? UploadedByUserId { get; set; }
        
        [JsonProperty("url")] 
        public string? Url { get; set; }
        
        [JsonProperty("uploadingDate")] 
        public DateTime UploadingDate { get; set; }
    }
}
