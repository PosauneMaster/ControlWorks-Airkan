using Newtonsoft.Json;

namespace ControlWorks.Services.Rest.Models
{
    public class BinItemComplete
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }
        [JsonProperty(PropertyName = "specification")]
        public string Specification { get; set; }

        public BinItemComplete()
        {
        }

        public BinItemComplete(string id, int quantity, string specification)
        {
            Id = id;
            Quantity = quantity;
            Specification = specification;
        }

        public override string ToString()
        {
            return $"Id={Id}, Quantity={Quantity}, Specification={Specification}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BinItemComplete bi))
            {
                return false;
            }

            return ToString().Equals(bi.ToString());
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

    }
}
