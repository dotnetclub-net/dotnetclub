using Jusfr.Persistent;

namespace Discussion.Web.Models
{
    public abstract class Entity : IAggregate<int>
    {
        public int Id
        {
            get; set;
        }
    }
}
