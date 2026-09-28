using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Domain.Specifications;

namespace BillsSystem.Application.Services
{
    // الأعداد بتيجي بـ COUNT في SQL، ومفيش تحميل لأي جدول كامل
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<DashboardDto> GetAsync()
        {
            var today = AppClock.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var month = await _unitOfWork.Bills.GetSalesTotalsAsync(monthStart, today);
            var recent = await _unitOfWork.Bills.ListAsync(new BillsPagedSpecification(null, 1, 5));

            return new DashboardDto
            {
                TotalClients = await _unitOfWork.Clients.CountAllAsync(),
                TotalItems = await _unitOfWork.Items.CountAllAsync(),
                TotalCompanies = await _unitOfWork.Companies.CountAllAsync(),
                TotalBillsAllTime = await _unitOfWork.Bills.CountAllAsync(),
                OutstandingAllTime = await _unitOfWork.Bills.GetTotalOutstandingAsync(),
                MonthNetSales = month.Net,
                MonthBillsCount = month.BillsCount,
                RecentBills = recent.ToList()
            };
        }
    }
}
