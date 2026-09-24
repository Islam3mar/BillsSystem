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
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CategoryInput> _validator;

        public CategoryService(IUnitOfWork unitOfWork, IValidator<CategoryInput> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
            => await _unitOfWork.Categories.GetAllAsync();

        public async Task<Category?> GetByIdAsync(int id)
            => await _unitOfWork.Categories.GetByIdAsync(id);

        public async Task<CategoryResult> CreateAsync(CategoryInput input)
        {
            var result = await ValidateAsync(input, excludeId: null);
            if (result.HasErrors) return result;

            var category = new Category
            {
                ItemTypeId = input.ItemTypeId,
                Name = input.Name.Trim(),
                Notes = input.Notes?.Trim()
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Category = category;
            return result;
        }

        public async Task<CategoryResult> UpdateAsync(int id, CategoryInput input)
        {
            var result = await ValidateAsync(input, excludeId: id);
            if (result.HasErrors) return result;

            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                result.NameError = "Category not found";
                return result;
            }

            category.ItemTypeId = input.ItemTypeId;
            category.Name = input.Name.Trim();
            category.Notes = input.Notes?.Trim();

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Category = category;
            return result;
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return (false, "Category not found");

            _unitOfWork.Categories.Delete(category);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException)
            {
                return (false, "This category can't be deleted because it has related data linked to it");
            }
        }

        private async Task<CategoryResult> ValidateAsync(CategoryInput input, int? excludeId)
        {
            var result = new CategoryResult();

            var validation = await _validator.ValidateAsync(input);
            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    if (error.PropertyName == nameof(input.ItemTypeId)) result.TypeError = error.ErrorMessage;
                    if (error.PropertyName == nameof(input.Name)) result.NameError = error.ErrorMessage;
                }
                return result;
            }

            if (await _unitOfWork.Categories.NameExistsInTypeAsync(input.ItemTypeId, input.Name.Trim(), excludeId))
                result.NameError = "CATEGORY NAME has already existed before";

            return result;
        }
    }
}
