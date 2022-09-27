using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimesheetGeneratorApp.Models;

namespace TimesheetGeneratorApp.Data
{
    public class MasterProjectContext : DbContext
    {
        public MasterProjectContext (DbContextOptions<MasterProjectContext> options)
            : base(options){

        }

        public DbSet<TimesheetGeneratorApp.Models.MasterProjectModel> MasterProjectModel { get; set; } = default!;
    }
}
