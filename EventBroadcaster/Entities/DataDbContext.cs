using Commun;
using Commun.Models;
using Microsoft.EntityFrameworkCore;

namespace EventBroadcaster.Entities
{
    public class DataDbContext : DbContext
    {
        public virtual DbSet<User> User { get; set; }


        public virtual DbSet<SequenceNumber> SequenceNumber { get; set; }
        public virtual DbSet<EventLog> EventLog { get; set; }

        public DataDbContext()
        {
        }

        public DataDbContext(DbContextOptions<DataDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Constants.DbConnectionString);
            }
        }
    }
}
