using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimesheetGeneratorApp.Models;

namespace TimesheetGeneratorApp.Data
{
    public class CommitContext : DbContext
    {
        public CommitContext (DbContextOptions<CommitContext> options)
            : base(options)
        {
        }

        public DbSet<TimesheetGeneratorApp.Models.CommitModel> CommitModel { get; set; } = default!;
    }
}
