using FaceApp.DAL.Extend;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceApp.DAL.Database
{
    public class FaceAppContext:IdentityDbContext<User>
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>(user =>
            {
                user.Property(x => x.FullName)
                .HasComputedColumnSql("[FirstName] + ' ' + [LastName]");
                user.Property(user => user.DateCreated)
                .HasDefaultValueSql("GetDate()");

                user.HasIndex(u => u.PhoneNumber)
                .IsUnique();
            });
            base.OnModelCreating(builder);
        }
        public FaceAppContext(DbContextOptions<FaceAppContext> options):base(options) { }
    }
}
