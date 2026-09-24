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
    public class UnitService : IUnitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<UnitInput> _validator;

        public UnitService(IUnitOfWork unitOfWork, IValidator<UnitInput> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<IEnumerable<Unit>> GetAllUnitsAsync()
            => await _unitOfWork.Units.GetAllAsync();

        public async Task<Unit?> GetUnitByIdAsync(int id)
            => await _unitOfWork.Units.GetByIdAsync(id);

        public async Task<UnitResult> CreateUnitAsync(UnitInput input)
        {
            var result = await ValidateAsync(input, excludeId: null);
            if (result.HasErrors) return result;

            var unit = new Unit
            {
                Name = input.Name.Trim(),
                Notes = input.Notes?.Trim()
            };

            await _unitOfWork.Units.AddAsync(unit);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Unit = unit;
            return result;
        }

        public async Task<UnitResult> UpdateUnitAsync(int id, UnitInput input)
        {
            var result = await ValidateAsync(input, excludeId: id);
            if (result.HasErrors) return result;

            var unit = await _unitOfWork.Units.GetByIdAsync(id);
            if (unit == null)
            {
                result.NameError = "Unit not found";
                return result;
            }

            unit.Name = input.Name.Trim();
            unit.Notes = input.Notes?.Trim();

            _unitOfWork.Units.Update(unit);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Unit = unit;
            return result;
        }

        public async Task<(bool Success, string? Error)> DeleteUnitAsync(int id)
        {
            var unit = await _unitOfWork.Units.GetByIdAsync(id);
            if (unit == null) return (false, "Unit not found");

            _unitOfWork.Units.Delete(unit);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException)
            {
                return (false, "This unit can't be deleted because it has related data linked to it");
            }
        }

        private async Task<UnitResult> ValidateAsync(UnitInput input, int? excludeId)
        {
            var result = new UnitResult();

            var validation = await _validator.ValidateAsync(input);
            if (!validation.IsValid)
            {
                result.NameError = validation.Errors.First().ErrorMessage;
                return result;
            }

            if (await _unitOfWork.Units.NameExistsAsync(input.Name.Trim(), excludeId))
                result.NameError = "UNIT NAME has already existed before";

            return result;
        }
    }
}
