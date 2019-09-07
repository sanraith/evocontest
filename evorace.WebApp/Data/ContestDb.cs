using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace evorace.WebApp.Data
{
    public class ContestDb : IdentityDbContext<ApplicationUser>
    {
        public virtual DbSet<Submission> Submissions { get; set; }

        public virtual DbSet<Match> Matches { get; set; }

        public virtual DbSet<Measurement> Measurements { get; set; }

        public ContestDb(DbContextOptions<ContestDb> options)
            : base(options)
        { }


        public IQueryable<TProperty> Query<TEntity, TProperty>(TEntity entity,
            Expression<Func<TEntity, IEnumerable<TProperty>>> expression)
            where TEntity : class
            where TProperty : class
        {
            return Entry(entity).Collection(expression).Query();
        }

        public TProperty Load<TEntity, TProperty>(TEntity entity,
            Expression<Func<TEntity, TProperty>> expression)
            where TEntity : class
            where TProperty : class
        {
            Entry(entity).Reference(expression).Load();
            return expression.Compile().Invoke(entity);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            Configure(builder);
            Seed(builder);
        }

        private static void Configure(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ApplicationUser.Configuration());
        }

        private static void Seed(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(new
            {
                Id = "68df6f0d-7056-4399-9e11-0a5a4ec2c5cb",
                ConcurrencyStamp = "21e4b3f3-b26b-4b20-9f95-3b4cb87e53f0",
                Name = Helper.Roles.Admin,
                NormalizedName = Helper.Roles.Admin.ToUpperInvariant()
            }, new
            {
                Id = "fc7b455a-6a16-4f4e-8e59-626d0727cd7a",
                ConcurrencyStamp = "00431db5-df2c-4502-8e33-649df720b220",
                Name = Helper.Roles.Worker,
                NormalizedName = Helper.Roles.Worker.ToUpperInvariant()
            });
        }
    }
}
