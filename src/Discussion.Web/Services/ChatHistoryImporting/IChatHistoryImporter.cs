using System.Collections.Generic;
using System.Threading.Tasks;
using Discussion.Core.Models;

namespace Discussion.Web.Services.ChatHistoryImporting
{
    public interface IChatHistoryImporter
    {
        Task<List<Reply>> Import(ChatMessage[] wechatMessages);
    }
}