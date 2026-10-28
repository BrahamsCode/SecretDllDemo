using System;
using System.Runtime.InteropServices;

// Clase reutilizable para leer contrasenas desde secret.dll.
// Copia este archivo a tu proyecto y llama Secrets.Get("postgres").
// Requisito: secret.dll junto al .exe.
public static class Secrets
{
    // P/Invoke. CharSet.Ansi marshaliza el string "nombre" como const char*.
    [DllImport("secret.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    static extern IntPtr getSecret(string name);

    // Devuelve la contrasena pedida. Lanza excepcion si el nombre no existe.
    public static string Get(string name)
    {
        IntPtr ptr = getSecret(name);
        if (ptr == IntPtr.Zero)
            throw new ArgumentException($"Secreto no encontrado: {name}");
        return Marshal.PtrToStringUTF8(ptr)!;
    }
}
