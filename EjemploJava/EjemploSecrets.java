/**
 * Ejemplo Java: usa la clase reutilizable Secrets (ver Secrets.java) para leer
 * varias contrasenas desde secret.dll por nombre. Ninguna esta en texto plano
 * en el .class ni en el DLL: se reconstruyen por XOR en memoria al pedirlas.
 */
public class EjemploSecrets {

    public static void main(String[] args) {
        System.out.println("==================================================");
        System.out.println("  Ejemplo Java: varias contrasenas desde secret.dll");
        System.out.println("==================================================");

        // --- Asi se usa en tu proyecto: una linea por contrasena ---
        for (String nombre : new String[] { "postgres", "server", "api", "redis" })
            System.out.printf("[Java] %-10s -> %s%n", nombre, Secrets.get(nombre));

        // Nombre que no existe: Secrets.get lanza excepcion (aqui la capturamos).
        try {
            Secrets.get("noexiste");
        } catch (IllegalArgumentException ex) {
            System.out.printf("[Java] %-10s -> (%s)%n", "noexiste", ex.getMessage());
        }

        System.out.println();
        System.out.println("Ejemplo real de conexion a PostgreSQL (ver README.md):");
        System.out.println("  DriverManager.getConnection(url, user, Secrets.get(\"postgres\"))");
    }
}
