using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace evowar.WebApp.Data
{
    public class ContestDb : IdentityDbContext
    {
        public ContestDb(DbContextOptions<ContestDb> options)
            : base(options)
        { }
    }
}
