using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BillsSystem.Application.Services
{
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<ItemInput> _validator;

        public ItemService(IUnitOfWork unitOfWork, IValidator<ItemInput> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
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
            catch (DbUpdateException)
            {
                result.NameError = "Couldn't save. The name may already exist, or the selected type/unit was removed";
                return result;
            }

            result.Success = true;
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

            item.ItemTypeId = input.ItemTypeId;
            item.UnitId = input.UnitId;
            item.Name = input.Name.Trim();
            item.SellingPrice = Money.Round(input.SellingPrice);
            item.BuyingPrice = Money.Round(input.BuyingPrice);
            item.QuantityInStock = input.QuantityInStock;
            item.Notes = input.Notes?.Trim();

            // مفيش _unitOfWork.Items.Update(item) هنا: الـ Entity متتبّع أصلًا، و Update() كانت
            // بتعلّم ItemType و Company و Unit (المحمّلين بالـ Include) كـ Modified وبتغيّر UpdatedAt بتاعهم

            try { await _unitOfWork.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                result.NameError = "This item was changed by someone else. Reload and try again";
                return result;
            }
            catch (DbUpdateException)
            {
                result.NameError = "Couldn't save. The name may already exist, or the selected type/unit was removed";
                return result;
            }

            result.Success = true;
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
                return (true, null);
            }
            catch (DbUpdateException)
            {
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
