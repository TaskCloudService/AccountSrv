// using Microsoft.AspNetCore.Identity;
// using Presentation.Models;

// namespace AuthMicroservice.Data;

// public static class DbSeeder
// {
//     public static async Task SeedAsync(IServiceProvider services)
//     {
//         using var scope = services.CreateScope();
//         var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
//         var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

//         var roles = new[] { "Admin", "User" };
//         foreach (var r in roles)
//             if (!await roleManager.RoleExistsAsync(r))
//                 await roleManager.CreateAsync(new ApplicationRole { Name = r, NormalizedName = r.ToUpperInvariant() });

//         // Seed a default admin
//         var adminEmail = "admin@local.com";
//         if (await userManager.FindByEmailAsync(adminEmail) == null)
//         {
//             var admin = new ApplicationUser { Id = Guid.NewGuid(), UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
//             await userManager.CreateAsync(admin, "Admin123$!");
//             await userManager.AddToRoleAsync(admin, "Admin");
//         }
//     }
// }
