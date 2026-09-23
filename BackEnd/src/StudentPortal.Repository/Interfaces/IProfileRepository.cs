using StudentPortal.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentPortal.Repository.Interfaces;

    public interface IProfileRepository
    {
        Task<User?> GetMyProfileAsync(
        Guid userId,
        CancellationToken ct = default);
    }

