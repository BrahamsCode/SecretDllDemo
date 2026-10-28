using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

// Generador de secret.c con VARIAS contrasenas ofuscadas (XOR de dos mitades).
//
// Lee un archivo de texto con lineas  nombre=contrasena  y genera el secret.c
// con cada contrasena partida en dos arrays (mitad A y mitad B) dentro de una
// matriz [2][largo]. La contrasena real = filaA[i] ^ filaB[i], y solo se
// reconstruye en memoria cuando el DLL es llamado.
//
// Uso:
//   dotnet run -- <archivo_entrada> <archivo_salida.c>
//   dotnet run -- secrets.txt ..\secret.c
//
// Si no se pasan argumentos usa:  secrets.txt  ->  secret.c
class Program
{
    static int Main(string[] args)
    {
        string entrada = args.Length > 0 ? args[0] : "secrets.txt";
        string salida  = args.Length > 1 ? args[1] : "secret.c";

        if (!File.Exists(entrada))
        {
            Console.WriteLine($"[ERROR] No se encontro el archivo de entrada: {entrada}");
            Console.WriteLine("Crea un archivo con lineas  nombre=contrasena  (una por linea).");
            return 1;
        }

        // Leer y parsear nombre=contrasena (ignora vacias y comentarios con #).
        var secretos = new List<(string nombre, byte[] bytes)>();
        foreach (var lineaCruda in File.ReadAllLines(entrada))
        {
            string linea = lineaCruda.Trim();
            if (linea.Length == 0 || linea.StartsWith("#")) continue;

            int ig = linea.IndexOf('=');
            if (ig <= 0)
            {
                Console.WriteLine($"[AVISO] Linea ignorada (sin '='): {linea}");
                continue;
            }

            string nombre = linea.Substring(0, ig).Trim();
            string pass   = linea.Substring(ig + 1); // la contrasena puede tener espacios
            secretos.Add((nombre, Encoding.UTF8.GetBytes(pass)));
        }

        if (secretos.Count == 0)
        {
            Console.WriteLine("[ERROR] No se encontraron contrasenas validas en el archivo.");
            return 1;
        }

        // Generar el codigo C.
        var sb = new StringBuilder();
        var rng = RandomNumberGenerator.Create();
        int maxLen = 0;

        sb.AppendLine("// secret.c - GENERADO AUTOMATICAMENTE. No editar a mano.");
        sb.AppendLine("// Varias contrasenas ofuscadas: cada una es una matriz [2][largo]");
        sb.AppendLine("// (fila 0 = mitad A, fila 1 = mitad B). Real = filaA[i] ^ filaB[i].");
        sb.AppendLine("// Llamar getSecret(\"nombre\") para reconstruirla en runtime.");
        sb.AppendLine();
        sb.AppendLine("#include <string.h>");
        sb.AppendLine();

        var identificadores = new List<string>();
        foreach (var (nombre, bytes) in secretos)
        {
            int len = bytes.Length;
            if (len > maxLen) maxLen = len;

            // Generar mitad A aleatoria; mitad B = pass ^ A.
            byte[] a = new byte[len];
            byte[] b = new byte[len];
            rng.GetBytes(a);
            for (int i = 0; i < len; i++)
                b[i] = (byte)(bytes[i] ^ a[i]);

            string id = Identificador(nombre);
            identificadores.Add(id);

            sb.AppendLine($"// \"{nombre}\" (largo {len})");
            sb.AppendLine($"static const unsigned char {id}[2][{len}] = {{");
            sb.AppendLine($"    {{ {Hex(a)} }}, // mitad A");
            sb.AppendLine($"    {{ {Hex(b)} }}, // mitad B");
            sb.AppendLine("};");
            sb.AppendLine();
        }

        // Tabla de registro: nombre -> (mitad A, mitad B, largo).
        sb.AppendLine("typedef struct {");
        sb.AppendLine("    const char* name;");
        sb.AppendLine("    const unsigned char* a;");
        sb.AppendLine("    const unsigned char* b;");
        sb.AppendLine("    int len;");
        sb.AppendLine("} SecretEntry;");
        sb.AppendLine();
        sb.AppendLine("static const SecretEntry secrets[] = {");
        for (int i = 0; i < secretos.Count; i++)
        {
            string id = identificadores[i];
            sb.AppendLine($"    {{ \"{secretos[i].nombre}\", {id}[0], {id}[1], {secretos[i].bytes.Length} }},");
        }
        sb.AppendLine("};");
        sb.AppendLine($"static const int secretCount = {secretos.Count};");
        sb.AppendLine();

        // Buffer estatico del tamano de la contrasena mas larga.
        sb.AppendLine($"static char buf[{maxLen} + 1];");
        sb.AppendLine();
        sb.AppendLine("// Devuelve la contrasena cuyo nombre coincide, o NULL si no existe.");
        sb.AppendLine("// La contrasena se reconstruye por XOR solo en esta llamada.");
        sb.AppendLine("__declspec(dllexport) const char* getSecret(const char* name) {");
        sb.AppendLine("    int i, j;");
        sb.AppendLine("    for (i = 0; i < secretCount; i++) {");
        sb.AppendLine("        if (strcmp(name, secrets[i].name) == 0) {");
        sb.AppendLine("            for (j = 0; j < secrets[i].len; j++) {");
        sb.AppendLine("                buf[j] = (char)(secrets[i].a[j] ^ secrets[i].b[j]);");
        sb.AppendLine("            }");
        sb.AppendLine("            buf[secrets[i].len] = '\\0';");
        sb.AppendLine("            return buf;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("    return (const char*)0; // nombre no encontrado");
        sb.AppendLine("}");

        File.WriteAllText(salida, sb.ToString());

        Console.WriteLine($"[OK] Generado {salida} con {secretos.Count} contrasena(s):");
        foreach (var (nombre, bytes) in secretos)
            Console.WriteLine($"       - {nombre}  ({bytes.Length} bytes)");
        Console.WriteLine();
        Console.WriteLine("La contrasena en texto plano NO aparece en el .c generado;");
        Console.WriteLine("solo quedan las dos mitades aleatorias (A y B).");
        return 0;
    }

    // Convierte un nombre en un identificador C valido (postgres -> postgresPass).
    static string Identificador(string nombre)
    {
        var sb = new StringBuilder();
        foreach (char c in nombre)
            sb.Append(char.IsLetterOrDigit(c) ? c : '_');
        string id = sb.ToString();
        if (id.Length == 0 || char.IsDigit(id[0])) id = "s_" + id;
        return id + "Pass";
    }

    // Formatea un arreglo de bytes como  0x02, 0xA2, 0x3E
    static string Hex(byte[] bytes)
    {
        var partes = new string[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
            partes[i] = "0x" + bytes[i].ToString("X2");
        return string.Join(", ", partes);
    }
}
