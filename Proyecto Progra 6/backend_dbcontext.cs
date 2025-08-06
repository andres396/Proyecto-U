// Data/FerreriaContext.cs
using Microsoft.EntityFrameworkCore;
using FerreriaAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FerreriaAPI.Data
{
    public class FerreriaContext : DbContext
    {
        public FerreriaContext(DbContextOptions<FerreriaContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relaciones
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Proveedor)
                .WithMany(s => s.Productos)
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleDetail>()
                .HasOne(sd => sd.Venta)
                .WithMany(s => s.DetallesVenta)
                .HasForeignKey(sd => sd.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleDetail>()
                .HasOne(sd => sd.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(sd => sd.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de índices
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CodigoBarras)
                .IsUnique();

            modelBuilder.Entity<Sale>()
                .HasIndex(s => s.NumeroVenta)
                .IsUnique();

            // Datos semilla
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Usuarios por defecto
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Nombre = "Administrador",
                    Email = "admin@ferreteria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Rol = "Admin",
                    Activo = true,
                    FechaCreacion = DateTime.Now
                },
                new User
                {
                    Id = 2,
                    Nombre = "Vendedor Demo",
                    Email = "vendedor@ferreteria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("vendedor123"),
                    Rol = "Vendedor",
                    Activo = true,
                    FechaCreacion = DateTime.Now
                }
            );

            // Categorías por defecto
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Nombre = "Herramientas", Descripcion = "Herramientas manuales y eléctricas", Icono = "🔨", Activo = true },
                new Category { Id = 2, Nombre = "Materiales", Descripcion = "Materiales de construcción", Icono = "🧱", Activo = true },
                new Category { Id = 3, Nombre = "Plomería", Descripcion = "Accesorios de plomería", Icono = "🔧", Activo = true },
                new Category { Id = 4, Nombre = "Electricidad", Descripcion = "Materiales eléctricos", Icono = "💡", Activo = true },
                new Category { Id = 5, Nombre = "Jardinería", Descripcion = "Herramientas de jardinería", Icono = "🌱", Activo = true }
            );

            // Proveedores por defecto
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { Id = 1, Nombre = "Distribuidora Central", Contacto = "Juan Pérez", Telefono = "2222-3333", Email = "ventas@distcentral.com", Direccion = "San José, Costa Rica", Activo = true },
                new Supplier { Id = 2, Nombre = "Importadora Tools", Contacto = "María González", Telefono = "2444-5555", Email = "info@tools.cr", Direccion = "Cartago, Costa Rica", Activo = true },
                new Supplier { Id = 3, Nombre = "Ferretería Nacional", Contacto = "Carlos Rodríguez", Telefono = "2666-7777", Email = "compras@ferrenacional.com", Direccion = "Alajuela, Costa Rica", Activo = true }
            );
        }
    }
}

// Services/IGenericRepository.cs
using System.Linq.Expressions;

namespace FerreriaAPI.Services
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(int id);
    }
}

// Services/GenericRepository.cs
using Microsoft.EntityFrameworkCore;
using FerreriaAPI.Data;
using System.Linq.Expressions;

namespace FerreriaAPI.Services
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly FerreriaContext _context;
        private readonly DbSet<T> _db;

        public GenericRepository(FerreriaContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _db.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(T entity)
        {
            _db.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var entity = await _db.FindAsync(id);
            return entity != null;
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await _db.Where(expression).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _db.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _db.FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            _db.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}

// Services/IAuthService.cs
using FerreriaAPI.DTOs;
using FerreriaAPI.Models;

namespace FerreriaAPI.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginDto loginDto);
        Task<User> RegisterAsync(RegisterDto registerDto);
        string GenerateJwtToken(User user);
    }
}

// Services/AuthService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FerreriaAPI.Data;
using FerreriaAPI.DTOs;
using FerreriaAPI.Models;

namespace FerreriaAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly FerreriaContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(FerreriaContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Activo);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            return GenerateJwtToken(user);
        }

        public async Task<User> RegisterAsync(RegisterDto registerDto)
        {
            // Verificar si el email ya existe
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new InvalidOperationException("El email ya está registrado");
            }

            var user = new User
            {
                Nombre = registerDto.Nombre,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Rol = registerDto.Rol ?? "Cliente",
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Nombre),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Rol)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FerreriaDB;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "your-super-secret-jwt-key-here-make-it-long-and-secure",
    "Issuer": "FerreriaAPI",
    "Audience": "FerreriaClient",
    "ExpireDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}