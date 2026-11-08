USE PuntoDeVentaDB;
GO

INSERT INTO Roles (Nombre) VALUES ('Administrador'), ('Cajero');

-- Hash SHA-256 de la contrasena 'admin123'
-- Hash: 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
INSERT INTO Usuarios (NombreCompleto, Usuario, PasswordHash, IdRol)
VALUES ('Administrador', 'admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 1);

INSERT INTO Categorias (Nombre) VALUES ('General'), ('Bebidas'), ('Snacks');

INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo)
VALUES 
('PROD001', 'Coca Cola 600ml', 'Refresco de cola', 2, 12.00, 18.00, 50, 10),
('PROD002', 'Papas Fritas 45g', 'Papas sabor original', 3, 10.00, 16.50, 40, 5),
('PROD003', 'Agua Mineral 500ml', 'Agua purificada', 2, 7.00, 12.00, 60, 10);

INSERT INTO Clientes (Documento, Nombre, Telefono, Email, Direccion)
VALUES ('00000000', 'Cliente General', '0000000000', 'general@pos.local', 'Mostrador');
GO
