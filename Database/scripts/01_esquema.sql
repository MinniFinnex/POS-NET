-- ============================================================================
-- 01_esquema.sql
-- Sistema Punto de Venta (POS) - Esquema de Base de Datos
-- ============================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PuntoDeVentaDB')
BEGIN
    CREATE DATABASE PuntoDeVentaDB;
END
GO

USE PuntoDeVentaDB;
GO

-- 1. Roles
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Roles')
BEGIN
    CREATE TABLE Roles (
        IdRol       INT IDENTITY(1,1) PRIMARY KEY,
        Nombre      NVARCHAR(50) NOT NULL UNIQUE
    );
END
GO

-- 2. Usuarios
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        IdUsuario       INT IDENTITY(1,1) PRIMARY KEY,
        NombreCompleto  NVARCHAR(100) NOT NULL,
        Usuario         NVARCHAR(50)  NOT NULL UNIQUE,
        Password        NVARCHAR(100) NOT NULL,
        IdRol           INT NOT NULL REFERENCES Roles(IdRol),
        Activo          BIT NOT NULL DEFAULT 1,
        FechaCreacion   DATETIME NOT NULL DEFAULT GETDATE()
    );
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'Password')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'PasswordHash')
        EXEC sp_rename 'Usuarios.PasswordHash', 'Password', 'COLUMN';
    ELSE
        ALTER TABLE Usuarios ADD Password NVARCHAR(100) NOT NULL DEFAULT 'admin123';
END
GO

-- 3. Categorias
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Categorias')
BEGIN
    CREATE TABLE Categorias (
        IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
        Nombre      NVARCHAR(100) NOT NULL,
        Activo      BIT NOT NULL DEFAULT 1
    );
END
GO

-- 4. Productos
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

-- 5. Clientes
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

-- 6. Cajas (Turnos y Arqueos)
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

-- 7. Ventas
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

-- 8. DetalleVentas
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
