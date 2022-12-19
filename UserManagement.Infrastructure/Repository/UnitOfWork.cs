using Microsoft.EntityFrameworkCore.Storage;
using UserManagement.Core.Interfaces;

namespace UserManagement.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private bool _disposedValue;
        private readonly UserManagementDbContext _context;
        private IDbContextTransaction _objTransaction;
        private IAppUserRepository _userRepository;
        public UnitOfWork(UserManagementDbContext context)
        {
            _context = context;
        }

        public IAppUserRepository UserRepository => _userRepository ??= new AppUserRepository(_context);

        public async Task CreateTransaction()
        {
            _objTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task Commit()
        {
            await _objTransaction.CommitAsync();
        }

        public async Task Rollback()
        {
            await _objTransaction?.RollbackAsync();
            await _objTransaction.DisposeAsync();
        }


        public async Task Save()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw new Exception();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _context.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
