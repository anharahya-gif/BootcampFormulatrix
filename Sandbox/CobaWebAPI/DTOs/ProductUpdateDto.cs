using System.ComponentModel.DataAnnotations;

namespace CobaWebAPI.DTOs
{
    public class ProductUpdateDto
    {
        [Required(ErrorMessage = "Name wajib diisi")]
        [MinLength(3, ErrorMessage = "Name minimal 3 karakter")]
        public string Name { get; set; } = string.Empty;

        [Range(1, double.MaxValue, ErrorMessage = "Price harus lebih dari 0")]
        public decimal Price { get; set; }
    }
}
