using System;
using System.Linq;
using System.Linq.Expressions;

namespace Discussion.Web.Data
{
    static class FilterByMemberCriteriaComposer
    {
        public static Expression<Func<TEntry, bool>> Compose<TEntry, TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys)
        {
            var parameters = selector.Parameters;
            var memberValue = Expression.Invoke(selector, parameters);

            Expression<Func<TMember, bool>> contains = member => keys.Contains(member);
            var containsMember = Expression.Invoke(contains, memberValue);

            var valueSelector = Expression.Lambda(containsMember, parameters) as Expression<Func<TEntry, bool>>;
            return valueSelector;
        }

        public static Expression<Func<TEntry, bool>> ComposeByField<TEntry, TMember>(string fieldName, params TMember[] keys)
        {
            var entryParameter = Expression.Parameter(typeof(TEntry), "entry");
            var memberExpr = Expression.PropertyOrField(entryParameter, fieldName);
            var selector = Expression.Lambda(memberExpr, entryParameter) as Expression<Func<TEntry, TMember>>;

            return Compose(selector, keys);
        }
    }
}
