using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurtisRutland.XrmTools.QueryBuilder
{
    /// <summary>
    /// QueryBuilder - A simple, fluent alternative to writing QueryExpressions manually
    /// </summary>
    public class XrmQueryBuilder : IQueryBuilder<XrmQueryBuilder>
    {
        protected readonly FilterExpressionBuilder _filterExpressionBuilder;
        protected readonly QueryExpression _queryExpression;

        /// <summary>
        /// Create a QueryBuilder for a specific entity
        /// </summary>
        /// <param name="entityName">Logical Name of Entity to query for</param>
        /// <param name="op">Operator by which query conditions are added to the FilterExpression</param>
        /// <param name="noLock">Sets the NoLock parameter for the resulting QueryExpression</param>
        public XrmQueryBuilder(string entityName, LogicalOperator op = LogicalOperator.And, bool noLock = true)
        {
            _queryExpression = new QueryExpression { EntityName = entityName, ColumnSet = new ColumnSet(true), NoLock = noLock };
            _filterExpressionBuilder = new FilterExpressionBuilder(op);
        }

        /// <summary>
        /// Creates a QueryBuilder with no Entity. Must call .For with an entity name.
        /// </summary>
        public XrmQueryBuilder() : this(string.Empty) { }

        /// <summary>
        /// Convert QueryBuilder to QueryExpression implicitly
        /// </summary>
        /// <param name="builder">QueryBuilder to convert to QueryExpression</param>
        public static implicit operator QueryExpression(XrmQueryBuilder builder) => builder.ToQuery();

        /// <summary>
        /// Adds a column or columns to the results. By default, all columns are returned.
        /// Calling this method sets AllColumns to false and returns only columns you explicitly add.
        /// This method can be called multiple times and columns will be added. 
        /// </summary>
        /// <param name="columnNames"></param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder AddColumns(params string[] columnNames)
        {
            _queryExpression.ColumnSet.AllColumns = false;
            _queryExpression.ColumnSet.AddColumns(columnNames);
            return this;
        }

        /// <summary>
        /// Condition for when CreatedOn is Today
        /// </summary>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder CreatedToday() => WhereToday(Constants.CreatedOn);

        /// <summary>
        /// Condition for when CreatedOn is Yesterday
        /// </summary>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder CreatedYesterday() => WhereYesterday(Constants.CreatedOn);

        /// <summary>
        /// Set or change the Entity that the QueryBuilder is building a query for
        /// </summary>
        /// <param name="entityName">Logical Name of the entity to query for</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder For(string entityName)
        {
            _queryExpression.EntityName = entityName;
            return this;
        }

        /// <summary>
        /// Adds a LinkEntity to the current query
        /// </summary>
        /// <param name="linkEntity">the LinkEntity to add to the query</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder Link(LinkEntity linkEntity)
        {
            _queryExpression.LinkEntities.Add(linkEntity);
            return this;
        }

        /// <summary>
        /// Adds a LinkEntity to the current query, built from provided parameters
        /// </summary>
        /// <param name="fromAttributeName">Link From Attribute Name</param>
        /// <param name="toEntityLogicalName">Link To Entity Logical Name</param>
        /// <param name="toAttributeName">Link To Attribute Name</param>
        /// <param name="linkColumns">Columns to add to the LinkEntity</param>
        /// <param name="expression">Callback to further manipulate the LinkEntity before it is added to the query</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder Link(string fromAttributeName, string toEntityLogicalName, string toAttributeName, string[] linkColumns = null, Func<LinkEntityBuilder, LinkEntityBuilder> expression = null)
        {
            var link = new LinkEntityBuilder()
                .From(_queryExpression.EntityName, fromAttributeName)
                .To(toEntityLogicalName, toAttributeName);

            if (expression != null)
                link = expression(link);

            if (linkColumns?.Any() ?? false)
                link.AddColumns(linkColumns);

            return Link(link);
        }

        /// <summary>
        /// Condition for a Date column matching Today
        /// </summary>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder ModifiedToday() => WhereToday(Constants.ModifiedOn);

        /// <summary>
        /// Condition for a Date column matching Yesterday
        /// </summary>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder ModifiedYesterday() => WhereYesterday(Constants.ModifiedOn);

        /// <summary>
        /// Adds an Order to the QueryExpression, Ascending
        /// </summary>
        /// <param name="attributeName">Attribute name to order the results by</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder OrderByAscending(string attributeName)
        {
            _queryExpression.AddOrder(attributeName, OrderType.Ascending);
            return this;
        }

        /// <summary>
        /// Adds an Order to the QueryExpression, Descending
        /// </summary>
        /// <param name="attributeName">Attribute name to order the results by</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder OrderByDescending(string attributeName)
        {
            _queryExpression.AddOrder(attributeName, OrderType.Descending);
            return this;
        }

        /// <summary>
        /// Clears any added columns and resets to a new ColumnSet
        /// </summary>
        /// <param name="allColumns">Set to true to include all columns</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder ResetColumnSet(bool allColumns = false)
        {
            _queryExpression.ColumnSet = new ColumnSet(allColumns);
            return this;
        }

        /// <summary>
        /// Adds a PagingInfo to the Query
        /// </summary>
        /// <param name="pageInfo">The PagingInfo to add to the query</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder SetPagingInfo(PagingInfo pageInfo)
        {
            _queryExpression.PageInfo = pageInfo;
            return this;
        }

        /// <summary>
        /// Sets PagingInfo to a single page with a page size of 1. Useful for when you are looking for a single result.
        /// </summary>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder OneResult() => SetPagingInfo(new PagingInfo
        {
            Count = 1,
            PageNumber = 1,
            ReturnTotalRecordCount = false,
            PagingCookie = null
        });

        /// <summary>
        /// Explicitly convert this QueryBuilder to a QueryExpression
        /// </summary>
        /// <returns>The built QueryExpression</returns>
        public QueryExpression ToQuery()
        {
            _queryExpression.Criteria = _filterExpressionBuilder.ToFilterExpression();
            return _queryExpression;
        }

        /// <summary>
        /// Adds a filter to the Query. This is the base for all the other Where* methods.
        /// Use this if you need to use a ConditionOperator that does not have an explicit Where* method.
        /// </summary>
        /// <param name="attributeName">The attribute to filter against</param>
        /// <param name="op">The ConditionOperator to use in the filter</param>
        /// <param name="value">The value to filter with</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder Where(string attributeName, ConditionOperator op, object value = null)
        {
            _filterExpressionBuilder.AddCondition(attributeName, op, value);
            return this;
        }

        /// <summary>
        /// Adds a filter to the Query to return active records.
        /// </summary>
        /// <param name="attributeName">defaults to statecode</param>
        /// <param name="value">defaults to 0</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereActive(string attributeName = "statecode", int value = 0) => WhereEquals(attributeName, value);

        /// <summary>
        /// Adds a filter to the Query to return records where the attribute equals the provided value
        /// </summary>
        /// <param name="attributeName">The attribute to filter against</param>
        /// <param name="value">The value that the attribute must match</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereEquals(string attributeName, object value) => Where(attributeName, ConditionOperator.Equal, value);

        /// <summary>
        /// Conditionally dds a filter to the Query to return records where the attribute equals the provided value, if the provided condition callback returns true
        /// </summary>
        /// <param name="attributeName">The attribute to filter against</param>
        /// <param name="value">The value that the attribute must match</param>
        /// <param name="condition">Callback that must return true for this criteria to be added to the query</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereEqualsIf<T>(string attributeName, T value, Func<T, bool> condition) => condition(value) ? WhereEquals(attributeName, value) : this;

        /// <summary>
        /// Adds a filter to the Query to return records where the attribute value is false. Can only be used for Boolean attributes.
        /// </summary>
        /// <param name="attributeName">The name of the boolean attribute to filter against</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereFalse(string attributeName) => WhereEquals(attributeName, false);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is contained in the provided collection of values.
        /// </summary>
        /// <typeparam name="T">generic type parameter for provided IEnumerable</typeparam>
        /// <param name="attributeName">The attribute to filter against</param>
        /// <param name="values">IEnumerable of values that the attribute must match one of to be included in the results</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereIn<T>(string attributeName, IEnumerable<T> values) => WhereIn(attributeName, values.ToArray());

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is contained in the provided params array of values.
        /// </summary>
        /// <typeparam name="T">generic type parameter for provided array</typeparam>
        /// <param name="attributeName">The attribute to filter against</param>
        /// <param name="values">params array of values that the attribute must match one of to be included in the results</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereIn<T>(string attributeName, params T[] values)
        {
            _filterExpressionBuilder.AddConditionArray(attributeName, ConditionOperator.In, values);
            return this;
        }

        /// <summary>
        /// Adds a filter to the Query to return inactive records.
        /// </summary>
        /// <param name="attributeName">defaults to statecode</param>
        /// <param name="value">defaults to 1</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereInactive(string attributeName = "statecode", int value = 1) => WhereEquals(attributeName, value);

        /// <summary>
        /// Adds a filter to the Query to return records where the attribute does not equal the provided value
        /// </summary>
        /// <param name="attributeName">The attribute to filter against</param>
        /// <param name="value">The value that the attribute must not match</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereNotEquals(string attributeName, object value) => Where(attributeName, ConditionOperator.NotEqual, value);

        /// <summary>
        /// Conditionally dds a filter to the Query to return records where the attribute does not equal the provided value, if the provided condition callback returns true
        /// </summary>
        /// <param name="attributeName">The attribute to filter against</param>
        /// <param name="value">The value that the attribute must not match</param>
        /// <param name="condition">Callback that must return true for this criteria to be added to the query</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereNotEqualsIf<T>(string attributeName, T value, Func<T, bool> condition) => condition(value) ? WhereNotEquals(attributeName, value) : this;

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is not contained in the provided collection of values.
        /// </summary>
        /// <typeparam name="T">generic type parameter for provided IEnumerable</typeparam>
        /// <param name="attributeName">The attribute to filter against</param>
        /// <param name="values">IEnumerable of values that the attribute must not match any of to be included in the results</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereNotIn<T>(string attributeName, IEnumerable<T> values) => WhereNotIn(attributeName, values.ToArray());

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is not contained in the provided params array of values.
        /// </summary>
        /// <typeparam name="T">generic type parameter for provided array</typeparam>
        /// <param name="attributeName">The attribute to filter against</param>
        /// <param name="values">params array of values that the attribute must not match any of to be included in the results</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereNotIn<T>(string attributeName, params T[] values)
        {
            _filterExpressionBuilder.AddConditionArray(attributeName, ConditionOperator.NotIn, values);
            return this;
        }

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is not null.
        /// </summary>
        /// <param name="attributeName">The attribute that must not be null</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereNotNull(string attributeName) => Where(attributeName, ConditionOperator.NotNull);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is null
        /// </summary>
        /// <param name="attributeName">The attribute that must be null</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereNull(string attributeName) => Where(attributeName, ConditionOperator.Null);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Today. Must be a Date/DateTime column.
        /// </summary>
        /// <param name="attributeName">The Date/DateTime Column that must match Today</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereToday(string attributeName) => Where(attributeName, ConditionOperator.Today);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is true. Must be a Boolean column.
        /// </summary>
        /// <param name="attributeName">The Boolean attribute that must be true</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereTrue(string attributeName) => WhereEquals(attributeName, true);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Yesterday. Must be a Date/DateTime column.
        /// </summary>
        /// <param name="attributeName">The Date/DateTime Column that must match Yesterday</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereYesterday(string attributeName) => Where(attributeName, ConditionOperator.Yesterday);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Greater Than the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be greater than the provided value</param>
        /// <param name="value">The value that the attribute must be greater than</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereGreaterThan(string attributeName, int value) => Where(attributeName, ConditionOperator.GreaterThan, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Greater Than the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be greater than the provided value</param>
        /// <param name="value">The value that the attribute must be greater than</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereGreaterThan(string attributeName, double value) => Where(attributeName, ConditionOperator.GreaterThan, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Greater Than the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be greater than the provided value</param>
        /// <param name="value">The value that the attribute must be greater than</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereGreaterThan(string attributeName, decimal value) => Where(attributeName, ConditionOperator.GreaterThan, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Greater Than or Equal To the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be greater than or equal to the provided value</param>
        /// <param name="value">The value that the attribute must be greater than or equal to</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereGreaterOrEqual(string attributeName, int value) => Where(attributeName, ConditionOperator.GreaterEqual, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Greater Than or Equal To the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be greater than or equal to the provided value</param>
        /// <param name="value">The value that the attribute must be greater than or equal to</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereGreaterOrEqual(string attributeName, double value) => Where(attributeName, ConditionOperator.GreaterEqual, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Greater Than or Equal To the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be greater than or equal to the provided value</param>
        /// <param name="value">The value that the attribute must be greater than or equal to</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereGreaterOrEqual(string attributeName, decimal value) => Where(attributeName, ConditionOperator.GreaterEqual, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Less Than the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be less than the provided value</param>
        /// <param name="value">The value that the attribute must be less than</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereLessThan(string attributeName, int value) => Where(attributeName, ConditionOperator.LessThan, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Less Than the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be less than the provided value</param>
        /// <param name="value">The value that the attribute must be less than</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereLessThan(string attributeName, double value) => Where(attributeName, ConditionOperator.LessThan, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Less Than the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be less than the provided value</param>
        /// <param name="value">The value that the attribute must be less than</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereLessThan(string attributeName, decimal value) => Where(attributeName, ConditionOperator.LessThan, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Less Than or Equal To the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be less than or equal to the provided value</param>
        /// <param name="value">The value that the attribute must be less than or equal to</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereLessOrEqual(string attributeName, int value) => Where(attributeName, ConditionOperator.LessEqual, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Less Than or Equal To the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be less than or equal to the provided value</param>
        /// <param name="value">The value that the attribute must be less than or equal to</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereLessOrEqual(string attributeName, double value) => Where(attributeName, ConditionOperator.LessEqual, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is Less Than or Equal To the provided value.
        /// </summary>
        /// <param name="attributeName">the numeric column that must be less than or equal to the provided value</param>
        /// <param name="value">The value that the attribute must be less than or equal to</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereLessOrEqual(string attributeName, decimal value) => Where(attributeName, ConditionOperator.LessEqual, value);

        /// <summary>
        /// Adds a filter to the query to return records where the attribute value is "like" the provided value.
        /// </summary>
        /// <param name="attributeName">The string column to match against</param>
        /// <param name="value">The value that contains the pattern to use in the match</param>
        /// <returns>This QueryBuilder instance</returns>
        public XrmQueryBuilder WhereLike(string attributeName, string value) => Where(attributeName, ConditionOperator.Like, value);
    }
}
