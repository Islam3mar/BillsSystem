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
    public class ItemTypeService : IItemTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<ItemTypeInput> _validator;

        public ItemTypeService(IUnitOfWork unitOfWork, IValidator<ItemTypeInput> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<IEnumerable<ItemType>> GetAllAsync()
            => await _unitOfWork.ItemTypes.GetAllAsync();

        public async Task<ItemType?> GetByIdAsync(int id)
            => await _unitOfWork.ItemTypes.GetByIdAsync(id);

        public async Task<IEnumerable<ItemType>> GetByCompanyAsync(int companyId)
            => await _unitOfWork.ItemTypes.GetByCompanyAsync(companyId);

        public async Task<ItemTypeResult> CreateAsync(ItemTypeInput input)
        {
            var result = await ValidateAsync(input, excludeId: null);
            if (result.HasErrors) return result;

            var itemType = new ItemType
            {
                CompanyId = input.CompanyId,
                Name = input.Name.Trim(),
                Notes = input.Notes?.Trim()
            };

            await _unitOfWork.ItemTypes.AddAsync(itemType);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.ItemType = itemType;
            return result;
        }

        public async Task<ItemTypeResult> UpdateAsync(int id, ItemTypeInput input)
        {
            var result = await ValidateAsync(input, excludeId: id);
            if (result.HasErrors) return result;

            var itemType = await _unitOfWork.ItemTypes.GetByIdAsync(id);
            if (itemType == null)
            {
                result.NameError = "Type not found";
                return result;
            }

            itemType.CompanyId = input.CompanyId;
            itemType.Name = input.Name.Trim();
            itemType.Notes = input.Notes?.Trim();

            _unitOfWork.ItemTypes.Update(itemType);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.ItemType = itemType;
            return result;
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id)
        {
            var itemType = await _unitOfWork.ItemTypes.GetByIdAsync(id);
            if (itemType == null) return (false, "Type not found");

            _unitOfWork.ItemTypes.Delete(itemType);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException)
            {
                return (false, "This type can't be deleted because it has related data linked to it");
            }
        }

        private async Task<ItemTypeResult> ValidateAsync(ItemTypeInput input, int? excludeId)
        {
            var result = new ItemTypeResult();

            var validation = await _validator.ValidateAsync(input);
            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    if (error.PropertyName == nameof(input.CompanyId)) result.CompanyError = error.ErrorMessage;
                    if (error.PropertyName == nameof(input.Name)) result.NameError = error.ErrorMessage;
                }
                return result;
            }

            if (await _unitOfWork.ItemTypes.NameExistsInCompanyAsync(input.CompanyId, input.Name.Trim(), excludeId))
                result.NameError = "TYPE NAME has already existed before";

            return result;
        }
    }
}
