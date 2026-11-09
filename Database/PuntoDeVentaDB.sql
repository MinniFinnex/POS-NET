-- ============================================================================
-- BASE DE DATOS COMPLETA: Sistema Punto de Venta (POS)
-- Incluye: Creación de BD, Tablas, Relaciones y Datos Semilla
-- ============================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PuntoDeVentaDB')
BEGIN
    CREATE DATABASE PuntoDeVentaDB;
END
GO

USE PuntoDeVentaDB;
GO

-- ----------------------------------------------------------------------------
-- 1. TABLA: Roles
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Roles')
BEGIN
    CREATE TABLE Roles (
        IdRol       INT IDENTITY(1,1) PRIMARY KEY,
        Nombre      NVARCHAR(50) NOT NULL UNIQUE
    );
END
GO

-- ----------------------------------------------------------------------------
-- 2. TABLA: Usuarios
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        IdUsuario       INT IDENTITY(1,1) PRIMARY KEY,
        NombreCompleto  NVARCHAR(100) NOT NULL,
        Usuario         NVARCHAR(50)  NOT NULL UNIQUE,
        PasswordHash    NVARCHAR(256) NOT NULL,
        IdRol           INT NOT NULL REFERENCES Roles(IdRol),
        Activo          BIT NOT NULL DEFAULT 1,
        FechaCreacion   DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- ----------------------------------------------------------------------------
-- 3. TABLA: Categorias
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Categorias')
BEGIN
    CREATE TABLE Categorias (
        IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
        Nombre      NVARCHAR(100) NOT NULL,
        Activo      BIT NOT NULL DEFAULT 1
    );
END
GO

-- ----------------------------------------------------------------------------
-- 4. TABLA: Productos
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Productos')
BEGIN
    CREATE TABLE Productos (
        IdProducto      INT IDENTITY(1,1) PRIMARY KEY,
        Codigo          NVARCHAR(50)  NOT NULL UNIQUE,
        Nombre          NVARCHAR(150) NOT NULL,
        Descripcion     NVARCHAR(300) NULL,
        IdCategoria     INT NOT NULL REFERENCES Categorias(IdCategoria),
        PrecioCompra    DECIMAL(10,2) NOT NULL,
        PrecioVenta     DECIMAL(10,2) NOT NULL,
        Stock           INT NOT NULL DEFAULT 0,
        StockMinimo     INT NOT NULL DEFAULT 5,
        Activo          BIT NOT NULL DEFAULT 1
    );
END
GO

-- ----------------------------------------------------------------------------
-- 5. TABLA: Clientes
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        IdCliente   INT IDENTITY(1,1) PRIMARY KEY,
        Documento   NVARCHAR(30)  NULL,
        Nombre      NVARCHAR(100) NOT NULL,
        Telefono    NVARCHAR(20)  NULL,
        Email       NVARCHAR(100) NULL,
        Direccion   NVARCHAR(200) NULL,
        Activo      BIT NOT NULL DEFAULT 1
    );
END
GO

-- ----------------------------------------------------------------------------
-- 6. TABLA: Cajas (Control de Turnos y Arqueos)
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cajas')
BEGIN
    CREATE TABLE Cajas (
        IdCaja          INT IDENTITY(1,1) PRIMARY KEY,
        IdUsuario       INT NOT NULL REFERENCES Usuarios(IdUsuario),
        FechaApertura   DATETIME NOT NULL DEFAULT GETDATE(),
        MontoInicial    DECIMAL(12,2) NOT NULL DEFAULT 0,
        FechaCierre     DATETIME NULL,
        MontoVentas     DECIMAL(12,2) NOT NULL DEFAULT 0,
        MontoCierre     DECIMAL(12,2) NULL,
        Diferencia      DECIMAL(12,2) NULL,
        Observaciones   NVARCHAR(300) NULL,
        Estado          NVARCHAR(20) NOT NULL DEFAULT 'ABIERTA'
    );
END
GO

-- ----------------------------------------------------------------------------
-- 7. TABLA: Ventas
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Ventas')
BEGIN
    CREATE TABLE Ventas (
        IdVenta     INT IDENTITY(1,1) PRIMARY KEY,
        Fecha       DATETIME NOT NULL DEFAULT GETDATE(),
        IdCliente   INT NULL REFERENCES Clientes(IdCliente),
        IdUsuario   INT NOT NULL REFERENCES Usuarios(IdUsuario),
        IdCaja      INT NULL REFERENCES Cajas(IdCaja),
        Subtotal    DECIMAL(12,2) NOT NULL,
        Impuesto    DECIMAL(12,2) NOT NULL,
        Total       DECIMAL(12,2) NOT NULL,
        Estado      NVARCHAR(20)  NOT NULL DEFAULT 'COMPLETADA'
    );
END
GO

-- ----------------------------------------------------------------------------
-- 8. TABLA: DetalleVentas
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DetalleVentas')
BEGIN
    CREATE TABLE DetalleVentas (
        IdDetalle       INT IDENTITY(1,1) PRIMARY KEY,
        IdVenta         INT NOT NULL REFERENCES Ventas(IdVenta),
        IdProducto      INT NOT NULL REFERENCES Productos(IdProducto),
        Cantidad        INT NOT NULL,
        PrecioUnitario  DECIMAL(10,2) NOT NULL,
        Subtotal        DECIMAL(12,2) NOT NULL
    );
END
GO

-- ============================================================================
-- INSERCIÓN DE DATOS INICIALES (SEED DATA)
-- ============================================================================

-- Roles
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'Administrador')
    INSERT INTO Roles (Nombre) VALUES ('Administrador');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'Cajero')
    INSERT INTO Roles (Nombre) VALUES ('Cajero');

-- Usuarios por defecto
-- admin / admin123 (SHA-256: 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9)
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Usuario = 'admin')
    INSERT INTO Usuarios (NombreCompleto, Usuario, PasswordHash, IdRol)
    VALUES ('Administrador General', 'admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 1);

-- cajero / cajero123 (SHA-256: 1ed4353e845e2e537e017c0fac3a0d402d231809b7989e90da15191c1148a93f)
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Usuario = 'cajero')
    INSERT INTO Usuarios (NombreCompleto, Usuario, PasswordHash, IdRol)
    VALUES ('Cajero Principal', 'cajero', '1ed4353e845e2e537e017c0fac3a0d402d231809b7989e90da15191c1148a93f', 2);

-- Categorías
IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'General')
    INSERT INTO Categorias (Nombre) VALUES ('General');
IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Bebidas')
    INSERT INTO Categorias (Nombre) VALUES ('Bebidas');
IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Snacks')
    INSERT INTO Categorias (Nombre) VALUES ('Snacks');

-- Productos iniciales
IF NOT EXISTS (SELECT 1 FROM Productos WHERE Codigo = 'PROD001')
    INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo)
    VALUES ('PROD001', 'Coca Cola 600ml', 'Refresco de cola en botella de plástico', 2, 12.00, 18.00, 50, 10);

IF NOT EXISTS (SELECT 1 FROM Productos WHERE Codigo = 'PROD002')
    INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo)
    VALUES ('PROD002', 'Papas Fritas 45g', 'Papas fritas sabor sal clásico', 3, 10.00, 16.50, 40, 5);

IF NOT EXISTS (SELECT 1 FROM Productos WHERE Codigo = 'PROD003')
    INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo)
    VALUES ('PROD003', 'Agua Mineral 500ml', 'Agua mineral purificada con gas', 2, 7.00, 12.00, 60, 10);

-- Cliente por defecto
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Documento = '00000000')
    INSERT INTO Clientes (Documento, Nombre, Telefono, Email, Direccion)
    VALUES ('00000000', 'Cliente General (Mostrador)', '0000000000', 'general@pos.local', 'Mostrador');

-- Turno de Caja Inicial (Ejemplo cerrado)
IF NOT EXISTS (SELECT 1 FROM Cajas)
BEGIN
    INSERT INTO Cajas (IdUsuario, FechaApertura, MontoInicial, FechaCierre, MontoVentas, MontoCierre, Diferencia, Observaciones, Estado)
    VALUES (1, DATEADD(HOUR, -6, GETDATE()), 500.00, DATEADD(HOUR, -2, GETDATE()), 105.00, 605.00, 0.00, 'Cierre de turno previo conforme', 'CERRADA');
END
GO
