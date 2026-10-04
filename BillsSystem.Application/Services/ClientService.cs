using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Enums;
using BillsSystem.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BillsSystem.Application.Services
{
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<ClientInput> _validator;
        private readonly ILogger<ClientService> _logger;
        private readonly INotificationService _notifications;

        public ClientService(IUnitOfWork unitOfWork, IValidator<ClientInput> validator, ILogger<ClientService> logger,
            INotificationService notifications)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _logger = logger;
            _notifications = notifications;
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
                Address = input.Address.Trim(),
                Email = input.Email,
                MaxCreditLimit = input.MaxCreditLimit.HasValue ? Money.Round(input.MaxCreditLimit.Value) : null
            };

            await _unitOfWork.Clients.AddAsync(client);
            try { await _unitOfWork.SaveChangesAsync(); }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to create client {Name}", input.Name);
                result.NameError = "Couldn't save. The name or email may already exist, or a linked record was removed";
                return result;
            }

            result.Success = true;
            await _notifications.NotifyAsync(NotificationType.Client, NotificationAction.Created, $"Client added: {client.Name}", client.Id);
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

            var oldLimit = client.MaxCreditLimit;

            client.Name = input.Name.Trim();
            client.Phone = input.Phone.Trim();
            client.Address = input.Address.Trim();
            client.Email = input.Email;
            client.MaxCreditLimit = input.MaxCreditLimit.HasValue ? Money.Round(input.MaxCreditLimit.Value) : null;

            try { await _unitOfWork.SaveChangesAsync(); }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update client {Id}", id);
                result.NameError = "Couldn't save. The name or email may already exist, or a linked record was removed";
                return result;
            }

            result.Success = true;
            await _notifications.NotifyAsync(NotificationType.Client, NotificationAction.Updated, $"Client updated: {client.Name}", client.Id);

            // السقف اتغيّر وبقى أقل من الدين الحالي: تنبيه للأدمن (التعديل نفسه مسموح)
            if (client.MaxCreditLimit is decimal newLimit && newLimit != oldLimit)
            {
                var debt = await _unitOfWork.Bills.GetClientOutstandingAsync(client.Id);
                if (debt > newLimit)
                    await _notifications.NotifyAsync(NotificationType.Client, NotificationAction.Alert,
                        $"'{client.Name}' credit limit was set to {newLimit:0.00}, below the current debt ({debt:0.00}). New credit invoices will be rejected until the debt is settled", client.Id);
            }

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
                await _notifications.NotifyAsync(NotificationType.Client, NotificationAction.Deleted, $"Client deleted: {client.Name}", null);
                return (true, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to delete client {Id}", id);
                return (false, "This client can't be deleted because it has related data linked to it");
            }
        }

        private async Task<ClientResult> ValidateAsync(ClientInput input, int? excludeId)
        {
            var result = new ClientResult();

            // الإيميل اختياري: فاضي/مسافات = null، وغير كده بيتقص قبل الفحص والحفظ
            input.Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();

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
                        case nameof(input.Email): result.EmailError = error.ErrorMessage; break;
                        case nameof(input.MaxCreditLimit): result.MaxCreditLimitError = error.ErrorMessage; break;
                    }
                }
                return result;
            }

            if (await _unitOfWork.Clients.NameExistsAsync(input.Name.Trim(), excludeId))
                result.NameError = "CLIENT NAME has already existed before";

            // الإيميل اختياري، بس لو اتكتب مينفعش يكون مستخدم عند عميل تاني
            if (input.Email != null && await _unitOfWork.Clients.EmailExistsAsync(input.Email, excludeId))
                result.EmailError = "This email is already used by another client";

            return result;
        }
    }
}
