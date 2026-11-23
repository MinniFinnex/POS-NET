/* ====================================================================================
   SCRIPT EMPRESARIAL DE INSERCIÓN DE DATOS PARA: PuntoDeVentaDB
   Motor: Microsoft SQL Server (T-SQL)
   
   ARQUITECTURA Y DISEÑO:
   1. CAPAS DE INTEGRIDAD: Organizado en 6 capas jerárquicas según dependencias FK.
   2. EXCEPCIONES GLOBALES: Bloque TRY/CATCH global, control de XACT_STATE(), 
      captura de metadata de error y re-lanzamiento atómico con THROW.
   3. TRAZABILIDAD: Tabla de auditoría persistente (dbo.BitacoraTrazabilidad) con 
      CorrelationId (GUID), timestamp ISO, usuario, host y conteo de filas afectadas.
   ==================================================================================== */

USE PuntoDeVentaDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* =====================================================================
   CAPA 0: INFRAESTRUCTURA DE TRAZABILIDAD Y AUDITORÍA
   ===================================================================== */
IF OBJECT_ID(N'dbo.BitacoraTrazabilidad', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BitacoraTrazabilidad (
        IdLog            BIGINT IDENTITY(1,1) NOT NULL,
        CorrelationId    UNIQUEIDENTIFIER     NOT NULL,
        Capa             NVARCHAR(50)         NOT NULL,
        Operacion        NVARCHAR(150)        NOT NULL,
        FilasAfectadas   INT                  NOT NULL CONSTRAINT DF_Bitacora_Filas DEFAULT 0,
        Estado           NVARCHAR(20)         NOT NULL, -- 'INICIADO', 'EXITO', 'ERROR'
        MensajeDetalle   NVARCHAR(MAX)        NULL,
        UsuarioEjecutor  NVARCHAR(100)        NOT NULL CONSTRAINT DF_Bitacora_Usuario DEFAULT SUSER_SNAME(),
        HostEjecutor     NVARCHAR(100)        NOT NULL CONSTRAINT DF_Bitacora_Host DEFAULT HOST_NAME(),
        FechaRegistro    DATETIMEOFFSET       NOT NULL CONSTRAINT DF_Bitacora_Fecha DEFAULT SYSDATETIMEOFFSET(),
        CONSTRAINT PK_BitacoraTrazabilidad PRIMARY KEY CLUSTERED (IdLog)
    );

    CREATE NONCLUSTERED INDEX IX_Bitacora_CorrelationId 
        ON dbo.BitacoraTrazabilidad (CorrelationId, FechaRegistro);
END;
GO

/* =====================================================================
   EJECUCIÓN TRANSACCIONAL CONTROLADA CON GESTIÓN GLOBAL DE EXCEPCIONES
   ===================================================================== */
DECLARE @CorrelationId UNIQUEIDENTIFIER = NEWID();
DECLARE @Filas INT = 0;
DECLARE @CapaActual NVARCHAR(50) = N'Capa 0: Inicio';

BEGIN TRANSACTION;

BEGIN TRY

    -- Registro inicial de trazabilidad
    INSERT INTO dbo.BitacoraTrazabilidad (CorrelationId, Capa, Operacion, FilasAfectadas, Estado, MensajeDetalle)
    VALUES (@CorrelationId, @CapaActual, N'Inicio del proceso de carga de datos', 0, N'INICIADO', N'Semilla de datos inicializada');

    /* -----------------------------------------------------------------
       CAPA 1: CATÁLOGOS BASE INDEPENDIENTES (Sin dependencias externas)
       ----------------------------------------------------------------- */
    SET @CapaActual = N'Capa 1: Catálogos Base';

    -- 1.1 Roles
    INSERT INTO dbo.Roles (Nombre)
    SELECT r.Nombre
    FROM (VALUES (N'Administrador'), (N'Cajero'), (N'Supervisor'), (N'Bodeguero')) AS r(Nombre)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles x WHERE x.Nombre = r.Nombre);
    SET @Filas = @@ROWCOUNT;

    INSERT INTO dbo.BitacoraTrazabilidad (CorrelationId, Capa, Operacion, FilasAfectadas, Estado, MensajeDetalle)
    VALUES (@CorrelationId, @CapaActual, N'Sincronización de Roles', @Filas, N'EXITO', N'Roles base verificados e insertados');

    -- 1.2 Categorías
    INSERT INTO dbo.Categorias (Nombre, Activo)
    SELECT c.Nombre, 1
    FROM (VALUES 
        (N'General'), (N'Bebidas'), (N'Snacks'), (N'Lácteos y Quesos'),
        (N'Abarrotes y Despensa'), (N'Limpieza del Hogar'), (N'Cuidado Personal'),
        (N'Panadería y Dulces'), (N'Embutidos y Carnes Frías'), (N'Frutas y Verduras')
    ) AS c(Nombre)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Categorias x WHERE x.Nombre = c.Nombre);
    SET @Filas = @@ROWCOUNT;

    INSERT INTO dbo.BitacoraTrazabilidad (CorrelationId, Capa, Operacion, FilasAfectadas, Estado, MensajeDetalle)
    VALUES (@CorrelationId, @CapaActual, N'Sincronización de Categorías', @Filas, N'EXITO', N'Categorías de productos verificadas e insertadas');


    /* -----------------------------------------------------------------
       CAPA 2: ENTIDADES MAESTRAS (Dependientes de Capa 1)
       ----------------------------------------------------------------- */
    SET @CapaActual = N'Capa 2: Entidades Maestras';

    DECLARE @RolAdmin INT = (SELECT TOP 1 IdRol FROM dbo.Roles WHERE Nombre = N'Administrador');
    DECLARE @RolCajero INT = (SELECT TOP 1 IdRol FROM dbo.Roles WHERE Nombre = N'Cajero');
    DECLARE @RolSupervisor INT = (SELECT TOP 1 IdRol FROM dbo.Roles WHERE Nombre = N'Supervisor');
    DECLARE @RolBodeguero INT = (SELECT TOP 1 IdRol FROM dbo.Roles WHERE Nombre = N'Bodeguero');

    -- 2.1 Usuarios
    INSERT INTO dbo.Usuarios (NombreCompleto, Usuario, Password, IdRol, Activo, FechaCreacion)
    SELECT u.NombreCompleto, u.Usuario, u.Password, u.IdRol, 1, GETDATE()
    FROM (VALUES 
        (N'Administrador General', N'admin',    N'admin123',      @RolAdmin),
        (N'Cajero Principal',      N'cajero',   N'cajero123',     @RolCajero),
        (N'Carlos Mendoza',        N'cmendoza', N'cajero123',     @RolCajero),
        (N'Lucía Gómez',           N'lgomez',   N'cajero123',     @RolCajero),
        (N'Roberto Morales',       N'rmorales', N'supervisor123', @RolSupervisor),
        (N'Elena Vásquez',         N'evasquez', N'almacen123',    @RolBodeguero)
    ) AS u(NombreCompleto, Usuario, Password, IdRol)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Usuarios x WHERE x.Usuario = u.Usuario);
    SET @Filas = @@ROWCOUNT;

    INSERT INTO dbo.BitacoraTrazabilidad (CorrelationId, Capa, Operacion, FilasAfectadas, Estado, MensajeDetalle)
    VALUES (@CorrelationId, @CapaActual, N'Sincronización de Usuarios', @Filas, N'EXITO', N'Usuarios y asignación de roles procesados');

    -- 2.2 Clientes
    INSERT INTO dbo.Clientes (Documento, Nombre, Telefono, Email, Direccion, Activo)
    SELECT c.Documento, c.Nombre, c.Telefono, c.Email, c.Direccion, 1
    FROM (VALUES 
        (N'00000000',   N'Cliente General / Mostrador', N'0000-0000', N'general@pos.local',          N'Mostrador Central'),
        (N'04829103-5', N'María Fernanda Castro',       N'7890-1234', N'mfernanda.castro@gmail.com', N'Col. Escalón, Pasaje Las Flores #12'),
        (N'03918274-1', N'Juan Carlos Rivas',          N'7123-4567', N'jcrivas.sv@outlook.com',     N'Res. San Rafael, Senda 4 #21'),
        (N'05728193-2', N'Sofía Elizabeth Morales',     N'7456-7890', N'sofia.morales@hotmail.com',  N'Urb. Prados de Venecia, Soyapango #45'),
        (N'06192837-9', N'David Antonio Flores',        N'7901-2345', N'dflores.antiguo@gmail.com',  N'Calle El Pedregal, Antiguo Cuscatlán #8'),
        (N'02837465-8', N'Andrea Michelle Peña',        N'7654-3210', N'andrea.pena99@yahoo.com',    N'Barrio San Francisco, San Miguel #104'),
        (N'01928374-6', N'Roberto Carlos Benítez',      N'7321-6549', N'rbenitez@empresa.com',       N'Boulevard de Los Héroes #55'),
        (N'04738291-7', N'Claudia Patricia Ramos',      N'7543-2198', N'claudia.ramos@live.com',     N'Col. Médica, Av. Dr. Emilio Álvarez #12')
    ) AS c(Documento, Nombre, Telefono, Email, Direccion)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Clientes x WHERE x.Documento = c.Documento);
    SET @Filas = @@ROWCOUNT;

    INSERT INTO dbo.BitacoraTrazabilidad (CorrelationId, Capa, Operacion, FilasAfectadas, Estado, MensajeDetalle)
    VALUES (@CorrelationId, @CapaActual, N'Sincronización de Clientes', @Filas, N'EXITO', N'Clientes procesados');


    /* -----------------------------------------------------------------
       CAPA 3: CATÁLOGO OPERATIVO / PRODUCTOS (Dependiente de Categorías)
       ----------------------------------------------------------------- */
    SET @CapaActual = N'Capa 3: Catálogo Operativo';

    DECLARE @CatBebidas INT   = (SELECT TOP 1 IdCategoria FROM dbo.Categorias WHERE Nombre = N'Bebidas');
    DECLARE @CatSnacks INT    = (SELECT TOP 1 IdCategoria FROM dbo.Categorias WHERE Nombre = N'Snacks');
    DECLARE @CatLacteos INT   = (SELECT TOP 1 IdCategoria FROM dbo.Categorias WHERE Nombre = N'Lácteos y Quesos');
    DECLARE @CatAbarrotes INT = (SELECT TOP 1 IdCategoria FROM dbo.Categorias WHERE Nombre = N'Abarrotes y Despensa');
    DECLARE @CatLimpieza INT  = (SELECT TOP 1 IdCategoria FROM dbo.Categorias WHERE Nombre = N'Limpieza del Hogar');
    DECLARE @CatCuidado INT   = (SELECT TOP 1 IdCategoria FROM dbo.Categorias WHERE Nombre = N'Cuidado Personal');
    DECLARE @CatPanaderia INT = (SELECT TOP 1 IdCategoria FROM dbo.Categorias WHERE Nombre = N'Panadería y Dulces');

    INSERT INTO dbo.Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo, Activo)
    SELECT p.Codigo, p.Nombre, p.Descripcion, p.IdCategoria, p.PrecioCompra, p.PrecioVenta, p.Stock, p.StockMinimo, 1
    FROM (VALUES 
        -- Bebidas
        (N'PROD010', N'Jugo de Naranja 1L',             N'Jugo natural pasteurizado',            @CatBebidas,   1.25, 1.85, 45, 10),
        (N'PROD011', N'Té Helado Limón 500ml',          N'Bebida de té negro sabor limón',       @CatBebidas,   0.70, 1.15, 60, 15),
        (N'PROD012', N'Bebida Energizante 355ml',       N'Bebida carbonatada con cafeína',       @CatBebidas,   1.10, 1.75, 35,  8),
        (N'PROD013', N'Café Molido Gourmet 400g',       N'Café arábica tostado y molido',        @CatBebidas,   3.50, 5.25, 25,  5),
        -- Snacks
        (N'PROD014', N'Galletas de Chocolate 150g',     N'Galletas crujientes con crema',        @CatSnacks,    0.65, 1.10, 80, 20),
        (N'PROD015', N'Platanitos Salados 85g',         N'Tiras de plátano verde frito con sal', @CatSnacks,    0.50, 0.90, 55, 15),
        (N'PROD016', N'Maní Japonés Tostado 120g',      N'Cacahuates con cubierta crujiente',    @CatSnacks,    0.75, 1.25, 40, 10),
        (N'PROD017', N'Chocolate con Almendras 50g',    N'Barra chocolate con almendras',        @CatSnacks,    0.85, 1.50, 50, 12),
        -- Lácteos
        (N'PROD018', N'Leche Entera 1L Tetrapack',      N'Leche ultrapasteurizada fortificada',  @CatLacteos,   1.15, 1.65, 60, 15),
        (N'PROD019', N'Queso Fresco Semiduro 400g',     N'Queso artesanal tradicional de mesa',  @CatLacteos,   2.40, 3.60, 30,  6),
        (N'PROD020', N'Yogurt Bebible Fresa 1L',        N'Yogurt descremado probiótico',         @CatLacteos,   1.50, 2.25, 35,  8),
        (N'PROD021', N'Mantequilla con Sal 200g',       N'Mantequilla pura pasteurizada',        @CatLacteos,   1.10, 1.75, 25,  5),
        -- Abarrotes
        (N'PROD022', N'Arroz Blanco Grano Largo 1kg',   N'Arroz pulido seleccionado',            @CatAbarrotes, 0.85, 1.30, 90, 20),
        (N'PROD023', N'Frijol Rojo Seda 1kg',           N'Frijoles rojos nacionales limpios',    @CatAbarrotes, 1.20, 1.80, 70, 15),
        (N'PROD024', N'Aceite Vegetal 900ml',           N'Aceite puro comestible de soya',       @CatAbarrotes, 1.75, 2.50, 50, 10),
        (N'PROD025', N'Azúcar Blanca 1kg',              N'Azúcar refinada estándar de caña',     @CatAbarrotes, 0.75, 1.10, 85, 20),
        (N'PROD026', N'Pasta Spaghetti 200g',           N'Pasta de sémola de trigo duro',        @CatAbarrotes, 0.40, 0.70,100, 25),
        (N'PROD027', N'Atún en Agua Trozos 140g',       N'Lomo de atún claro en conserva',       @CatAbarrotes, 1.00, 1.60, 65, 15),
        (N'PROD028', N'Harina de Trigo 1kg',            N'Harina multiusos enriquecida',         @CatAbarrotes, 0.90, 1.40, 40, 10),
        -- Limpieza
        (N'PROD029', N'Detergente en Polvo 1kg',        N'Detergente biodegradable aroma floral',@CatLimpieza,  1.60, 2.45, 40, 10),
        (N'PROD030', N'Desinfectante Líquido 1L',       N'Limpiador multiusos lavanda pisos',    @CatLimpieza,  0.95, 1.55, 50, 12),
        (N'PROD031', N'Cloro Concentrado 1L',           N'Solución desinfectante hipoclorito 5%',@CatLimpieza,  0.65, 1.10, 60, 15),
        (N'PROD032', N'Jabón Líquido Trastes 750ml',    N'Detergente lavavajillas aroma limón',  @CatLimpieza,  1.20, 1.90, 45, 10),
        (N'PROD033', N'Papel Higiénico 4 Rollos',       N'Papel higiénico suave doble hoja',     @CatLimpieza,  1.40, 2.15, 55, 15),
        -- Cuidado Personal
        (N'PROD034', N'Shampoo Control Caspa 400ml',    N'Shampoo dermatológico con zinc',       @CatCuidado,   2.80, 4.25, 30,  8),
        (N'PROD035', N'Pasta Dental Triple Acción 100m',N'Dentífrico con flúor anticaries',      @CatCuidado,   1.10, 1.75, 50, 12),
        (N'PROD036', N'Jabón de Tocador 150g',          N'Jabón antibacterial en barra',         @CatCuidado,   0.55, 0.95, 70, 20),
        -- Panadería
        (N'PROD037', N'Pan de Caja Blanco 680g',        N'Pan blanco suave rebanado',            @CatPanaderia, 1.60, 2.40, 35, 10),
        (N'PROD038', N'Pan Dulce Tradicional (Paq x6)', N'Surtido variado de pan dulce',         @CatPanaderia, 1.00, 1.60, 25,  8)
    ) AS p(Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Productos x WHERE x.Codigo = p.Codigo);
    SET @Filas = @@ROWCOUNT;

    INSERT INTO dbo.BitacoraTrazabilidad (CorrelationId, Capa, Operacion, FilasAfectadas, Estado, MensajeDetalle)
    VALUES (@CorrelationId, @CapaActual, N'Sincronización de Catálogo de Productos', @Filas, N'EXITO', N'Productos clasificados con stock y precios');


    /* -----------------------------------------------------------------
       CAPA 4: CONTROL OPERATIVO / TURNOS DE CAJA
       ----------------------------------------------------------------- */
    SET @CapaActual = N'Capa 4: Control Operativo (Cajas)';

    DECLARE @IdUsuarioCajero INT = (SELECT TOP 1 IdUsuario FROM dbo.Usuarios WHERE Usuario = N'cajero');
    IF @IdUsuarioCajero IS NULL SET @IdUsuarioCajero = @RolAdmin;

    DECLARE @IdCajaTurno7 INT;
    IF NOT EXISTS (SELECT 1 FROM dbo.Cajas WHERE Observaciones = N'Turno matutino comercial regular')
    BEGIN
        INSERT INTO dbo.Cajas (IdUsuario, FechaApertura, MontoInicial, FechaCierre, MontoVentas, MontoCierre, Diferencia, Observaciones, Estado)
        VALUES (@IdUsuarioCajero, DATEADD(hour, -12, GETDATE()), 300.00, DATEADD(hour, -4, GETDATE()), 185.50, 485.50, 0.00, N'Turno matutino comercial regular', N'CERRADA');
        SET @IdCajaTurno7 = SCOPE_IDENTITY();
        SET @Filas = 1;
    END
    ELSE
    BEGIN
        SET @IdCajaTurno7 = (SELECT TOP 1 IdCaja FROM dbo.Cajas WHERE Observaciones = N'Turno matutino comercial regular');
        SET @Filas = 0;
    END;

    DECLARE @IdCajaActual INT;
    IF NOT EXISTS (SELECT 1 FROM dbo.Cajas WHERE Estado = N'ABIERTA')
    BEGIN
        INSERT INTO dbo.Cajas (IdUsuario, FechaApertura, MontoInicial, FechaCierre, MontoVentas, MontoCierre, Diferencia, Observaciones, Estado)
        VALUES (@IdUsuarioCajero, DATEADD(hour, -2, GETDATE()), 300.00, NULL, 64.80, NULL, NULL, N'Turno vespertino en operación', N'ABIERTA');
        SET @IdCajaActual = SCOPE_IDENTITY();
        SET @Filas = @Filas + 1;
    END
    ELSE
    BEGIN
        SET @IdCajaActual = (SELECT TOP 1 IdCaja FROM dbo.Cajas WHERE Estado = N'ABIERTA');
    END;

    INSERT INTO dbo.BitacoraTrazabilidad (CorrelationId, Capa, Operacion, FilasAfectadas, Estado, MensajeDetalle)
    VALUES (@CorrelationId, @CapaActual, N'Apertura/Cierre de Turnos de Caja', @Filas, N'EXITO', N'Turnos de caja sincronizados');


    /* -----------------------------------------------------------------
       CAPA 5: TRANSACCIONES CABECERA (Ventas)
       ----------------------------------------------------------------- */
    SET @CapaActual = N'Capa 5: Transacciones de Venta';

    DECLARE @CliMaria INT   = (SELECT TOP 1 IdCliente FROM dbo.Clientes WHERE Documento = N'04829103-5');
    DECLARE @CliJuan INT    = (SELECT TOP 1 IdCliente FROM dbo.Clientes WHERE Documento = N'03918274-1');
    DECLARE @CliSofia INT   = (SELECT TOP 1 IdCliente FROM dbo.Clientes WHERE Documento = N'05728193-2');
    DECLARE @CliDavid INT   = (SELECT TOP 1 IdCliente FROM dbo.Clientes WHERE Documento = N'06192837-9');
    DECLARE @CliGeneral INT = (SELECT TOP 1 IdCliente FROM dbo.Clientes WHERE Documento = N'00000000');

    DECLARE @IdVentaA INT, @IdVentaB INT, @IdVentaC INT, @IdVentaD INT, @IdVentaE INT;
    SET @Filas = 0;

    -- Venta A
    IF NOT EXISTS (SELECT 1 FROM dbo.Ventas WHERE IdCliente = @CliMaria AND Subtotal = 9.80)
    BEGIN
        INSERT INTO dbo.Ventas (Fecha, IdCliente, IdUsuario, Subtotal, Impuesto, Total, Estado, IdCaja)
        VALUES (DATEADD(hour, -10, GETDATE()), @CliMaria, @IdUsuarioCajero, 9.80, 1.27, 11.07, N'COMPLETADA', @IdCajaTurno7);
        SET @IdVentaA = SCOPE_IDENTITY();
        SET @Filas = @Filas + 1;
    END;

    -- Venta B
    IF NOT EXISTS (SELECT 1 FROM dbo.Ventas WHERE IdCliente = @CliJuan AND Subtotal = 11.60)
    BEGIN
        INSERT INTO dbo.Ventas (Fecha, IdCliente, IdUsuario, Subtotal, Impuesto, Total, Estado, IdCaja)
        VALUES (DATEADD(hour, -8, GETDATE()), @CliJuan, @IdUsuarioCajero, 11.60, 1.51, 13.11, N'COMPLETADA', @IdCajaTurno7);
        SET @IdVentaB = SCOPE_IDENTITY();
        SET @Filas = @Filas + 1;
    END;

    -- Venta C
    IF NOT EXISTS (SELECT 1 FROM dbo.Ventas WHERE IdCliente = @CliSofia AND Subtotal = 10.25)
    BEGIN
        INSERT INTO dbo.Ventas (Fecha, IdCliente, IdUsuario, Subtotal, Impuesto, Total, Estado, IdCaja)
        VALUES (DATEADD(hour, -6, GETDATE()), @CliSofia, @IdUsuarioCajero, 10.25, 1.33, 11.58, N'COMPLETADA', @IdCajaTurno7);
        SET @IdVentaC = SCOPE_IDENTITY();
        SET @Filas = @Filas + 1;
    END;

    -- Venta D
    IF NOT EXISTS (SELECT 1 FROM dbo.Ventas WHERE IdCaja = @IdCajaActual AND Subtotal = 14.25)
    BEGIN
        INSERT INTO dbo.Ventas (Fecha, IdCliente, IdUsuario, Subtotal, Impuesto, Total, Estado, IdCaja)
        VALUES (DATEADD(minute, -45, GETDATE()), @CliGeneral, @IdUsuarioCajero, 14.25, 1.85, 16.10, N'COMPLETADA', @IdCajaActual);
        SET @IdVentaD = SCOPE_IDENTITY();
        SET @Filas = @Filas + 1;
    END;

    -- Venta E
    IF NOT EXISTS (SELECT 1 FROM dbo.Ventas WHERE IdCliente = @CliDavid AND Subtotal = 8.85)
    BEGIN
        INSERT INTO dbo.Ventas (Fecha, IdCliente, IdUsuario, Subtotal, Impuesto, Total, Estado, IdCaja)
        VALUES (DATEADD(minute, -15, GETDATE()), @CliDavid, @IdUsuarioCajero, 8.85, 1.15, 10.00, N'COMPLETADA', @IdCajaActual);
        SET @IdVentaE = SCOPE_IDENTITY();
        SET @Filas = @Filas + 1;
    END;

    INSERT INTO dbo.BitacoraTrazabilidad (CorrelationId, Capa, Operacion, FilasAfectadas, Estado, MensajeDetalle)
    VALUES (@CorrelationId, @CapaActual, N'Emisión de Comprobantes de Venta', @Filas, N'EXITO', N'Cabeceras de venta generadas');


    /* -----------------------------------------------------------------
       CAPA 6: TRANSACCIONES HIJAS (Detalle de Ventas)
       ----------------------------------------------------------------- */
    SET @CapaActual = N'Capa 6: Detalle de Ventas';
    SET @Filas = 0;

    IF @IdVentaA IS NOT NULL
    BEGIN
        INSERT INTO dbo.DetalleVentas (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal) VALUES
        (@IdVentaA, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD018'), 2, 1.65, 3.30),
        (@IdVentaA, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD037'), 1, 2.40, 2.40),
        (@IdVentaA, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD014'), 1, 1.10, 1.10),
        (@IdVentaA, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD010'), 1, 1.85, 1.85),
        (@IdVentaA, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD011'), 1, 1.15, 1.15);
        SET @Filas = @Filas + 5;
    END;

    IF @IdVentaB IS NOT NULL
    BEGIN
        INSERT INTO dbo.DetalleVentas (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal) VALUES
        (@IdVentaB, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD022'), 2, 1.30, 2.60),
        (@IdVentaB, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD023'), 2, 1.80, 3.60),
        (@IdVentaB, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD024'), 1, 2.50, 2.50),
        (@IdVentaB, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD025'), 1, 1.10, 1.10),
        (@IdVentaB, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD026'), 2, 0.70, 1.40);
        SET @Filas = @Filas + 5;
    END;

    IF @IdVentaC IS NOT NULL
    BEGIN
        INSERT INTO dbo.DetalleVentas (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal) VALUES
        (@IdVentaC, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD029'), 1, 2.45, 2.45),
        (@IdVentaC, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD030'), 1, 1.55, 1.55),
        (@IdVentaC, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD031'), 2, 1.10, 2.20),
        (@IdVentaC, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD032'), 1, 1.90, 1.90),
        (@IdVentaC, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD033'), 1, 2.15, 2.15);
        SET @Filas = @Filas + 5;
    END;

    IF @IdVentaD IS NOT NULL
    BEGIN
        INSERT INTO dbo.DetalleVentas (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal) VALUES
        (@IdVentaD, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD013'), 1, 5.25, 5.25),
        (@IdVentaD, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD019'), 1, 3.60, 3.60),
        (@IdVentaD, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD038'), 1, 1.60, 1.60),
        (@IdVentaD, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD020'), 1, 2.25, 2.25),
        (@IdVentaD, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD012'), 1, 1.55, 1.55);
        SET @Filas = @Filas + 5;
    END;

    IF @IdVentaE IS NOT NULL
    BEGIN
        INSERT INTO dbo.DetalleVentas (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal) VALUES
        (@IdVentaE, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD034'), 1, 4.25, 4.25),
        (@IdVentaE, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD035'), 1, 1.75, 1.75),
        (@IdVentaE, (SELECT TOP 1 IdProducto FROM dbo.Productos WHERE Codigo = N'PROD036'), 3, 0.95, 2.85);
        SET @Filas = @Filas + 3;
    END;

    INSERT INTO dbo.BitacoraTrazabilidad (CorrelationId, Capa, Operacion, FilasAfectadas, Estado, MensajeDetalle)
    VALUES (@CorrelationId, @CapaActual, N'Inserción de Detalle de Líneas de Venta', @Filas, N'EXITO', N'Líneas de detalle vinculadas');

    -- Cierre exitoso del proceso
    INSERT INTO dbo.BitacoraTrazabilidad (CorrelationId, Capa, Operacion, FilasAfectadas, Estado, MensajeDetalle)
    VALUES (@CorrelationId, N'Capa 0: Finalización', N'Fin del proceso de carga', 0, N'EXITO', N'Transacción confirmada');

    COMMIT TRANSACTION;

END TRY
BEGIN CATCH

    DECLARE @ErrNum INT            = ERROR_NUMBER();
    DECLARE @ErrSev INT            = ERROR_SEVERITY();
    DECLARE @ErrState INT          = ERROR_STATE();
    DECLARE @ErrLine INT           = ERROR_LINE();
    DECLARE @ErrProc NVARCHAR(128) = ISNULL(ERROR_PROCEDURE(), N'-');
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();

    -- Revertir transacción activa de forma segura
    IF XACT_STATE() <> 0 OR @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    -- Registrar trazabilidad de error persistente fuera de la transacción revertida
    BEGIN TRY
        INSERT INTO dbo.BitacoraTrazabilidad (
            CorrelationId, Capa, Operacion, FilasAfectadas, Estado, MensajeDetalle
        )
        VALUES (
            @CorrelationId,
            @CapaActual,
            N'Falla durante ejecución',
            0,
            N'ERROR',
            CONCAT(N'Error ', @ErrNum, N' [Línea ', @ErrLine, N', Proc: ', @ErrProc, N']: ', @ErrMsg)
        );
    END TRY
    BEGIN CATCH
        -- Si falla la inserción del log, no ocultar el error principal
    END CATCH;

    -- Relanzar excepción original para preservar stack trace
    THROW @ErrNum, @ErrMsg, @ErrState;

END CATCH;
GO

/* =====================================================================
   CONSULTA DE VERIFICACIÓN Y TELEMETRÍA DE EJECUCIÓN
   ===================================================================== */
SELECT TOP 10 
    IdLog,
    CorrelationId,
    Capa,
    Operacion,
    FilasAfectadas,
    Estado,
    MensajeDetalle,
    UsuarioEjecutor,
    HostEjecutor,
    CONVERT(VARCHAR(30), FechaRegistro, 120) AS Fecha
FROM dbo.BitacoraTrazabilidad
ORDER BY IdLog DESC;
GO
