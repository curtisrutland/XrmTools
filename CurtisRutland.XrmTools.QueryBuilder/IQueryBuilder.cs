using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace CurtisRutland.XrmTools.QueryBuilder
{
    public interface IQueryBuilder<Q> where Q : IQueryBuilder<Q>
    {
        Q AddColumns(params string[] columnNames);
        Q For(string entityName);
        Q Link(LinkEntity linkEntity);
        Q Link(string fromAttributeName, string toEntityLogicalName, string toAttributeName, string[] linkColumns = null, Func<LinkEntityBuilder, LinkEntityBuilder> expression = null);
        Q OrderByAscending(string attributeName);
        Q OrderByDescending(string attributeName);
        Q ResetColumnSet(bool allColumns = false);
        Q SetPagingInfo(PagingInfo pageInfo);
        QueryExpression ToQuery();
        Q Where(string attributeName, ConditionOperator op, object value = null);
        Q WhereActive(string attributeName = "statecode", int value = 0);
        Q WhereEquals(string attributeName, object value);
        Q WhereEqualsIf<T>(string attributeName, T value, Func<T, bool> condition);
        Q WhereFalse(string attributeName);
        Q WhereIn<T>(string attributeName, IEnumerable<T> values);
        Q WhereIn<T>(string attributeName, params T[] values);
        Q WhereInactive(string attributeName = "statecode", int value = 1);
        Q WhereNotEquals(string attributeName, object value);
        Q WhereNotEqualsIf<T>(string attributeName, T value, Func<T, bool> condition);
        Q WhereNotIn<T>(string attributeName, IEnumerable<T> values);
        Q WhereNotIn<T>(string attributeName, params T[] values);
        Q WhereNotNull(string attributeName);
        Q WhereNull(string attributeName);
        Q WhereToday(string attributeName);
        Q WhereTrue(string attributeName);
        Q WhereYesterday(string attributeName);
        Q CreatedToday();
        Q ModifiedToday();
        Q CreatedYesterday();
        Q ModifiedYesterday();

    }
}
