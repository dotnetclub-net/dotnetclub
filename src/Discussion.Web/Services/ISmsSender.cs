using System.Threading.Tasks;

namespace Discussion.Web.Services
{
    public interface ISmsSender
    {
        Task SendMessageAsync(string phoneNumber, string content);
    }
}