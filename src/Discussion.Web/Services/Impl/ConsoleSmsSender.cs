using System.Threading.Tasks;

namespace Discussion.Web.Services.Impl
{
    public class ConsoleSmsSender: ISmsSender
    {
        public Task SendMessageAsync(string phoneNumber, string content)
        {
            throw new System.NotImplementedException();
        }
    }
}