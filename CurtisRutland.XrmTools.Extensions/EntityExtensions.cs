using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;

namespace CurtisRutland.XrmTools.Extensions
{
    public static class EntityExtensions
    {
        public static Entity CloneEmpty(this Entity e) => new Entity(e.LogicalName) { Id = e.Id };

        public static T GetAttributeValueWithError<T>(this Entity e, string attributeName)
        {
            object value;
            try
            {
                value = e[attributeName];
            }
            catch (Exception ex)
            {
                var msg = $"No attribute named {attributeName} was present on Entity. Id: {e.Id} | LogicalName: {e.LogicalName}";
                throw new Exception(msg, ex);
            }
            return (T)value;
        }

        public static string GetOptionSetText(this Entity entity, string attribute, IOrganizationService orgService)
        {
            if (!entity.Attributes.ContainsKey(attribute)) return null;
            var osv = entity.GetAttributeValue<OptionSetValue>(attribute);
            var request = new RetrieveAttributeRequest { EntityLogicalName = entity.LogicalName, LogicalName = attribute };
            var response = orgService.Execute(request) as RetrieveAttributeResponse;
            var metadata = (PicklistAttributeMetadata)response?.AttributeMetadata;
            if (metadata == null) return null;
            var selected = metadata.OptionSet.Options.FirstOrDefault(o => o.Value == osv.Value);
            return selected?.Label.UserLocalizedLabel.Label;
        }

        public static string GetString(this Entity e, string attributeName) => e.GetAttributeValue<string>(attributeName);

        public static int? GetInt(this Entity e, string attributeName) => e.GetAttributeValue<int?>(attributeName);

        public static double? GetDouble(this Entity e, string attributeName) => e.GetAttributeValue<double?>(attributeName);

        public static EntityReference GetReference(this Entity e, string attributeName) => e.GetAttributeValue<EntityReference>(attributeName);

        public static bool? GetBool(this Entity e, string attributeName) => e.GetAttributeValue<bool?>(attributeName);

        public static decimal? GetDecimal(this Entity e, string attributeName) => e.GetAttributeValue<decimal?>(attributeName);

        public static decimal? GetMoneyValue(this Entity e, string attributeName) => e.GetAttributeValue<Money>(attributeName)?.Value;

        public static int? GetOptionSetValue(this Entity e, string attributeName) => e.GetAttributeValue<OptionSetValue>(attributeName)?.Value;

        public static DateTime? GetDate(this Entity e, string attributeName) => e.GetAttributeValue<DateTime?>(attributeName);

        public static DateTime? GetLocalDate(this Entity e, string attributeName) => e.GetDate(attributeName)?.ToLocalTime();

        public static string GetFormattedValue(this Entity e, string attributeName) => e.FormattedValues.ContainsKey(attributeName) ? e.FormattedValues[attributeName] : null;

        public static T GetAliasedValue<T>(this Entity e, string alias, string attributeName)
        {
            var aliasedValue = e.GetAttributeValue<AliasedValue>($"{alias}.{attributeName}");
            if (aliasedValue?.Value == null) return default;
            return (T)aliasedValue.Value;
        }
    }
}
