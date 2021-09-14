# XrmTools.QueryBuilder

A fluent API to simplify creating QueryExpressions

## Dependencies

- .NET 4.7.2
- Nuget package: Microsoft.CrmSdk.CoreAssemblies 9.0.x

## Example usages

_Note_: All examples assume that `client` is an instance of `CrmServiceClient`

**Query with a condition and specify columns**

```csharp
var query = new XrmQueryBuilder("crb2c_prospects")
    .WhereGreaterThan("crb2c_contractamount", 2000)
    .AddColumns("crb2c_contractamount", "crb2c_prospectname", "crb2c_probability");
var response = client.RetrieveMultiple(query);
```

**Query with conditions, specific columns, and a join to another entity**

```csharp
var query = new XrmQueryBuilder("crb2c_invoice")
    .AddColumns("crb2c_invoicedescription", "crb2c_invoiceamount")
    .WhereGreaterThan("crb2c_invoiceamount", 1000m)
    .Link("crb2c_customerid", "crb2c_customer", "crb2c_customerid", new[] { "crb2c_customername" },
        link => link.SetEntityAlias("customer").WhereEquals("crb2c_customername", "Jack Johnson"));
var response = client.RetrieveMultiple(query);
```