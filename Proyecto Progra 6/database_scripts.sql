-- =============================================
-- SCRIPT DE CREACIÓN DE BASE DE DATOS
-- Sistema de Inventario y Ventas - Ferretería
-- =============================================

-- Crear la base de datos
CREATE DATABASE FerreriaDB;
GO

USE FerreriaDB;
GO

-- =============================================
-- CREACIÓN DE TABLAS
-- =============================================

-- Tabla de Usuarios
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Rol NVARCHAR(50) NOT NULL CHECK (Rol IN ('Admin', 'Vendedor', 'Cliente')),
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    INDEX IX_Users_Email (Email),
    INDEX IX_Users_Rol (Rol)
);

-- Tabla de Categorías
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Icono NVARCHAR(10) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    INDEX IX_Categories_Nombre (Nombre)
);

-- Tabla de Proveedores
CREATE TABLE Suppliers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(200) NOT NULL,
    Contacto NVARCHAR(100) NULL,
    Telefono NVARCHAR(20) NULL,
    Email NVARCHAR(255) NULL,
    Direccion NVARCHAR(300) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    INDEX IX_Suppliers_Nombre (Nombre)
);

-- Tabla de Productos
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Precio DECIMAL(18,2) NOT NULL CHECK (Precio >= 0),
    Stock INT NOT NULL CHECK (Stock >= 0),
    StockMinimo INT NOT NULL CHECK (StockMinimo > 0),
    CodigoBarras NVARCHAR(50) NULL UNIQUE,
    Imagen NVARCHAR(255) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    CategoriaId INT NOT NULL,
    ProveedorId INT NOT NULL,
    
    CONSTRAINT FK_Products_Categories 
        FOREIGN KEY (CategoriaId) REFERENCES Categories(Id),
    CONSTRAINT FK_Products_Suppliers 
        FOREIGN KEY (ProveedorId) REFERENCES Suppliers(Id),
    
    INDEX IX_Products_Nombre (Nombre),
    INDEX IX_Products_CodigoBarras (CodigoBarras),
    INDEX IX_Products_Stock (Stock),
    INDEX IX_Products_Categoria (CategoriaId),
    INDEX IX_Products_Proveedor (ProveedorId)
);

-- Tabla de Ventas
CREATE TABLE Sales (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NumeroVenta NVARCHAR(50) NOT NULL UNIQUE,
    ClienteNombre NVARCHAR(200) NOT NULL,
    ClienteIdentificacion NVARCHAR(50) NULL,
    ClienteTelefono NVARCHAR(20) NULL,
    Total DECIMAL(18,2) NOT NULL CHECK (Total >= 0),
    Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Impuesto >= 0),
    Descuento DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Descuento >= 0),
    FechaVenta DATETIME2 NOT NULL DEFAULT GETDATE(),
    Estado NVARCHAR(20) NOT NULL DEFAULT 'Completada' 
        CHECK (Estado IN ('Completada', 'Cancelada', 'Pendiente')),
    UsuarioId INT NOT NULL,
    
    CONSTRAINT FK_Sales_Users 
        FOREIGN KEY (UsuarioId) REFERENCES Users(Id),
    
    INDEX IX_Sales_NumeroVenta (NumeroVenta),
    INDEX IX_Sales_FechaVenta (FechaVenta),
    INDEX IX_Sales_Cliente (ClienteNombre),
    INDEX IX_Sales_Usuario (UsuarioId)
);

-- Tabla de Detalles de Venta
CREATE TABLE SaleDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario DECIMAL(18,2) NOT NULL CHECK (PrecioUnitario >= 0),
    Descuento DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Descuento >= 0),
    Subtotal DECIMAL(18,2) NOT NULL CHECK (Subtotal >= 0),
    VentaId INT NOT NULL,
    ProductoId INT NOT NULL,
    
    CONSTRAINT FK_SaleDetails_Sales 
        FOREIGN KEY (VentaId) REFERENCES Sales(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SaleDetails_Products 
        FOREIGN KEY (ProductoId) REFERENCES Products(Id),
    
    INDEX IX_SaleDetails_Venta (VentaId),
    INDEX IX_SaleDetails_Producto (ProductoId)
);

-- =============================================
-- TRIGGERS PARA AUDITORÍA Y CONTROL
-- =============================================

-- Trigger para actualizar stock automáticamente
CREATE TRIGGER TR_SaleDetails_UpdateStock
ON SaleDetails
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Para inserciones y actualizaciones
    IF EXISTS(SELECT * FROM inserted)
    BEGIN
        -- Reducir stock para nuevos detalles
        UPDATE p
        SET Stock = Stock - i.Cantidad
        FROM Products p
        INNER JOIN inserted i ON p.Id = i.ProductoId;
        
        -- Si es actualización, restaurar stock anterior
        IF EXISTS(SELECT * FROM deleted)
        BEGIN
            UPDATE p
            SET Stock = Stock + d.Cantidad
            FROM Products p
            INNER JOIN deleted d ON p.Id = d.ProductoId;
        END
    END
    
    -- Para eliminaciones únicamente
    IF EXISTS(SELECT * FROM deleted) AND NOT EXISTS(SELECT * FROM inserted)
    BEGIN
        -- Restaurar stock
        UPDATE p
        SET Stock = Stock + d.Cantidad
        FROM Products p
        INNER JOIN deleted d ON p.Id = d.ProductoId;
    END
END;
GO

-- Trigger para generar número de venta automático
CREATE TRIGGER TR_Sales_GenerateNumber
ON Sales
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Sales (
        NumeroVenta, ClienteNombre, ClienteIdentificacion, ClienteTelefono,
        Total, Impuesto, Descuento, FechaVenta, Estado, UsuarioId
    )
    SELECT 
        'V' + FORMAT(GETDATE(), 'yyyyMMdd') + FORMAT(GETDATE(), 'HHmmss'),
        ClienteNombre, ClienteIdentificacion, ClienteTelefono,
        Total, Impuesto, Descuento, 
        ISNULL(FechaVenta, GETDATE()), 
        ISNULL(Estado, 'Completada'), 
        UsuarioId
    FROM inserted;
END;
GO

-- =============================================
-- DATOS DE PRUEBA (SEED DATA)
-- =============================================

-- Insertar usuarios predeterminados
INSERT INTO Users (Nombre, Email, PasswordHash, Rol) VALUES
('Administrador', 'admin@ferreteria.com', '$2a$11$8gvzKz5hE9xVx.x3iF4fUeHc.5f3QgvgZgvKz5hE9xVx.x3iF4fUe', 'Admin'),
('Vendedor Demo', 'vendedor@ferreteria.com', '$2a$11$8gvzKz5hE9xVx.x3iF4fUeHc.5f3QgvgZgvKz5hE9xVx.x3iF4fUe', 'Vendedor'),
('Juan Pérez', 'juan.perez@email.com', '$2a$11$8gvzKz5hE9xVx.x3iF4fUeHc.5f3QgvgZgvKz5hE9xVx.x3iF4fUe', 'Vendedor'),
('María González', 'maria.gonzalez@email.com', '$2a$11$8gvzKz5hE9xVx.x3iF4fUeHc.5f3QgvgZgvKz5hE9xVx.x3iF4fUe', 'Vendedor');

-- Insertar categorías
INSERT INTO Categories (Nombre, Descripcion, Icono) VALUES
('Herramientas', 'Herramientas manuales y eléctricas para construcción', '🔨'),
('Materiales', 'Materiales de construcción y acabados', '🧱'),
('Plomería', 'Accesorios y materiales de plomería', '🔧'),
('Electricidad', 'Materiales e instalaciones eléctricas', '💡'),
('Jardinería', 'Herramientas y productos para jardín', '🌱'),
('Ferretería', 'Artículos varios de ferretería', '⚙️'),
('Pinturas', 'Pinturas y accesorios para pintura', '🎨'),
('Seguridad', 'Equipos de protección y seguridad', '🦺');

-- Insertar proveedores
INSERT INTO Suppliers (Nombre, Contacto, Telefono, Email, Direccion) VALUES
('Distribuidora Central S.A.', 'Juan Pérez', '2222-3333', 'ventas@distcentral.com', 'San José, Costa Rica'),
('Importadora Tools Ltda.', 'María González', '2444-5555', 'info@tools.cr', 'Cartago, Costa Rica'),
('Ferretería Nacional', 'Carlos Rodríguez', '2666-7777', 'compras@ferrenacional.com', 'Alajuela, Costa Rica'),
('Materiales del Pacífico', 'Ana Jiménez', '2777-8888', 'ventas@matpacifico.co.cr', 'Puntarenas, Costa Rica'),
('Eléctricos Modernos', 'Luis Vargas', '2888-9999', 'pedidos@elecmodernos.cr', 'Heredia, Costa Rica'),
('Pinturas y Más S.A.', 'Carmen Solano', '2999-1111', 'ventas@pinturasmas.com', 'San José, Costa Rica');

-- Insertar productos de ejemplo
INSERT INTO Products (Nombre, Descripcion, Precio, Stock, StockMinimo, CodigoBarras, CategoriaId, ProveedorId) VALUES
-- Herramientas
('Martillo de Carpintero 16oz', 'Martillo con mango de madera, cabeza de acero forjado', 15.50, 25, 5, '7501234567890', 1, 1),
('Taladro Eléctrico 1/2"', 'Taladro reversible con cable, 650W de potencia', 89.99, 12, 3, '7501234567891', 1, 2),
('Destornillador Phillips #2', 'Destornillador con mango ergonómico antideslizante', 4.25, 50, 10, '7501234567892', 1, 1),
('Sierra Circular 7 1/4"', 'Sierra circular profesional con guía láser', 156.00, 8, 2, '7501234567893', 1, 2),
('Nivel de Burbuja 24"', 'Nivel de aluminio con 3 burbujas, precisión garantizada', 28.75, 18, 4, '7501234567894', 1, 1),

-- Materiales
('Cemento Gris 50kg', 'Cemento Portland para construcción general', 8.50, 120, 20, '7501234567895', 2, 3),
('Varilla de Hierro 1/2"', 'Varilla corrugada de 12 metros de longitud', 12.30, 200, 30, '7501234567896', 2, 3),
('Bloque de Concreto 15cm', 'Bloque para construcción estándar', 1.85, 500, 50, '7501234567897', 2, 4),
('Arena de Río m³', 'Arena lavada para construcción', 35.00, 15, 3, '7501234567898', 2, 4),
('Grava Triturada m³', 'Grava de diferentes tamaños para concreto', 42.50, 12, 2, '7501234567899', 2, 4),

-- Plomería
('Tubería PVC 4" x 6m', 'Tubería para desagües y alcantarillado', 18.90, 45, 8, '7501234567900', 3, 1),
('Codo PVC 90° 4"', 'Codo de 90 grados para cambio de dirección', 3.75, 80, 15, '7501234567901', 3, 1),
('Llave de Paso 1/2"', 'Válvula de cierre con rosca NPT', 12.60, 30, 6, '7501234567902', 3, 1),
('Inodoro Completo', 'Inodoro de una pieza con tapa y accesorios', 185.00, 6, 2, '7501234567903', 3, 3),
('Lavatorio Pedestal', 'Lavamanos con pedestal estilo clásico', 95.50, 8, 2, '7501234567904', 3, 3),

-- Electricidad
('Cable Eléctrico 12 AWG', 'Cable sólido de cobre para instalaciones', 2.85, 300, 50, '7501234567905', 4, 5),
('Toma Corriente Doble', 'Tomacorriente de 110V con placa', 8.90, 60, 12, '7501234567906', 4, 5),
('Interruptor Simple', 'Interruptor de pared estándar', 6.45, 75, 15, '7501234567907', 4, 5),
('Bombilla LED 9W', 'Bombilla LED equivalente a 60W incandescente', 7.20, 100, 20, '7501234567908', 4, 5),
('Caja de Breaker 12 Espacios', 'Panel eléctrico residencial completo', 125.00, 5, 1, '7501234567909', 4, 5),

-- Jardinería
('Pala Jardín Mango Largo', 'Pala con hoja de acero y mango de madera', 22.50, 20, 4, '7501234567910', 5, 1),
('Manguera Jardín 50pies', 'Manguera flexible con conectores', 35.80, 15, 3, '7501234567911', 5, 1),
('Tijeras de Podar', 'Tijeras de acero inoxidable para ramas', 18.60, 25, 5, '7501234567912', 5, 2),
('Fertilizante 10-10-10', 'Fertilizante balanceado para plantas', 8.90, 40, 8, '7501234567913', 5, 1),
('Regadera 2 Galones', 'Regadera plástica con rociador', 12.75, 18, 4, '7501234567914', 5, 1),

-- Pinturas
('Pintura Látex Blanca 1Gal', 'Pintura de interior lavable', 24.50, 35, 8, '7501234567915', 7, 6),
('Pintura Aceite Azul 1/4Gal', 'Pintura de exterior resistente', 18.90, 20, 4, '7501234567916', 7, 6),
('Rodillo 9" con Bandeja', 'Kit completo para pintar paredes', 15.60, 30, 6, '7501234567917', 7, 6),
('Pincel 2" Cerda Natural', 'Pincel profesional para acabados', 8.75, 45, 10, '7501234567918', 7, 6),
('Thinner Estándar 1Gal', 'Diluyente para pinturas a base de aceite', 12.30, 25, 5, '7501234567919', 7, 6);

-- =============================================
-- VISTAS PARA REPORTES
-- =============================================

-- Vista de productos con información completa
CREATE VIEW VW_ProductosCompletos AS
SELECT 
    p.Id,
    p.Nombre,
    p.Descripcion,
    p.Precio,
    p.Stock,
    p.StockMinimo,
    p.CodigoBarras,
    c.Nombre AS Categoria,
    s.Nombre AS Proveedor,
    p.FechaCreacion,
    CASE 
        WHEN p.Stock <= p.StockMinimo THEN 'Crítico'
        WHEN p.Stock <= (p.StockMinimo * 2) THEN 'Bajo'
        ELSE 'Normal'
    END AS EstadoStock
FROM Products p
INNER JOIN Categories c ON p.CategoriaId = c.Id
INNER JOIN Suppliers s ON p.ProveedorId = s.Id
WHERE p.Activo = 1;
GO

-- Vista de ventas con detalles
CREATE VIEW VW_VentasCompletas AS
SELECT 
    s.Id,
    s.NumeroVenta,
    s.ClienteNombre,
    s.Total,
    s.FechaVenta,
    u.Nombre AS Vendedor,
    COUNT(sd.Id) AS CantidadItems,
    SUM(sd.Cantidad) AS TotalProductos
FROM Sales s
INNER JOIN Users u ON s.UsuarioId = u.Id
LEFT JOIN SaleDetails sd ON s.Id = sd.VentaId
GROUP BY s.Id, s.NumeroVenta, s.ClienteNombre, s.Total, s.FechaVenta, u.Nombre;
GO

-- Vista de productos más vendidos
CREATE VIEW VW_ProductosMasVendidos AS
SELECT TOP 100
    p.Id,
    p.Nombre AS Producto,
    c.Nombre AS Categoria,
    SUM(sd.Cantidad) AS TotalVendido,
    SUM(sd.Subtotal) AS MontoTotal,
    COUNT(DISTINCT sd.VentaId) AS NumeroVentas
FROM Products p
INNER JOIN Categories c ON p.CategoriaId = c.Id
INNER JOIN SaleDetails sd ON p.Id = sd.ProductoId
INNER JOIN Sales s ON sd.VentaId = s.Id
WHERE s.Estado = 'Completada'
GROUP BY p.Id, p.Nombre, c.Nombre
ORDER BY TotalVendido DESC;
GO

-- =============================================
-- PROCEDIMIENTOS ALMACENADOS
-- =============================================

-- Procedimiento para obtener reporte de ventas por período
CREATE PROCEDURE SP_ReporteVentasPorPeriodo
    @FechaInicio DATE,
    @FechaFin DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CAST(s.FechaVenta AS DATE) AS Fecha,
        COUNT(s.Id) AS NumeroVentas,
        SUM(s.Total) AS MontoTotal,
        SUM(sd.Cantidad) AS ProductosVendidos,
        AVG(s.Total) AS PromedioVenta
    FROM Sales s
    INNER JOIN SaleDetails sd ON s.Id = sd.VentaId
    WHERE s.FechaVenta BETWEEN @FechaInicio AND @FechaFin
        AND s.Estado = 'Completada'
    GROUP BY CAST(s.FechaVenta AS DATE)
    ORDER BY Fecha;
END;
GO

-- Procedimiento para obtener inventario crítico
CREATE PROCEDURE SP_InventarioCritico
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id,
        p.Nombre,
        p.Stock,
        p.StockMinimo,
        c.Nombre AS Categoria,
        s.Nombre AS Proveedor,
        s.Telefono AS TelefonoProveedor,
        CASE 
            WHEN p.Stock = 0 THEN 'Sin Stock'
            WHEN p.Stock <= p.StockMinimo THEN 'Crítico'
            ELSE 'Bajo'
        END AS Prioridad
    FROM Products p
    INNER JOIN Categories c ON p.CategoriaId = c.Id
    INNER JOIN Suppliers s ON p.ProveedorId = s.Id
    WHERE p.Stock <= (p.StockMinimo * 1.5)
        AND p.Activo = 1
    ORDER BY 
        CASE WHEN p.Stock = 0 THEN 1 
             WHEN p.Stock <= p.StockMinimo THEN 2 
             ELSE 3 END,
        p.Stock ASC;
END;
GO

-- =============================================
-- ÍNDICES ADICIONALES PARA OPTIMIZACIÓN
-- =============================================

CREATE INDEX IX_Sales_FechaVenta_Estado ON Sales (FechaVenta, Estado);
CREATE INDEX IX_SaleDetails_VentaId_ProductoId ON SaleDetails (VentaId, ProductoId);
CREATE INDEX IX_Products_Stock_StockMinimo ON Products (Stock, StockMinimo);

-- =============================================
-- SCRIPT DE VERIFICACIÓN
-- =============================================

PRINT 'Base de datos creada exitosamente';
PRINT 'Tablas creadas: ' + CAST((SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE') AS VARCHAR);
PRINT 'Usuarios insertados: ' + CAST((SELECT COUNT(*) FROM Users) AS VARCHAR);
PRINT 'Categorías insertadas: ' + CAST((SELECT COUNT(*) FROM Categories) AS VARCHAR);
PRINT 'Proveedores insertados: ' + CAST((SELECT COUNT(*) FROM Suppliers) AS VARCHAR);
PRINT 'Productos insertados: ' + CAST((SELECT COUNT(*) FROM Products) AS VARCHAR);

-- Verificar integridad
SELECT 'Verificación completada - ' + CAST(GETDATE() AS VARCHAR) AS Resultado;