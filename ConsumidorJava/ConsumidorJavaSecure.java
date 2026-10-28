import com.sun.jna.Library;
import com.sun.jna.Native;
import java.nio.charset.StandardCharsets;

// Consumidor Java SEGURO que usa secret_secure.dll con JNA
//
// DIFERENCIAS vs ConsumidorJava.java original:
// 1. Provee su propio buffer (no usa memoria estática del DLL)
// 2. Llama a clearSecretKey() después de usar la clave
// 3. Thread-safe (cada instancia tiene su buffer)
public class ConsumidorJavaSecure {

    // Interfaz JNA para la nueva API segura
    public interface SecretLibSecure extends Library {
        SecretLibSecure INSTANCE = Native.load("secret_secure", SecretLibSecure.class);

        // Obtener clave en buffer provisto (devuelve 1 si OK, 0 si error)
        int getSecretKey(byte[] buffer, int bufferSize);

        // Limpiar buffer con ceros
        void clearSecretKey(byte[] buffer, int bufferSize);

        // Obtener tamaño necesario del buffer
        int getSecretKeyLength();
    }

    public static void main(String[] args) {
        SecretLibSecure lib = SecretLibSecure.INSTANCE;

        // 1. Obtener el tamaño necesario del buffer
        int bufferSize = lib.getSecretKeyLength();
        byte[] buffer = new byte[bufferSize];

        try {
            // 2. Obtener la clave en nuestro buffer
            int result = lib.getSecretKey(buffer, bufferSize);

            if (result == 0) {
                System.out.println("[Java SEGURO] Error al obtener la clave");
                return;
            }

            // 3. Convertir a String y usar la clave
            // Buscar el null terminator para no incluir basura
            int length = 0;
            while (length < buffer.length && buffer[length] != 0) {
                length++;
            }
            String clave = new String(buffer, 0, length, StandardCharsets.US_ASCII);
            System.out.println("[Java SEGURO] Clave desde secret_secure.dll: " + clave);

            // 4. AQUÍ usarías la clave (conectar a DB, descifrar, etc.)
            // ...

        } finally {
            // 5. SIEMPRE limpiar el buffer antes de salir
            lib.clearSecretKey(buffer, bufferSize);
            System.out.println("[Java SEGURO] Buffer limpiado de memoria");
        }
    }
}
