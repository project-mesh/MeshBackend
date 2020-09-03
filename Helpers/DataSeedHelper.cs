using Microsoft.Extensions.DependencyInjection;
using MeshBackend.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace MeshBackend.Helpers
{
    public static class DataSeedHelper
    {
        public static void SeedData(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<MeshContext>();
                context.Database.EnsureCreated();
            }
        }
    }
}