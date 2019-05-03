using Discussion.Web.Services.ChatHistoryImporting;

namespace Discussion.Web.Tests.Stubs
{
    /// <summary>
    /// 当与 Moq 一起使用时，此类无需提供任何方法的桩实现
    /// 仅为解决在 Mock 父类时，需要提供"默认构造函数"的问题
    /// </summary>
    /// <remarks>
    /// 当使用 Virtuosity.Fody 时，父类的各个方法将默认标记为 virtual
    /// 请参考：https://github.com/Fody/Virtuosity
    /// </remarks>
    public class ChatyApiServiceMock : ChatyApiService
    {
        public ChatyApiServiceMock() : base(null, null, null)
        {
        }
    }
}