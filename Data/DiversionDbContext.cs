using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Diversion.Data
{
    public class DiversionDbContext(DbContextOptions<DiversionDbContext> options) : IdentityDbContext(options)
    {
    }
}
