using System;
using MySql.Data.MySqlClient;

namespace FerreteriaSistema
{
    class Program
    {
        // Cadena de conexión - AJUSTA TU PASSWORD DE MYSQL AQUÍ
        static string connectionString = "server=localhost;database=ferreteria_db;uid=root;pwd=1234;";

        static void Main(string[] args)
        {
            Console.WriteLine("=== SISTEMA DE GESTIÓN FERRETERÍA ===\n");

            string username = "";
            string rol = "";

            // Login
            if (!Login(ref username, ref rol))
            {
                Console.WriteLine("Acceso denegado. Presiona cualquier tecla para salir...");
                Console.ReadKey();
                return;
            }

            // Menú principal
            int opcion;
            do
            {
                Console.WriteLine($"\n=== MENÚ PRINCIPAL (Usuario: {username}, Rol: {rol}) ===");
                Console.WriteLine("1. Gestionar Productos");
                Console.WriteLine("2. Registrar Venta");
                Console.WriteLine("3. Ver Alertas de Stock");
                Console.WriteLine("4. Salir");
                Console.Write("Opción: ");

                if (!int.TryParse(Console.ReadLine(), out opcion))
                {
                    Console.WriteLine("Opción inválida!");
                    continue;
                }

                switch (opcion)
                {
                    case 1:
                        if (rol == "admin")
                        {
                            GestionarProductos();
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Solo el administrador puede gestionar productos.");
                            Console.ReadKey();
                        }
                        break;
                    case 2:
                        RegistrarVenta(username);
                        break;
                    case 3:
                        AlertasStock();
                        break;
                    case 4:
                        Console.WriteLine("Saliendo del sistema...");
                        break;
                    default:
                        Console.WriteLine("Opción inválida!");
                        break;
                }
            } while (opcion != 4);
        }

        // Función de Login
        static bool Login(ref string username, ref string rol)
        {
            Console.Write("Usuario: ");
            string user = Console.ReadLine();
            Console.Write("Password: ");
            string pass = Console.ReadLine();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT rol FROM usuarios WHERE username = @user AND password = @pass";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", user);
                        cmd.Parameters.AddWithValue("@pass", pass);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                username = user;
                                rol = reader.GetString("rol");
                                Console.WriteLine($"¡Bienvenido {username}! (Rol: {rol})");
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de conexión: {ex.Message}");
            }

            Console.WriteLine("Credenciales incorrectas!");
            return false;
        }

        // Módulo 1: CRUD de Productos
        static void GestionarProductos()
        {
            int opcion;
            do
            {
                Console.WriteLine("\n=== GESTIÓN DE PRODUCTOS ===");
                Console.WriteLine("1. Ver todos los productos");
                Console.WriteLine("2. Agregar nuevo producto");
                Console.WriteLine("3. Actualizar stock");
                Console.WriteLine("4. Eliminar producto");
                Console.WriteLine("5. Volver al menú principal");
                Console.Write("Opción: ");

                if (!int.TryParse(Console.ReadLine(), out opcion))
                {
                    Console.WriteLine("Opción inválida!");
                    continue;
                }

                switch (opcion)
                {
                    case 1:
                        VerProductos();
                        break;
                    case 2:
                        AgregarProducto();
                        break;
                    case 3:
                        ActualizarStock();
                        break;
                    case 4:
                        EliminarProducto();
                        break;
                    case 5:
                        break;
                    default:
                        Console.WriteLine("Opción inválida!");
                        break;
                }
            } while (opcion != 5);
        }

        static void VerProductos()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT codigo, nombre, categoria, precio, stock FROM productos ORDER BY categoria, nombre";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("\n--- LISTADO DE PRODUCTOS ---");
                        Console.WriteLine("Código | Nombre | Categoría | Precio | Stock");
                        Console.WriteLine("------------------------------------------------");

                        while (reader.Read())
                        {
                            Console.WriteLine($"{reader.GetString("codigo")} | " +
                                            $"{reader.GetString("nombre")} | " +
                                            $"{reader.GetString("categoria")} | " +
                                            $"${reader.GetDecimal("precio")} | " +
                                            $"{reader.GetInt32("stock")}");
                        }
                    }
                }
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void AgregarProducto()
        {
            Console.WriteLine("\n--- AGREGAR PRODUCTO ---");
            Console.Write("Código: ");
            string codigo = Console.ReadLine();
            Console.Write("Nombre: ");
            string nombre = Console.ReadLine();
            Console.Write("Categoría: ");
            string categoria = Console.ReadLine();
            Console.Write("Precio: ");
            decimal precio = decimal.Parse(Console.ReadLine());
            Console.Write("Stock inicial: ");
            int stock = int.Parse(Console.ReadLine());
            Console.Write("Stock mínimo: ");
            int stockMin = int.Parse(Console.ReadLine());

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO productos (codigo, nombre, categoria, precio, stock, stock_minimo) 
                                   VALUES (@codigo, @nombre, @categoria, @precio, @stock, @stockMin)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@codigo", codigo);
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@categoria", categoria);
                        cmd.Parameters.AddWithValue("@precio", precio);
                        cmd.Parameters.AddWithValue("@stock", stock);
                        cmd.Parameters.AddWithValue("@stockMin", stockMin);

                        cmd.ExecuteNonQuery();
                        Console.WriteLine("¡Producto agregado exitosamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.ReadKey();
        }

        static void ActualizarStock()
        {
            Console.WriteLine("\n--- ACTUALIZAR STOCK ---");
            Console.Write("Código del producto: ");
            string codigo = Console.ReadLine();
            Console.Write("Nuevo stock: ");
            int nuevoStock = int.Parse(Console.ReadLine());

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE productos SET stock = @stock WHERE codigo = @codigo";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@stock", nuevoStock);
                        cmd.Parameters.AddWithValue("@codigo", codigo);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                            Console.WriteLine("¡Stock actualizado!");
                        else
                            Console.WriteLine("Producto no encontrado!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.ReadKey();
        }

        static void EliminarProducto()
        {
            Console.WriteLine("\n--- ELIMINAR PRODUCTO ---");
            Console.Write("Código del producto: ");
            string codigo = Console.ReadLine();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM productos WHERE codigo = @codigo";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@codigo", codigo);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                            Console.WriteLine("¡Producto eliminado!");
                        else
                            Console.WriteLine("Producto no encontrado!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.ReadKey();
        }

        // Módulo 2: Registrar Venta
        static void RegistrarVenta(string username)
        {
            Console.WriteLine("\n=== REGISTRAR NUEVA VENTA ===");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Obtener ID del usuario
                    int idUsuario = 0;
                    using (MySqlCommand cmd = new MySqlCommand("SELECT id FROM usuarios WHERE username = @user", conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        idUsuario = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Crear venta
                    int idVenta = 0;
                    using (MySqlCommand cmd = new MySqlCommand("INSERT INTO ventas (total, id_usuario) VALUES (0, @idUsuario); SELECT LAST_INSERT_ID();", conn))
                    {
                        cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                        idVenta = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    decimal totalVenta = 0;
                    string continuar;

                    do
                    {
                        Console.Write("\nIngrese código del producto (o 'FIN' para terminar): ");
                        string codigo = Console.ReadLine();

                        if (codigo.ToUpper() == "FIN") break;

                        Console.Write("Cantidad: ");
                        int cantidad = int.Parse(Console.ReadLine());

                        // Verificar stock y obtener precio
                        int idProducto = 0;
                        decimal precio = 0;
                        int stockActual = 0;

                        using (MySqlCommand cmd = new MySqlCommand("SELECT id, precio, stock FROM productos WHERE codigo = @codigo", conn))
                        {
                            cmd.Parameters.AddWithValue("@codigo", codigo);
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    idProducto = reader.GetInt32("id");
                                    precio = reader.GetDecimal("precio");
                                    stockActual = reader.GetInt32("stock");
                                }
                                else
                                {
                                    Console.WriteLine("✗ Producto no encontrado!");
                                    continue;
                                }
                            }
                        }

                        if (stockActual >= cantidad)
                        {
                            decimal subtotal = precio * cantidad;
                            totalVenta += subtotal;

                            // Insertar detalle de venta
                            using (MySqlCommand cmd = new MySqlCommand(@"INSERT INTO detalle_ventas 
                                (id_venta, id_producto, cantidad, precio_unitario, subtotal) 
                                VALUES (@idVenta, @idProducto, @cantidad, @precio, @subtotal)", conn))
                            {
                                cmd.Parameters.AddWithValue("@idVenta", idVenta);
                                cmd.Parameters.AddWithValue("@idProducto", idProducto);
                                cmd.Parameters.AddWithValue("@cantidad", cantidad);
                                cmd.Parameters.AddWithValue("@precio", precio);
                                cmd.Parameters.AddWithValue("@subtotal", subtotal);
                                cmd.ExecuteNonQuery();
                            }

                            // Actualizar stock
                            using (MySqlCommand cmd = new MySqlCommand("UPDATE productos SET stock = stock - @cantidad WHERE id = @idProducto", conn))
                            {
                                cmd.Parameters.AddWithValue("@cantidad", cantidad);
                                cmd.Parameters.AddWithValue("@idProducto", idProducto);
                                cmd.ExecuteNonQuery();
                            }

                            Console.WriteLine($"✓ Producto agregado. Subtotal: ${subtotal}");
                        }
                        else
                        {
                            Console.WriteLine($"✗ Stock insuficiente. Disponible: {stockActual}");
                        }

                    } while (true);

                    // Actualizar total de la venta
                    using (MySqlCommand cmd = new MySqlCommand("UPDATE ventas SET total = @total WHERE id = @idVenta", conn))
                    {
                        cmd.Parameters.AddWithValue("@total", totalVenta);
                        cmd.Parameters.AddWithValue("@idVenta", idVenta);
                        cmd.ExecuteNonQuery();
                    }

                    Console.WriteLine("\n==================================");
                    Console.WriteLine("VENTA REGISTRADA EXITOSAMENTE");
                    Console.WriteLine($"Total: ${totalVenta}");
                    Console.WriteLine("==================================");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar venta: {ex.Message}");
            }
            Console.ReadKey();
        }

        // Módulo 3: Alertas de Stock Mínimo
        static void AlertasStock()
        {
            Console.WriteLine("\n=== ALERTAS DE STOCK MÍNIMO ===");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT codigo, nombre, categoria, stock, stock_minimo 
                                   FROM productos WHERE stock <= stock_minimo ORDER BY stock ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("\n⚠️ PRODUCTOS CON STOCK BAJO ⚠️");
                        Console.WriteLine("Código | Nombre | Categoría | Stock Actual | Stock Mínimo");
                        Console.WriteLine("----------------------------------------------------------");

                        bool hayAlertas = false;
                        while (reader.Read())
                        {
                            hayAlertas = true;
                            Console.WriteLine($"{reader.GetString("codigo")} | " +
                                            $"{reader.GetString("nombre")} | " +
                                            $"{reader.GetString("categoria")} | " +
                                            $"{reader.GetInt32("stock")} | " +
                                            $"{reader.GetInt32("stock_minimo")}");
                        }

                        if (!hayAlertas)
                        {
                            Console.WriteLine("✓ Todos los productos tienen stock suficiente.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.ReadKey();
        }
    }
}