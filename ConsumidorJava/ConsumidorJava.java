import com.sun.jna.Library;
import com.sun.jna.Native;

// Programa Java que llama al MISMO secret.dll (escrito en C) usando JNA.
// No usa JNI: JNA permite invocar funciones C "planas" sin escribir nombres
// especiales ni recompilar el DLL. La clave vive ofuscada dentro del DLL.
public class ConsumidorJava {

    // Interfaz que describe las funciones del DLL que queremos usar.
    public interface SecretLib extends Library {
        // Native.load("secret", ...) carga secret.dll (lo busca en jna.library.path / PATH).
        SecretLib INSTANCE = Native.load("secret", SecretLib.class);

        // La función C devuelve const char*; JNA lo convierte a String automáticamente.
        String getSecretKey();
    }

    public static void main(String[] args) {
        String clave = SecretLib.INSTANCE.getSecretKey();
        System.out.println("[Java] clave desde secret.dll: " + clave);
    }
}
