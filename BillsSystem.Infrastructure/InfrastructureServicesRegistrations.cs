using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Infrastructure.Background;
using BillsSystem.Infrastructure.Data;
using BillsSystem.Infrastructure.Email;
using BillsSystem.Infrastructure.Payments;
using BillsSystem.Infrastructure.Payments;
using BillsSystem.Infrastructure.Repositories;
using BillsSystem.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BillsSystem.Infrastructure
{
    public static class InfrastructureServicesRegistrations
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
           sql => sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));

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

            // الإيميل والتذكيرات
            services.Configure<EmailSettings>(configuration.GetSection("Email"));
            services.Configure<ReminderSettings>(configuration.GetSection("Reminders"));
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddHostedService<BillReminderBackgroundService>();

            return services;
        }
    }
}
