using Microsoft.Xrm.Tooling.Connector;
using System;
using CurtisRutland.XrmTools.QueryBuilder;
using CurtisRutland.XrmTools.Extensions;

namespace CurtisRutland.XrmTools.TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = ConnectionHelper.CreateClient())
            {
                Example2(client);
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static void Example1(CrmServiceClient client)
        {
            var query = new XrmQueryBuilder("crb2c_prospects")
                .WhereGreaterThan("crb2c_contractamount", 2000)
                .AddColumns("crb2c_contractamount", "crb2c_prospectname", "crb2c_probability");
            var response = client.RetrieveMultiple(query);
            var entities = response.Entities;
            foreach (var prospect in entities)
            {
                Console.WriteLine($"Prospect name: {prospect.GetString("crb2c_prospectname")}, Amount: {prospect.GetMoneyValue("crb2c_contractamount"):C2}, Probability: {prospect.GetInt("crb2c_probability")}");
            }
        }

        static void Example2(CrmServiceClient client)
        {
            var query = new XrmQueryBuilder("crb2c_invoice")
                .AddColumns("crb2c_invoicedescription", "crb2c_invoiceamount")
                .WhereGreaterThan("crb2c_invoiceamount", 1000m)
                .Link("crb2c_customerid", "crb2c_customer", "crb2c_customerid", new[] { "crb2c_customername" },
                    link => link.SetEntityAlias("customer").WhereEquals("crb2c_customername", "Jack Johnson"));
            var response = client.RetrieveMultiple(query);
            var entities = response.Entities;
            foreach (var invoice in entities)
            {
                var customerName = invoice.GetAliasedValue<string>("customer", "crb2c_customername");
                var desc = invoice.GetString("crb2c_invoicedescription");
                var amount = invoice.GetMoneyValue("crb2c_invoiceamount");
                var output = $"Desc: {desc}, amount: {amount:C2}, Cust Name: {customerName}";
                Console.WriteLine(output);
            }
        }
    }
}
