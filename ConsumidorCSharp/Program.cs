using System;
using System.Runtime.InteropServices;

// Programa C# que llama al secret.dll (escrito en C) y obtiene la clave ofuscada.
// La clave no está en este código: vive partida (XOR) dentro del DLL y se
// reconstruye en C en tiempo de ejecución.
class Program
{
    // P/Invoke: enlaza con la función exportada por secret.dll.
    // CallingConvention.Cdecl = convención por defecto de cl.exe para funciones C.
    [DllImport("secret.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern IntPtr getSecretKey();

    static void Main()
    {
        IntPtr ptr = getSecretKey();                 // puntero al char* que devuelve el DLL
        string? clave = Marshal.PtrToStringAnsi(ptr); // char* -> string de C#
        Console.WriteLine("[C#]   clave desde secret.dll: " + clave);
    }
}
