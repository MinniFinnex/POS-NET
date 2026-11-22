-- ============================================================================
-- 02_datos_semilla.sql
-- Sistema Punto de Venta (POS) - Datos Iniciales y Catálogos Semilla
-- ============================================================================

USE PuntoDeVentaDB;
GO

-- Roles del Sistema
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'Administrador')
    INSERT INTO Roles (Nombre) VALUES ('Administrador');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'Cajero')
    INSERT INTO Roles (Nombre) VALUES ('Cajero');

-- Usuarios por Defecto
-- Administrador: admin / admin123
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Usuario = 'admin')
    INSERT INTO Usuarios (NombreCompleto, Usuario, Password, IdRol)
    VALUES ('Administrador General', 'admin', 'admin123', 1);
ELSE
    UPDATE Usuarios SET Password = 'admin123' WHERE Usuario = 'admin';

-- Cajero: cajero / cajero123
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Usuario = 'cajero')
    INSERT INTO Usuarios (NombreCompleto, Usuario, Password, IdRol)
    VALUES ('Cajero Principal', 'cajero', 'cajero123', 2);
ELSE
    UPDATE Usuarios SET Password = 'cajero123' WHERE Usuario = 'cajero';

-- Categorías
IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'General')
    INSERT INTO Categorias (Nombre) VALUES ('General');
IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Bebidas')
    INSERT INTO Categorias (Nombre) VALUES ('Bebidas');
IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Snacks')
    INSERT INTO Categorias (Nombre) VALUES ('Snacks');

-- Productos Iniciales
IF NOT EXISTS (SELECT 1 FROM Productos WHERE Codigo = 'PROD001')
    INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo)
    VALUES ('PROD001', 'Coca Cola 600ml', 'Refresco de cola en botella de plástico', 2, 12.00, 18.00, 50, 10);

IF NOT EXISTS (SELECT 1 FROM Productos WHERE Codigo = 'PROD002')
    INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo)
    VALUES ('PROD002', 'Papas Fritas 45g', 'Papas fritas sabor sal clásico', 3, 10.00, 16.50, 40, 5);

IF NOT EXISTS (SELECT 1 FROM Productos WHERE Codigo = 'PROD003')
    INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo)
    VALUES ('PROD003', 'Agua Mineral 500ml', 'Agua mineral purificada con gas', 2, 7.00, 12.00, 60, 10);

-- Cliente por Defecto (Consumidor Final)
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Documento = '00000000')
    INSERT INTO Clientes (Documento, Nombre, Telefono, Email, Direccion)
    VALUES ('00000000', 'Cliente General (Mostrador)', '0000000000', 'general@pos.local', 'Mostrador');

-- Turno de Caja Inicial (Ejemplo cerrado para trazabilidad histórica)
IF NOT EXISTS (SELECT 1 FROM Cajas)
BEGIN
    INSERT INTO Cajas (IdUsuario, FechaApertura, MontoInicial, FechaCierre, MontoVentas, MontoCierre, Diferencia, Observaciones, Estado)
    VALUES (1, DATEADD(HOUR, -6, GETDATE()), 500.00, DATEADD(HOUR, -2, GETDATE()), 105.00, 605.00, 0.00, 'Cierre de turno previo conforme', 'CERRADA');
END
GO
