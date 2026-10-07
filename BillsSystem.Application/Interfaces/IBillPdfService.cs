using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.Interfaces
{
    public interface IBillPdfService
    {
        // logo اختياري (bytes لصورة PNG/JPG) بيتحط في هيدر الفاتورة
        byte[] Generate(Bill bill, byte[]? logo = null);
    }
}
