using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MeshBackend.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MeshBackend.Helpers
{
    public static class DataSeedHelper
    {
        public static void SeedData(this IApplicationBuilder app,IConfiguration configuration)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<MeshContext>();
                context.Database.EnsureCreated();
                var password = PasswordCheckHelper.GetHashPassword(configuration["AdminInformation:Password"]);
                if (context.Admins.FirstOrDefault() != null) return;
                try
                {
                    context.Admins.Add(
                        new Admin()
                        {
                            Email = configuration["AdminInformation:Username"],
                            Nickname = configuration["AdminInformation:Username"],
                            PasswordDigest = password.PasswordDigest,
                            PasswordSalt = password.PasswordSalt
                        });
                    context.SaveChanges();
                }
                catch
                {
                    return;
                }
            }
        }
    }
}