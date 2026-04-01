using LingNova.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingNova.Infreaestructure.Data
{
    public partial class LingNovaContext : DbContext
    {
        public LingNovaContext(DbContextOptions<LingNovaContext> options) : base(options)
        {
        }

        public DbSet<User> Users {  get; set; }


    }
}
