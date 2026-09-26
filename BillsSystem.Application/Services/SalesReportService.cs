using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Domain.Specifications;
using FluentValidation;

namespace BillsSystem.Application.Services
{
    public class SalesReportService : ISalesReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<SalesReportFilter> _validator;

        public SalesReportService(IUnitOfWork unitOfWork, IValidator<SalesReportFilter> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<SalesReportRequestResult> GenerateAsync(SalesReportFilter filter)
        {
            var result = new SalesReportRequestResult();

            var validation = await _validator.ValidateAsync(filter);
            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    switch (error.PropertyName)
                    {
                        case nameof(SalesReportFilter.FromDate): result.FromDateError = error.ErrorMessage; break;
                        case nameof(SalesReportFilter.ToDate): result.ToDateError = error.ErrorMessage; break;
                    }
                }
                return result;
            }

            var spec = new BillsInDateRangeSpecification(filter.FromDate, filter.ToDate);
            var bills = (await _unitOfWork.Bills.ListAsync(spec)).ToList();

            var report = new SalesReportResult
            {
                FromDate = filter.FromDate.Date,
                ToDate = filter.ToDate.Date,
                BillsCount = bills.Count,
                TotalBillsAmount = bills.Sum(b => b.BillsTotal),
                TotalDiscounts = bills.Sum(b => b.ValueDiscount),
                TotalNetSales = bills.Sum(b => b.TheNet),
                TotalCollected = bills.Sum(b => b.PaidUp),
                TotalOutstanding = bills.Sum(b => b.TheRest)
            };

            report.AverageBillValue = report.BillsCount > 0
                ? report.TotalNetSales / report.BillsCount
                : 0;

            report.Bills = bills
                .OrderByDescending(b => b.BillDate).ThenByDescending(b => b.Id)
                .Select(b => new BillSummaryDto
                {
                    Id = b.Id,
                    BillDate = b.BillDate,
                    ClientName = b.Client.Name,
                    ItemsCount = b.Items.Count,
                    BillsTotal = b.BillsTotal,
                    ValueDiscount = b.ValueDiscount,
                    TheNet = b.TheNet,
                    PaidUp = b.PaidUp,
                    TheRest = b.TheRest
                }).ToList();

            report.TopSellingItems = bills
                .SelectMany(b => b.Items)
                .GroupBy(i => new { i.ItemId, i.Item.Name, UnitName = i.Item.Unit.Name })
                .Select(g => new ItemSalesDto
                {
                    ItemName = g.Key.Name,
                    UnitName = g.Key.UnitName,
                    QuantitySold = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.Balance)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(5)
                .ToList();

            report.TopClients = bills
                .GroupBy(b => new { b.ClientId, b.Client.Name })
                .Select(g => new ClientSalesDto
                {
                    ClientName = g.Key.Name,
                    BillsCount = g.Count(),
                    TotalNet = g.Sum(b => b.TheNet)
                })
                .OrderByDescending(x => x.TotalNet)
                .Take(5)
                .ToList();

            result.Success = true;
            result.Report = report;
            return result;
        }
    }
}
