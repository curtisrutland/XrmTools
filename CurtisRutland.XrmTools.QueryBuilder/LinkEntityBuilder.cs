using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurtisRutland.XrmTools.QueryBuilder
{
    public class LinkEntityBuilder
    {
        private readonly LinkEntity _linkEntity;
        private readonly FilterExpressionBuilder _filterExpressionBuilder;

        /// <summary>
        /// Creates a new LinkEntityBuilder
        /// </summary>
        public LinkEntityBuilder()
        {
            _linkEntity = new LinkEntity { Columns = new ColumnSet(true) };
            _filterExpressionBuilder = new FilterExpressionBuilder(LogicalOperator.And);
        }

        /// <summary>
        /// Implicitly converts to a LinkEntity
        /// </summary>
        /// <param name="builder">The builder to convert</param>
        public static implicit operator LinkEntity(LinkEntityBuilder builder) => builder.ToLinkEntity();

        /// <summary>
        /// Explicitly converts the current LinkEntityBuilder to a LinkEntity
        /// </summary>
        /// <returns>The rendered LinkEntity</returns>
        public LinkEntity ToLinkEntity()
        {
            _linkEntity.LinkCriteria = _filterExpressionBuilder;
            return _linkEntity;
        }

        /// <summary>
        /// Links another LinkEntity to the one being built.
        /// </summary>
        /// <param name="linkEntity">LinkEntity to link to the currently-building LinkEntity</param>
        /// <returns>The current LinkEntityBuilder</returns>
        public LinkEntityBuilder Link(LinkEntity linkEntity)
        {
            _linkEntity.LinkEntities.Add(linkEntity);
            return this;
        }

        /// <summary>
        /// Sets the LinkToAttributeName and LinkToEntityName of the resulting LinkEntity
        /// </summary>
        /// <param name="linkToEntityName">The "Link To Entity Name" property</param>
        /// <param name="linkToAttributeName">The "Link to Attribute Name" property</param>
        /// <returns>The current LinkEntityBuilder</returns>
        public LinkEntityBuilder To(string linkToEntityName, string linkToAttributeName)
        {
            _linkEntity.LinkToAttributeName = linkToAttributeName;
            _linkEntity.LinkToEntityName = linkToEntityName;
            return this;
        }

        /// <summary>
        /// Sets the LinkFromEntityName and LinkFromAttributeName of the resulting LinkEntity
        /// </summary>
        /// <param name="linkFromEntityName">The "Link From Entity Name" property</param>
        /// <param name="linkFromAttributeName">The "Link From Attribute Name" property</param>
        /// <returns>The current LinkEntityBuilder</returns>
        public LinkEntityBuilder From(string linkFromEntityName, string linkFromAttributeName)
        {
            _linkEntity.LinkFromAttributeName = linkFromAttributeName;
            _linkEntity.LinkFromEntityName = linkFromEntityName;
            return this;
        }

        public LinkEntityBuilder WhereNotNull(string attributeName) => Where(attributeName, ConditionOperator.NotNull, null);

        public LinkEntityBuilder WhereNull(string attributeName) => Where(attributeName, ConditionOperator.Null, null);

        public LinkEntityBuilder WhereEquals(string attributeName, object value) => Where(attributeName, ConditionOperator.Equal, value);

        public LinkEntityBuilder WhereNotEquals(string attributeName, object value) => Where(attributeName, ConditionOperator.NotEqual, value);

        public LinkEntityBuilder WhereEqualsIf<T>(string attributeName, T value, Func<T, bool> condition) => condition(value) ? WhereEquals(attributeName, value) : this;

        public LinkEntityBuilder WhereNotEqualsIf<T>(string attributeName, T value, Func<T, bool> condition) => condition(value) ? WhereNotEquals(attributeName, value) : this;

        public LinkEntityBuilder WhereIn<T>(string attributeName, params T[] values)
        {
            _filterExpressionBuilder.AddConditionArray(attributeName, ConditionOperator.In, values);
            return this;
        }

        public LinkEntityBuilder WhereIn<T>(string attributeName, IEnumerable<T> values)
        {
            var vals = values.ToArray();
            return WhereIn(attributeName, vals);
        }

        public LinkEntityBuilder Where(string attributeName, ConditionOperator op, object value)
        {
            _filterExpressionBuilder.AddCondition(attributeName, op, value);
            return this;
        }

        public LinkEntityBuilder AddColumns(params string[] columnNames)
        {
            _linkEntity.Columns.AllColumns = false;
            _linkEntity.Columns.AddColumns(columnNames);

            return this;
        }
    }
}
