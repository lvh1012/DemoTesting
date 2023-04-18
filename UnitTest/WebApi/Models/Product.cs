using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
        public DateTime CreateDate { get; set; }
    }
}