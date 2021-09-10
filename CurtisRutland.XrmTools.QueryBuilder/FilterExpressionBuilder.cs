using Microsoft.Xrm.Sdk.Query;
using System.Linq;

namespace CurtisRutland.XrmTools.QueryBuilder
{
    public class FilterExpressionBuilder
    {
        private readonly FilterExpression _filterExpression;

        /// <summary>
        /// Creates a FilterExpression Builder. Implicitly casts to FilterExpression
        /// </summary>
        /// <param name="logicalOperator"></param>
        public FilterExpressionBuilder(LogicalOperator logicalOperator) => _filterExpression = new FilterExpression(logicalOperator);

        /// <summary>
        /// Implicitly converts Builder to FilterExpression
        /// </summary>
        /// <param name="builder">The Builder to convert</param>
        public static implicit operator FilterExpression(FilterExpressionBuilder builder) => builder.ToFilterExpression();

        /// <summary>
        /// Explicitly converts Builder to FilterExpression
        /// </summary>
        /// <returns>The current FilterExpressionBuilder resolved to a FilterExpression</returns>
        public FilterExpression ToFilterExpression() => _filterExpression;

        /// <summary>
        /// Adds a condition to the FilterExpression
        /// </summary>
        /// <param name="attributeName">Attribute to apply the filter against</param>
        /// <param name="op">The operator to use with the filter</param>
        /// <param name="value">Value to filter with</param>
        /// <returns>The current FilterExpressionBuilder</returns>
        public FilterExpressionBuilder AddCondition(string attributeName, ConditionOperator op, object value)
        {
            var conditionExpression = new ConditionExpression
            {
                AttributeName = attributeName,
                Operator = op
            };

            if (value != null)
                conditionExpression.Values.Add(value);

            _filterExpression.Conditions.Add(conditionExpression);

            return this;
        }

        /// <summary>
        /// Add a condition with multiple values to the FilterExpression
        /// </summary>
        /// <param name="attributeName">Attribute to apply filter against</param>
        /// <param name="op">The operator to use with the filter. Should be one of the multi-value operators</param>
        /// <param name="values">The values to filter with</param>
        /// <returns>The current FilterExpressionBuilder</returns>
        public FilterExpressionBuilder AddConditionArray<T>(string attributeName, ConditionOperator op, params T[] values)
        {
            var conditionExpression = new ConditionExpression
            {
                AttributeName = attributeName,
                Operator = op
            };

            if (values != null)
                conditionExpression.Values.AddRange(values.Select(v => (object)v).ToArray());

            _filterExpression.Conditions.Add(conditionExpression);

            return this;
        }
    }
}
