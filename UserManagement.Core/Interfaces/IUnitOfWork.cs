namespace UserManagement.Core.Interfaces
{
    public interface IUnitOfWork
    {
        IAppUserRepository UserRepository { get; }

        Task Commit();
        Task CreateTransaction();
        void Dispose();
        Task Rollback();
        Task Save();
    }
}