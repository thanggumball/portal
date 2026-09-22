using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentPortal.Repository.Entities
{
    public class Category: BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<AnnouncementCategory> AnnouncementCategory { get; set; } = new List<AnnouncementCategory>();
    }
}
