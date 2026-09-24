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
                SellingPrice = input.SellingPrice,
                BuyingPrice = input.BuyingPrice,
                Notes = input.Notes?.Trim()
            };

            await _unitOfWork.Items.AddAsync(item);
            await _unitOfWork.SaveChangesAsync();

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
            item.SellingPrice = input.SellingPrice;
            item.BuyingPrice = input.BuyingPrice;
            item.Notes = input.Notes?.Trim();

            _unitOfWork.Items.Update(item);
            await _unitOfWork.SaveChangesAsync();

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
                    }
                }
                return result;
            }

            if (await _unitOfWork.Items.NameExistsInTypeAsync(input.ItemTypeId, input.Name.Trim(), excludeId))
                result.NameError = "ITEM NAME has already existed before";

            return result;
        }
    }
}
