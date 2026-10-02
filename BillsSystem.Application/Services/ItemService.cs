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
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<ItemInput> _validator;

        private readonly ILogger<ItemService> _logger;
        private readonly INotificationService _notifications;

        public ItemService(IUnitOfWork unitOfWork, IValidator<ItemInput> validator, ILogger<ItemService> logger,
            INotificationService notifications)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _logger = logger;
            _notifications = notifications;
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
            => await _unitOfWork.Items.GetAllAsync();

        public async Task<Item?> GetByIdAsync(int id)
            => await _unitOfWork.Items.GetByIdAsync(id);

        public async Task<ItemResult> CreateAsync(ItemInput input)
        {
            var result = await ValidateAsync(input, excludeId: null);
            if (result.HasErrors) return result;

            var item = new Item
            {
                ItemTypeId = input.ItemTypeId,
                UnitId = input.UnitId,
                Name = input.Name.Trim(),
                SellingPrice = Money.Round(input.SellingPrice),
                BuyingPrice = Money.Round(input.BuyingPrice),
                QuantityInStock = input.QuantityInStock,
                Notes = input.Notes?.Trim()
            };

            await _unitOfWork.Items.AddAsync(item);

            try { await _unitOfWork.SaveChangesAsync(); }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to create item {Name}", input.Name);
                result.NameError = "Couldn't save. The name may already exist, or the selected type/unit was removed";
                return result;
            }

            result.Success = true;
            await _notifications.NotifyAsync(NotificationType.Item, NotificationAction.Created, $"Item added: {item.Name}", item.Id);
            result.Item = item;
            return result;
        }

        public async Task<ItemResult> UpdateAsync(int id, ItemInput input)
        {
            var result = await ValidateAsync(input, excludeId: id);
            if (result.HasErrors) return result;

            var item = await _unitOfWork.Items.GetByIdAsync(id);
            if (item == null)
            {
                result.NameError = "Item not found";
                return result;
            }

            // الـ RowVersion اللي اليوزر شافه وهو فاتح الصفحة، مش الحالي من الداتابيز.
            // لو المخزون اتغير بعد كده (بيع مثلاً) الـ UPDATE هيفشل بدل ما يمسح التغيير.
            if (input.RowVersion == null)
            {
                result.NameError = "This form is outdated. Reload the page and try again";
                return result;
            }
            _unitOfWork.Items.SetOriginalRowVersion(item, input.RowVersion);

            item.ItemTypeId = input.ItemTypeId;
            item.UnitId = input.UnitId;
            item.Name = input.Name.Trim();
            item.SellingPrice = Money.Round(input.SellingPrice);
            item.BuyingPrice = Money.Round(input.BuyingPrice);
            item.QuantityInStock = input.QuantityInStock;
            item.Notes = input.Notes?.Trim();

            try { await _unitOfWork.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while updating item {ItemId}", id);
                result.NameError = "This item was changed by someone else (for example a sale). Reload and try again";
                return result;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update item {ItemId}", id);
                result.NameError = "Couldn't save. The name may already exist, or the selected type/unit was removed";
                return result;
            }

            result.Success = true;
            await _notifications.NotifyAsync(NotificationType.Item, NotificationAction.Updated, $"Item updated: {item.Name}", item.Id);
            result.Item = item;
            return result;
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id)
        {
            var item = await _unitOfWork.Items.GetByIdAsync(id);
            if (item == null) return (false, "Item not found");

            _unitOfWork.Items.Delete(item);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                await _notifications.NotifyAsync(NotificationType.Item, NotificationAction.Deleted, $"Item deleted: {item.Name}", null);
                return (true, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to delete item {Id}", id);
                return (false, "This item can't be deleted because it has related data linked to it");
            }
        }

        private async Task<ItemResult> ValidateAsync(ItemInput input, int? excludeId)
        {
            var result = new ItemResult();

            var validation = await _validator.ValidateAsync(input);
            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    switch (error.PropertyName)
                    {
                        case nameof(input.ItemTypeId): result.TypeError = error.ErrorMessage; break;
                        case nameof(input.UnitId): result.UnitError = error.ErrorMessage; break;
                        case nameof(input.Name): result.NameError = error.ErrorMessage; break;
                        case nameof(input.SellingPrice): result.SellingPriceError = error.ErrorMessage; break;
                        case nameof(input.BuyingPrice): result.BuyingPriceError = error.ErrorMessage; break;
                        case nameof(input.QuantityInStock): result.StockError = error.ErrorMessage; break;
                    }
                }
                return result;
            }

            // الـ FK لازم يكون موجود فعلًا (بدل ما نسيب الداتابيز ترمي 500)
            if (await _unitOfWork.ItemTypes.GetByIdAsync(input.ItemTypeId) == null)
                result.TypeError = "Selected type no longer exists";
            if (await _unitOfWork.Units.GetByIdAsync(input.UnitId) == null)
                result.UnitError = "Selected unit no longer exists";
            if (result.HasErrors) return result;

            if (await _unitOfWork.Items.NameExistsInTypeAsync(input.ItemTypeId, input.Name.Trim(), excludeId))
                result.NameError = "ITEM NAME has already existed before";

            return result;
        }
    }
}
