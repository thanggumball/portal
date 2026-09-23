using Microsoft.EntityFrameworkCore;
using StudentPortal.Repository.Data;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentPortal.Repository.Implementations
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly AppDbContext _context;

        public ProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetMyProfileAsync(
        Guid userId,
        CancellationToken ct = default)
        => await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(
                u => u.Id == userId && !u.IsDeleted,
                ct);
    }
}
