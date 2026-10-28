// secret.c - DLL de Windows con la clave ofuscada (XOR ka/kb).
// Clave de EJEMPLO para aprendizaje (no es la de produccion).
// Misma tecnica que el secret.c del NDK en Android, pero compilado como .dll
// y con una funcion C plana exportada, para que la llamen C# (P/Invoke) y Java (JNA).

#include <string.h>

#define SECRET_LEN 18

static const unsigned char ka[18] = {
    0xcf, 0x41, 0x09, 0x61, 0x3f, 0xd5, 0x7c, 0x60, 0x58, 0x48, 0x45, 0xa1,
    0x02, 0x5b, 0x3a, 0xf2, 0x33, 0xd4,
};

static const unsigned char kb[18] = {
    0x8c, 0x2d, 0x68, 0x17, 0x5a, 0x91, 0x19, 0x0d, 0x37, 0x65, 0x01, 0xed,
    0x4e, 0x76, 0x08, 0xc2, 0x01, 0xe2,
};

// Buffer donde se reconstruye la clave. __declspec(dllexport) expone la funcion
// fuera del DLL con su nombre tal cual: getSecretKey.
static char buf[SECRET_LEN + 1];

__declspec(dllexport) const char* getSecretKey(void) {
    int i;
    for (i = 0; i < SECRET_LEN; i++) {
        buf[i] = (char)(ka[i] ^ kb[i]);
    }
    buf[SECRET_LEN] = 0;
    return buf;
}
