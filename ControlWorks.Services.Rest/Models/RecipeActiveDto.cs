using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace ControlWorks.Services.Rest.Models
{
    public class RecipeActiveDto
    {
        [BsonId]
        public int Id { get; set; }
        public int BinId { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public RecipeItemDto[] Items { get; set; }
        public DateTime Created { get; set; }

        public RecipeActiveDto()
        {

        }

        public RecipeActiveDto(string reference, string description, IEnumerable<RecipeItemDto> items)
        {
            Reference = reference;
            Description = description;
            Items = items.ToArray();
            Created = DateTime.Now;
        }

        [BsonCtor]
        public RecipeActiveDto(string reference, string description, IEnumerable<RecipeItemDto> items, DateTime created)
        {
            Reference = reference;
            Description = description;
            Items = items.ToArray();
            Created = created;
        }


    }
}
