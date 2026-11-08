USE PuntoDeVentaDB;
GO

-- 1. Insertar Roles del Sistema
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'Administrador')
    INSERT INTO Roles (Nombre) VALUES ('Administrador');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'Cajero')
    INSERT INTO Roles (Nombre) VALUES ('Cajero');

-- 2. Insertar Usuarios con sus credenciales por defecto (SHA-256)
-- Usuario 1: Administrador (Usuario: admin | Password: admin123)
-- Hash SHA-256: 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Usuario = 'admin')
    INSERT INTO Usuarios (NombreCompleto, Usuario, PasswordHash, IdRol)
    VALUES ('Administrador General', 'admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 1);

-- Usuario 2: Cajero (Usuario: cajero | Password: cajero123)
-- Hash SHA-256: 1ed4353e845e2e537e017c0fac3a0d402d231809b7989e90da15191c1148a93f
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Usuario = 'cajero')
    INSERT INTO Usuarios (NombreCompleto, Usuario, PasswordHash, IdRol)
    VALUES ('Cajero Principal', 'cajero', '1ed4353e845e2e537e017c0fac3a0d402d231809b7989e90da15191c1148a93f', 2);

-- 3. Categorías
IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'General')
    INSERT INTO Categorias (Nombre) VALUES ('General');
IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Bebidas')
    INSERT INTO Categorias (Nombre) VALUES ('Bebidas');
IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Snacks')
    INSERT INTO Categorias (Nombre) VALUES ('Snacks');

-- 4. Productos Iniciales de Muestra
IF NOT EXISTS (SELECT 1 FROM Productos WHERE Codigo = 'PROD001')
    INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo)
    VALUES ('PROD001', 'Coca Cola 600ml', 'Refresco de cola', 2, 12.00, 18.00, 50, 10);

IF NOT EXISTS (SELECT 1 FROM Productos WHERE Codigo = 'PROD002')
    INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo)
    VALUES ('PROD002', 'Papas Fritas 45g', 'Papas sabor original', 3, 10.00, 16.50, 40, 5);

IF NOT EXISTS (SELECT 1 FROM Productos WHERE Codigo = 'PROD003')
    INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo)
    VALUES ('PROD003', 'Agua Mineral 500ml', 'Agua purificada', 2, 7.00, 12.00, 60, 10);

-- 5. Clientes Iniciales
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Documento = '00000000')
    INSERT INTO Clientes (Documento, Nombre, Telefono, Email, Direccion)
    VALUES ('00000000', 'Cliente General (Mostrador)', '0000000000', 'general@pos.local', 'Mostrador');
GO
