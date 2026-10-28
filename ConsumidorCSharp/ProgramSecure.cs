using System;
using System.Runtime.InteropServices;
using System.Text;

// Consumidor C# SEGURO que usa secret_secure.dll
//
// DIFERENCIAS vs Program.cs original:
// 1. Provee su propio buffer (no usa memoria estática del DLL)
// 2. Llama a clearSecretKey() después de usar la clave
// 3. Thread-safe (cada instancia tiene su buffer)
class ProgramSecure
{
    // P/Invoke para la nueva API segura
    [DllImport("secret_secure.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int getSecretKey(byte[] buffer, int bufferSize);

    [DllImport("secret_secure.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern void clearSecretKey(byte[] buffer, int bufferSize);

    [DllImport("secret_secure.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int getSecretKeyLength();

    static void Main()
    {
        // 1. Obtener el tamaño necesario del buffer
        int bufferSize = getSecretKeyLength();
        byte[] buffer = new byte[bufferSize];

        try
        {
            // 2. Obtener la clave en nuestro buffer
            int result = getSecretKey(buffer, bufferSize);

            if (result == 0)
            {
                Console.WriteLine("[C# SEGURO] Error al obtener la clave");
                return;
            }

            // 3. Convertir a string y usar la clave
            string clave = Encoding.ASCII.GetString(buffer, 0, bufferSize - 1);
            Console.WriteLine("[C# SEGURO] Clave desde secret_secure.dll: " + clave);

            // 4. AQUÍ usarías la clave (conectar a DB, descifrar, etc.)
            // ...
        }
        finally
        {
            // 5. SIEMPRE limpiar el buffer antes de salir
            clearSecretKey(buffer, bufferSize);
            Console.WriteLine("[C# SEGURO] Buffer limpiado de memoria");
        }
    }
}
