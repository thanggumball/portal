using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentPortal.Common.DTOs.Announcement
{
    public class CategoryResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
