using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BillsSystem.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CompanyInput> _validator;

        public CompanyService(IUnitOfWork unitOfWork, IValidator<CompanyInput> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
            => await _unitOfWork.Companies.GetAllAsync();

        public async Task<Company?> GetCompanyByIdAsync(int id)
            => await _unitOfWork.Companies.GetByIdAsync(id);

        public async Task<CompanyResult> CreateCompanyAsync(CompanyInput input)
        {
            var result = await ValidateAsync(input, excludeId: null);
            if (result.HasErrors) return result;

            var company = new Company
            {
                Name = input.Name.Trim(),
                Notes = input.Notes?.Trim()
            };

            await _unitOfWork.Companies.AddAsync(company);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Company = company;
            return result;
        }

        public async Task<CompanyResult> UpdateCompanyAsync(int id, CompanyInput input)
        {
            var result = await ValidateAsync(input, excludeId: id);
            if (result.HasErrors) return result;

            var company = await _unitOfWork.Companies.GetByIdAsync(id);
            if (company == null)
            {
                result.NameError = "Company not found";
                return result;
            }

            company.Name = input.Name.Trim();
            company.Notes = input.Notes?.Trim();

            _unitOfWork.Companies.Update(company);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Company = company;
            return result;
        }

        public async Task<(bool Success, string? Error)> DeleteCompanyAsync(int id)
        {
            var company = await _unitOfWork.Companies.GetByIdAsync(id);
            if (company == null) return (false, "Company not found");

            _unitOfWork.Companies.Delete(company);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException)
            {
                // هيحصل لو الشركة مرتبطة بـ Types أو Items بعدين
                return (false, "This company can't be deleted because it has related data linked to it");
            }
        }

        private async Task<CompanyResult> ValidateAsync(CompanyInput input, int? excludeId)
        {
            var result = new CompanyResult();

            var validation = await _validator.ValidateAsync(input);
            if (!validation.IsValid)
            {
                result.NameError = validation.Errors.First().ErrorMessage;
                return result;
            }

            if (await _unitOfWork.Companies.NameExistsAsync(input.Name.Trim(), excludeId))
                result.NameError = "COMPANY NAME has already existed before";

            return result;
        }
    }
}
