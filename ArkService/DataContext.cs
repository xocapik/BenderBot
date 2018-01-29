using xphps.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xphps
{
    public class DataContext : DbContext
    {
        public DataContext() 
        {
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
            // Set the properties for the data source.
            this.Configuration.LazyLoadingEnabled = false;

#if DEBUG
            sqlBuilder.DataSource = @"localhost";
#else
                sqlBuilder.DataSource = @"localhost\SQLEXPRESS";
#endif
            sqlBuilder.InitialCatalog = "wordpress";
            sqlBuilder.IntegratedSecurity = true;
            base.Database.Connection.ConnectionString = sqlBuilder.ToString();

        }
        public DbSet<Player> Players { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Activity> Activities { get; set; }
    }
}
