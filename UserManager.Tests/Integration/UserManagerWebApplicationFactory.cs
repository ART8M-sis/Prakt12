using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using UserManager.Services;

namespace UserManager.Tests.Integration
{
    public class UserManagerWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IUserService> UserService { get; set; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(service =>
            {
                var userRepo = service.SingleOrDefault(
                    d => d.ServiceType == typeof(IUserService));
                if (userRepo != null)
                {
                    service.Remove(userRepo);
                }

                service.AddScoped(_ => UserService.Object);
            });
        }
    }
}
