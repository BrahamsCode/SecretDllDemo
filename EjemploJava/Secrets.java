import com.sun.jna.Library;
import com.sun.jna.Native;

/**
 * Clase reutilizable para leer contrasenas desde secret.dll.
 * Copia este archivo a tu proyecto y llama Secrets.get("postgres").
 * Requisitos: secret.dll y jna.jar disponibles.
 */
public final class Secrets {

    // Busca secret.dll en la carpeta actual y lee los textos como UTF-8.
    static {
        if (System.getProperty("jna.library.path") == null)
            System.setProperty("jna.library.path", System.getProperty("user.dir"));
        if (System.getProperty("jna.encoding") == null)
            System.setProperty("jna.encoding", "UTF-8");
    }

    private interface Lib extends Library {
        Lib INSTANCE = Native.load("secret", Lib.class);
        String getSecret(String name);
    }

    private Secrets() {}

    /** Devuelve la contrasena pedida. Lanza excepcion si el nombre no existe. */
    public static String get(String name) {
        String v = Lib.INSTANCE.getSecret(name);
        if (v == null)
            throw new IllegalArgumentException("Secreto no encontrado: " + name);
        return v;
    }
}
