using Microsoft.Xrm.Tooling.Connector;
using System;

namespace CurtisRutland.XrmTools.TestProject
{
    static class ConnectionHelper
    {
        static ConnectionHelper() => DotNetEnv.Env.Load();

        public static CrmServiceClient CreateClient()
        {
            string url = Environment.GetEnvironmentVariable("ORG_URL"); 
            string user = Environment.GetEnvironmentVariable("ORG_USERNAME");
            string pass = Environment.GetEnvironmentVariable("ORG_PASSWORD");

            string conn = $@"Url = {url};AuthType = OAuth;UserName = {user};Password = {pass};AppId = 51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri = app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto;RequireNewInstance = True";
            return new CrmServiceClient(conn);
        }
    }
}
