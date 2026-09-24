using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Infrastructure.Data;

namespace BillsSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        #region Private Field
        private ICompanyRepository? _companies;

        private IItemTypeRepository? _itemTypes;
        private ICategoryRepository? _categories;
        private IUnitRepository? _units;
        #endregion

        public UnitOfWork(ApplicationDbContext context) => _context = context;


        #region Public Property
        public ICompanyRepository Companies => _companies ??= new CompanyRepository(_context);

        public IItemTypeRepository ItemTypes => _itemTypes ??= new ItemTypeRepository(_context);
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
        public IUnitRepository Units => _units ??= new UnitRepository(_context);
        #endregion

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
}
