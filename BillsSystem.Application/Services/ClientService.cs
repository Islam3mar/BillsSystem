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
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<ClientInput> _validator;

        public ClientService(IUnitOfWork unitOfWork, IValidator<ClientInput> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
            => await _unitOfWork.Clients.GetAllAsync();

        public async Task<Client?> GetByIdAsync(int id)
            => await _unitOfWork.Clients.GetByIdAsync(id);

        public async Task<ClientResult> CreateAsync(ClientInput input)
        {
            var result = await ValidateAsync(input, excludeId: null);
            if (result.HasErrors) return result;

            var client = new Client
            {
                Name = input.Name.Trim(),
                Phone = input.Phone.Trim(),
                Address = input.Address.Trim()
            };

            await _unitOfWork.Clients.AddAsync(client);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Client = client;
            return result;
        }

        public async Task<ClientResult> UpdateAsync(int id, ClientInput input)
        {
            var result = await ValidateAsync(input, excludeId: id);
            if (result.HasErrors) return result;

            var client = await _unitOfWork.Clients.GetByIdAsync(id);
            if (client == null)
            {
                result.NameError = "Client not found";
                return result;
            }

            client.Name = input.Name.Trim();
            client.Phone = input.Phone.Trim();
            client.Address = input.Address.Trim();

            _unitOfWork.Clients.Update(client);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Client = client;
            return result;
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id)
        {
            var client = await _unitOfWork.Clients.GetByIdAsync(id);
            if (client == null) return (false, "Client not found");

            _unitOfWork.Clients.Delete(client);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException)
            {
                return (false, "This client can't be deleted because it has related data linked to it");
            }
        }

        private async Task<ClientResult> ValidateAsync(ClientInput input, int? excludeId)
        {
            var result = new ClientResult();

            var validation = await _validator.ValidateAsync(input);
            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    switch (error.PropertyName)
                    {
                        case nameof(input.Name): result.NameError = error.ErrorMessage; break;
                        case nameof(input.Phone): result.PhoneError = error.ErrorMessage; break;
                        case nameof(input.Address): result.AddressError = error.ErrorMessage; break;
                    }
                }
                return result;
            }

            if (await _unitOfWork.Clients.NameExistsAsync(input.Name.Trim(), excludeId))
                result.NameError = "CLIENT NAME has already existed before";

            return result;
        }
    }
}
