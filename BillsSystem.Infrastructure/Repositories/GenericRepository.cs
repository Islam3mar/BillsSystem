using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Infrastructure.Data;

namespace BillsSystem.Infrastructure.Repositories
{
    public class GenericRepository<T> : BaseRepository<T>, IGenericRepository<T> where T : BaseEntity
    {
        public GenericRepository(ApplicationDbContext context) : base(context) { }
    }
}
