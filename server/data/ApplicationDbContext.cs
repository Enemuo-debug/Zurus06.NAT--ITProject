using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using server.NATModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace server.data
{
    public class ApplicationDbContext : IdentityDbContext<NATUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<NATPosts> Posts;
        public DbSet<NATContent> Contents;
        public DbSet<NATSimulation> Diagrams;
    }
}