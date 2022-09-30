using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimesheetGeneratorApp.Models;

namespace TimesheetGeneratorApp.Data
{
    public class HariLiburContext : DbContext
    {
        public HariLiburContext (DbContextOptions<HariLiburContext> options)
            : base(options)
        {
        }

        public DbSet<TimesheetGeneratorApp.Models.HariLiburModel> HariLiburModel { get; set; } = default!;
    }
}
