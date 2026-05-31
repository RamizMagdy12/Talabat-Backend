using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entities
{
   public class BaseEntity
    {
        public int Id { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime? DeletedDateTime { get; set; }


		// Base
		public string? CreatedById { get; set; }
		public string? CreatedByName { get; set; }
		public DateTime? CreatedDateTime { get; set; }


		public string? LastModifiedById { get; set; }
		public string? LastModifiedByName { get; set; }
		public DateTime? LastModifiedDateTime { get; set; }
	}
}
