namespace Discussion.Core.Data
{
    public interface IReadonlyDataSettings
    {
        bool IsReadonly { get; }
    }
    
    public class ReadonlyDataSettings : IReadonlyDataSettings
    {
        public bool IsReadonly { get; set; }
    }
}