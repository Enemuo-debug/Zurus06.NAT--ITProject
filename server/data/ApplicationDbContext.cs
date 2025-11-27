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

        public DbSet<NATPosts> Posts { get; set; } = null!;
        public DbSet<NATContent> Contents { get; set; } = null!;
        public DbSet<NATSimulation> Diagrams { get; set; } = null!;
        public DbSet<NATRequests> Requests { get; set; } = null!;
        public DbSet<NATComments> Comments { get; set; } = null!;
    }
}