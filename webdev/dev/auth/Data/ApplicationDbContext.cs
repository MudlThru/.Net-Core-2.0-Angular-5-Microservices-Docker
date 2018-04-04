using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore.Metadata; 

namespace auth.Data { 
    
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>//DbContext 
    { 
        
        #region Constructor 
        
        public ApplicationDbContext( DbContextOptions options) : base( options) { } 
        
        #endregion Constructor 
        
        #region Methods 
        
        protected override void OnModelCreating( ModelBuilder modelBuilder) { 

            base.OnModelCreating( modelBuilder); 

            modelBuilder.Entity<ApplicationUser>().ToTable("Users"); 
            modelBuilder.Entity<ApplicationUser>().HasMany(u => u.Tokens).WithOne(i=> i.User);

            modelBuilder.Entity<Token>().ToTable("Tokens");
            modelBuilder.Entity<Token>().Property(i => i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Token>().HasOne(i=>i.User).WithMany(u => u.Tokens);

        } 
            
        #endregion Methods 
        
        #region Properties 
        
        public DbSet <Token> Tokens { get; set; }
        
        #endregion Properties 
    } 
}


