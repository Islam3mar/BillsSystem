using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Common;
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

            var from = filter.FromDate.Date;
            var to = filter.ToDate.Date;

            // كله بيتحسب في SQL
            var totals = await _unitOfWork.Bills.GetSalesTotalsAsync(from, to);
            var bills = await _unitOfWork.Bills.GetBillSummariesAsync(from, to);
            var topItems = await _unitOfWork.Bills.GetTopItemsAsync(from, to, 5);
            var topClients = await _unitOfWork.Bills.GetTopClientsAsync(from, to, 5);

            var report = new SalesReportResult
            {
                FromDate = from,
                ToDate = to,
                BillsCount = totals.BillsCount,
                TotalBillsAmount = totals.GrossTotal,
                TotalDiscounts = totals.TotalDiscounts,        // خصم الأصناف + خصم الفاتورة
                TotalNetSales = totals.Net,
                TotalCollected = totals.Collected,
                TotalOutstanding = totals.Outstanding,
                AverageBillValue = totals.BillsCount > 0 ? Money.Round(totals.Net / totals.BillsCount) : 0,

                Bills = bills.Select(b => new BillSummaryDto
                {
                    Id = b.Id,
                    BillDate = b.BillDate,
                    ClientName = b.ClientName,
                    ItemsCount = b.ItemsCount,
                    GrossTotal = b.GrossTotal,
                    TotalDiscount = b.TotalDiscount,
                    TheNet = b.TheNet,
                    PaidUp = b.PaidUp,
                    TheRest = b.TheRest
                }).ToList(),

                // الاتنين (Top Items و Top Clients) بيطرحوا خصم الفاتورة العام بنفس القاعدة
                TopSellingItems = topItems.Select(i => new ItemSalesDto
                {
                    ItemName = i.ItemName,
                    UnitName = i.UnitName,
                    QuantitySold = i.QuantitySold,
                    TotalRevenue = Money.Round(i.Revenue)
                }).ToList(),

                TopClients = topClients.Select(c => new ClientSalesDto
                {
                    ClientName = c.ClientName,
                    BillsCount = c.BillsCount,
                    TotalNet = c.TotalNet
                }).ToList()
            };

            result.Success = true;
            result.Report = report;
            return result;
        }
    }
}
