using System.Collections.Generic;
using Discussion.Core.Models;

namespace Discussion.Web.ViewModels
{
    public class TopicListViewModel
    {
        public List<Topic> Topics { get; set; }

        public int CurrentPage { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}