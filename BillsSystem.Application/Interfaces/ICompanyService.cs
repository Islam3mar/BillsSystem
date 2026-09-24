using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.Interfaces
{
    public interface ICompanyService
    {
        Task<IEnumerable<Company>> GetAllCompaniesAsync();
        Task<Company?> GetCompanyByIdAsync(int id);
        Task<CompanyResult> CreateCompanyAsync(CompanyInput input);
        Task<CompanyResult> UpdateCompanyAsync(int id, CompanyInput input);
        Task<(bool Success, string? Error)> DeleteCompanyAsync(int id);
    }
}
