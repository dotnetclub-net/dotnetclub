namespace Discussion.Core.Models
{
    public class FileRecord: Entity
    {
        public int UploadedBy { get; set; }
        public long Size { get; set; }
        public string OriginalName { get; set; }
        public string StoragePath { get; set; }
    }
}