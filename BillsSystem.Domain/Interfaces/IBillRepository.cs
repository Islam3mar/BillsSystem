using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Interfaces
{
    public interface IBillRepository : IGenericRepository<Bill>
    {
    }
}
