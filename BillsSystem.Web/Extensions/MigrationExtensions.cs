using BillsSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BillsSystem.Web.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task MigrationAndSeedAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            // لما نضيف نظام Users/Roles حقيقي بعدين، هنضيف هنا استدعاء Seeder
            // بنفس نمط الـ IdentitySeeder اللي فى HRMS (GetRequiredKeyedService<IDataSeeder>)
        }
    }
}
