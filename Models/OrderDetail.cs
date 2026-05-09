using System.ComponentModel.DataAnnotations.Schema;

[Table("OrderDetails")]
public class OrderDetail
{
    public int OrderId { get; set;}

    public int ProductId { get; set;}

    [Column(TypeName = "decimal(19, 4)")]
    public decimal UnitPrice { get; set;}
    public short Quantity { get; set;}

    [Column(TypeName = "decimal(4,4)")]
    public decimal Discount { get; set;}

    public Order Order { get; set;}
    public Product Product { get; set;}
}