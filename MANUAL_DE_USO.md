# Manual de Uso - Sistema Punto de Venta (POS)

## 1. Perfiles y Roles de Usuario

| Rol | Usuario | Contraseña | Alcance Operativo |
| **Administrador** | `admin` | `admin123` | **Acceso total:** Crear, editar y eliminar productos; administrar precios, descripción y stock; abrir y cerrar caja; registrar clientes; consultar reportes y métricas KPI. |
| **Cajero** | `cajero` | `cajero123` | **Acceso operativo:** Apertura y cierre de turno de caja, cobro de productos, emisión de tickets y consulta de catálogo/precios en modo lectura. |

### Matriz de Privilegios

| Función | Administrador | Cajero |
| :--- | :---: | :---: |
| Iniciar Sesión en el Sistema | Sí | Sí |
| Apertura y Cierre de Turno de Caja | Sí | Sí |
| Cobro y Facturación de Ventas | Sí | Sí |
| Consulta y Reimpresión de Tickets | Sí | Sí |
| Consultar Catálogo y Precios | Sí | Sí |
| Crear o Modificar Productos | Sí | No (Solo lectura) |
| Modificar Precios o Stock Directo | Sí | No (Solo lectura) |
| Eliminar Productos | Sí | No (Solo lectura) |
| Directorio de Clientes | Sí | Sí |
| Consultar Reportes de Ventas y KPIs | Sí | Sí |

---

## 2. Base de Datos (SQL Server)

El script unificado se encuentra en `Database/PuntoDeVentaDB.sql` y crea la base de datos `PuntoDeVentaDB` con sus tablas, relaciones y datos iniciales.

Comando para iniciar LocalDB en powershell:
```
sqllocaldb start MSSQLLocalDB
```

