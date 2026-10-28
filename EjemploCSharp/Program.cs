using System;

// Ejemplo C#: usa la clase reutilizable Secrets (ver Secrets.cs) para leer
// varias contrasenas desde secret.dll por nombre. Ninguna esta en texto plano
// en el .exe ni en el DLL: se reconstruyen por XOR en memoria al pedirlas.
class Program
{
    static void Main()
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("  Ejemplo C#: varias contrasenas desde secret.dll");
        Console.WriteLine("==================================================");

        // --- Asi se usa en tu proyecto: una linea por contrasena ---
        foreach (string nombre in new[] { "postgres", "server", "api", "redis" })
            Console.WriteLine($"[C#] {nombre,-10} -> {Secrets.Get(nombre)}");

        // Nombre que no existe: Secrets.Get lanza excepcion (aqui la capturamos).
        try { Secrets.Get("noexiste"); }
        catch (ArgumentException ex) { Console.WriteLine($"[C#] {"noexiste",-10} -> ({ex.Message})"); }

        Console.WriteLine();
        Console.WriteLine("Ejemplo real de conexion a PostgreSQL (ver README.md):");
        Console.WriteLine("  Password={Secrets.Get(\"postgres\")}");
    }
}
