using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ControlWorks.Common
{
    public class SqlConnectionService
    {
        public string OrdersTableName => _ordersTableName;
        public string ProductionTableName => _productionTableName;

        private readonly string _serverName;
        private readonly string _databaseName;
        private readonly string _ordersTableName;
        private readonly string _productionTableName;
        private readonly bool _integratedSecurity;
        private readonly string _userName;
        private readonly string _password;

        public SqlConnectionService()
        {
            _serverName = ConfigurationManager.AppSettings["ServerName"];
            _databaseName = ConfigurationManager.AppSettings["DatabaseName"]; 
            _ordersTableName = ConfigurationManager.AppSettings["OrdersTableName"];
            _productionTableName = ConfigurationManager.AppSettings["ProductionTableName"];
            var integratedSecurity = ConfigurationManager.AppSettings["IntegratedSecurity"];
            _userName = ConfigurationManager.AppSettings["Username"];
            _password = ConfigurationManager.AppSettings["Password"];

            _integratedSecurity = !integratedSecurity.Equals("False", StringComparison.OrdinalIgnoreCase);
        }

        public string OrdersConnectionString()
        {
            return ConnectionString(_ordersTableName);
        }

        public string ProductionConnectionString()
        {
            return ConnectionString(_productionTableName);
        }

        private string ConnectionString(string tableName)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = _serverName,
                
                InitialCatalog = _databaseName
            };


            if (_integratedSecurity)
            {
                builder.IntegratedSecurity = true;

            }
            else
            {
                builder.IntegratedSecurity = false;
                builder["User Id"] = _userName;
                builder["Password"] = _password;

            }
            return builder.ToString();
        }
    }
}
