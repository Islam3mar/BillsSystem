using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.Interfaces;
using BillsSystem.Application.Services;
using BillsSystem.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BillsSystem.Application
{
    public static class ApplicationServicesRegistrations
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IItemTypeService, ItemTypeService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IUnitService, UnitService>();
            services.AddScoped<IItemService, ItemService>();

            services.AddValidatorsFromAssemblyContaining<CompanyInputValidator>();

            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(ApplicationServicesRegistrations).Assembly);
            });

            return services;
        }
    }
}
