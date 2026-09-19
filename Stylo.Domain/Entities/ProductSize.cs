using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.Domain.Entities
{
    public class ProductSize
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public Size  Size { get; set; } 
        public int Stock { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public bool IsAvailable => Stock > 0;
    }
}
