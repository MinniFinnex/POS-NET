# 📖 Manual de Uso - Sistema Punto de Venta (POS)

Bienvenido a la guía de usuario del **Sistema Punto de Venta (POS)** desarrollado en **C# (.NET 9 WinForms)** y **SQL Server**. Este documento detalla los perfiles de usuario, permisos, configuración de la base de datos y el flujo operativo de cada módulo.

---


## 1. perfiles y Roles de Usuario

| Rol | Usuario | Contraseña | Alcance Operativo |
| **Administrador** | `admin` | `admin123` | **Acceso total:** Crear, editar y eliminar productos; administrar precios, descripción y stock; abrir y cerrar caja; registrar clientes; consultar reportes y métricas KPI. |
| **Cajero** | `cajero` | `cajero123` | **Acceso operativo:** Apertura y cierre de su turno de caja, escaneo y cobro de productos, emisión de tickets y consulta de catálogo/precios en modo solo lectura. |

### Matriz de Privilegios

| Función | Administrador | Cajero |
| :--- | :---: | :---: |
| Iniciar Sesión en el Sistema | ✅ | ✅ |
| Apertura y Cierre de Turno de Caja | ✅ | ✅ |
| Cobro y Facturación de Ventas | ✅ | ✅ |
| Consulta y Reimpresión de Tickets | ✅ | ✅ |
| Consultar Catálogo y Precios | ✅ | ✅ |
| Crear o Modificar Productos y Descripción | ✅ | ❌ (Solo lectura)|
| Modificar Precios o Stock Directo | ✅ | ❌ (Solo lectura) |
| Eliminar Productos | ✅ | ❌ (Solo lectura) |
| Directorio de Clientes | ✅ | ✅ |
| Consultar Reportes de Ventas y KPIs | ✅ | ✅ |
| Consultar Top Productos Más Vendidos | ✅ | ✅ |

---

## 2. Base de Datos (SQL Server)

Todos los scripts de base de datos se encuentran unificados en un único archivo:
- **Ruta:** `Database/PuntoDeVentaDB.sql`
- **Contenido del script:**
  1. Creación de la base de datos `PuntoDeVentaDB`.
  2. Creación de las 8 tablas: `Roles`, `Usuarios`, `Categorias`, `Productos`, `Clientes`, `Cajas`, `Ventas` y `DetalleVentas`.
## 3.  Acceso e Inicio de Sesión

1. Inicia la aplicación.
2. Ingresa el **Usuario** (`admin` o `cajero`) y la **Contraseña** (`admin123` o `cajero123`).
3. Haz clic en **"Iniciar Sesión"**.
4. La aplicación valida las credenciales directamente contra la base de datos SQL Server. Si las credenciales no son válidas o el servicio SQL no está activo, el sistema informará el error correspondiente.


## 4.  Control de Caja (Apertura y Cierre)

El sistema requiere que exista una caja abierta antes de realizar cobros.

### A. Apertura de Turno
1. Ingresa a **" Control de Caja"** en el menú lateral.
2. Si la caja está cerrada, se mostrará el estado ** CAJA CERRADA**.
3. En el formulario de apertura:
   - Captura el **Fondo Inicial en Efectivo ($)** (por ejemplo: `$500.00`).
   - *(Opcional)* Escribe observaciones de apertura.
4. Haz clic en **" ABRIR CAJA"**.
5. El sistema registrará el nuevo turno en la base de datos y cambiará a estado **🟢 CAJA ABIERTA**.

### B. Cierre y Arqueo de Turno
Al finalizar la jornada:
1. Ingresa a **" Control de Caja"**.
2. El sistema calculará en tiempo real:
   - **Fondo Inicial:** Dinero base de apertura.
   - **Ventas Turno:** Total de cobros efectuados en dicho turno.
   - **Total Esperado:** `Fondo Inicial + Ventas del Turno`.
3. Escribe en **"Efectivo Físico Contado ($)"** el monto total de dinero contado en el cajón.
4. El sistema calculará la **Diferencia**:
   - `$0.00`: Cuadre exacto.
   - Valor positivo: Sobrante en caja.
   - Valor negativo Faltante en caja.
5. Haz clic en **" REALIZAR CIERRE"** y confirma la operación.
---

## 5. Registro y Cobro de Ventas

### A. Escaneo y Acumulación de Productos
1. Escribe o escanea el código en el campo **"Código de Barras"**.
2. Haz clic en el botón **"➕ Agregar al Carrito"** (o presiona Enter tras escanear el código).
3. **Acumulación automática:** Si el producto ya se encuentra en el carrito, se incrementa la cantidad en esa misma fila sin generar duplicados.
4. **Validación de Inventario:** La columna **"Stock Disp."** muestra las existencias físicas en bodega. El sistema no permite agregar ni acumular más unidades de las existentes en la base de datos.

### B. Modificación en el Carrito
Usa los botones debajo de la tabla para ajustar productos:
- **` Sumar (+1)`**: Incrementa en 1 la cantidad del producto seleccionado.
- **` Restar (-1)`**: Disminuye en 1 la cantidad.
- **`Quitar Ítem`**: Elimina el producto seleccionado del carrito.
- **` Vaciar Carrito`**: Limpia todo el carrito de compras.

### C. Cobro y Emisión de Ticket
1. **Cliente:** Selecciona un cliente registrado o mantén el valor *"Consumidor Final"*.
2. **Paga con y Cambio:**
   - Escribe en **"Paga con ($)"** el dinero entregado por el cliente.
   - El indicador **"Cambio"** mostrará el vuelto a regresar.
3. Haz clic en el botón verde **"COBRAR"**.
4. Se generará en pantalla el **Ticket de Venta** detallado y el stock de cada producto se descontará de manera inmediata en SQL Server.

---

## 6.  Catálogo e Inventario de Productos

Permite dar de alta, modificar y controlar existencias de artículos.

1. **Campos Disponibles:**
   - **Código de Barras:** Código único identificador.
   - **Nombre:** Nombre del producto.
   - **Descripción / Detalles:** Especificaciones, contenido o notas del producto.
   - **Categoría:** Categoría asignada.
   - **Precio ($):** Precio unitario de venta.
   - **Stock:** Cantidad física en existencia.
2. **Búsqueda:** Utiliza la barra **"🔍 Buscar en catálogo"** para filtrar en tiempo real por código, nombre o descripción.
3. **Guardar:** Completa los datos y haz clic en **"💾 Guardar"** (los datos se almacenan directamente en la tabla `Productos` de SQL Server).
4. **Eliminar:** Selecciona un producto y haz clic en **"🗑️ Eliminar"** para desactivarlo lógicamente.

---

## 7. Directorio de Clientes

Permite registrar clientes para asociarlos a los tickets de venta:
1. Haz clic en **"Nuevo"**.
2. Ingresa documento, nombre completo, teléfono, correo electrónico y dirección.
3. Haz clic en **" Guardar"**.

---

## 8.  Historial y Reportes de Ventas

Módulo para auditorías, consulta histórica y análisis comercial:

### A. Filtros por Fecha
- Botones de acceso rápido: **Hoy**, **Últimos 7 Días**, **Este Mes**.
- Filtro personalizado mediante selectores **"Desde"** y **"Hasta"** con el botón **"🔍 Consultar"**.

### B. Indicadores Clave (KPIs)
-  **TOTAL VENTAS:** Importe acumulado facturado en el periodo.
-  **Nº VENTAS:** Cantidad total de transacciones registradas.
-  **TICKET PROMEDIO:** Promedio monetario por compra.
-  **ARTÍCULOS VENDIDOS:** Total de unidades despachadas.

### C. Desglose y Reimpresión de Tickets
1. Al seleccionar un folio en la tabla de ventas, se mostrarán al lado todos los artículos y subtotales correspondientes.
2. El botón **" Ver Ticket"** genera una copia del comprobante de venta.

### D. Top Productos Más Vendidos
En la pestaña **" Top Productos Más Vendidos"** se muestra el ranking de los productos con mayor demanda y los ingresos generados por cada uno.

## Comando para encender la instancia Local SQL en VISUAL STUDIO
 sqllocaldb start MSSQLLocalDB

 ## SQl Server Management

## 9.  Preguntas (team)

### ¿Qué ocurre si intento cobrar y la caja está cerrada?
El sistema solicitará abrir la caja antes de continuar para vincular la venta al turno activo.

### ¿Dónde se almacenan los datos de las operaciones?
Todos los registros (productos, existencias, clientes, aperturas de caja, ventas y detalles) se guardan directamente en la base de datos `PuntoDeVentaDB` en SQL Server.
