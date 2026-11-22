-- ============================================================================
-- 03_vistas_consultas.sql
-- Sistema Punto de Venta (POS) - Vistas Multi-Tabla y Consultas para Reportes
-- ============================================================================

USE PuntoDeVentaDB;
GO

-- 1. Vista: Resumen de Ventas con datos de Cliente, Cajero y Caja
CREATE OR ALTER VIEW vw_VentasResumen AS
SELECT 
    v.IdVenta,
    v.Fecha,
    ISNULL(c.Nombre, 'Consumidor Final') AS Cliente,
    u.NombreCompleto AS Cajero,
    v.IdCaja,
    v.Subtotal,
    v.Impuesto,
    v.Total,
    v.Estado,
    (SELECT COUNT(1) FROM DetalleVentas d WHERE d.IdVenta = v.IdVenta) AS CantidadLineas,
    (SELECT ISNULL(SUM(d.Cantidad), 0) FROM DetalleVentas d WHERE d.IdVenta = v.IdVenta) AS ArticulosTotales
FROM Ventas v
LEFT JOIN Clientes c ON v.IdCliente = c.IdCliente
INNER JOIN Usuarios u ON v.IdUsuario = u.IdUsuario;
GO

-- 2. Vista: Detalle completo de ventas cruzando Producto y Categoría
CREATE OR ALTER VIEW vw_DetalleVentasCompleto AS
SELECT 
    d.IdDetalle,
    d.IdVenta,
    v.Fecha,
    p.IdProducto,
    p.Codigo,
    p.Nombre AS Producto,
    cat.Nombre AS Categoria,
    d.Cantidad,
    d.PrecioUnitario,
    d.Subtotal
FROM DetalleVentas d
INNER JOIN Ventas v ON d.IdVenta = v.IdVenta
INNER JOIN Productos p ON d.IdProducto = p.IdProducto
INNER JOIN Categorias cat ON p.IdCategoria = cat.IdCategoria;
GO

-- 3. Vista: Top de productos más vendidos
CREATE OR ALTER VIEW vw_TopProductos AS
SELECT 
    p.IdProducto,
    p.Codigo,
    p.Nombre,
    cat.Nombre AS Categoria,
    SUM(d.Cantidad) AS CantidadVendida,
    SUM(d.Subtotal) AS TotalRecaudado
FROM DetalleVentas d
INNER JOIN Productos p ON d.IdProducto = p.IdProducto
INNER JOIN Categorias cat ON p.IdCategoria = cat.IdCategoria
INNER JOIN Ventas v ON d.IdVenta = v.IdVenta
WHERE v.Estado = 'COMPLETADA'
GROUP BY p.IdProducto, p.Codigo, p.Nombre, cat.Nombre;
GO

-- 4. Vista: Resumen de Cajas y Arqueos
CREATE OR ALTER VIEW vw_ResumenCajas AS
SELECT 
    c.IdCaja,
    u.NombreCompleto AS Cajero,
    c.FechaApertura,
    c.MontoInicial,
    c.FechaCierre,
    c.MontoVentas,
    c.MontoCierre,
    c.Diferencia,
    c.Estado,
    c.Observaciones
FROM Cajas c
INNER JOIN Usuarios u ON c.IdUsuario = u.IdUsuario;
GO
