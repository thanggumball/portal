using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentPortal.Repository.Entities;
public class AnnouncementCategory : BaseEntity
{
    public Guid AnnouncementId { get; set; }
    public Guid CategoryId { get; set; }

    public Announcement Announcement { get; set; } = null!;

    public Category Category { get; set; } = null!;
}
