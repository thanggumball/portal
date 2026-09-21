using Microsoft.EntityFrameworkCore.Storage;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Interfaces;

namespace StudentPortal.Repository.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(
        CancellationToken ct = default)
    {
        var transaction = await _context.Database
            .BeginTransactionAsync(ct);

        return new UnitOfWorkTransaction(transaction);
    }

    private sealed class UnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public UnitOfWorkTransaction(
            IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(
            CancellationToken ct = default)
            => _transaction.CommitAsync(ct);

        public Task RollbackAsync(
            CancellationToken ct = default)
            => _transaction.RollbackAsync(ct);

        public ValueTask DisposeAsync()
            => _transaction.DisposeAsync();
    }
}
