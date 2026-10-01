using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Infrastructure.Data;
using BillsSystem.Infrastructure.Payments;
using BillsSystem.Infrastructure.Repositories;
using BillsSystem.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BillsSystem.Infrastructure.Payments;
using BillsSystem.Application.DTOs;

namespace BillsSystem.Infrastructure
{
    public static class InfrastructureServicesRegistrations
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IAdminCredentialsProvider, AppSettingsAdminCredentialsProvider>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                });

            services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
            services.AddScoped<IStripeCheckoutService, StripeCheckoutService>();

            return services;
        }
    }
}
