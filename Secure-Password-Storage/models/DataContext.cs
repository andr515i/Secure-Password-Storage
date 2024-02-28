using Microsoft.EntityFrameworkCore;

namespace Secure_Password_Storage.models
{
	public class DataContext : DbContext 
	{ 
		private IConfiguration _configuration;
        public DataContext(DbContextOptions<DataContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
		}
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		   => optionsBuilder.UseSqlServer(_configuration.GetConnectionString("connString"));

		public DbSet<Users> Users { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Users>().HasNoKey() ;
		}

	}
}
