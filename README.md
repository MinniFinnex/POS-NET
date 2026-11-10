# Sistema de Punto de Venta (POS)

Sistema de punto de venta de escritorio desarrollado en **C#**, **Windows Forms (.NET 9)** y **SQL Server**. Diseñado con arquitectura por capas (N-Tier), control transaccional de inventario, seguridad mediante contraseñas con hash SHA-256 y control de acceso basado en roles (RBAC).

## Roles del Sistema y Credenciales

| Rol | Usuario | Contraseña | Alcance y Permisos |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin` | `admin123` | Acceso total: gestión de catálogo y precios, clientes, ventas, control de caja y reportes. |
| **Cajero** | `cajero` | `cajero123` | Acceso operativo: apertura/cierre de caja, ventas, catálogo en modo consulta. |

### Matriz de Permisos por Módulo

| Módulo / Acción | Administrador | Cajero |
| :--- | :---: | :---: |
| Iniciar Sesión | Sí | Sí |
| Apertura y Cierre de Caja | Sí | Sí |
| Registrar y Cobrar Venta | Sí | Sí |
| Consultar Productos y Precios | Sí | Sí |
| Agregar Nuevos Productos | Sí | No (Solo lectura) |
| Modificar Precios y Stock | Sí | No (Solo lectura) |
| Eliminar Productos | Sí | No (Solo lectura) |
| Administrar Clientes | Sí | Sí |
| Consultar Reportes | Sí | Sí |

---

## Arquitectura de la Solución

```text
PuntoDeVenta/
├── PuntoDeVenta.sln                  # Solución de Visual Studio
├── README.md                         # Documentación del proyecto
├── MANUAL_DE_USO.md                  # Manual de operación
├── Database/
│   └── PuntoDeVentaDB.sql            # Script unificado de base de datos
└── PuntoDeVenta/
    ├── Presentacion/                 # Formularios de interfaz de usuario
    │   ├── frmLogin.cs               # Autenticación de usuarios
    │   ├── frmPrincipal.cs           # Ventana contenedora y menú de navegación
    │   ├── frmVentas.cs              # Módulo de ventas, carrito y cobro
    │   ├── frmProductos.cs           # Catálogo y mantenimiento de productos
    │   ├── frmClientes.cs            # Directorio y mantenimiento de clientes
    │   ├── frmCaja.cs                # Turnos, apertura, arqueo y cierre de caja
    │   └── frmReportes.cs            # Historial de ventas y métricas clave
    ├── Negocio/                      # Servicios y reglas de negocio (BLL)
    ├── Datos/                        # Repositorios y conexión ADO.NET (DAL)
    ├── Entidades/                    # Modelos de datos y DTOs
    ├── Utilidades/                   # Configuración de tema visual (TemaUI) y seguridad
    ├── App.config                    # Cadena de conexión activa
    └── Program.cs                    # Punto de entrada
```

---

## Configuración y Ejecución

1. **Iniciar la base de datos (SQL Server LocalDB):**
   ```powershell
   sqllocaldb start MSSQLLocalDB
   ```
2. **Ejecutar el script de base de datos:**
   Ejecutar `Database/PuntoDeVentaDB.sql` en SQL Server Management Studio o mediante `sqlcmd`.
3. **Compilar y ejecutar la aplicación:**
   ```powershell
   dotnet build
   dotnet run --project PuntoDeVenta/PuntoDeVenta.csproj
   ```
