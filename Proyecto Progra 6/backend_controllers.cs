// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using FerreriaAPI.DTOs;
using FerreriaAPI.Services;

namespace FerreriaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var token = await _authService.LoginAsync(loginDto);
                if (token == null)
                {
                    return Unauthorized(new { message = "Email o contraseña incorrectos" });
                }

                return Ok(new { token, message = "Login exitoso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var user = await _authService.RegisterAsync(registerDto);
                var token = _authService.GenerateJwtToken(user);
                
                return Ok(new { 
                    token, 
                    user = new { 
                        user.Id, 
                        user.Nombre, 
                        user.Email, 
                        user.Rol 
                    }, 
                    message = "Usuario registrado exitosamente" 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

// Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FerreriaAPI.Data;
using FerreriaAPI.Models;
using FerreriaAPI.DTOs;

namespace FerreriaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly FerreriaContext _context;
        private readonly IMapper _mapper;

        public ProductsController(FerreriaContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Where(p => p.Activo)
                .ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Stock = p.Stock,
                StockMinimo = p.StockMinimo,
                CodigoBarras = p.CodigoBarras,
                Imagen = p.Imagen,
                Activo = p.Activo,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria.Nombre,
                ProveedorId = p.ProveedorId,
                ProveedorNombre = p.Proveedor.Nombre,
                FechaCreacion = p.FechaCreacion
            });

            return Ok(productDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var productDto = new ProductDto
            {
                Id = product.Id,
                Nombre = product.Nombre,
                Descripcion = product.Descripcion,
                Precio = product.Precio,
                Stock = product.Stock,
                StockMinimo = product.StockMinimo,
                CodigoBarras = product.CodigoBarras,
                Imagen = product.Imagen,
                Activo = product.Activo,
                CategoriaId = product.CategoriaId,
                CategoriaNombre = product.Categoria.Nombre,
                ProveedorId = product.ProveedorId,
                ProveedorNombre = product.Proveedor.Nombre,
                FechaCreacion = product.FechaCreacion
            };

            return Ok(productDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            product.FechaCreacion = DateTime.Now;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Soft delete
            product.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts()
        {
            var products = await _context.Products
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Where(p => p.Activo && p.Stock <= p.StockMinimo)
                .ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Stock = p.Stock,
                StockMinimo = p.StockMinimo,
                CategoriaNombre = p.Categoria.Nombre,
                ProveedorNombre = p.Proveedor.Nombre
            });

            return Ok(productDtos);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}

// Controllers/SalesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FerreriaAPI.Data;
using FerreriaAPI.Models;
using FerreriaAPI.DTOs;

namespace FerreriaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly FerreriaContext _context;

        public SalesController(FerreriaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetSales()
        {
            var sales = await _context.Sales
                .Include(s => s.Usuario)
                .Include(s => s.DetallesVenta)
                    .ThenInclude(sd => sd.Producto)
                .OrderByDescending(s => s.FechaVenta)
                .ToListAsync();

            var salesDtos = sales.Select(s => new SaleDto
            {
                Id = s.Id,
                NumeroVenta = s.NumeroVenta,
                ClienteNombre = s.ClienteNombre,
                ClienteIdentificacion = s.ClienteIdentificacion,
                ClienteTelefono = s.ClienteTelefono,
                Total = s.Total,
                Impuesto = s.Impuesto,
                Descuento = s.Descuento,
                FechaVenta = s.FechaVenta,
                Estado = s.Estado,
                UsuarioId = s.UsuarioId,
                UsuarioNombre = s.Usuario.Nombre,
                DetallesVenta = s.DetallesVenta.Select(sd => new SaleDetailDto
                {
                    Id = sd.Id,
                    Cantidad = sd.Cantidad,
                    PrecioUnitario = sd.PrecioUnitario,
                    Descuento = sd.Descuento,
                    Subtotal = sd.Subtotal,
                    ProductoId = sd.ProductoId,
                    ProductoNombre = sd.Producto.Nombre
                }).ToList()
            });

            return Ok(salesDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SaleDto>> GetSale(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Usuario)
                .Include(s => s.DetallesVenta)
                    .ThenInclude(sd => sd.Producto)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            var saleDto = new SaleDto
            {
                Id = sale.Id,
                NumeroVenta = sale.NumeroVenta,
                ClienteNombre = sale.ClienteNombre,
                ClienteIdentificacion = sale.ClienteIdentificacion,
                ClienteTelefono = sale.ClienteTelefono,
                Total = sale.Total,
                Impuesto = sale.Impuesto,
                Descuento = sale.Descuento,
                FechaVenta = sale.FechaVenta,
                Estado = sale.Estado,
                UsuarioId = sale.UsuarioId,
                UsuarioNombre = sale.Usuario.Nombre,
                DetallesVenta = sale.DetallesVenta.Select(sd => new SaleDetailDto
                {
                    Id = sd.Id,
                    Cantidad = sd.Cantidad,
                    PrecioUnitario = sd.PrecioUnitario,
                    Descuento = sd.Descuento,
                    Subtotal = sd.Subtotal,
                    ProductoId = sd.ProductoId,
                    ProductoNombre = sd.Producto.Nombre
                }).ToList()
            };

            return Ok(saleDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<ActionResult<Sale>> CreateSale(CreateSaleDto createSaleDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Obtener el ID del usuario actual
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Generar número de venta único
                var numeroVenta = GenerateNumeroVenta();

                // Crear la venta
                var sale = new Sale
                {
                    NumeroVenta = numeroVenta,
                    ClienteNombre = createSaleDto.ClienteNombre,
                    ClienteIdentificacion = createSaleDto.ClienteIdentificacion,
                    ClienteTelefono = createSaleDto.ClienteTelefono,
                    Descuento = createSaleDto.Descuento,
                    FechaVenta = DateTime.Now,
                    Estado = "Completada",
                    UsuarioId = userId
                };

                decimal subtotalGeneral = 0;
                var detallesVenta = new List<SaleDetail>();

                // Procesar cada detalle de venta
                foreach (var detalle in createSaleDto.DetallesVenta)
                {
                    var producto = await _context.Products.FindAsync(detalle.ProductoId);
                    if (producto == null)
                    {
                        return BadRequest($"Producto con ID {detalle.ProductoId} no encontrado");
                    }

                    if (producto.Stock < detalle.Cantidad)
                    {
                        return BadRequest($"Stock insuficiente para el producto {producto.Nombre}");
                    }

                    var subtotal = (producto.Precio * detalle.Cantidad) - detalle.Descuento;
                    subtotalGeneral += subtotal;

                    var saleDetail = new SaleDetail
                    {
                        ProductoId = detalle.ProductoId,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Descuento = detalle.Descuento,
                        Subtotal = subtotal
                    };

                    detallesVenta.Add(saleDetail);

                    // Actualizar stock
                    producto.Stock -= detalle.Cantidad;
                }

                // Calcular impuestos (13% en Costa Rica)
                var impuesto = subtotalGeneral * 0.13m;
                sale.Impuesto = impuesto;
                sale.Total = subtotalGeneral + impuesto - createSaleDto.Descuento;

                // Guardar venta
                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                // Asignar VentaId a los detalles
                foreach (var detalle in detallesVenta)
                {
                    detalle.VentaId = sale.Id;
                }

                _context.SaleDetails.AddRange(detallesVenta);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("reports/daily")]
        public async Task<ActionResult> GetDailyReport(DateTime? fecha = null)
        {
            var targetDate = fecha ?? DateTime.Today;
            
            var ventasDelDia = await _context.Sales
                .Include(s => s.DetallesVenta)
                    .ThenInclude(sd => sd.Producto)
                .Where(s => s.FechaVenta.Date == targetDate.Date && s.Estado == "Completada")
                .ToListAsync();

            var reporte = new
            {
                Fecha = targetDate,
                TotalVentas = ventasDelDia.Count,
                MontoTotal = ventasDelDia.Sum(v => v.Total),
                ProductosVendidos = ventasDelDia.SelectMany(v => v.DetallesVenta)
                    .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
                    .Select(g => new
                    {
                        ProductoId = g.Key.ProductoId,
                        Nombre = g.Key.Nombre,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        MontoTotal = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(p => p.CantidadVendida)
                    .Take(10)
            };

            return Ok(reporte);
        }

        private string GenerateNumeroVenta()
        {
            var fecha = DateTime.Now;
            var numero = $"V{fecha:yyyyMMdd}{fecha:HHmmss}";
            return numero;
        }
    }
}

// Controllers/CategoriesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FerreriaAPI.Data;
using FerreriaAPI.Models;

namespace FerreriaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly FerreriaContext _context;

        public CategoriesController(FerreriaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Category>> CreateCategory(Category category)
        {
            category.FechaCreacion = DateTime.Now;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Verificar si hay productos asociados
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoriaId == id && p.Activo);
            if (hasProducts)
            {
                return BadRequest(new { message = "No se puede eliminar la categoría porque tiene productos asociados" });
            }

            category.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}

// Controllers/DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FerreriaAPI.Data;

namespace FerreriaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly FerreriaContext _context;

        public DashboardController(FerreriaContext context)
        {
            _context = context;
        }

        [HttpGet("kpis")]
        public async Task<ActionResult> GetKPIs()
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var ventasDelMes = await _context.Sales
                .Where(s => s.FechaVenta >= inicioMes && s.Estado == "Completada")
                .SumAsync(s => s.Total);

            var ventasHoy = await _context.Sales
                .CountAsync(s => s.FechaVenta.Date == hoy && s.Estado == "Completada");

            var productosEnStock = await _context.Products
                .Where(p => p.Activo)
                .CountAsync();

            var productosStockBajo = await _context.Products
                .Where(p => p.Activo && p.Stock <= p.StockMinimo)
                .CountAsync();

            var kpis = new
            {
                VentasDelMes = ventasDelMes,
                VentasHoy = ventasHoy,
                ProductosEnStock = productosEnStock,
                ProductosStockBajo = productosStockBajo
            };

            return Ok(kpis);
        }

        [HttpGet("ventas-por-mes")]
        public async Task<ActionResult> GetVentasPorMes()
        {
            var fechaInicio = DateTime.Now.AddMonths(-11);
            var ventasPorMes = await _context.Sales
                .Where(s => s.FechaVenta >= fechaInicio && s.Estado == "Completada")
                .GroupBy(s => new { s.FechaVenta.Year, s.FechaVenta.Month })
                .Select(g => new
                {
                    Año = g.Key.Year,
                    Mes = g.Key.Month,
                    TotalVentas = g.Sum(s => s.Total),
                    CantidadVentas = g.Count()
                })
                .OrderBy(x => x.Año)
                .ThenBy(x => x.Mes)
                .ToListAsync();

            return Ok(ventasPorMes);
        }

        [HttpGet("productos-mas-vendidos")]
        public async Task<ActionResult> GetProductosMasVendidos()
        {
            var fechaInicio = DateTime.Now.AddMonths(-1);
            
            var productosMasVendidos = await _context.SaleDetails
                .Include(sd => sd.Producto)
                .Include(sd => sd.Venta)
                .Where(sd => sd.Venta.FechaVenta >= fechaInicio && sd.Venta.Estado == "Completada")
                .GroupBy(sd => new { sd.ProductoId, sd.Producto.Nombre })
                .Select(g => new
                {
                    ProductoId = g.Key.ProductoId,
                    Nombre = g.Key.Nombre,
                    CantidadVendida = g.Sum(sd => sd.Cantidad),
                    MontoTotal = g.Sum(sd => sd.Subtotal)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(10)
                .ToListAsync();

            return Ok(productosMasVendidos);
        }
    }
}

// Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FerreriaAPI.Data;
using FerreriaAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<FerreriaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ferretería API",
        Version = "v1",
        Description = "API para sistema de inventario y ventas"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FerreriaContext>();
    await context.Database.EnsureCreatedAsync();
}