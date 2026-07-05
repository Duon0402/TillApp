namespace TillApp.Models
{
    public record ProductDto(int Id, string Name, string Barcode, decimal Price, int CurrentStock, string CategoryName);
}
