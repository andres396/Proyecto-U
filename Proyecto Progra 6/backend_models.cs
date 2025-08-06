// Models/User.cs
using System.ComponentModel.DataAnnotations;

namespace FerreriaAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [Required]
        public string Rol { get; set; } // Admin, Vendedor, Cliente
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Relaciones
        public virtual ICollection<Sale> Ventas { get; set; }
    }
}

// Models/Category.cs
using System.ComponentModel.DataAnnotations;

namespace FerreriaAPI.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }
        
        [StringLength(500)]
        public string Descripcion { get; set; }
        
        public string Icono { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Relaciones
        public virtual ICollection<Product> Productos { get; set; }
    }
}

// Models/Supplier.cs
using System.ComponentModel.DataAnnotations;

namespace FerreriaAPI.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Nombre { get; set; }
        
        [StringLength(100)]
        public string Contacto { get; set; }
        
        [StringLength(20)]
        public string Telefono { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [StringLength(300)]
        public string Direccion { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Relaciones
        public virtual ICollection<Product> Productos { get; set; }
    }
}

// Models/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerreriaAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Nombre { get; set; }
        
        [StringLength(500)]
        public string Descripcion { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }
        
        [Required]
        public int Stock { get; set; }
        
        [Required]
        public int StockMinimo { get; set; }
        
        public string CodigoBarras { get; set; }
        
        public string Imagen { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Foreign Keys
        public int CategoriaId { get; set; }
        public int ProveedorId { get; set; }
        
        // Navigation Properties
        public virtual Category Categoria { get; set; }
        public virtual Supplier Proveedor { get; set; }
        public virtual ICollection<SaleDetail> DetallesVenta { get; set; }
    }
}

// Models/Sale.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerreriaAPI.Models
{
    public class Sale
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string NumeroVenta { get; set; }
        
        [Required]
        [StringLength(200)]
        public string ClienteNombre { get; set; }
        
        public string ClienteIdentificacion { get; set; }
        
        public string ClienteTelefono { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Impuesto { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Descuento { get; set; }
        
        public DateTime FechaVenta { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Completada"; // Completada, Cancelada, Pendiente
        
        // Foreign Key
        public int UsuarioId { get; set; }
        
        // Navigation Properties
        public virtual User Usuario { get; set; }
        public virtual ICollection<SaleDetail> DetallesVenta { get; set; }
    }
}

// Models/SaleDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerreriaAPI.Models
{
    public class SaleDetail
    {
        public int Id { get; set; }
        
        [Required]
        public int Cantidad { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Descuento { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
        
        // Foreign Keys
        public int VentaId { get; set; }
        public int ProductoId { get; set; }
        
        // Navigation Properties
        public virtual Sale Venta { get; set; }
        public virtual Product Producto { get; set; }
    }
}

// DTOs/LoginDto.cs
namespace FerreriaAPI.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

// DTOs/RegisterDto.cs
namespace FerreriaAPI.DTOs
{
    public class RegisterDto
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; }
    }
}

// DTOs/ProductDto.cs
namespace FerreriaAPI.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string CodigoBarras { get; set; }
        public string Imagen { get; set; }
        public bool Activo { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; }
        public int ProveedorId { get; set; }
        public string ProveedorNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}

// DTOs/SaleDto.cs
namespace FerreriaAPI.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; }
        public string ClienteNombre { get; set; }
        public string ClienteIdentificacion { get; set; }
        public string ClienteTelefono { get; set; }
        public decimal Total { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public DateTime FechaVenta { get; set; }
        public string Estado { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }
        public List<SaleDetailDto> DetallesVenta { get; set; }
    }
}

// DTOs/SaleDetailDto.cs
namespace FerreriaAPI.DTOs
{
    public class SaleDetailDto
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal Subtotal { get; set; }
        public int VentaId { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
    }
}

// DTOs/CreateSaleDto.cs
namespace FerreriaAPI.DTOs
{
    public class CreateSaleDto
    {
        public string ClienteNombre { get; set; }
        public string ClienteIdentificacion { get; set; }
        public string ClienteTelefono { get; set; }
        public decimal Descuento { get; set; }
        public List<CreateSaleDetailDto> DetallesVenta { get; set; }
    }
}

// DTOs/CreateSaleDetailDto.cs
namespace FerreriaAPI.DTOs
{
    public class CreateSaleDetailDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Descuento { get; set; }
    }
}