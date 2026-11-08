USE PuntoDeVentaDB;
GO

CREATE TABLE Roles (
    IdRol       INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Usuarios (
    IdUsuario       INT IDENTITY(1,1) PRIMARY KEY,
    NombreCompleto  NVARCHAR(100) NOT NULL,
    Usuario         NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(256) NOT NULL,
    IdRol           INT NOT NULL REFERENCES Roles(IdRol),
    Activo          BIT NOT NULL DEFAULT 1,
    FechaCreacion   DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Categorias (
    IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(100) NOT NULL,
    Activo      BIT NOT NULL DEFAULT 1
);

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

CREATE TABLE Clientes (
    IdCliente   INT IDENTITY(1,1) PRIMARY KEY,
    Documento   NVARCHAR(30)  NULL,
    Nombre      NVARCHAR(100) NOT NULL,
    Telefono    NVARCHAR(20)  NULL,
    Email       NVARCHAR(100) NULL,
    Direccion   NVARCHAR(200) NULL,
    Activo      BIT NOT NULL DEFAULT 1
);

CREATE TABLE Ventas (
    IdVenta     INT IDENTITY(1,1) PRIMARY KEY,
    Fecha       DATETIME NOT NULL DEFAULT GETDATE(),
    IdCliente   INT NULL REFERENCES Clientes(IdCliente),
    IdUsuario   INT NOT NULL REFERENCES Usuarios(IdUsuario),
    Subtotal    DECIMAL(12,2) NOT NULL,
    Impuesto    DECIMAL(12,2) NOT NULL,
    Total       DECIMAL(12,2) NOT NULL,
    Estado      NVARCHAR(20)  NOT NULL DEFAULT 'COMPLETADA'
);

CREATE TABLE DetalleVentas (
    IdDetalle       INT IDENTITY(1,1) PRIMARY KEY,
    IdVenta         INT NOT NULL REFERENCES Ventas(IdVenta),
    IdProducto      INT NOT NULL REFERENCES Productos(IdProducto),
    Cantidad        INT NOT NULL,
    PrecioUnitario  DECIMAL(10,2) NOT NULL,
    Subtotal        DECIMAL(12,2) NOT NULL
);
GO
