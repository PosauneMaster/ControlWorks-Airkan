using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.Rest.Models
{
    public class RecipeItemDto
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public string Specification { get; set; }
        public int QuantityScanned { get; set; }
    }
}
