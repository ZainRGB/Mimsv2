namespace Mimsv2.Models
{
    public class PendingAddendum
    {
        public string AddendumName { get; set; }
        public string Criteria { get; set; }
        public byte[] FileBytes { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
