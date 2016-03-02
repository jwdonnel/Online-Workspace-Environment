using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.OracleClient;

/// <summary> Provides the List and Method for supported Database Providers
/// </summary>
public class DatabaseProviders {

    /// <summary> List of the available providers supported
    /// </summary>
    public static List<string> ProviderList = new List<string>() { 
        "System.Data.Odbc",
        "System.Data.OleDb",
        "System.Data.OracleClient",
        "System.Data.SqlClient",
        "System.Data.SqlServerCe.4.0"
    };

    /// <summary> Create the specific DbConnection object for the given provider
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static DbConnection CreateConnectionObject(string provider) {
        switch (provider) {
            case "System.Data.Odbc":
                return new OdbcConnection();

            case "System.Data.OleDb":
                return new OleDbConnection();

            case "System.Data.OracleClient":
                return new OracleConnection();

            case "System.Data.SqlClient":
                return new SqlConnection();

            case "System.Data.SqlServerCe.4.0":
                return new SqlCeConnection();

            default:
                try {
                    DbProviderFactory defaultfactory = DbProviderFactories.GetFactory(provider);
                    if (defaultfactory != null) {
                        return defaultfactory.CreateConnection();
                    }
                }
                catch {
                    return null;
                }
                break;
        }

        return null;
    }

}
