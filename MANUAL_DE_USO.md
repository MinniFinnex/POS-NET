# Manual de Uso - Sistema Punto de Venta (POS)

Guía de usuario para el **Sistema Punto de Venta (POS)** desarrollado en **C# (.NET 9 WinForms)** y **SQL Server**.

---

## 1. Perfiles y Roles de Usuario

| Rol | Usuario | Contraseña | Alcance Operativo |
| :--- | :--- | :--- | :--- |
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

Comando para iniciar LocalDB en Windows:
```powershell
sqllocaldb start MSSQLLocalDB
```

---

## 3. Acceso e Inicio de Sesión

1. Inicia la aplicación.
2. Ingresa el **Usuario** (`admin` o `cajero`) y la **Contraseña** (`admin123` o `cajero123`).
3. Haz clic en **Ingresar** (o presiona Enter).
4. El sistema valida las credenciales y carga el menú principal con los permisos del rol.

---

## 4. Control de Caja (Apertura y Cierre)

El sistema requiere que exista una caja abierta antes de realizar cobros.

### A. Apertura de Turno
1. Ingresa a **Caja** en el menú de navegación.
2. Si la caja está cerrada, captura el **Fondo Inicial ($)** (por defecto `$500.00`).
3. Haz clic en **Abrir Caja**.

### B. Cierre y Arqueo de Turno
1. En el módulo **Caja**, consulta los valores calculados:
   - **Fondo Inicial:** Dinero base de apertura.
   - **Ventas:** Cobros acumulados en el turno.
   - **Esperado:** Suma de fondo inicial y ventas.
2. Ingresa en **Efectivo Contado ($)** el total contado en físico.
3. El sistema calcula la **Diferencia** (Exacto, Sobrante o Faltante).
4. Haz clic en **Cerrar Caja** y confirma.

---

## 5. Registro y Cobro de Ventas

### A. Búsqueda y Adición de Productos
1. Escribe o escanea el código en el campo **Código**.
2. Especifica la cantidad (por defecto 1) y haz clic en **Agregar** (o presiona Enter).
3. Si el producto ya está en la lista, se acumula la cantidad respetando el stock disponible.

### B. Atajos y Acciones Rápidas
- **F12:** Enfoca el campo de pago o ejecuta el cobro.
- **Escape:** Limpia el campo de código y devuelve el foco para escanear.
- **Botones +1 / -1:** Ajusta la cantidad del ítem seleccionado.
- **Quitar:** Elimina la fila seleccionada.
- **Vaciar:** Limpia la lista completa de la venta.

### C. Cobro
1. Selecciona el **Cliente** (o deja "Consumidor Final").
2. Ingresa el monto en **Paga con** o presiona **Exacto**.
3. Revisa el valor de **Cambio**.
4. Haz clic en **Cobrar**. Se descuenta el inventario y se genera el comprobante.

---

## 6. Catálogo de Productos

Permite dar de alta, editar y consultar existencias:
1. **Campos:** Código, Nombre, Categoría, Precio, Stock y Descripción.
2. **Búsqueda:** Utiliza el campo **Buscar** para filtrar en tiempo real por código o nombre.
3. **Guardar / Eliminar:** Operaciones protegidas según el rol del usuario.

---

## 7. Directorio de Clientes

Permite registrar clientes para asociarlos a las ventas:
1. Haz clic en **Limpiar** para un registro nuevo.
2. Completa documento, nombre, teléfono, correo y dirección.
3. Haz clic en **Guardar**.

---

## 8. Reportes y Ventas

Módulo para auditorías y seguimiento de transacciones:
- **Filtros rápidos:** Botones **Hoy**, **7 Días**, **Mes** y selectores **Desde / Hasta**.
- **Tarjetas KPI:** Total Ventas, Nº Ventas, Ticket Promedio y Artículos Vendidos en azul marino.
- **Desglose:** Al seleccionar un folio, se visualizan los artículos de la transacción.
- **Ver Ticket:** Muestra una copia del comprobante de venta.
