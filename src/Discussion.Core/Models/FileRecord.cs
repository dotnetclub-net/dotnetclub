namespace Discussion.Core.Models
{
    public class FileRecord: Entity
    {
        public int UploadedBy { get; set; }
        public long Size { get; set; }
        public string OriginalName { get; set; }
        public string StoragePath { get; set; }
        public string Category { get; set; }
        public string Slug { get; set; }
    }
}