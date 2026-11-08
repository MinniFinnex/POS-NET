# 🛒 Sistema POS (Punto de Venta) en C# + WinForms + SQL Server

Sistema de punto de venta de escritorio desarrollado con **C#**, **Windows Forms (.NET)** y **SQL Server**. Permite gestionar productos, clientes, usuarios y ventas, con control de inventario y reportes básicos.

> Este README sirve como guía del proyecto y como tutorial paso a paso para construirlo y publicarlo en GitHub.

---

## 📑 Tabla de contenido

1. [Características](#-características)
2. [Tecnologías](#-tecnologías)
3. [Requisitos previos](#-requisitos-previos)
4. [Arquitectura y estructura del proyecto](#-arquitectura-y-estructura-del-proyecto)
5. [Base de datos](#-base-de-datos)
6. [Instalación y configuración](#-instalación-y-configuración)
7. [Ejemplo de código](#-ejemplo-de-código)
8. [Crear el repositorio en GitHub](#-crear-el-repositorio-en-github)
9. [Flujo de trabajo con Git](#-flujo-de-trabajo-con-git)
10. [Roadmap](#-roadmap)
11. [Contribuir](#-contribuir)
12. [Licencia](#-licencia)

---

## ✨ Características

- 🔐 Inicio de sesión con roles (Administrador / Cajero)
- 📦 CRUD de productos y categorías
- 👥 CRUD de clientes
- 🧾 Registro de ventas con detalle (carrito de compra)
- 📉 Descuento automático de inventario al vender
- 💵 Cálculo de subtotal, impuestos y total
- 🖨️ Impresión de ticket / factura
- 📊 Reportes de ventas por fecha, producto y usuario
- ⚠️ Alertas de stock mínimo

---

## 🧰 Tecnologías

| Componente        | Tecnología                                   |
|-------------------|----------------------------------------------|
| Lenguaje          | C#                                           |
| Interfaz          | Windows Forms (WinForms)                     |
| Framework         | .NET 8 (o .NET Framework 4.8)                |
| Base de datos     | SQL Server (Express / Developer / LocalDB)   |
| Acceso a datos    | ADO.NET (`Microsoft.Data.SqlClient`)         |
| IDE               | Visual Studio 2022                           |
| Control de versión| Git + GitHub                                 |

---

## ✅ Requisitos previos

- [Visual Studio 2022](https://visualstudio.microsoft.com/) con la carga de trabajo **Desarrollo de escritorio de .NET**
- [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) o Developer Edition
- [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)
- [Git](https://git-scm.com/downloads)
- Una cuenta de [GitHub](https://github.com/)
- (Opcional) [GitHub CLI](https://cli.github.com/)

---

## 🏗️ Arquitectura y estructura del proyecto

Se recomienda una arquitectura por **capas** para mantener el código ordenado y mantenible:

```
PuntoDeVenta/
│
├── PuntoDeVenta.sln
├── README.md
├── .gitignore
├── LICENSE
│
├── Database/
│   ├── 01_CreateDatabase.sql
│   ├── 02_CreateTables.sql
│   └── 03_SeedData.sql
│
└── PuntoDeVenta/
    ├── Presentacion/        # Formularios WinForms (UI)
    │   ├── frmLogin.cs
    │   ├── frmPrincipal.cs
    │   ├── frmProductos.cs
    │   ├── frmClientes.cs
    │   └── frmVentas.cs
    │
    ├── Negocio/             # Lógica de negocio y validaciones
    │   ├── ProductoService.cs
    │   ├── VentaService.cs
    │   └── UsuarioService.cs
    │
    ├── Datos/               # Acceso a datos (SQL Server)
    │   ├── Conexion.cs
    │   ├── ProductoRepository.cs
    │   ├── VentaRepository.cs
    │   └── UsuarioRepository.cs
    │
    ├── Entidades/           # Modelos / DTOs
    │   ├── Producto.cs
    │   ├── Cliente.cs
    │   ├── Venta.cs
    │   ├── DetalleVenta.cs
    │   └── Usuario.cs
    │
    ├── Utilidades/          # Helpers (hash de contraseñas, formatos, etc.)
    ├── App.config
    └── Program.cs
```

**Flujo:** `Presentación → Negocio → Datos → SQL Server`

---

## 🗄️ Base de datos

### Crear la base de datos

```sql
CREATE DATABASE PuntoDeVentaDB;
GO

USE PuntoDeVentaDB;
GO
```

### Crear las tablas

```sql
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
```

### Datos iniciales

```sql
INSERT INTO Roles (Nombre) VALUES ('Administrador'), ('Cajero');

-- ⚠️ Reemplaza el hash por uno generado desde la aplicación (nunca guardes contraseñas en texto plano)
INSERT INTO Usuarios (NombreCompleto, Usuario, PasswordHash, IdRol)
VALUES ('Administrador', 'admin', 'HASH_GENERADO_AQUI', 1);

INSERT INTO Categorias (Nombre) VALUES ('General'), ('Bebidas'), ('Snacks');
```

### Procedimiento almacenado (ejemplo: registrar venta con transacción)

Para garantizar integridad, la venta, su detalle y el descuento de stock deben ejecutarse en **una sola transacción**. Esto se puede hacer desde C# con `SqlTransaction` o desde un procedimiento almacenado.

---

## ⚙️ Instalación y configuración

### 1. Clonar el repositorio

```bash
git clone https://github.com/TU_USUARIO/PuntoDeVenta.git
cd PuntoDeVenta
```

### 2. Crear la base de datos

1. Abre **SSMS** y conéctate a tu instancia de SQL Server.
2. Ejecuta, en orden, los scripts de la carpeta `Database/`.

### 3. Configurar la cadena de conexión

Edita `App.config`:

```xml
<configuration>
  <connectionStrings>
    <add name="PuntoDeVentaDB"
         connectionString="Server=localhost\SQLEXPRESS;Database=PuntoDeVentaDB;Trusted_Connection=True;TrustServerCertificate=True;"
         providerName="Microsoft.Data.SqlClient" />
  </connectionStrings>
</configuration>
```

> 💡 Si usas autenticación SQL: `Server=localhost;Database=PuntoDeVentaDB;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;`
>
> 🔒 **No subas contraseñas reales a GitHub.** Usa un `App.config.example` en el repo y agrega `App.config` al `.gitignore`.

### 4. Instalar dependencias

En Visual Studio: **Herramientas → Administrador de paquetes NuGet → Consola**:

```powershell
Install-Package Microsoft.Data.SqlClient
```

### 5. Ejecutar

Abre `PuntoDeVenta.sln`, establece el proyecto como inicio y presiona **F5**.

---

## 💻 Ejemplo de código

### Clase de conexión (`Datos/Conexion.cs`)

```csharp
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace PuntoDeVenta.Datos
{
    public static class Conexion
    {
        private static readonly string _cadena =
            ConfigurationManager.ConnectionStrings["PuntoDeVentaDB"].ConnectionString;

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_cadena);
        }
    }
}
```

### Entidad (`Entidades/Producto.cs`)

```csharp
namespace PuntoDeVenta.Entidades
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int IdCategoria { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
    }
}
```

### Repositorio (`Datos/ProductoRepository.cs`)

```csharp
using Microsoft.Data.SqlClient;
using PuntoDeVenta.Entidades;

namespace PuntoDeVenta.Datos
{
    public class ProductoRepository
    {
        public List<Producto> ObtenerTodos()
        {
            var lista = new List<Producto>();

            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                "SELECT IdProducto, Codigo, Nombre, IdCategoria, PrecioVenta, Stock " +
                "FROM Productos WHERE Activo = 1", cn);

            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Producto
                {
                    IdProducto  = dr.GetInt32(0),
                    Codigo      = dr.GetString(1),
                    Nombre      = dr.GetString(2),
                    IdCategoria = dr.GetInt32(3),
                    PrecioVenta = dr.GetDecimal(4),
                    Stock       = dr.GetInt32(5)
                });
            }
            return lista;
        }

        public int Insertar(Producto p)
        {
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"INSERT INTO Productos (Codigo, Nombre, IdCategoria, PrecioCompra, PrecioVenta, Stock)
                  VALUES (@Codigo, @Nombre, @IdCategoria, 0, @PrecioVenta, @Stock);
                  SELECT SCOPE_IDENTITY();", cn);

            // ✅ Siempre usa parámetros para evitar SQL Injection
            cmd.Parameters.AddWithValue("@Codigo", p.Codigo);
            cmd.Parameters.AddWithValue("@Nombre", p.Nombre);
            cmd.Parameters.AddWithValue("@IdCategoria", p.IdCategoria);
            cmd.Parameters.AddWithValue("@PrecioVenta", p.PrecioVenta);
            cmd.Parameters.AddWithValue("@Stock", p.Stock);

            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
```

### Registrar una venta con transacción

```csharp
public int RegistrarVenta(Venta venta, List<DetalleVenta> detalles)
{
    using var cn = Conexion.ObtenerConexion();
    cn.Open();
    using var tx = cn.BeginTransaction();

    try
    {
        // 1. Insertar cabecera
        var cmdVenta = new SqlCommand(
            @"INSERT INTO Ventas (IdCliente, IdUsuario, Subtotal, Impuesto, Total)
              VALUES (@IdCliente, @IdUsuario, @Subtotal, @Impuesto, @Total);
              SELECT SCOPE_IDENTITY();", cn, tx);
        cmdVenta.Parameters.AddWithValue("@IdCliente", (object?)venta.IdCliente ?? DBNull.Value);
        cmdVenta.Parameters.AddWithValue("@IdUsuario", venta.IdUsuario);
        cmdVenta.Parameters.AddWithValue("@Subtotal", venta.Subtotal);
        cmdVenta.Parameters.AddWithValue("@Impuesto", venta.Impuesto);
        cmdVenta.Parameters.AddWithValue("@Total", venta.Total);
        int idVenta = Convert.ToInt32(cmdVenta.ExecuteScalar());

        // 2. Insertar detalle y descontar stock
        foreach (var d in detalles)
        {
            var cmdDet = new SqlCommand(
                @"INSERT INTO DetalleVentas (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal)
                  VALUES (@IdVenta, @IdProducto, @Cantidad, @Precio, @Subtotal);

                  UPDATE Productos SET Stock = Stock - @Cantidad
                  WHERE IdProducto = @IdProducto AND Stock >= @Cantidad;", cn, tx);

            cmdDet.Parameters.AddWithValue("@IdVenta", idVenta);
            cmdDet.Parameters.AddWithValue("@IdProducto", d.IdProducto);
            cmdDet.Parameters.AddWithValue("@Cantidad", d.Cantidad);
            cmdDet.Parameters.AddWithValue("@Precio", d.PrecioUnitario);
            cmdDet.Parameters.AddWithValue("@Subtotal", d.Subtotal);
            cmdDet.ExecuteNonQuery();
        }

        tx.Commit();
        return idVenta;
    }
    catch
    {
        tx.Rollback();
        throw;
    }
}
```

---

## 🐙 Crear el repositorio en GitHub

### Paso 1: Crear el archivo `.gitignore`

En la raíz del proyecto, genera un `.gitignore` para Visual Studio / .NET:

```bash
dotnet new gitignore
```

Y agrega al final:

```gitignore
# Configuración local con credenciales
App.config
appsettings.Development.json
```

### Paso 2: Inicializar Git en local

```bash
cd PuntoDeVenta
git init
git add .
git commit -m "Commit inicial: estructura del proyecto POS"
git branch -M main
```

### Paso 3: Crear el repositorio en GitHub

**Opción A — Desde la web**

1. Entra a [github.com/new](https://github.com/new).
2. Escribe el nombre: `PuntoDeVenta`.
3. Agrega una descripción, por ejemplo: *"Sistema POS en C# WinForms y SQL Server"*.
4. Elige **Público** o **Privado**.
5. ⚠️ **No** marques "Add README / .gitignore / license" (ya los tienes en local).
6. Clic en **Create repository**.

**Opción B — Desde la terminal con GitHub CLI**

```bash
gh auth login
gh repo create PuntoDeVenta --public --source=. --remote=origin --push
```

### Paso 4: Vincular y subir (si usaste la Opción A)

```bash
git remote add origin https://github.com/TU_USUARIO/PuntoDeVenta.git
git push -u origin main
```

> Si Git pide autenticación, usa un **Personal Access Token** (Settings → Developer settings → Personal access tokens) en lugar de tu contraseña, o inicia sesión con **Git Credential Manager**.

### Opción C — Desde Visual Studio

1. Menú **Git → Crear repositorio Git**.
2. Selecciona **GitHub**, inicia sesión, define nombre, descripción y visibilidad.
3. Clic en **Crear y enviar**.

---

## 🔁 Flujo de trabajo con Git

Estrategia sencilla recomendada:

```bash
# Crear una rama para cada funcionalidad
git checkout -b feature/modulo-ventas

# Trabajar, guardar cambios
git add .
git commit -m "feat: agrega carrito de ventas"

# Subir la rama
git push -u origin feature/modulo-ventas
```

Luego abre un **Pull Request** en GitHub y haz *merge* a `main`.

**Convención de commits sugerida:**

| Prefijo     | Uso                                  |
|-------------|--------------------------------------|
| `feat:`     | Nueva funcionalidad                  |
| `fix:`      | Corrección de errores                |
| `docs:`     | Cambios en documentación             |
| `refactor:` | Mejora de código sin cambiar función |
| `style:`    | Formato, espacios, etc.              |

---

## 🗺️ Roadmap

- [x] Estructura del proyecto y base de datos
- [ ] Login y manejo de roles
- [ ] CRUD de productos y categorías
- [ ] CRUD de clientes
- [ ] Módulo de ventas (carrito + transacción)
- [ ] Impresión de tickets
- [ ] Reportes (ventas por día, producto, usuario)
- [ ] Control de caja (apertura / cierre)
- [ ] Respaldo automático de base de datos
- [ ] Lector de código de barras
- [ ] Pruebas unitarias para la capa de negocio

---

## 🔒 Buenas prácticas de seguridad

- Guarda las contraseñas con **hash + salt** (BCrypt o PBKDF2), nunca en texto plano.
- Usa **consultas parametrizadas** siempre (evita SQL Injection).
- No subas cadenas de conexión con credenciales reales al repositorio.
- Aplica el principio de **mínimo privilegio** al usuario de SQL Server que usa la app.

---

## 🤝 Contribuir

1. Haz un *fork* del repositorio.
2. Crea una rama: `git checkout -b feature/mi-mejora`.
3. Haz commit de tus cambios: `git commit -m "feat: descripción"`.
4. Sube la rama: `git push origin feature/mi-mejora`.
5. Abre un *Pull Request*.

---

## 📄 Licencia

Este proyecto está bajo la licencia **MIT**. Consulta el archivo [LICENSE](LICENSE) para más detalles.

---

## 👤 Autor

**Tu Nombre**
GitHub: [@TU_USUARIO](https://github.com/TU_USUARIO)

---

⭐ Si este proyecto te sirvió, ¡dale una estrella en GitHub!
