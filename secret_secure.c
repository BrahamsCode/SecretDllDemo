// secret_secure.c - DLL de Windows con clave ofuscada (XOR) - VERSIÓN PRODUCCIÓN
// Mejoras sobre secret.c:
// 1. El llamador provee el buffer (no usa memoria estática)
// 2. Función explícita para limpiar el buffer después de usarlo
// 3. Thread-safe (cada llamada usa su propio buffer)
// 4. Validación de parámetros

#include <string.h>

#define SECRET_LEN 18

// Claves ofuscadas (ka ^ kb = clave real)
// IMPORTANTE: En producción, estas vienen del generador C# como en Android
static const unsigned char ka[18] = {
    0xcf, 0x41, 0x09, 0x61, 0x3f, 0xd5, 0x7c, 0x60, 0x58, 0x48, 0x45, 0xa1,
    0x02, 0x5b, 0x3a, 0xf2, 0x33, 0xd4,
};

static const unsigned char kb[18] = {
    0x8c, 0x2d, 0x68, 0x17, 0x5a, 0x91, 0x19, 0x0d, 0x37, 0x65, 0x01, 0xed,
    0x4e, 0x76, 0x08, 0xc2, 0x01, 0xe2,
};

// Reconstruye la clave en el buffer provisto por el llamador.
// Devuelve 1 si OK, 0 si error.
//
// PARÁMETROS:
//   buffer: puntero a char[] con espacio >= SECRET_LEN + 1
//   bufferSize: tamaño del buffer (debe ser >= SECRET_LEN + 1)
__declspec(dllexport) int getSecretKey(char* buffer, int bufferSize) {
    if (buffer == NULL || bufferSize < SECRET_LEN + 1) {
        return 0; // Error: buffer inválido o muy pequeño
    }

    // XOR ka/kb para reconstruir la clave
    for (int i = 0; i < SECRET_LEN; i++) {
        buffer[i] = (char)(ka[i] ^ kb[i]);
    }
    buffer[SECRET_LEN] = '\0';

    return 1; // Éxito
}

// Limpia (sobrescribe con ceros) el buffer que contiene la clave.
// SIEMPRE llamar después de usar la clave, antes de liberar el buffer.
//
// PARÁMETROS:
//   buffer: el mismo buffer usado en getSecretKey()
//   bufferSize: tamaño del buffer
__declspec(dllexport) void clearSecretKey(char* buffer, int bufferSize) {
    if (buffer == NULL || bufferSize <= 0) {
        return;
    }

    // Sobrescribir con ceros (volatile para evitar optimizaciones del compilador)
    volatile char* vptr = buffer;
    for (int i = 0; i < bufferSize; i++) {
        vptr[i] = 0;
    }
}

// Función de conveniencia que devuelve el tamaño mínimo del buffer necesario
__declspec(dllexport) int getSecretKeyLength(void) {
    return SECRET_LEN + 1;
}
