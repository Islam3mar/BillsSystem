using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.Interfaces
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task<ClientResult> CreateAsync(ClientInput input);
        Task<ClientResult> UpdateAsync(int id, ClientInput input);
        Task<(bool Success, string? Error)> DeleteAsync(int id);
    }
}
