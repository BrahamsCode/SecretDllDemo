# MultiPassword — contrasenas ofuscadas en un DLL nativo

Varias contrasenas ofuscadas con XOR dentro de un solo `secret.dll` (escrito en C),
leidas desde **C#** (P/Invoke) y **Java** (JNA) por nombre: `getSecret("postgres")`.

Ninguna contrasena aparece en texto plano en el `.dll`, `.exe` ni `.class`.
Cada una vive partida en dos mitades (A y B) y se reconstruye en RAM solo al pedirla.

## Estructura

```
MultiPassword/
├── secrets.txt              Entrada: nombre=contrasena (una por linea)
├── Generador/               Genera secret.c desde secrets.txt (C#)
├── secret.c                 GENERADO: las mitades A/B ofuscadas
├── secret.dll               Compilado con MSVC
├── generar_y_compilar.bat   1 clic: genera + compila + copia el DLL
├── EjemploCSharp/           Secrets.cs (reutilizable) + demo (run.bat)
└── EjemploJava/             Secrets.java (reutilizable) + demo (run.bat)
```

## Como cambiar las contrasenas (3 pasos)

1. Edita `secrets.txt` con las contrasenas reales:
   ```
   postgres=LaClaveRealDePostgres
   server=OtraClaveReal
   ```
2. Doble clic en `generar_y_compilar.bat` (regenera `secret.c` y recompila `secret.dll`).
3. Los proyectos ya leen las nuevas contrasenas, sin tocar codigo.

> Requisitos: .NET SDK (generador + ejemplo C#), Java JDK (ejemplo Java),
> y Visual Studio Build Tools con `cl.exe` (compilar el DLL).

## Como se usa en tu proyecto

Copia la clase `Secrets` una vez y llama `Secrets.Get("nombre")` donde necesites.
El unico requisito es que `secret.dll` este junto al ejecutable.

### C# — leer una contrasena
```csharp
string pass = Secrets.Get("postgres");
```

### C# — conectar a PostgreSQL (Npgsql)
```csharp
string connStr = $"Host=localhost;Port=5432;Database=neotech;" +
                 $"Username=postgres;Password={Secrets.Get("postgres")}";

await using var conn = new NpgsqlConnection(connStr);
await conn.OpenAsync();
// la contrasena nunca estuvo hardcodeada en el codigo
```

### Java — leer una contrasena
```java
String pass = Secrets.get("postgres");
```

### Java — conectar a PostgreSQL (JDBC)
```java
String url  = "jdbc:postgresql://localhost:5432/neotech";
String user = "postgres";
String pass = Secrets.get("postgres");

try (Connection conn = DriverManager.getConnection(url, user, pass)) {
    // la contrasena nunca estuvo en el codigo Java
}
```

## Como funciona (XOR de dos mitades)

Cada contrasena se guarda como una matriz `[2][largo]`: fila 0 = mitad A, fila 1 = mitad B.
La mitad A es aleatoria; la mitad B = contrasena XOR A. Al pedirla, el DLL hace
`A[i] ^ B[i]` byte a byte y reconstruye la contrasena en un buffer, solo en ese momento.

```c
static const unsigned char postgresPass[2][18] = {
    { ... },  // mitad A (aleatoria)
    { ... },  // mitad B (A XOR contrasena)
};
// getSecret("postgres") -> A[i] ^ B[i] -> "PgSql-Neotech-2026"
```

Mismo concepto que `libsecret.so` + JNI en e-regi-android-app; aqui es un DLL de
Windows con una funcion C plana que cualquier lenguaje puede llamar.

## Nota de seguridad

En este estilo simple la contrasena queda en un buffer del DLL despues de leerla.
Protege contra analisis estatico del binario (descompilar el `.exe`/`.class`/`.dll`),
que es el objetivo. Si se necesita ademas borrar la contrasena de RAM tras usarla,
se puede usar la variante con buffer del llamador + limpieza explicita.
