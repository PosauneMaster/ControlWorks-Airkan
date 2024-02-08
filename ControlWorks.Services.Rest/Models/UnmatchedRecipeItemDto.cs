using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.Rest.Models
{
    public class UnmatchedRecipeItemDto
    {
        public int Id { get; set; }
        public string ItemId { get; set; }
        public string Reference { get; set; }
        public string Specification { get; set; }

        public UnmatchedRecipeItemDto() { }

        public UnmatchedRecipeItemDto(string itemId, string reference, string specification)
        {
            ItemId = itemId;
            Reference = reference;
            Specification = specification;
        }

        public UnmatchedRecipeItemDto(int id, string itemId, string reference, string specification)
        {
            Id = id;
            ItemId = itemId;
            Reference = reference;
            Specification = specification;
        }


    }
}
