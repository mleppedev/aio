# 100 Preguntas Típicas para Ingeniero .NET Senior

- **Fundamentos .NET** (15 preguntas)
- **C# Avanzado** (15 preguntas)
- **Arquitectura y Patrones** (15 preguntas)
- **APIs y Microservicios** (10 preguntas)
- **Base de Datos y ORM** (10 preguntas)
- **Pruebas y Calidad** (10 preguntas)
- **Performance y Optimización** (10 preguntas)
- **Seguridad** (8 preguntas)
- **DevOps y CI/CD** (7 preguntas)

## 🔧 **FUNDAMENTOS .NET (15 preguntas)**

### 1. ¿Cuál es la diferencia entre .NET Framework, .NET Core y .NET 5+?

**Contexto** Evolución de la plataforma .NET y sus características distintivas.

**Respuesta detallada**

- **.NET Framework (2002-2019)**

  - Solo Windows, monolítico
  - Incluye WinForms, WPF, ASP.NET Web Forms
  - GAC para assembly sharing
  - Versionado side-by-side limitado
  - Última versión: 4.8

- **.NET Core (2016-2020)**

  - Cross-platform (Windows, Linux, macOS)
  - Modular, open source
  - Self-contained deployments
  - Performance mejorado
  - No incluye WinForms/WPF inicialmente

- **.NET 5+ (2020-presente)**
  - Unificación de .NET Framework y Core
  - "One .NET" strategy
  - Release anual (6, 7, 8, 9...)
  - LTS cada 2 años (6, 8...)
  - Incluye todas las workloads

```csharp
// Ejemplo de target framework
<TargetFramework>net8.0</TargetFramework>
<TargetFramework>netstandard2.1</TargetFramework>
<TargetFramework>net48</TargetFramework>
```

### 2. ¿Qué es el CLR (Common Language Runtime) y cuáles son sus responsabilidades?

**Contexto** Motor de ejecución, garbage collection, compilación JIT.

**Respuesta detallada**

El CLR es el entorno de ejecución que proporciona servicios como:

**Responsabilidades principales**

1. **Compilación JIT** Convierte IL a código nativo
2. **Garbage Collection** Gestión automática de memoria
3. **Type Safety** Verificación de tipos en runtime
4. **Exception Handling** Manejo estructurado de excepciones
5. **Security** Code Access Security, sandboxing
6. **Threading** Gestión de threads y sincronización

**Proceso de ejecución**

```csharp
// 1. Código C# → 2. IL (MSIL) → 3. JIT → 4. Código nativo
public class Example
{
    public void Method() => Console.WriteLine("Hello");
}

// IL generado:
// IL_0000: ldstr "Hello"
// IL_0005: call void [System.Console]::WriteLine(string)
// IL_000a: ret
```

**Ventajas del CLR**

- Interoperabilidad entre lenguajes
- Gestión automática de memoria
- Verificación de tipos en runtime
- Manejo consistente de excepciones

### 3. Explica el proceso de compilación en .NET: IL, JIT, AOT

**Contexto** Desde código fuente hasta ejecución en máquina.

**Respuesta detallada**

**Proceso de compilación tradicional**

1. **Código fuente** (C#, VB.NET, F#) →
2. **Compilador** (csc.exe, vbc.exe) →
3. **IL (Intermediate Language)** →
4. **JIT Compiler** →
5. **Código nativo**

**IL (Intermediate Language)**

```csharp
// C# Code
int sum = a + b;

// IL equivalente
ldarg.0    // Load argument 'a'
ldarg.1    // Load argument 'b'
add        // Add top two stack values
stloc.0    // Store result in local variable 'sum'
```

**JIT (Just-In-Time) Compilation**

- Compila IL a código nativo en runtime
- Optimizaciones específicas del hardware
- Compiled methods se cachean
- Tipos: Normal JIT, Pre-JIT, Econo-JIT

**AOT (Ahead-Of-Time) Compilation**

```csharp
<!-- Proyecto AOT -->
<PropertyGroup>
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>
</PropertyGroup>
```

**Ventajas AOT**

- Startup más rápido
- Menor consumo de memoria
- No requiere JIT en runtime

**Trade-offs**

- Tamaño de archivo mayor
- Menos optimizaciones runtime
- Limitaciones en reflection

### 4. ¿Cuál es la diferencia entre Value Types y Reference Types?

**Contexto** Stack vs Heap, boxing/unboxing, performance implications.

**Respuesta detallada**

**Value Types**

- Almacenados en **stack** (variables locales) o **inline** (campos de clase)
- Contienen directamente sus datos
- Asignación copia el valor
- Derivan de `System.ValueType`

```csharp
// Value Types
int a = 5;
int b = a;    // 'b' es una copia independiente
a = 10;       // 'b' sigue siendo 5

struct Point
{
    public int X, Y;
    public Point(int x, int y) => (X, Y) = (x, y);
}
```

**Reference Types**

- Almacenados en **heap**
- Variable contiene referencia (puntero) al objeto
- Asignación copia la referencia
- Derivan de `System.Object`

```csharp
// Reference Types
var list1 = new List<int> { 1, 2, 3 };
var list2 = list1;        // Ambas variables apuntan al mismo objeto
list1.Add(4);             // list2 también ve el cambio

class Person
{
    public string Name { get; set; }
}
```

**Boxing/Unboxing**

```csharp
// Boxing: Value Type → Reference Type
int value = 42;
object boxed = value;     // Boxing allocation en heap

// Unboxing: Reference Type → Value Type
int unboxed = (int)boxed; // Unboxing requiere cast explícito
```

**Performance implications**

- Value types: Sin overhead de heap allocation
- Reference types: GC pressure, indirection cost
- Boxing: Allocation innecesaria, evitar en hot paths

### 5. ¿Qué son los Assembly y el GAC (Global Assembly Cache)?

**Contexto** Deployment, versionado, resolución de dependencias.

**Respuesta detallada**

**Assembly**

- Unidad lógica de deployment y versionado
- Contiene uno o más archivos (EXE, DLL)
- Metadata completa sobre tipos y dependencias
- Boundary de seguridad y versionado

```csharp
// Información de Assembly
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0-beta")]

// Reflexión de Assembly
Assembly assembly = Assembly.GetExecutingAssembly();
string version = assembly.GetName().Version.ToString();
```

**Tipos de Assembly**

- **Executable Assembly** .exe files
- **Library Assembly** .dll files
- **Private Assembly** Local a la aplicación
- **Shared Assembly** En GAC (solo .NET Framework)

**GAC (Global Assembly Cache)**

- Solo en .NET Framework
- C:\Windows\Microsoft.NET\assembly\
- Permite multiple versiones del mismo assembly
- Requiere strong naming

```csharp
// Strong naming para GAC
[assembly: AssemblyKeyFile("mykey.snk")]

// Instalación en GAC
// gacutil -i MyLibrary.dll
// gacutil -l MyLibrary
```

**En .NET Core/.NET 5+**

- No hay GAC
- NuGet packages y framework-dependent deployment
- Shared frameworks en runtime store

### 6. Explica el concepto de AppDomain en .NET Framework vs .NET Core

**Contexto** Aislamiento de aplicaciones, seguridad, compatibilidad.

**Respuesta detallada**

**.NET Framework AppDomains**

- Boundary de aislamiento dentro de un proceso
- Cada AppDomain tiene su propio security context
- Permite cargar/unload assemblies independientemente
- Comunicación via marshaling o serialization

```csharp
// .NET Framework - Crear AppDomain
AppDomain domain = AppDomain.CreateDomain("MyDomain");
try
{
    // Cargar assembly en el AppDomain
    domain.ExecuteAssembly("plugin.exe");

    // Crear objeto en otro AppDomain
    var obj = domain.CreateInstanceAndUnwrap("MyLib", "MyClass");
}
finally
{
    AppDomain.Unload(domain); // Liberar recursos
}
```

**Funcionalidades en .NET Framework**

- Assembly loading/unloading
- Security isolation
- Configuration isolation
- Exception isolation

**.NET Core/.NET 5+ cambios**

- **No hay AppDomains** (excepto default)
- **AssemblyLoadContext** reemplaza funcionalidad
- **Proceso isolation** en lugar de AppDomain isolation
- **Containers/microservices** para aislamiento

```csharp
// .NET Core - AssemblyLoadContext
public class PluginLoadContext : AssemblyLoadContext
{
    public PluginLoadContext() : base(isCollectible: true) { }

    protected override Assembly Load(AssemblyName name)
    {
        // Custom assembly loading logic
        return null;
    }
}

// Uso
var context = new PluginLoadContext();
var assembly = context.LoadFromAssemblyPath("plugin.dll");
// context.Unload(); // Para unload
```

**Razones del cambio**

- Simplificación del modelo
- Mejor performance
- Cross-platform compatibility
- Containers como boundary natural

### 7. ¿Qué es Reflection y cuándo usarías/evitarías usarlo?

**Contexto** Introspección, performance trade-offs, casos de uso.

**Respuesta detallada**

**Reflection** permite inspeccionar y manipular types, assemblies, y members en runtime.

**Capacidades principales**

```csharp
// Obtener información de tipos
Type type = typeof(string);
Type type2 = "hello".GetType();
Type type3 = Type.GetType("System.String");

// Inspeccionar members
PropertyInfo[] props = type.GetProperties();
MethodInfo[] methods = type.GetMethods();
FieldInfo[] fields = type.GetFields();

// Crear instancias dinámicamente
object instance = Activator.CreateInstance(type);
object instance2 = Activator.CreateInstance(type, "constructor param");

// Invocar métodos dinámicamente
MethodInfo method = type.GetMethod("Substring");
object result = method.Invoke("Hello World", new object[] { 0, 5 });
```

**Casos de uso apropiados**

1. **Frameworks** (Entity Framework, ASP.NET MVC)
2. **Serialization** (JSON.NET, System.Text.Json)
3. **Dependency Injection containers**
4. **Unit testing frameworks**
5. **Plugin architectures**
6. **Configuration binding**

```csharp
// Ejemplo: Attribute-based validation
public class ValidationAttribute : Attribute
{
    public virtual bool IsValid(object value) => true;
}

public static bool ValidateObject(object obj)
{
    var type = obj.GetType();

    foreach (var prop in type.GetProperties())
    {
        var attrs = prop.GetCustomAttributes<ValidationAttribute>();
        var value = prop.GetValue(obj);

        if (attrs.Any(attr => !attr.IsValid(value)))
            return false;
    }
    return true;
}
```

**Cuándo evitarlo**

- **Performance crítico** 10-100x más lento que direct calls
- **Hot paths** Código ejecutado frecuentemente
- **Type safety** Errores en runtime vs compile time
- **Code analysis** Difícil para herramientas estáticas

**Alternativas modernas**

```csharp
// Source generators (C# 9+)
[JsonSerializable(typeof(Person))]
public partial class PersonContext : JsonSerializerContext { }

// Expression trees para performance
Expression<Func<T, object>> expr = x => x.PropertyName;
var compiled = expr.Compile(); // Más rápido que reflection
```

### 8. ¿Cuáles son las diferencias entre .NET Standard, .NET Framework PCL?

**Contexto** Portabilidad, compatibilidad entre plataformas.

**Respuesta detallada**

**.NET Standard**

- **Specification** de APIs que todas las implementaciones .NET deben soportar
- Reemplaza PCL (Portable Class Libraries)
- Versionado con mayor número = más APIs

```csharp
// .NET Standard targeting
<TargetFramework>netstandard2.0</TargetFramework>
<TargetFramework>netstandard2.1</TargetFramework>

// Multi-targeting
<TargetFrameworks>netstandard2.0;net48;net6.0</TargetFrameworks>
```

**Versiones principales**

- **.NET Standard 1.x** APIs básicas, limitadas
- **.NET Standard 2.0** Gran expansión, ~32K APIs
- **.NET Standard 2.1** Span<T>, Async streams, más performance APIs

**PCL (Portable Class Libraries)**

- Enfoque anterior a .NET Standard
- Profile-based targeting (Profile 111, Profile 259, etc.)
- Intersection model: solo APIs comunes a ALL targets
- Deprecated en favor de .NET Standard

```csharp
// PCL antiguo
<TargetFrameworkProfile>Profile111</TargetFrameworkProfile>
<SupportedCultures>portable-net45+win8+wpa81</SupportedCultures>
```

**Compatibility matrix**

```
.NET Standard 2.0:
├── .NET Framework 4.6.1+
├── .NET Core 2.0+
├── Mono 5.4+
├── Xamarin.iOS 10.14+
├── Xamarin.Android 8.0+
└── UWP 10.0.16299+

.NET Standard 2.1:
├── .NET Core 3.0+
├── .NET 5.0+
├── Mono 6.4+
└── Xamarin.iOS 12.16+
```

**Cuándo usar cada uno**

- **.NET Standard 2.0** Máxima compatibilidad (incluye .NET Framework)
- **.NET Standard 2.1** Mejor performance, APIs modernas (no .NET Framework)
- **Specific framework** Cuando necesitas APIs específicas

**Migration path**

```csharp
<!-- Old PCL -->
<TargetFrameworkProfile>Profile111</TargetFrameworkProfile>

<!-- New .NET Standard -->
<TargetFramework>netstandard2.0</TargetFramework>
```

### 9. Explica el concepto de Strong Naming en .NET

**Contexto** Seguridad, versionado, deployment scenarios.

**Respuesta detallada**

**Strong Naming** proporciona identidad única global y verificación de integridad para assemblies.

**Componentes de Strong Name**

1. **Simple name** (filename sin extensión)
2. **Version number** (major.minor.build.revision)
3. **Culture** (para localization)
4. **Public key** (de la key pair)
5. **Digital signature**

**Generación de keys**

```bash
# Generar key pair
sn -k mykey.snk

# Extraer public key
sn -p mykey.snk mypublic.snk

# Ver public key token
sn -tp mypublic.snk
```

**Aplicación en código**

```csharp
// En AssemblyInfo.cs
[assembly: AssemblyKeyFile("mykey.snk")]
// O en project file
<AssemblyOriginatorKeyFile>mykey.snk</AssemblyOriginatorKeyFile>
<SignAssembly>true</SignAssembly>

// Resultado en assembly
[assembly: AssemblyVersion("1.0.0.0")]
// Public Key Token: b77a5c561934e089
```

**Verificación**

```csharp
// Runtime verification
Assembly asm = Assembly.LoadFrom("MyLibrary.dll");
byte[] publicKey = asm.GetName().GetPublicKey();
byte[] publicKeyToken = asm.GetName().GetPublicKeyToken();

// Manual verification
sn -v MyLibrary.dll
```

**Beneficios**

- **Global uniqueness** Evita name collisions
- **Integrity** Detecta tampering
- **Version checking** Side-by-side deployment
- **GAC eligibility** Requerido para GAC

**Limitaciones y consideraciones**

- **Performance** Verificación en load time
- **Complexity** Key management, build process
- **Debugging** Complica testing con signed assemblies
- **.NET Core** Menos relevante, no hay GAC

**Delay signing** (para desarrollo):

```csharp
[assembly: AssemblyDelaySign(true)]
[assembly: AssemblyKeyFile("publickey.snk")] // Solo public key

// Skip verification durante desarrollo
sn -Vr MyLibrary.dll
```

### 10. ¿Qué son los Attributes en .NET y cómo crear uno custom?

**Contexto** Metadata, serialización, validation, frameworks.

**Respuesta detallada**

**Attributes** son metadata declarativa que se anexa a assemblies, types, members, y parameters.

**Attributes built-in comunes**

```csharp
[Obsolete("Use NewMethod instead", true)]
public void OldMethod() { }

[Serializable]
public class Person
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [JsonIgnore]
    public string Password { get; set; }

    [DisplayName("Birth Date")]
    public DateTime BirthDate { get; set; }
}

[Conditional("DEBUG")]
public void DebugMethod() { }

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public int FastMethod() => 42;
```

**Crear custom attribute**

```csharp
// 1. Heredar de Attribute
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
                AllowMultiple = false, Inherited = true)]
public class RangeValidationAttribute : Attribute
{
    public int Min { get; }
    public int Max { get; }
    public string ErrorMessage { get; set; }

    public RangeValidationAttribute(int min, int max)
    {
        Min = min;
        Max = max;
        ErrorMessage = $"Value must be between {min} and {max}";
    }

    public bool IsValid(object value)
    {
        if (value is int intValue)
            return intValue >= Min && intValue <= Max;
        return false;
    }
}

// 2. Uso del custom attribute
public class Product
{
    [RangeValidation(1, 100, ErrorMessage = "Quantity must be 1-100")]
    public int Quantity { get; set; }
}
```

**Leer attributes via reflection**

```csharp
public static List<string> ValidateObject(object obj)
{
    var errors = new List<string>();
    var type = obj.GetType();

    foreach (var prop in type.GetProperties())
    {
        // Obtener custom attributes
        var validators = prop.GetCustomAttributes<RangeValidationAttribute>();
        var value = prop.GetValue(obj);

        foreach (var validator in validators)
        {
            if (!validator.IsValid(value))
                errors.Add($"{prop.Name}: {validator.ErrorMessage}");
        }
    }
    return errors;
}

// Uso
var product = new Product { Quantity = 150 };
var errors = ValidateObject(product);
// errors: ["Quantity: Quantity must be 1-100"]
```

**AttributeUsage configuration**

```csharp
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,  // Donde se puede aplicar
    AllowMultiple = true,                              // Múltiples instancias
    Inherited = false)]                                // Herencia
public class LogAttribute : Attribute
{
    public string Category { get; set; }
    public LogLevel Level { get; set; } = LogLevel.Info;
}

// Múltiples attributes
[Log(Category = "Security", Level = LogLevel.Warning)]
[Log(Category = "Performance", Level = LogLevel.Debug)]
public void CriticalMethod() { }
```

**Casos de uso comunes**

- **Validation frameworks** (Data Annotations)
- **Serialization control** (JsonIgnore, XmlElement)
- **ORM mapping** (Entity Framework attributes)
- **Dependency injection** (Inject, Component)
- **Security** (Authorize, AllowAnonymous)
- **Testing** (Test, TestCase, Setup)

### 11. ¿Cuál es la diferencia entre Managed y Unmanaged Code?

**Contexto** Interoperabilidad, P/Invoke, COM interop.

**Respuesta detallada**

**Managed Code**

- Ejecuta bajo control del CLR
- Gestión automática de memoria (GC)
- Type safety y verificación en runtime
- Exception handling estructurado
- Security sandboxing

```csharp
// Managed code típico
public class ManagedExample
{
    public void SafeMethod()
    {
        var list = new List<string>(); // Memory managed by GC
        list.Add("Hello");             // Type-safe operations
        // No explicit memory cleanup needed
    }
}
```

**Unmanaged Code**

- Ejecuta directamente en SO/hardware
- Gestión manual de memoria
- No type safety guarantees
- Direct memory access
- Ejemplos: C++, C, assembly

**Interoperabilidad P/Invoke**

```csharp
// Llamar función Win32 API
[DllImport("user32.dll", CharSet = CharSet.Auto)]
public static extern int MessageBox(
    IntPtr hWnd,
    string text,
    string caption,
    uint type);

// Uso
MessageBox(IntPtr.Zero, "Hello", "Title", 0);

// Marshaling complejo
[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}

[DllImport("user32.dll")]
public static extern bool GetCursorPos(out POINT lpPoint);
```

**COM Interop**

```csharp
// Reference to COM object
using System.Runtime.InteropServices;

[ComImport]
[Guid("00000000-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IUnknown
{
    IntPtr QueryInterface(ref Guid riid);
    uint AddRef();
    uint Release();
}
```

**Mixed mode assemblies** (C++/CLI):

```cpp
// C++/CLI - Bridge entre managed y unmanaged
#pragma managed
public ref class ManagedWrapper
{
    NativeClass* m_pNative;
public:
    ManagedWrapper() { m_pNative = new NativeClass(); }
    ~ManagedWrapper() { this->!ManagedWrapper(); }
    !ManagedWrapper() { delete m_pNative; }
};
```

### 12. Explica el proceso de Garbage Collection en .NET

**Contexto** Generaciones, finalizers, IDisposable pattern.

**Respuesta detallada**

**Generational GC**

- **Gen 0** Objetos nuevos, colecciones frecuentes (~1MB)
- **Gen 1** Objetos que sobrevivieron 1 GC (~16MB)
- **Gen 2** Objetos long-lived, colecciones costosas
- **LOH** Large Object Heap (>85KB), solo Gen 2

```csharp
// Forzar GC (NO recomendado en producción)
GC.Collect(0); // Solo Gen 0
GC.Collect();  // Full GC
GC.WaitForPendingFinalizers();

// Información de GC
long gen0 = GC.CollectionCount(0);
long totalMemory = GC.GetTotalMemory(false);
```

**Proceso de colección**

1. **Mark** Identificar objetos alcanzables desde roots
2. **Sweep** Liberar memoria de objetos no alcanzables
3. **Compact** Compactar heap para reducir fragmentación

**Roots incluyen**

- Variables locales y parámetros
- Static variables
- CPU registers
- GC handles

**Finalizers** (evitar si es posible):

```csharp
public class UnmanagedResource
{
    private IntPtr handle;
    private bool disposed = false;

    // Finalizer - solo para recursos unmanaged
    ~UnmanagedResource()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // Evitar finalizer
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Limpiar managed resources
            }

            // Limpiar unmanaged resources
            if (handle != IntPtr.Zero)
            {
                CloseHandle(handle);
                handle = IntPtr.Zero;
            }

            disposed = true;
        }
    }
}
```

**GC Modes**

```csharp
<!-- Workstation vs Server GC -->
<PropertyGroup>
    <ServerGarbageCollection>true</ServerGarbageCollection>
    <ConcurrentGarbageCollection>true</ConcurrentGarbageCollection>
</PropertyGroup>
```

### 13. ¿Qué es el CTS (Common Type System) y CLS (Common Language Specification)?

**Contexto** Interoperabilidad entre lenguajes .NET.

**Respuesta detallada**

**CTS (Common Type System)**

- Define cómo tipos son declarados, usados y gestionados en runtime
- Unified type system para todos los lenguajes .NET
- Base para cross-language integration

**Categorías de tipos en CTS**

```csharp
// Value Types
struct Point { public int X, Y; }
enum Status { Active, Inactive }

// Reference Types
class Person { }
interface IComparable { }
delegate void EventHandler();
```

**CLS (Common Language Specification)**

- Subset de CTS que garantiza interoperabilidad
- Rules que los lenguajes deben seguir para ser CLS-compliant

**CLS compliance rules**

```csharp
[assembly: CLSCompliant(true)]

public class CompliantClass
{
    // ✅ CLS-compliant
    public string Name { get; set; }
    public int Count { get; set; }

    // ❌ NOT CLS-compliant
    public uint UnsignedCount { get; set; }  // uint no es CLS-compliant

    [CLSCompliant(false)]
    public uint InternalUnsigned { get; set; } // Marcado como non-compliant
}

// ✅ Case-sensitive names OK in C#
public void Process() { }
public void PROCESS() { } // Diferente método

// ❌ Case-insensitive conflict para VB.NET consumers
```

**Type mapping entre lenguajes**

```
C# Type     VB.NET Type    CTS Type
int         Integer        System.Int32
string      String         System.String
bool        Boolean        System.Boolean
object      Object         System.Object
```

**Benefits of CTS/CLS**

- Cross-language inheritance
- Exception handling across languages
- Debugging across language boundaries
- Metadata consistency

### 14. ¿Cuáles son las diferencias entre Debug y Release builds?

**Contexto** Optimizaciones, símbolos de debug, performance.

**Respuesta detallada**

**Debug Build**

```csharp
<PropertyGroup Condition=" '$(Configuration)' == 'Debug' ">
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <DebugSymbols>true</DebugSymbols>
</PropertyGroup>
```

**Características Debug**

- **No optimizations** Código tal como se escribe
- **Debug symbols** .pdb files para debugging
- **Conditional compilation** DEBUG constant definido
- **Larger binaries** Más metadata y symbols

```csharp
#if DEBUG
    Console.WriteLine("Debug mode active");
    // Code solo en debug
#endif

[Conditional("DEBUG")]
public static void DebugLog(string message)
{
    Console.WriteLine($"DEBUG: {message}");
    // Método removido completamente en Release
}
```

**Release Build**

```csharp
<PropertyGroup Condition=" '$(Configuration)' == 'Release' ">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <DefineConstants>TRACE</DefineConstants>
    <DebugSymbols>false</DebugSymbols>
</PropertyGroup>
```

**Optimizaciones en Release**

- **Inlining** Métodos pequeños inlined
- **Dead code elimination** Código no usado removido
- **Constant folding** Expressions constantes pre-calculadas
- **Loop optimizations** Unrolling, strength reduction

```csharp
// Debug: Llamadas individuales
public int Calculate(int x)
{
    return x * 2 + 1; // En Release puede ser inlined
}

// Release optimization example
const int FACTOR = 10;
int result = value * FACTOR; // Optimizado en compile time si value es constante
```

**Performance differences**

- **Startup time** Release 2-5x más rápido
- **Execution speed** Release 10-50% mejor performance
- **Memory usage** Release usa menos memoria
- **File size** Release binaries más pequeños

**Debugging considerations**

```csharp
// Variables pueden ser "optimized away" en Release
public void Method()
{
    int temp = CalculateValue();
    return temp + 1; // 'temp' puede no existir en Release
}
```

### 15. Explica los diferentes tipos de deployment en .NET (Framework-dependent, Self-contained)

**Contexto** Distribución, tamaño, dependencias, performance.

**Respuesta detallada**

**Framework-Dependent Deployment (FDD)**

- Requiere .NET runtime preinstalado en target machine
- Binarios más pequeños
- Shared runtime actualizado automáticamente

```csharp
<!-- FDD Configuration -->
<PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <!-- No self-contained -->
</PropertyGroup>
```

```bash
# Publish FDD
dotnet publish -c Release
# Output: ~50KB-5MB depending on app
```

**Self-Contained Deployment (SCD)**

- Incluye .NET runtime con la aplicación
- No requiere runtime preinstalado
- Control total sobre runtime version

```csharp
<!-- SCD Configuration -->
<PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
</PropertyGroup>
```

```bash
# Publish SCD
dotnet publish -c Release --self-contained true -r win-x64
# Output: ~150-200MB
```

**Single File Deployment**

```csharp
<PropertyGroup>
    <PublishSingleFile>true</PublishSingleFile>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
</PropertyGroup>
```

**AOT (Ahead-of-Time) Deployment**

```csharp
<PropertyGroup>
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>
</PropertyGroup>
```

**Trimming para reducir tamaño**

```csharp
<PropertyGroup>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>link</TrimMode>
</PropertyGroup>

<!-- Preservar código específico -->
<ItemGroup>
    <TrimmerRootAssembly Include="MyLibrary" />
</ItemGroup>
```

**Runtime Identifiers (RID)**

```bash
# Windows
win-x64, win-x86, win-arm64

# Linux
linux-x64, linux-arm64, linux-musl-x64

# macOS
osx-x64, osx-arm64
```

**Comparación de estrategias**

| Aspecto      | FDD               | SCD     | Single File     | AOT        |
| ------------ | ----------------- | ------- | --------------- | ---------- |
| Tamaño       | ~5MB              | ~150MB  | ~80MB           | ~10MB      |
| Startup      | Normal            | Normal  | Lento (extract) | Muy rápido |
| Memory       | Normal            | Normal  | Normal          | Menos      |
| Dependencies | Runtime requerido | Ninguna | Ninguna         | Ninguna    |
| Updates      | Automatic runtime | Manual  | Manual          | Manual     |

---

## 💎 **C# AVANZADO (15 preguntas)**

### 16. ¿Cuáles son las diferencias entre `abstract class` e `interface`? ¿Cuándo usar cada uno?

**Contexto** Default implementations en interfaces (C# 8+), herencia múltiple.

**Respuesta detallada**

**Abstract Class**

```csharp
public abstract class Animal
{
    // Estado (fields)
    protected string name;

    // Constructor
    protected Animal(string name) => this.name = name;

    // Implementación concreta
    public virtual void Eat() => Console.WriteLine($"{name} is eating");

    // Método abstracto - debe implementarse
    public abstract void MakeSound();

    // Propiedades concretas
    public string Name => name;
}

public class Dog : Animal
{
    public Dog(string name) : base(name) { }

    public override void MakeSound() => Console.WriteLine("Woof!");

    // Solo puede heredar de UNA clase
}
```

**Interface (Tradicional)**

```csharp
public interface IFlyable
{
    // Solo contratos (hasta C# 7)
    void Fly();
    int MaxAltitude { get; }
}

public interface ISwimmable
{
    void Swim();
    bool CanDive { get; }
}

// Múltiples interfaces
public class Duck : Animal, IFlyable, ISwimmable
{
    public Duck() : base("Duck") { }

    public override void MakeSound() => Console.WriteLine("Quack!");

    public void Fly() => Console.WriteLine("Flying high!");
    public int MaxAltitude => 1000;

    public void Swim() => Console.WriteLine("Swimming gracefully!");
    public bool CanDive => true;
}
```

**Interface con Default Implementation (C# 8+)**

```csharp
public interface ILogger
{
    void Log(string message);

    // Default implementation
    void LogInfo(string message) => Log($"INFO: {message}");
    void LogError(string message) => Log($"ERROR: {message}");

    // Static members (C# 8+)
    static ILogger CreateConsoleLogger() => new ConsoleLogger();
}

public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine(message);
    // LogInfo y LogError heredados automáticamente
}
```

**Cuándo usar cada uno**

**Use Abstract Class cuando**

- Quieres compartir código entre clases relacionadas
- Necesitas constructors con parámetros
- Requieres fields o state compartido
- Tienes una relación "is-a" clara
- Quieres controlar el acceso (protected members)

**Use Interface cuando**

- Defines un contrato que clases no relacionadas pueden implementar
- Necesitas herencia múltiple de comportamiento
- Quieres loose coupling y high testability
- La relación es más "can-do" que "is-a"
- APIs públicas que pueden evolucionar (default implementations)

### 17. Explica los conceptos de `virtual`, `override`, `new`, `sealed`

**Contexto** Polimorfismo, method hiding vs overriding.

**Respuesta detallada**

**Virtual Methods**

```csharp
public class BaseClass
{
    // Método virtual - puede ser overridden
    public virtual void DoWork()
    {
        Console.WriteLine("Base implementation");
    }

    // Método regular - no virtual
    public void DoOtherWork()
    {
        Console.WriteLine("Base other work");
    }
}
```

**Override** (Method Overriding):

```csharp
public class DerivedClass : BaseClass
{
    // Override reemplaza completamente la implementación base
    public override void DoWork()
    {
        Console.WriteLine("Derived implementation");
        // Opcionalmente llamar base
        base.DoWork();
    }
}

// Polimorfismo en acción
BaseClass obj = new DerivedClass();
obj.DoWork(); // Output: "Derived implementation" + "Base implementation"
```

**New** (Method Hiding):

```csharp
public class HidingClass : BaseClass
{
    // New oculta el método base (no es override)
    public new void DoOtherWork()
    {
        Console.WriteLine("Hidden implementation");
    }
}

// Diferencia importante
BaseClass baseRef = new HidingClass();
HidingClass derivedRef = new HidingClass();

baseRef.DoOtherWork();    // "Base other work" - usa implementación base
derivedRef.DoOtherWork(); // "Hidden implementation" - usa implementación nueva
```

**Sealed** (Prevent Further Inheritance):

```csharp
public class MiddleClass : BaseClass
{
    // Sealed override - no se puede override más adelante
    public sealed override void DoWork()
    {
        Console.WriteLine("Final implementation");
    }
}

public class FinalClass : MiddleClass
{
    // ❌ ERROR: No se puede override un sealed method
    // public override void DoWork() { }
}

// Sealed class - no se puede heredar
public sealed class UtilityClass
{
    public static void Helper() { }
}

// ❌ ERROR: No se puede heredar de sealed class
// public class MyClass : UtilityClass { }
```

**Virtual Property Example**

```csharp
public class Shape
{
    public virtual double Area { get; protected set; }
    public virtual string Name => "Generic Shape";
}

public class Circle : Shape
{
    private double radius;

    public Circle(double radius)
    {
        this.radius = radius;
        Area = Math.PI * radius * radius;
    }

    public override string Name => "Circle";
}
```

**Best Practices**

- Usa `virtual` cuando quieras permitir customización
- Usa `override` para polimorfismo verdadero
- Usa `new` solo cuando realmente quieras hiding (raro)
- Usa `sealed` para prevenir extensión no deseada
- Siempre marca virtual methods como `protected` o `public`

### 18. ¿Qué son los Generics y cuáles son sus beneficios? Explica constraints

**Contexto** Type safety, performance, covariance/contravariance.

**Respuesta detallada**

**Generics básicos**

```csharp
// Generic class
public class GenericList<T>
{
    private T[] items = new T[10];
    private int count = 0;

    public void Add(T item)
    {
        if (count < items.Length)
            items[count++] = item;
    }

    public T Get(int index) => items[index];
}

// Uso
var stringList = new GenericList<string>();
var intList = new GenericList<int>();
```

**Generic Methods**

```csharp
public static class GenericMethods
{
    // Method con múltiples type parameters
    public static TResult Map<TSource, TResult>(
        TSource source,
        Func<TSource, TResult> mapper)
    {
        return mapper(source);
    }

    // Generic method con constraints
    public static T CreateInstance<T>() where T : new()
    {
        return new T();
    }
}

// Uso
string result = GenericMethods.Map(42, x => x.ToString());
var list = GenericMethods.CreateInstance<List<int>>();
```

**Type Constraints**

```csharp
// where T : class - reference type
public class RefContainer<T> where T : class
{
    public T Value { get; set; }
    public bool IsNull => Value == null; // Solo para reference types
}

// where T : struct - value type
public class ValueContainer<T> where T : struct
{
    public T Value { get; set; }
    public bool HasValue { get; set; }
}

// where T : new() - parameterless constructor
public class Factory<T> where T : new()
{
    public T Create() => new T();
}

// where T : BaseClass - inheritance constraint
public class Repository<T> where T : Entity
{
    public void Save(T entity) => entity.Save();
}

// where T : IInterface - interface constraint
public class Processor<T> where T : IDisposable
{
    public void Process(T item)
    {
        using (item) // Garantizado que tiene Dispose()
        {
            // Process logic
        }
    }
}

// Multiple constraints
public class ComplexContainer<T>
    where T : class, IComparable<T>, new()
{
    public T CreateAndCompare(T other)
    {
        var instance = new T();
        return instance.CompareTo(other) > 0 ? instance : other;
    }
}
```

**Covariance y Contravariance**

```csharp
// Covariance (out) - puede return más derived types
public interface IProducer<out T>
{
    T Produce();
    // No puede tener T como parameter
}

IProducer<string> stringProducer = /* ... */;
IProducer<object> objectProducer = stringProducer; // ✅ Covariant

// Contravariance (in) - puede accept más base types
public interface IConsumer<in T>
{
    void Consume(T item);
    // No puede return T
}

IConsumer<object> objectConsumer = /* ... */;
IConsumer<string> stringConsumer = objectConsumer; // ✅ Contravariant

// Ejemplo práctico
IEnumerable<string> strings = new List<string> { "a", "b" };
IEnumerable<object> objects = strings; // Covariant

Action<object> objectAction = obj => Console.WriteLine(obj);
Action<string> stringAction = objectAction; // Contravariant
```

**Benefits de Generics**

1. **Type Safety** Errores en compile time vs runtime
2. **Performance** No boxing/unboxing para value types
3. **Code Reuse** Una implementación para múltiples tipos
4. **IntelliSense** Mejor tooling support

```csharp
// Sin Generics (old way)
ArrayList list = new ArrayList();
list.Add(1);
list.Add("string"); // ❌ Runtime error potencial
int value = (int)list[0]; // Boxing/unboxing + casting

// Con Generics
List<int> genericList = new List<int>();
genericList.Add(1);
// genericList.Add("string"); // ❌ Compile error
int value2 = genericList[0]; // No casting, no boxing
```

### 19. Explica los diferentes tipos de delegates: `Action`, `Func`, `Predicate`

**Contexto** Functional programming, callbacks, event handling.

**Respuesta detallada**

**Delegate básico**

```csharp
// Custom delegate declaration
public delegate void MyDelegate(string message);
public delegate int MathOperation(int a, int b);

// Usage
MathOperation add = (a, b) => a + b;
MathOperation multiply = (a, b) => a * b;

int result = add(5, 3); // 8
```

**Action<T>** (void methods):

```csharp
// Action sin parámetros
Action simpleAction = () => Console.WriteLine("Hello");

// Action con parámetros
Action<string> printMessage = message => Console.WriteLine(message);
Action<int, int> printNumbers = (a, b) => Console.WriteLine($"{a}, {b}");

// Múltiples Actions
Action<string, int, bool> complexAction = (name, age, active) =>
    Console.WriteLine($"{name} is {age} and {(active ? "active" : "inactive")}");

// Usage en eventos
public event Action<string> OnMessageReceived;
OnMessageReceived += msg => LogMessage(msg);
OnMessageReceived += msg => SendNotification(msg);
```

**Func<T, TResult>** (methods que retornan valor):

```csharp
// Func<TResult> - sin parámetros, retorna TResult
Func<int> getRandomNumber = () => new Random().Next();

// Func<T, TResult> - un parámetro
Func<string, int> getStringLength = str => str.Length;

// Func<T1, T2, TResult> - múltiples parámetros
Func<int, int, int> add = (a, b) => a + b;
Func<string, int, bool, string> formatUser = (name, age, active) =>
    $"{name} ({age}) - {(active ? "Active" : "Inactive")}";

// Uso en LINQ
var numbers = new[] { 1, 2, 3, 4, 5 };
var doubled = numbers.Select(x => x * 2); // Func<int, int>
var evens = numbers.Where(x => x % 2 == 0); // Func<int, bool>
```

**Predicate<T>** (specific case of Func<T, bool>):

```csharp
// Predicate es equivalente a Func<T, bool>
Predicate<int> isEven = x => x % 2 == 0;
Predicate<string> isLongString = str => str.Length > 10;

// Uso con List.FindAll
var numbers = new List<int> { 1, 2, 3, 4, 5, 6 };
var evenNumbers = numbers.FindAll(isEven);

// Combinando predicates
Predicate<int> isPositive = x => x > 0;
Predicate<int> isEvenAndPositive = x => isEven(x) && isPositive(x);
```

**Comparación práctica**

```csharp
public class EventProcessor
{
    // Action para side effects
    public Action<string> OnError { get; set; }
    public Action<string> OnSuccess { get; set; }

    // Func para transformaciones
    public Func<string, string> MessageTransformer { get; set; }

    // Predicate para validaciones
    public Predicate<string> IsValidMessage { get; set; }

    public void ProcessMessage(string message)
    {
        if (IsValidMessage?.Invoke(message) == true)
        {
            var transformed = MessageTransformer?.Invoke(message) ?? message;
            OnSuccess?.Invoke(transformed);
        }
        else
        {
            OnError?.Invoke($"Invalid message: {message}");
        }
    }
}

// Usage
var processor = new EventProcessor
{
    IsValidMessage = msg => !string.IsNullOrEmpty(msg),
    MessageTransformer = msg => msg.ToUpper(),
    OnSuccess = msg => Console.WriteLine($"✅ {msg}"),
    OnError = msg => Console.WriteLine($"❌ {msg}")
};
```

**Multicast Delegates**

```csharp
Action<string> handler = null;
handler += msg => Console.WriteLine($"Handler 1: {msg}");
handler += msg => Console.WriteLine($"Handler 2: {msg}");
handler += msg => File.AppendAllText("log.txt", msg);

handler("Test message"); // Ejecuta todos los handlers

// Para Func, solo retorna el último resultado
Func<int> multiFunc = () => 1;
multiFunc += () => 2;
multiFunc += () => 3;
int result = multiFunc(); // result = 3 (solo el último)
```

### 20. ¿Qué son las Expression Trees y cuándo las usarías?

**Contexto** LINQ providers, ORM, dynamic code generation.

**Respuesta detallada**

**Expression Trees** representan código como data structures en lugar de compiled code.

**Diferencia entre Delegate y Expression**

```csharp
// Delegate - compiled code
Func<int, bool> compiledFunc = x => x > 5;

// Expression Tree - data structure que representa el código
Expression<Func<int, bool>> expressionTree = x => x > 5;

// El expression se puede analizar e interpretar
var parameter = expressionTree.Parameters[0]; // ParameterExpression
var body = expressionTree.Body;               // BinaryExpression
var left = ((BinaryExpression)body).Left;     // ParameterExpression
var right = ((BinaryExpression)body).Right;   // ConstantExpression
```

**Construcción manual de Expression Trees**

```csharp
// x => x * 2 + 1
var parameter = Expression.Parameter(typeof(int), "x");
var multiply = Expression.Multiply(parameter, Expression.Constant(2));
var add = Expression.Add(multiply, Expression.Constant(1));
var lambda = Expression.Lambda<Func<int, int>>(add, parameter);

// Compile y ejecutar
var compiled = lambda.Compile();
int result = compiled(5); // 11
```

**Uso en LINQ to SQL/EF**

```csharp
// LINQ to Objects - se ejecuta en memoria
var localUsers = users.Where(u => u.Age > 18).ToList();

// LINQ to Entities - se traduce a SQL
var dbUsers = context.Users.Where(u => u.Age > 18).ToList();
// WHERE Age > 18 (SQL generado)

// El expression tree permite al provider analizar y traducir
Expression<Func<User, bool>> predicate = u => u.Age > 18 && u.IsActive;
// Se puede analizar: BinaryExpression (&&)
//   Left: BinaryExpression (Age > 18)
//   Right: MemberExpression (IsActive)
```

**Visitor Pattern para recorrer Expression Trees**

```csharp
public class ParameterNameVisitor : ExpressionVisitor
{
    private readonly string oldName;
    private readonly string newName;

    public ParameterNameVisitor(string oldName, string newName)
    {
        this.oldName = oldName;
        this.newName = newName;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (node.Name == oldName)
            return Expression.Parameter(node.Type, newName);
        return base.VisitParameter(node);
    }
}

// Usage: cambiar nombre de parámetro
Expression<Func<int, bool>> original = x => x > 5;
var visitor = new ParameterNameVisitor("x", "value");
var modified = visitor.Visit(original); // value => value > 5
```

**Casos de uso avanzados**

```csharp
// 1. Dynamic LINQ
public static IQueryable<T> ApplyFilter<T>(
    IQueryable<T> query,
    string propertyName,
    object value)
{
    var parameter = Expression.Parameter(typeof(T), "x");
    var property = Expression.Property(parameter, propertyName);
    var constant = Expression.Constant(value);
    var equals = Expression.Equal(property, constant);
    var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

    return query.Where(lambda);
}

// 2. Property name extraction
public static string GetPropertyName<T, TProperty>(
    Expression<Func<T, TProperty>> propertyExpression)
{
    if (propertyExpression.Body is MemberExpression member)
        return member.Member.Name;
    throw new ArgumentException("Expression must be a property access");
}

// Usage
string propName = GetPropertyName<User, string>(u => u.Name); // "Name"

// 3. Configuration binding
public class ConfigBinder<T>
{
    public void Bind<TProperty>(
        Expression<Func<T, TProperty>> property,
        TProperty value)
    {
        var propName = GetPropertyName(property);
        // Set property via reflection
    }
}
```

**Performance considerations**

- Expression compilation es costoso
- Cache compiled expressions cuando sea posible
- Para hot paths, considera pre-compilation

### 21. Explica `async`/`await`: ¿Cómo funciona internamente?

**Contexto** State machines, Task, ConfigureAwait, deadlocks.

**Respuesta detallada**

**Async/Await básico**

```csharp
public async Task<string> DownloadDataAsync()
{
    using var client = new HttpClient();

    // El thread NO se bloquea aquí
    string data = await client.GetStringAsync("https://api.example.com");

    // Continúa en el mismo o diferente thread
    return data.ToUpper();
}

// Uso
string result = await DownloadDataAsync();
```

**State Machine generada por el compilador**

```csharp
// El compilador genera algo similar a esto:
[StructLayout(LayoutKind.Auto)]
private struct <DownloadDataAsync>d__1 : IAsyncStateMachine
{
    public int <>1__state;
    public AsyncTaskMethodBuilder<string> <>t__builder;
    public string <>s__1; // local variables
    private TaskAwaiter<string> <>u__1; // awaiter

    void IAsyncStateMachine.MoveNext()
    {
        int num = <>1__state;
        try
        {
            TaskAwaiter<string> awaiter;
            if (num != 0)
            {
                // Primera vez - iniciar operación async
                using var client = new HttpClient();
                awaiter = client.GetStringAsync("https://api.example.com").GetAwaiter();

                if (!awaiter.IsCompleted)
                {
                    // Suspender state machine
                    <>1__state = 0;
                    <>u__1 = awaiter;
                    <>t__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                    return;
                }
            }
            else
            {
                // Continuación después del await
                awaiter = <>u__1;
                <>u__1 = default;
                <>1__state = -1;
            }

            // Obtener resultado y continuar
            string data = awaiter.GetResult();
            string result = data.ToUpper();
            <>t__builder.SetResult(result);
        }
        catch (Exception ex)
        {
            <>t__builder.SetException(ex);
        }
    }
}
```

**ConfigureAwait y SynchronizationContext**

```csharp
// ❌ Deadlock potencial en UI/ASP.NET Framework
public string GetDataSync()
{
    return GetDataAsync().Result; // NUNCA hacer esto
}

private async Task<string> GetDataAsync()
{
    // Retorna al UI thread - deadlock si se llama sincrónicamente
    return await httpClient.GetStringAsync(url);
}

// ✅ Evitar deadlock
private async Task<string> GetDataAsync()
{
    // No retorna al original context
    return await httpClient.GetStringAsync(url).ConfigureAwait(false);
}

// ✅ Mejor: usar async todo el camino
public async Task<string> GetDataAsync()
{
    return await GetDataAsync();
}
```

**Task vs ValueTask**

```csharp
// Task - always heap allocated
public async Task<int> GetCachedValueAsync()
{
    if (cache.ContainsKey("key"))
        return cache["key"]; // Task allocation incluso para valor cached

    return await ExpensiveOperationAsync();
}

// ValueTask - stack allocated para valores sincrónicos
public async ValueTask<int> GetCachedValueOptimizedAsync()
{
    if (cache.ContainsKey("key"))
        return cache["key"]; // No allocation

    return await ExpensiveOperationAsync();
}
```

**Exception handling en async**

```csharp
public async Task<string> ProcessDataAsync()
{
    try
    {
        var data = await FetchDataAsync();
        var processed = await ProcessAsync(data);
        return processed;
    }
    catch (HttpRequestException ex)
    {
        // Específico para HTTP errors
        Logger.LogError("HTTP error: {Error}", ex.Message);
        throw;
    }
    catch (Exception ex)
    {
        // General exception handling
        Logger.LogError("Unexpected error: {Error}", ex.Message);
        throw new ProcessingException("Data processing failed", ex);
    }
}

// ❌ Exception se pierde
Task.Run(async () => await ProcessDataAsync()); // Fire and forget

// ✅ Exception se captura
try
{
    await Task.Run(async () => await ProcessDataAsync());
}
catch (Exception ex)
{
    // Handle exception
}
```

**Parallel async operations**

```csharp
// ❌ Secuencial - lento
var result1 = await Operation1Async();
var result2 = await Operation2Async();
var result3 = await Operation3Async();

// ✅ Paralelo - más rápido
var task1 = Operation1Async();
var task2 = Operation2Async();
var task3 = Operation3Async();

await Task.WhenAll(task1, task2, task3);
var result1 = task1.Result;
var result2 = task2.Result;
var result3 = task3.Result;

// ✅ Con resultados directos
var results = await Task.WhenAll(
    Operation1Async(),
    Operation2Async(),
    Operation3Async()
);
```

**Best practices**

- Usa `ConfigureAwait(false)` en library code
- No uses `.Result` o `.Wait()` en UI/ASP.NET contexts
- Prefer `ValueTask<T>` para hot paths con caching
- Siempre handle exceptions en async void (solo para event handlers)
- Usa `Task.WhenAll` para operations paralelas

### 22. ¿Cuál es la diferencia entre `Task.Run`, `Task.Factory.StartNew`, y `new Task()`?

**Contexto** Task creation, thread pool, performance implications.

**Respuesta detallada**

**Task.Run** (Recomendado para la mayoría de casos):

```csharp
// Forma más simple y segura de crear tareas
Task<int> task1 = Task.Run(() => {
    // Trabajo CPU-intensive
    return CalculateSum(1000000);
});

Task<string> task2 = Task.Run(async () => {
    // También soporta async lambdas
    await Task.Delay(1000);
    return "Completed";
});

// Automáticamente:
// - Usa ThreadPool
// - TaskCreationOptions.DenyChildAttach
// - TaskScheduler.Default
```

**Task.Factory.StartNew** (Control granular):

```csharp
// Más control sobre opciones de creación
Task<int> task = Task.Factory.StartNew(
    () => CalculateSum(1000000),
    CancellationToken.None,
    TaskCreationOptions.LongRunning, // Para tareas largas
    TaskScheduler.Default
);

// Problema común: nested Task con async
// ❌ MALO - retorna Task<Task<string>>
Task<Task<string>> nested = Task.Factory.StartNew(async () => {
    await Task.Delay(1000);
    return "Done";
});

// ✅ BUENO - usar Unwrap() o Task.Run
Task<string> unwrapped = Task.Factory.StartNew(async () => {
    await Task.Delay(1000);
    return "Done";
}).Unwrap();

// ✅ MEJOR - usar Task.Run para async lambdas
Task<string> better = Task.Run(async () => {
    await Task.Delay(1000);
    return "Done";
});
```

**new Task()** (Raramente usado):

```csharp
// Crea Task en estado Created - NO iniciado
Task task = new Task(() => Console.WriteLine("Working"));

// Debe iniciarse manualmente
task.Start(); // O Start(TaskScheduler)

// Uso típico: cuando necesitas controlar cuándo inicia
var task = new Task<int>(() => CalculateSum(1000));
// ... configurar otros parámetros
task.Start();

// ❌ Fácil olvidar llamar Start()
// ❌ No usa ThreadPool por defecto
```

**Comparación práctica**

```csharp
// Ejemplo: procesamiento de archivos
public async Task ProcessFilesAsync(string[] files)
{
    var tasks = new List<Task<string>>();

    foreach (var file in files)
    {
        // ✅ Task.Run - simple y eficiente
        tasks.Add(Task.Run(() => ProcessFile(file)));

        // Para operaciones I/O-bound, mejor usar async methods
        // tasks.Add(ProcessFileAsync(file));
    }

    var results = await Task.WhenAll(tasks);
}

// CPU-intensive con opciones específicas
public Task<int> ProcessLargeDataset(byte[] data)
{
    return Task.Factory.StartNew(
        () => ComputeComplexAlgorithm(data),
        TaskCreationOptions.LongRunning // Dedicated thread
    );
}
```

**Cuándo usar cada uno**

- **Task.Run** Default choice, CPU-bound work, simple scenarios
- **Task.Factory.StartNew** Control granular, custom TaskScheduler, specific options
- **new Task()** Scenarios especiales donde necesitas controlar timing

### 23. ¿Qué son los `nullable reference types` (C# 8+)?

**Contexto** Null safety, migration strategies, annotations.

**Respuesta detallada**

**Nullable Reference Types** ayuda a prevenir `NullReferenceException` en compile time.

**Habilitación**

```csharp
<!-- En .csproj -->
<PropertyGroup>
    <Nullable>enable</Nullable>
    <!-- O granular: -->
    <Nullable>warnings</Nullable>
    <Nullable>annotations</Nullable>
</PropertyGroup>
```

```csharp
// En archivo individual
#nullable enable

public class UserService
{
    // Non-nullable - nunca debería ser null
    public string Name { get; set; } = string.Empty;

    // Nullable - puede ser null
    public string? MiddleName { get; set; }

    // Warning si no se inicializa
    public string LastName { get; set; } // CS8618 warning
}
```

**Annotations y operators**

```csharp
public class UserProcessor
{
    // Nullable parameter
    public void ProcessUser(User? user)
    {
        // Compiler warning: posible null reference
        // Console.WriteLine(user.Name); // CS8602

        // ✅ Null check
        if (user != null)
        {
            Console.WriteLine(user.Name); // Safe
        }

        // ✅ Null-conditional operator
        Console.WriteLine(user?.Name ?? "Unknown");
    }

    // Non-nullable return, pero puede retornar null
    public User GetUser(int id)
    {
        var user = FindUserInDatabase(id);
        return user!; // Null-forgiving operator - "confía en mí"
    }

    // Nullable return type
    public User? FindUser(int id)
    {
        return id > 0 ? new User() : null;
    }
}
```

**Null-state analysis**

```csharp
public void AnalysisExample(string? input)
{
    // Compiler tracks null-state
    if (string.IsNullOrEmpty(input))
    {
        return; // Compiler knows input could be null
    }

    // Here compiler knows input is not null
    Console.WriteLine(input.Length); // No warning

    // Flow analysis
    string? text = GetText();
    if (text is not null)
    {
        Console.WriteLine(text.Length); // Safe
    }
}
```

**Attributes para mejor analysis**

```csharp
using System.Diagnostics.CodeAnalysis;

public class ValidationHelper
{
    // Tells compiler that if method returns true, value is not null
    public static bool IsNotNull([NotNullWhen(true)] object? value)
    {
        return value != null;
    }

    // After this method, value will not be null (if no exception)
    public static void ThrowIfNull([NotNull] object? value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
    }

    // This method may set the out parameter to null
    public static bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        // Implementation
        value = GetValueOrNull(key);
        return value != null;
    }
}

// Usage
string? text = GetSomeText();
if (ValidationHelper.IsNotNull(text))
{
    Console.WriteLine(text.Length); // No warning - compiler knows it's not null
}
```

**Migration strategies**

```csharp
// 1. Gradual migration
#nullable enable warnings // Solo warnings, no errors

// 2. Per-file migration
#nullable disable // En archivos legacy
#nullable restore // Restaurar setting del proyecto

// 3. Suppress specific warnings
#pragma warning disable CS8618 // Non-nullable property must contain non-null value
public string LegacyProperty { get; set; }
#pragma warning restore CS8618
```

**Generic constraints con nullable**

```csharp
// T? is only valid when T is constrained to reference type
public class Container<T> where T : class
{
    public T? Value { get; set; } // Valid
}

// For value types, use Nullable<T>
public class ValueContainer<T> where T : struct
{
    public T? Value { get; set; } // This is Nullable<T>
}
```

### 24. Explica `yield return` y cuándo usarlo

**Contexto** Iterators, lazy evaluation, memory efficiency.

**Respuesta detallada**

**Yield return** crea iterators que generan secuencias lazy (bajo demanda).

**Iterator básico**

```csharp
public static IEnumerable<int> GenerateNumbers(int count)
{
    Console.WriteLine("Generator started");

    for (int i = 0; i < count; i++)
    {
        Console.WriteLine($"Generating {i}");
        yield return i; // Pausa aquí, retorna valor
    }

    Console.WriteLine("Generator finished");
}

// Uso
var numbers = GenerateNumbers(3); // NO ejecuta nada aún
Console.WriteLine("About to iterate");

foreach (var num in numbers) // AQUÍ empieza la ejecución
{
    Console.WriteLine($"Received: {num}");
    // Output interleaved:
    // Generator started
    // Generating 0
    // Received: 0
    // Generating 1
    // Received: 1
    // Generating 2
    // Received: 2
    // Generator finished
}
```

**Diferencia vs return normal**

```csharp
// ❌ Con return normal - toda la memoria de una vez
public static List<int> GetNumbersEager(int count)
{
    var list = new List<int>();
    for (int i = 0; i < count; i++)
    {
        list.Add(i); // Toda la memoria asignada
    }
    return list; // Retorna toda la colección
}

// ✅ Con yield - un elemento a la vez
public static IEnumerable<int> GetNumbersLazy(int count)
{
    for (int i = 0; i < count; i++)
    {
        yield return i; // Solo memoria para un elemento
    }
}

// Diferencia en uso de memoria:
var eager = GetNumbersEager(1_000_000);   // ~4MB inmediatamente
var lazy = GetNumbersLazy(1_000_000);     // ~0 bytes hasta iterar
```

**Yield break**

```csharp
public static IEnumerable<string> ProcessLines(string[] lines)
{
    foreach (var line in lines)
    {
        if (string.IsNullOrEmpty(line))
            yield break; // Termina el iterator completamente

        if (line.StartsWith("#"))
            continue; // Salta este elemento, continúa iterator

        yield return line.ToUpper();
    }
}
```

**Casos de uso avanzados**

```csharp
// 1. Infinite sequences
public static IEnumerable<int> Fibonacci()
{
    int a = 0, b = 1;
    while (true)
    {
        yield return a;
        (a, b) = (b, a + b);
    }
}

// Uso con Take()
var first10Fib = Fibonacci().Take(10).ToList();

// 2. File processing sin cargar todo en memoria
public static IEnumerable<string> ReadLargeFile(string filePath)
{
    using var reader = new StreamReader(filePath);
    string? line;
    while ((line = reader.ReadLine()) != null)
    {
        yield return line;
    }
}

// 3. Combinación de iterators
public static IEnumerable<T> Flatten<T>(IEnumerable<IEnumerable<T>> nested)
{
    foreach (var inner in nested)
    {
        foreach (var item in inner)
        {
            yield return item;
        }
    }
}

// 4. State machine personalizada
public static IEnumerable<string> StateMachine()
{
    Console.WriteLine("State 1");
    yield return "First";

    Console.WriteLine("State 2");
    yield return "Second";

    Console.WriteLine("State 3");
    yield return "Third";
}
```

**Performance considerations**

```csharp
// ✅ BUENO - lazy evaluation
public static IEnumerable<ProcessedData> ProcessLargeDataset(IEnumerable<RawData> data)
{
    foreach (var item in data)
    {
        if (item.IsValid())
        {
            yield return ProcessItem(item); // Solo procesa cuando se necesita
        }
    }
}

// Chain multiple lazy operations
var result = ReadLargeFile("data.txt")
    .Where(line => !string.IsNullOrEmpty(line))
    .Select(line => line.Trim())
    .Where(line => line.StartsWith("DATA:"))
    .Take(100); // Solo procesa hasta encontrar 100 elementos válidos

// ❌ Cuidado con múltiples enumeraciones
var data = GetExpensiveData();
var count = data.Count();        // Primera enumeración
var items = data.ToList();       // Segunda enumeración - duplica trabajo

// ✅ Mejor: materializar una vez si usas múltiples veces
var materializedData = GetExpensiveData().ToList();
var count = materializedData.Count;
var items = materializedData;
```

### 25. ¿Cuáles son las diferencias entre `IEnumerable`, `ICollection`, `IList`?

**Contexto** Collection interfaces, performance characteristics.

**Respuesta detallada**

**Jerarquía de interfaces**

```
IEnumerable<T>
    ↑
ICollection<T>
    ↑
IList<T>
```

**IEnumerable<T>** - Iteración básica:

```csharp
public interface IEnumerable<T>
{
    IEnumerator<T> GetEnumerator();
}

// Funcionalidad básica
IEnumerable<string> names = GetNames();

// ✅ Puede hacer:
foreach (var name in names) { } // Iterar
var count = names.Count();      // LINQ extension (puede ser O(n))
var first = names.First();      // LINQ extension

// ❌ NO puede hacer directamente:
// names.Add("New");            // No método Add
// names[0];                    // No indexing
// names.Count;                 // No property Count
```

**ICollection<T>** - Operaciones de colección:

```csharp
public interface ICollection<T> : IEnumerable<T>
{
    int Count { get; }           // O(1) performance
    bool IsReadOnly { get; }
    void Add(T item);
    bool Remove(T item);
    void Clear();
    bool Contains(T item);
    void CopyTo(T[] array, int arrayIndex);
}

// Funcionalidad extendida
ICollection<string> names = new List<string>();

// ✅ Puede hacer:
names.Add("John");              // Modificar colección
var count = names.Count;        // O(1) - no LINQ overhead
bool hasJohn = names.Contains("John");
names.Remove("John");
names.Clear();
```

**IList<T>** - Acceso por índice:

```csharp
public interface IList<T> : ICollection<T>
{
    T this[int index] { get; set; }  // Indexer
    int IndexOf(T item);
    void Insert(int index, T item);
    void RemoveAt(int index);
}

// Funcionalidad completa
IList<string> names = new List<string> { "John", "Jane", "Bob" };

// ✅ Puede hacer todo:
string first = names[0];        // Acceso por índice O(1)
names[1] = "Janet";            // Modificar por índice
int index = names.IndexOf("Bob"); // Buscar índice
names.Insert(1, "Alice");      // Insertar en posición específica
names.RemoveAt(2);             // Remover por índice
```

**Implementaciones comunes**

```csharp
// List<T> - implementa IList<T>
List<string> list = new() { "a", "b", "c" };
// - Acceso O(1)
// - Inserción al final O(1) amortized
// - Inserción en medio O(n)
// - Búsqueda O(n)

// HashSet<T> - implementa ICollection<T> pero NO IList<T>
HashSet<string> set = new() { "a", "b", "c" };
// - Add/Remove/Contains O(1) average
// - Sin orden específico
// - Sin duplicados
// - NO tiene indexing

// Dictionary<TKey, TValue> - implementa ICollection<KeyValuePair<TKey, TValue>>
Dictionary<string, int> dict = new() { ["a"] = 1, ["b"] = 2 };
// - Acceso por key O(1) average
// - NO implementa IList<T>

// Array - implementa IList<T>
string[] array = { "a", "b", "c" };
// - Tamaño fijo
// - Acceso O(1)
// - IList<T>.Add() throws NotSupportedException
```

**Choosing the right interface**

```csharp
// ✅ Method parameters - usa la interfaz más restrictiva
public void ProcessItems(IEnumerable<string> items) // Solo necesita iterar
{
    foreach (var item in items) { /* process */ }
}

public void AddItems(ICollection<string> collection, IEnumerable<string> newItems)
{
    foreach (var item in newItems)
        collection.Add(item); // Necesita Add()
}

public void SortItems(IList<string> items) // Necesita acceso por índice
{
    // Bubble sort example
    for (int i = 0; i < items.Count - 1; i++)
    {
        for (int j = 0; j < items.Count - i - 1; j++)
        {
            if (string.Compare(items[j], items[j + 1]) > 0)
            {
                (items[j], items[j + 1]) = (items[j + 1], items[j]);
            }
        }
    }
}

// Return types - usa implementación concreta para mejor performance
public List<string> GetUserNames() // Caller puede usar todas las funcionalidades
{
    return new List<string> { "John", "Jane" };
}

// O interfaz si quieres flexibility
public IEnumerable<string> GetUserNamesLazy() // Lazy evaluation
{
    yield return "John";
    yield return "Jane";
}
```

**Performance implications**

```csharp
// Diferentes costs según interfaz
IEnumerable<int> enumerable = Enumerable.Range(1, 1000);
ICollection<int> collection = enumerable.ToList();
IList<int> list = (List<int>)collection;

// Count operation
var count1 = enumerable.Count();  // O(n) - itera toda la secuencia
var count2 = collection.Count;    // O(1) - property
var count3 = list.Count;          // O(1) - property

// Membership testing
bool contains1 = enumerable.Contains(500);  // O(n) - linear search
bool contains2 = collection.Contains(500);  // O(n) - depends on implementation
bool contains3 = list.Contains(500);        // O(n) - List<T> is linear

// Para búsquedas rápidas, usar HashSet<T>
var hashSet = new HashSet<int>(enumerable);
bool contains4 = hashSet.Contains(500);     // O(1) average
```

### 26. ¿Qué son los `Record types` (C# 9+) y cuándo usarlos?

**Contexto** Immutability, value semantics, pattern matching.

**Respuesta detallada**

**Record types** proporcionan una sintaxis concisa para tipos inmutables con value semantics.

**Record básico**

```csharp
// Sintaxis simple
public record Person(string Name, int Age);

// Equivale a una clase con:
public class Person : IEquatable<Person>
{
    public string Name { get; init; }
    public int Age { get; init; }

    public Person(string Name, int Age)
    {
        this.Name = Name;
        this.Age = Age;
    }

    // Value-based equality
    public virtual bool Equals(Person? other) => /* implementation */;
    public override bool Equals(object? obj) => /* implementation */;
    public override int GetHashCode() => /* implementation */;

    // ToString override
    public override string ToString() => $"Person {{ Name = {Name}, Age = {Age} }}";

    // Deconstruction
    public void Deconstruct(out string Name, out int Age) => /* implementation */;
}
```

**Value semantics**

```csharp
var person1 = new Person("John", 25);
var person2 = new Person("John", 25);

// ✅ Records tienen value equality
Console.WriteLine(person1 == person2);    // True
Console.WriteLine(person1.Equals(person2)); // True

// Con clases normales sería referential equality
public class PersonClass
{
    public string Name { get; set; }
    public int Age { get; set; }
}

var class1 = new PersonClass { Name = "John", Age = 25 };
var class2 = new PersonClass { Name = "John", Age = 25 };
Console.WriteLine(class1 == class2);  // False - different references
```

**Immutability con `with` expressions**

```csharp
var person = new Person("John", 25);

// ✅ 'with' crea nueva instancia con cambios
var olderPerson = person with { Age = 26 };
var differentPerson = person with { Name = "Jane", Age = 30 };

Console.WriteLine(person);        // Person { Name = John, Age = 25 }
Console.WriteLine(olderPerson);   // Person { Name = John, Age = 26 }

// person original NO cambia - immutable
```

**Inheritance en records**

```csharp
public record Animal(string Name);
public record Dog(string Name, string Breed) : Animal(Name);
public record Cat(string Name, bool IsIndoor) : Animal(Name);

var dog = new Dog("Buddy", "Golden Retriever");
var cat = new Cat("Whiskers", true);

// Polymorphic behavior
Animal animal = dog;
Console.WriteLine(animal); // Dog { Name = Buddy, Breed = Golden Retriever }

// Pattern matching
string description = animal switch
{
    Dog d => $"Dog named {d.Name} of breed {d.Breed}",
    Cat c => $"Cat named {c.Name}, indoor: {c.IsIndoor}",
    _ => $"Animal named {animal.Name}"
};
```

**Record classes vs record structs (C# 10+)**

```csharp
// Record class (default) - reference type
public record PersonClass(string Name, int Age);

// Record struct - value type
public record struct PersonStruct(string Name, int Age);

// Diferencias
var refRecord1 = new PersonClass("John", 25);
var refRecord2 = refRecord1;
refRecord2 = refRecord2 with { Age = 26 }; // refRecord1 unchanged

var valRecord1 = new PersonStruct("John", 25);
var valRecord2 = valRecord1;
valRecord2 = valRecord2 with { Age = 26 }; // Independent copy
```

**Deconstruction**

```csharp
var person = new Person("John", 25);

// Deconstruct into variables
var (name, age) = person;
Console.WriteLine($"{name} is {age} years old");

// En pattern matching
var description = person switch
{
    ("John", var age) when age >= 18 => "Adult John",
    ("John", var age) => $"Young John, age {age}",
    (var name, var age) => $"{name}, age {age}"
};
```

**Cuándo usar Records**

**✅ Usar Records para**

- **DTOs/POCOs** Data transfer objects
- **Value objects** Inmutables que representan valores
- **Configuration objects** Settings inmutables
- **API responses** Datos que no cambian
- **Event data** Información de eventos

```csharp
// Perfect for DTOs
public record UserDto(int Id, string Name, string Email, DateTime CreatedAt);

// API responses
public record ApiResponse<T>(bool Success, T? Data, string? Error);

// Configuration
public record DatabaseConfig(string ConnectionString, int TimeoutSeconds, bool EnableLogging);
```

**❌ NO usar Records para**

- **Entities con behavior** Clases con mucha lógica de negocio
- **Mutable objects** Objetos que cambian frecuentemente
- **Large objects** Records copian todas las propiedades en `with`
- **Objects con side effects** Constructor con efectos secundarios

### 27. Explica `Span<T>` y `Memory<T>`: ¿Cuáles son sus beneficios?

**Contexto** Performance, memory efficiency, ref structs.

**Respuesta detallada**

**Span<T>** es un ref struct que proporciona una vista de memoria contigua sin allocations.

**Span<T> basics**

```csharp
// Span puede wrappear diferentes tipos de memoria
int[] array = { 1, 2, 3, 4, 5 };

// Span sobre array completo
Span<int> span = array;

// Slice - vista de parte del array
Span<int> slice = array.AsSpan(1, 3); // {2, 3, 4}

// Stack allocated memory
Span<int> stackSpan = stackalloc int[5] { 1, 2, 3, 4, 5 };

// Modificar a través de span
slice[0] = 99; // array ahora es {1, 99, 3, 4, 5}
```

**Memory<T>** - versión heap-friendly de Span<T>:

```csharp
// Memory<T> puede ser stored en heap
public class BufferManager
{
    private Memory<byte> buffer; // ✅ OK - Memory puede ser field
    // private Span<byte> span;  // ❌ ERROR - Span no puede ser field

    public Memory<byte> GetBuffer() => buffer;

    public void ProcessAsync()
    {
        // Memory puede usarse en async methods
        ProcessDataAsync(buffer);
    }
}

// Conversión Memory -> Span
Memory<int> memory = new int[10];
Span<int> span = memory.Span; // Get span from memory
```

**Performance benefits**

```csharp
// ❌ Traditional approach - allocations
public string ProcessString(string input)
{
    var substring = input.Substring(5, 10);    // New string allocation
    var upper = substring.ToUpper();           // Another allocation
    return upper.Trim();                       // Yet another allocation
}

// ✅ Span approach - zero allocations
public string ProcessStringOptimized(string input)
{
    ReadOnlySpan<char> span = input.AsSpan(5, 10); // No allocation

    // Work with span directly
    Span<char> result = stackalloc char[span.Length];

    for (int i = 0; i < span.Length; i++)
    {
        result[i] = char.ToUpper(span[i]);
    }

    return result.ToString(); // Single allocation for final result
}
```

**Working with different memory sources**

```csharp
public static void ProcessMemory()
{
    // Array
    int[] array = { 1, 2, 3, 4, 5 };
    ProcessSpan(array);

    // Stack memory
    Span<int> stackSpan = stackalloc int[] { 1, 2, 3, 4, 5 };
    ProcessSpan(stackSpan);

    // Unmanaged memory
    unsafe
    {
        int* ptr = stackalloc int[5] { 1, 2, 3, 4, 5 };
        Span<int> unsafeSpan = new Span<int>(ptr, 5);
        ProcessSpan(unsafeSpan);
    }
}

public static void ProcessSpan(Span<int> span)
{
    // Same code works for all memory sources
    for (int i = 0; i < span.Length; i++)
    {
        span[i] *= 2;
    }
}
```

**ReadOnlySpan<T>** para datos inmutables:

```csharp
public static class StringHelper
{
    // No allocations para string operations
    public static bool ContainsIgnoreCase(ReadOnlySpan<char> text, ReadOnlySpan<char> value)
    {
        return text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static ReadOnlySpan<char> TrimPrefix(ReadOnlySpan<char> text, ReadOnlySpan<char> prefix)
    {
        return text.StartsWith(prefix, StringComparison.Ordinal)
            ? text.Slice(prefix.Length)
            : text;
    }
}

// Usage - no string allocations
string input = "PrefixActualData";
if (StringHelper.ContainsIgnoreCase(input, "prefix"))
{
    var withoutPrefix = StringHelper.TrimPrefix(input, "Prefix");
    // withoutPrefix es ReadOnlySpan<char> pointing to "ActualData"
}
```

**Limitaciones de Span<T>**

```csharp
public class SpanLimitations
{
    // ❌ No puede ser field en class
    // private Span<int> field;

    // ❌ No puede ser generic type parameter
    // private List<Span<int>> list;

    // ❌ No puede usarse en async methods
    // public async Task ProcessAsync(Span<int> span) { }

    // ✅ Alternativas para async
    public async Task ProcessAsync(Memory<int> memory)
    {
        // Convert to span when needed
        Span<int> span = memory.Span;
        // Process synchronously
    }

    public async Task ProcessAsyncWithCallback(ReadOnlyMemory<char> text)
    {
        await SomeAsyncOperation();

        // Access span after await
        ReadOnlySpan<char> span = text.Span;
        ProcessSpan(span);
    }
}
```

**Real-world example - parsing**

```csharp
// ❌ String-based parsing - many allocations
public static List<int> ParseIntsOld(string input)
{
    var parts = input.Split(',');        // String[] allocation
    var result = new List<int>();

    foreach (var part in parts)
    {
        var trimmed = part.Trim();       // String allocation per part
        if (int.TryParse(trimmed, out int value))
            result.Add(value);
    }
    return result;
}

// ✅ Span-based parsing - minimal allocations
public static List<int> ParseIntsOptimized(ReadOnlySpan<char> input)
{
    var result = new List<int>();

    while (!input.IsEmpty)
    {
        var commaIndex = input.IndexOf(',');
        var part = commaIndex >= 0 ? input.Slice(0, commaIndex) : input;

        // Trim without allocation
        part = part.Trim();

        if (int.TryParse(part, out int value))
            result.Add(value);

        if (commaIndex < 0) break;
        input = input.Slice(commaIndex + 1);
    }
    return result;
}
```

### 28. ¿Qué son las `Extension Methods` y cuáles son sus limitaciones?

**Contexto** LINQ, fluent APIs, static vs instance methods.

**Respuesta detallada**

**Extension Methods** permiten "añadir" métodos a tipos existentes sin modificar su código fuente.

**Sintaxis básica**

```csharp
// Debe ser static class
public static class StringExtensions
{
    // Primer parámetro con 'this' - tipo a extender
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength) + "...";
    }
}

// Usage - como si fuera método del tipo
string email = "user@example.com";
bool isValid = email.IsValidEmail();     // Extension method call
string short = email.Truncate(10);       // Extension method call
```

**LINQ - ejemplo perfecto de extension methods**

```csharp
// LINQ methods son extension methods en IEnumerable<T>
var numbers = new[] { 1, 2, 3, 4, 5 };

var result = numbers
    .Where(x => x % 2 == 0)      // Extension method
    .Select(x => x * x)          // Extension method
    .OrderByDescending(x => x)   // Extension method
    .ToList();                   // Extension method

// Sin extension methods sería:
var result2 = Enumerable.ToList(
    Enumerable.OrderByDescending(
        Enumerable.Select(
            Enumerable.Where(numbers, x => x % 2 == 0),
            x => x * x),
        x => x));
```

**Extension methods con generics**

```csharp
public static class EnumerableExtensions
{
    public static bool IsEmpty<T>(this IEnumerable<T> source)
    {
        return !source.Any();
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        return source.Where(item => item != null)!;
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}

// Usage
var items = new List<string> { "a", null, "b", "c" };
items.WhereNotNull().ForEach(Console.WriteLine);
```

**Extension methods para custom types**

```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
}

public static class PersonExtensions
{
    public static bool IsAdult(this Person person)
    {
        return person.Age >= 18;
    }

    public static Person WithAge(this Person person, int age)
    {
        // Immutable update pattern
        return new Person
        {
            Name = person.Name,
            Age = age,
            BirthDate = person.BirthDate
        };
    }

    public static TimeSpan GetAge(this Person person)
    {
        return DateTime.Now - person.BirthDate;
    }
}

// Fluent API style
var person = new Person { Name = "John", Age = 25, BirthDate = DateTime.Now.AddYears(-25) };
var updatedPerson = person.WithAge(26);
bool isAdult = updatedPerson.IsAdult();
```

**Limitaciones importantes**

**1. Resolution order**

```csharp
public class MyClass
{
    public void Method() => Console.WriteLine("Instance method");
}

public static class MyClassExtensions
{
    public static void Method(this MyClass obj) => Console.WriteLine("Extension method");
}

var obj = new MyClass();
obj.Method(); // "Instance method" - instance methods have priority
```

**2. No pueden acceder a private members**

```csharp
public class SecretClass
{
    private string secret = "hidden";
    internal string Internal = "internal";
}

public static class SecretExtensions
{
    public static void TryAccess(this SecretClass obj)
    {
        // Console.WriteLine(obj.secret);    // ❌ Cannot access private
        Console.WriteLine(obj.Internal);     // ✅ Can access internal (same assembly)
    }
}
```

**3. No virtual dispatch**

```csharp
public class Base { }
public class Derived : Base { }

public static class Extensions
{
    public static void Method(this Base obj) => Console.WriteLine("Base extension");
    public static void Method(this Derived obj) => Console.WriteLine("Derived extension");
}

Base obj = new Derived();
obj.Method(); // "Base extension" - resolved at compile time, not runtime
```

**4. Namespace requirements**

```csharp
namespace MyApp.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNumeric(this string str) => /* implementation */;
    }
}

namespace MyApp.Other
{
    public class Test
    {
        public void TestMethod()
        {
            string value = "123";
            // value.IsNumeric(); // ❌ Not available without using directive
        }
    }
}

namespace MyApp.Other
{
    using MyApp.Extensions; // ✅ Now extension methods are available

    public class Test
    {
        public void TestMethod()
        {
            string value = "123";
            bool isNum = value.IsNumeric(); // ✅ Works
        }
    }
}
```

**Best practices**

```csharp
// ✅ Good extension methods
public static class GoodExtensions
{
    // Clear, focused purpose
    public static bool IsWeekend(this DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday ||
               date.DayOfWeek == DayOfWeek.Sunday;
    }

    // Null-safe operations
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    // Enhance existing APIs
    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        return await task.WaitAsync(cts.Token);
    }
}

// ❌ Questionable extension methods
public static class QuestionableExtensions
{
    // Too complex for extension method
    public static void ProcessComplexBusinessLogic(this Customer customer) { }

    // Should probably be instance method
    public static void Save(this Entity entity) { }

    // Unclear benefit over static method
    public static int Add(this int a, int b) => a + b;
}
```

### 29. Explica el patrón `IDisposable` y `using statements`

**Contexto** Resource management, finalizers, best practices.

**Respuesta detallada**

**IDisposable** proporciona un mecanismo para liberar recursos no managed de manera determinística.

**IDisposable básico**

```csharp
public interface IDisposable
{
    void Dispose();
}

// Implementación simple
public class SimpleResource : IDisposable
{
    private bool disposed = false;

    public void Dispose()
    {
        if (!disposed)
        {
            // Liberar recursos
            Console.WriteLine("Resource disposed");
            disposed = true;
        }
    }

    public void DoWork()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(SimpleResource));

        Console.WriteLine("Doing work...");
    }
}
```

**Using statement** - ensures disposal:

```csharp
// ✅ Using statement garantiza Dispose()
using (var resource = new SimpleResource())
{
    resource.DoWork();
} // Dispose() called automatically, even if exception occurs

// ✅ Using declaration (C# 8+) - más conciso
using var resource2 = new SimpleResource();
resource2.DoWork();
// Dispose() called at end of scope

// ❌ Sin using - manual disposal required
var resource3 = new SimpleResource();
try
{
    resource3.DoWork();
}
finally
{
    resource3.Dispose(); // Must remember to call
}
```

**Dispose pattern completo**

```csharp
public class ManagedAndUnmanagedResource : IDisposable
{
    // Managed resources
    private FileStream? managedResource;

    // Unmanaged resources
    private IntPtr unmanagedHandle;

    private bool disposed = false;

    public ManagedAndUnmanagedResource()
    {
        managedResource = new FileStream("temp.txt", FileMode.Create);
        unmanagedHandle = AllocateUnmanagedHandle(); // Hypothetical unmanaged resource
    }

    // Public Dispose method
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this); // No need for finalizer
    }

    // Protected virtual method for inheritance
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                managedResource?.Dispose();
                managedResource = null;
            }

            // Always dispose unmanaged resources
            if (unmanagedHandle != IntPtr.Zero)
            {
                FreeUnmanagedHandle(unmanagedHandle);
                unmanagedHandle = IntPtr.Zero;
            }

            disposed = true;
        }
    }

    // Finalizer - only if you have unmanaged resources
    ~ManagedAndUnmanagedResource()
    {
        Dispose(disposing: false);
    }

    // Helper methods for demo
    private IntPtr AllocateUnmanagedHandle() => new IntPtr(123);
    private void FreeUnmanagedHandle(IntPtr handle) { }
}
```

**IAsyncDisposable** (C# 8+) para recursos async:

```csharp
public class AsyncResource : IAsyncDisposable
{
    private bool disposed = false;

    public async ValueTask DisposeAsync()
    {
        if (!disposed)
        {
            // Async cleanup
            await CleanupAsync();
            disposed = true;
        }
    }

    private async Task CleanupAsync()
    {
        // Simulate async cleanup
        await Task.Delay(100);
        Console.WriteLine("Async cleanup completed");
    }
}

// Usage with await using
await using var asyncResource = new AsyncResource();
// DisposeAsync() called automatically
```

**Multiple disposables**

```csharp
// ❌ Nested using - gets messy
using (var resource1 = new SimpleResource())
using (var resource2 = new SimpleResource())
using (var resource3 = new SimpleResource())
{
    // Use resources
}

// ✅ Using declarations - cleaner
using var resource1 = new SimpleResource();
using var resource2 = new SimpleResource();
using var resource3 = new SimpleResource();
// All disposed in reverse order at end of scope

// ✅ CompositeDisposable for dynamic collections
public class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> disposables = new();
    private bool disposed = false;

    public void Add(IDisposable disposable)
    {
        if (disposed) throw new ObjectDisposedException(nameof(CompositeDisposable));
        disposables.Add(disposable);
    }

    public void Dispose()
    {
        if (!disposed)
        {
            // Dispose in reverse order
            for (int i = disposables.Count - 1; i >= 0; i--)
            {
                try
                {
                    disposables[i].Dispose();
                }
                catch (Exception ex)
                {
                    // Log but don't throw in Dispose
                    Console.WriteLine($"Error disposing: {ex.Message}");
                }
            }
            disposables.Clear();
            disposed = true;
        }
    }
}
```

**Common patterns and best practices**

```csharp
// ✅ Dispose pattern for inherited classes
public abstract class BaseResource : IDisposable
{
    private bool disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed && disposing)
        {
            DisposeManaged();
            disposed = true;
        }
    }

    protected abstract void DisposeManaged();

    // Helper to check if disposed
    protected void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().Name);
    }
}

public class DerivedResource : BaseResource
{
    private FileStream? fileStream;

    protected override void DisposeManaged()
    {
        fileStream?.Dispose();
        fileStream = null;
    }

    public void DoWork()
    {
        ThrowIfDisposed();
        // Work with fileStream
    }
}

// ✅ Factory with automatic disposal
public class ResourceFactory
{
    public static T CreateAndUse<T>(Func<T> factory, Action<T> action) where T : IDisposable
    {
        using var resource = factory();
        action(resource);
        return resource;
    }
}

// Usage
ResourceFactory.CreateAndUse(
    () => new FileStream("temp.txt", FileMode.Create),
    stream => stream.Write(Encoding.UTF8.GetBytes("Hello"))
);
```

**Anti-patterns to avoid**

```csharp
// ❌ Disposing in finalizer without dispose pattern
public class BadResource : IDisposable
{
    ~BadResource()
    {
        Dispose(); // NEVER call public Dispose from finalizer
    }

    public void Dispose()
    {
        // This could cause issues if called from finalizer
        managedResource.Dispose(); // Managed resource might be already finalized
    }
}

// ❌ Not making Dispose idempotent
public class BadDisposable : IDisposable
{
    public void Dispose()
    {
        resource.Dispose(); // Will throw if called multiple times
    }
}

// ❌ Throwing exceptions in Dispose
public class ThrowingDisposable : IDisposable
{
    public void Dispose()
    {
        throw new Exception("Never throw in Dispose!");
    }
}
```

### 30. ¿Cuáles son los diferentes tipos de casting en C#?

**Contexto** Implicit, explicit, `as`, `is`, pattern matching.

**Respuesta detallada**

**Implicit casting** - conversiones automáticas seguras:

```csharp
// Numeric conversions - no data loss
int intValue = 42;
long longValue = intValue;        // int -> long (safe)
double doubleValue = intValue;    // int -> double (safe)

// Reference type inheritance
string text = "Hello";
object obj = text;                // string -> object (safe)

// Custom implicit conversions
public struct Temperature
{
    public double Celsius { get; }

    public Temperature(double celsius) => Celsius = celsius;

    // Define implicit conversion
    public static implicit operator double(Temperature temp) => temp.Celsius;
    public static implicit operator Temperature(double celsius) => new(celsius);
}

Temperature temp = 25.5;          // double -> Temperature (implicit)
double value = temp;              // Temperature -> double (implicit)
```

**Explicit casting** - conversiones que pueden perder datos:

```csharp
// Numeric conversions with potential data loss
double doubleValue = 42.7;
int intValue = (int)doubleValue;  // 42 - truncates decimal part

long longValue = 3000000000L;
int intValue2 = (int)longValue;   // Overflow - undefined behavior

// Reference type casting
object obj = "Hello";
string text = (string)obj;        // object -> string (explicit cast)

// Custom explicit conversions
public struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    // Explicit conversion - can lose currency information
    public static explicit operator decimal(Money money) => money.Amount;
}

var money = new Money(100.50m, "USD");
decimal amount = (decimal)money;  // Explicit cast required
```

**`as` operator** - safe casting que retorna null:

```csharp
object obj = "Hello World";

// ✅ Safe casting with 'as'
string? text = obj as string;     // Returns "Hello World"
int? number = obj as int?;        // Returns null (not an int)

if (text != null)
{
    Console.WriteLine(text.Length);
}

// Equivalent to:
string? text2 = (obj is string) ? (string)obj : null;

// Common pattern with null-conditional
var length = (obj as string)?.Length ?? 0;

// ❌ 'as' cannot be used with value types directly
// int number = obj as int;       // Compile error
int? number2 = obj as int?;       // ✅ OK with nullable value type
```

**`is` operator** - type checking:

```csharp
object obj = 42;

// Basic type checking
if (obj is int)
{
    int value = (int)obj;  // Safe to cast after 'is' check
    Console.WriteLine(value);
}

// Pattern matching with 'is' (C# 7+)
if (obj is int intValue)
{
    Console.WriteLine(intValue); // intValue is automatically declared and cast
}

// Complex patterns (C# 8+)
object data = GetSomeData();

var result = data switch
{
    int i when i > 0 => $"Positive integer: {i}",
    int i when i < 0 => $"Negative integer: {i}",
    int => "Zero",
    string s when s.Length > 0 => $"Non-empty string: {s}",
    string => "Empty string",
    null => "Null value",
    _ => "Unknown type"
};
```

**Pattern matching avanzado** (C# 8+):

```csharp
// Property patterns
public record Person(string Name, int Age);

public static string DescribePerson(Person person) => person switch
{
    { Age: < 18 } => "Minor",
    { Age: >= 18, Age: < 65 } => "Adult",
    { Age: >= 65 } => "Senior",
    _ => "Unknown"
};

// Tuple patterns
public static string GetQuadrant(int x, int y) => (x, y) switch
{
    ( > 0, > 0) => "First quadrant",
    ( < 0, > 0) => "Second quadrant",
    ( < 0, < 0) => "Third quadrant",
    ( > 0, < 0) => "Fourth quadrant",
    (0, 0) => "Origin",
    _ => "On axis"
};

// Relational patterns
public static string ClassifyTemperature(double temp) => temp switch
{
    < 0 => "Freezing",
    >= 0 and < 20 => "Cold",
    >= 20 and < 30 => "Warm",
    >= 30 => "Hot"
};
```

**Nullable value types casting**

```csharp
int? nullableInt = 42;

// Safe casting
if (nullableInt.HasValue)
{
    int value = nullableInt.Value;  // Safe
}

// Or using pattern matching
if (nullableInt is int actualValue)
{
    Console.WriteLine(actualValue);
}

// Null-coalescing
int value2 = nullableInt ?? 0;     // Default value if null

// ❌ Dangerous - can throw
int value3 = nullableInt.Value;    // NullReferenceException if null
```

**Boxing/Unboxing casting**

```csharp
// Boxing - value type to object
int value = 42;
object boxed = value;             // Boxing occurs

// Unboxing - object to value type
int unboxed = (int)boxed;         // Explicit unboxing required

// ❌ Wrong type unboxing
try
{
    long wrongType = (long)boxed; // InvalidCastException - boxed as int, not long
}
catch (InvalidCastException)
{
    // Must unbox to original type first
    long correct = (long)(int)boxed; // ✅ Correct
}

// Safe unboxing with 'as'
int? safeUnbox = boxed as int?;   // Returns 42
long? safeWrong = boxed as long?; // Returns null
```

**Performance considerations**

```csharp
// Performance comparison
object obj = "test string";

// ✅ Fastest - direct type check
if (obj.GetType() == typeof(string))
{
    var str = (string)obj;
}

// ✅ Fast - 'is' with pattern matching
if (obj is string str)
{
    // Use str directly
}

// ✅ Good - 'as' operator
var str2 = obj as string;
if (str2 != null)
{
    // Use str2
}

// ❌ Slower - try/catch for casting
try
{
    var str3 = (string)obj;
}
catch (InvalidCastException)
{
    // Handle error
}
```

**Best practices**

- Usa `is` para type checking con pattern matching
- Usa `as` para safe casting cuando null es aceptable
- Prefer implicit conversions en tus tipos custom
- Evita casting en hot paths - usa generics cuando sea posible
- Siempre valida antes de casting desde `object`
- Usa pattern matching para código más expresivo y seguro

---

## 🏗️ **ARQUITECTURA Y PATRONES (15 preguntas)**

### 31. Explica el patrón Dependency Injection y sus beneficios

**Contexto** IoC containers, lifetime management, testability.

**Respuesta detallada**

**Dependency Injection (DI)** es un patrón que implementa Inversion of Control (IoC) para gestionar dependencias entre objetos.

**Sin DI - Tight Coupling**

```csharp
// ❌ Clase con dependencias hardcodeadas
public class OrderService
{
    private readonly EmailService emailService;
    private readonly DatabaseRepository repository;

    public OrderService()
    {
        // Dependencias hardcodeadas - difícil testing
        emailService = new EmailService();
        repository = new DatabaseRepository("connectionString");
    }

    public void ProcessOrder(Order order)
    {
        repository.Save(order);
        emailService.SendConfirmation(order.CustomerEmail);
    }
}
```

**Con DI - Loose Coupling**

```csharp
// ✅ Interfaces para abstraer dependencias
public interface IEmailService
{
    Task SendConfirmationAsync(string email, Order order);
}

public interface IOrderRepository
{
    Task SaveAsync(Order order);
    Task<Order> GetByIdAsync(int id);
}

// ✅ Clase recibe dependencias via constructor
public class OrderService
{
    private readonly IEmailService emailService;
    private readonly IOrderRepository repository;
    private readonly ILogger<OrderService> logger;

    public OrderService(
        IEmailService emailService,
        IOrderRepository repository,
        ILogger<OrderService> logger)
    {
        this.emailService = emailService;
        this.repository = repository;
        this.logger = logger;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        try
        {
            await repository.SaveAsync(order);
            await emailService.SendConfirmationAsync(order.CustomerEmail, order);
            logger.LogInformation("Order {OrderId} processed successfully", order.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
            throw;
        }
    }
}
```

**Configuración de DI Container (.NET Core)**

```csharp
// Program.cs / Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register dependencies
    services.AddScoped<IOrderService, OrderService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IOrderRepository, DatabaseOrderRepository>();

    // Different lifetimes
    services.AddSingleton<IConfiguration>(configuration);
    services.AddTransient<IValidator<Order>, OrderValidator>();

    // Conditional registration
    if (Environment.IsDevelopment())
    {
        services.AddScoped<IEmailService, MockEmailService>();
    }

    // Factory pattern with DI
    services.AddScoped<Func<string, IPaymentProcessor>>(provider => paymentType =>
    {
        return paymentType switch
        {
            "credit" => provider.GetService<CreditCardProcessor>(),
            "paypal" => provider.GetService<PayPalProcessor>(),
            _ => throw new ArgumentException($"Unknown payment type: {paymentType}")
        };
    });
}
```

**Beneficios principales**

**1. Testability**

```csharp
// Unit test con mocks
[Test]
public async Task ProcessOrder_Should_SaveAndSendEmail()
{
    // Arrange
    var mockEmail = new Mock<IEmailService>();
    var mockRepository = new Mock<IOrderRepository>();
    var mockLogger = new Mock<ILogger<OrderService>>();

    var service = new OrderService(mockEmail.Object, mockRepository.Object, mockLogger.Object);
    var order = new Order { Id = 1, CustomerEmail = "test@example.com" };

    // Act
    await service.ProcessOrderAsync(order);

    // Assert
    mockRepository.Verify(r => r.SaveAsync(order), Times.Once);
    mockEmail.Verify(e => e.SendConfirmationAsync(order.CustomerEmail, order), Times.Once);
}
```

**2. Flexibility & Configuration**

```csharp
// Diferentes implementaciones según ambiente
public class ProductionEmailService : IEmailService
{
    public async Task SendConfirmationAsync(string email, Order order)
    {
        // Real email sending via SendGrid/SMTP
    }
}

public class DevelopmentEmailService : IEmailService
{
    public async Task SendConfirmationAsync(string email, Order order)
    {
        // Log to console instead of sending email
        Console.WriteLine($"Would send email to {email} for order {order.Id}");
    }
}
```

**3. Single Responsibility**

```csharp
// Cada clase tiene una responsabilidad específica
public class OrderValidator : IValidator<Order>
{
    public ValidationResult Validate(Order order) { /* validation logic */ }
}

public class PaymentProcessor : IPaymentProcessor
{
    public async Task<PaymentResult> ProcessPaymentAsync(Payment payment) { /* payment logic */ }
}

public class InventoryService : IInventoryService
{
    public async Task ReserveItemsAsync(IEnumerable<OrderItem> items) { /* inventory logic */ }
}
```

**Patrones avanzados con DI**

```csharp
// Decorator pattern
services.AddScoped<IOrderService, OrderService>();
services.Decorate<IOrderService, CachedOrderService>();
services.Decorate<IOrderService, LoggingOrderService>();

public class LoggingOrderService : IOrderService
{
    private readonly IOrderService decorated;
    private readonly ILogger logger;

    public LoggingOrderService(IOrderService decorated, ILogger logger)
    {
        this.decorated = decorated;
        this.logger = logger;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        logger.LogInformation("Processing order {OrderId}", order.Id);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await decorated.ProcessOrderAsync(order);
            logger.LogInformation("Order {OrderId} processed in {ElapsedMs}ms",
                order.Id, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
            throw;
        }
    }
}
```

### 32. ¿Cuáles son los diferentes lifetime scopes en DI? (Singleton, Transient, Scoped)

**Contexto** Memory leaks, thread safety, web applications.

**Respuesta detallada**

Los **lifetime scopes** controlan cuándo y cómo se crean y liberan las instancias de los servicios.

**Transient Lifetime**

```csharp
// Nueva instancia cada vez que se solicita
services.AddTransient<IEmailService, EmailService>();

public class EmailService : IEmailService
{
    private readonly Guid instanceId = Guid.NewGuid();

    public void SendEmail(string message)
    {
        Console.WriteLine($"Email sent from instance: {instanceId}");
    }
}

// Uso
public class Controller
{
    public Controller(IEmailService email1, IEmailService email2)
    {
        // email1 y email2 son instancias DIFERENTES
        email1.SendEmail("Test1"); // Instance: abc-123
        email2.SendEmail("Test2"); // Instance: def-456
    }
}
```

**Singleton Lifetime**

```csharp
// Una sola instancia para toda la aplicación
services.AddSingleton<IConfiguration, Configuration>();
services.AddSingleton<ICacheService, MemoryCacheService>();

public class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, object> cache = new();
    private readonly Guid instanceId = Guid.NewGuid();

    public void Set(string key, object value)
    {
        cache[key] = value;
        Console.WriteLine($"Cache set from instance: {instanceId}");
    }

    public T Get<T>(string key)
    {
        Console.WriteLine($"Cache get from instance: {instanceId}");
        return cache.TryGetValue(key, out var value) ? (T)value : default(T);
    }
}

// ✅ Thread-safe implementation requerida para Singleton
public class ThreadSafeSingleton
{
    private readonly object lockObject = new object();
    private volatile bool initialized = false;

    public void Initialize()
    {
        if (!initialized)
        {
            lock (lockObject)
            {
                if (!initialized)
                {
                    // Expensive initialization
                    initialized = true;
                }
            }
        }
    }
}
```

**Scoped Lifetime** (Web Applications):

```csharp
// Una instancia por request HTTP
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<IDbContext, ApplicationDbContext>();

public class OrderService
{
    private readonly ApplicationDbContext context;
    private readonly Guid instanceId = Guid.NewGuid();

    public OrderService(ApplicationDbContext context)
    {
        this.context = context;
        Console.WriteLine($"OrderService created: {instanceId}");
    }

    public async Task ProcessOrderAsync(Order order)
    {
        // Mismo contexto durante todo el request
        context.Orders.Add(order);
        await context.SaveChangesAsync();
    }
}

// En el mismo HTTP request
public class OrderController : ControllerBase
{
    public OrderController(IOrderService service1, IOrderService service2)
    {
        // service1 y service2 son la MISMA instancia durante este request
    }
}
```

**Lifetime comparisons y use cases**

```csharp
public class LifetimeDemo
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ✅ Transient - Use cases
        services.AddTransient<IValidator<Order>, OrderValidator>();     // Stateless, lightweight
        services.AddTransient<IMapper<Order, OrderDto>, OrderMapper>(); // Pure functions
        services.AddTransient<IEmailSender, EmailSender>();             // Short-lived operations

        // ✅ Scoped - Use cases
        services.AddScoped<IDbContext, ApplicationDbContext>();         // Per-request database context
        services.AddScoped<IUserService, UserService>();                // User-specific operations
        services.AddScoped<IUnitOfWork, UnitOfWork>();                  // Transaction boundaries

        // ✅ Singleton - Use cases
        services.AddSingleton<IConfiguration>(configuration);           // Configuration data
        services.AddSingleton<ICacheService, MemoryCacheService>();     // Global cache
        services.AddSingleton<ILogger<T>>();                            // Logging infrastructure
        services.AddSingleton<IHostedService, BackgroundTaskService>(); // Background services
    }
}
```

**Memory leak scenarios**

```csharp
// ❌ MEMORY LEAK: Singleton holding references to Scoped/Transient
services.AddSingleton<ProblemService>();      // Lives forever
services.AddScoped<ScopedService>();          // Lives per request

public class ProblemService  // Singleton
{
    private readonly List<ScopedService> services = new();

    public void AddService(ScopedService service)
    {
        services.Add(service); // ❌ Singleton keeping reference to Scoped
                               // ScopedService never gets garbage collected
    }
}

// ✅ SOLUTION: Use factory pattern
public class FixedService  // Singleton
{
    private readonly IServiceProvider serviceProvider;

    public FixedService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void DoWork()
    {
        using var scope = serviceProvider.CreateScope();
        var scopedService = scope.ServiceProvider.GetRequiredService<ScopedService>();
        // Use scopedService
        // Scope disposes automatically, releasing ScopedService
    }
}
```

**Custom scopes**

```csharp
// Creating custom scopes
public class CustomScopeService
{
    private readonly IServiceProvider serviceProvider;

    public CustomScopeService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task ProcessBatchAsync(IEnumerable<Order> orders)
    {
        foreach (var batch in orders.Chunk(100))
        {
            // New scope per batch
            using var scope = serviceProvider.CreateScope();
            var batchProcessor = scope.ServiceProvider.GetRequiredService<IBatchProcessor>();

            await batchProcessor.ProcessAsync(batch);
            // Scope disposed, releasing all scoped services
        }
    }
}
```

**Performance implications**

```csharp
// Performance test example
[Benchmark]
public class LifetimePerformance
{
    private IServiceProvider serviceProvider;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddTransient<TransientService>();
        services.AddScoped<ScopedService>();
        services.AddSingleton<SingletonService>();
        serviceProvider = services.BuildServiceProvider();
    }

    [Benchmark]
    public void GetTransient()
    {
        var service = serviceProvider.GetService<TransientService>(); // ~50ns
    }

    [Benchmark]
    public void GetScoped()
    {
        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetService<ScopedService>(); // ~100ns
    }

    [Benchmark]
    public void GetSingleton()
    {
        var service = serviceProvider.GetService<SingletonService>(); // ~10ns
    }
}
```

### 33. ¿Qué es SOLID? Explica cada principio con ejemplos en C#

**Contexto** Design principles, maintainability, extensibility.

**Respuesta detallada**

**SOLID** son cinco principios de diseño para crear software mantenible, extensible y testeable.

**S - Single Responsibility Principle (SRP)**
_Una clase debe tener una sola razón para cambiar._

```csharp
// ❌ VIOLA SRP - múltiples responsabilidades
public class User
{
    public string Name { get; set; }
    public string Email { get; set; }

    // Responsabilidad 1: Validación
    public bool IsValidEmail()
    {
        return Email.Contains("@");
    }

    // Responsabilidad 2: Persistencia
    public void SaveToDatabase()
    {
        // Database logic
    }

    // Responsabilidad 3: Notificación
    public void SendWelcomeEmail()
    {
        // Email logic
    }
}

// ✅ CUMPLE SRP - responsabilidades separadas
public class User  // Solo representa datos del usuario
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class UserValidator  // Solo validación
{
    public bool IsValidEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains("@");
    }

    public ValidationResult Validate(User user)
    {
        var result = new ValidationResult();

        if (string.IsNullOrEmpty(user.Name))
            result.AddError("Name is required");

        if (!IsValidEmail(user.Email))
            result.AddError("Invalid email format");

        return result;
    }
}

public class UserRepository  // Solo persistencia
{
    public async Task SaveAsync(User user)
    {
        // Database logic
    }

    public async Task<User> GetByIdAsync(int id)
    {
        // Database retrieval logic
    }
}

public class EmailService  // Solo notificaciones
{
    public async Task SendWelcomeEmailAsync(User user)
    {
        // Email sending logic
    }
}
```

**O - Open/Closed Principle (OCP)**
_Las clases deben estar abiertas para extensión pero cerradas para modificación._

```csharp
// ❌ VIOLA OCP - necesita modificar clase para nuevos tipos
public class DiscountCalculator
{
    public decimal Calculate(decimal amount, CustomerType customerType)
    {
        return customerType switch
        {
            CustomerType.Regular => amount,
            CustomerType.Premium => amount * 0.9m,
            CustomerType.VIP => amount * 0.8m,
            // ❌ Para agregar nuevo tipo, hay que modificar esta clase
            _ => amount
        };
    }
}

// ✅ CUMPLE OCP - extensible sin modificación
public abstract class DiscountStrategy
{
    public abstract decimal Apply(decimal amount);
}

public class RegularCustomerDiscount : DiscountStrategy
{
    public override decimal Apply(decimal amount) => amount;
}

public class PremiumCustomerDiscount : DiscountStrategy
{
    public override decimal Apply(decimal amount) => amount * 0.9m;
}

public class VIPCustomerDiscount : DiscountStrategy
{
    public override decimal Apply(decimal amount) => amount * 0.8m;
}

// Nueva estrategia sin modificar código existente
public class SuperVIPCustomerDiscount : DiscountStrategy
{
    public override decimal Apply(decimal amount) => amount * 0.7m;
}

public class DiscountCalculator
{
    public decimal Calculate(decimal amount, DiscountStrategy strategy)
    {
        return strategy.Apply(amount);
    }
}
```

**L - Liskov Substitution Principle (LSP)**
_Los objetos de una superclase deben ser reemplazables por objetos de sus subclases sin alterar el funcionamiento._

```csharp
// ❌ VIOLA LSP - el subtipo cambia el comportamiento esperado
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }

    public int Area => Width * Height;
}

public class Square : Rectangle
{
    public override int Width
    {
        get => base.Width;
        set
        {
            base.Width = value;
            base.Height = value; // ❌ Cambia comportamiento inesperado
        }
    }

    public override int Height
    {
        get => base.Height;
        set
        {
            base.Width = value;  // ❌ Viola expectativas
            base.Height = value;
        }
    }
}

// Problema en uso
public void TestRectangle(Rectangle rectangle)
{
    rectangle.Width = 5;
    rectangle.Height = 4;

    // Se espera área = 20, pero con Square será 16
    Assert.AreEqual(20, rectangle.Area); // ❌ Falla con Square
}

// ✅ CUMPLE LSP - diseño correcto
public abstract class Shape
{
    public abstract int Area { get; }
}

public class Rectangle : Shape
{
    public int Width { get; set; }
    public int Height { get; set; }

    public override int Area => Width * Height;
}

public class Square : Shape
{
    public int Side { get; set; }

    public override int Area => Side * Side;
}

// Factory para crear shapes apropiadas
public static class ShapeFactory
{
    public static Shape CreateRectangle(int width, int height)
    {
        return width == height
            ? new Square { Side = width }
            : new Rectangle { Width = width, Height = height };
    }
}
```

**I - Interface Segregation Principle (ISP)**
_Los clientes no deben depender de interfaces que no utilizan._

```csharp
// ❌ VIOLA ISP - interfaz muy grande
public interface IWorker
{
    void Work();
    void Eat();      // No todos los workers comen
    void Sleep();    // No todos los workers duermen
    void Code();     // Solo programadores
    void Design();   // Solo diseñadores
}

public class Programmer : IWorker
{
    public void Work() { /* implementation */ }
    public void Eat() { /* implementation */ }
    public void Sleep() { /* implementation */ }
    public void Code() { /* implementation */ }

    public void Design()
    {
        throw new NotImplementedException(); // ❌ No necesita esto
    }
}

public class Robot : IWorker
{
    public void Work() { /* implementation */ }
    public void Code() { /* implementation */ }

    public void Eat()
    {
        throw new NotImplementedException(); // ❌ Robots no comen
    }

    public void Sleep()
    {
        throw new NotImplementedException(); // ❌ Robots no duermen
    }

    public void Design()
    {
        throw new NotImplementedException(); // ❌ No diseña
    }
}

// ✅ CUMPLE ISP - interfaces específicas y pequeñas
public interface IWorkable
{
    void Work();
}

public interface IFeedable
{
    void Eat();
}

public interface ISleepable
{
    void Sleep();
}

public interface IProgrammable
{
    void Code();
}

public interface IDesignable
{
    void Design();
}

// Implementaciones específicas
public class Programmer : IWorkable, IFeedable, ISleepable, IProgrammable
{
    public void Work() { /* implementation */ }
    public void Eat() { /* implementation */ }
    public void Sleep() { /* implementation */ }
    public void Code() { /* implementation */ }
}

public class Robot : IWorkable, IProgrammable
{
    public void Work() { /* implementation */ }
    public void Code() { /* implementation */ }
    // Solo implementa lo que necesita
}

public class Designer : IWorkable, IFeedable, ISleepable, IDesignable
{
    public void Work() { /* implementation */ }
    public void Eat() { /* implementation */ }
    public void Sleep() { /* implementation */ }
    public void Design() { /* implementation */ }
}
```

**D - Dependency Inversion Principle (DIP)**
_Los módulos de alto nivel no deben depender de módulos de bajo nivel. Ambos deben depender de abstracciones._

```csharp
// ❌ VIOLA DIP - clase de alto nivel depende de implementación concreta
public class OrderService  // Alto nivel
{
    private readonly MySqlRepository repository;    // ❌ Dependencia concreta
    private readonly SmtpEmailService emailService; // ❌ Dependencia concreta

    public OrderService()
    {
        repository = new MySqlRepository();          // ❌ Acoplamiento fuerte
        emailService = new SmtpEmailService();      // ❌ Acoplamiento fuerte
    }

    public void ProcessOrder(Order order)
    {
        repository.Save(order);
        emailService.SendConfirmation(order.CustomerEmail);
    }
}

// ✅ CUMPLE DIP - depende de abstracciones
public interface IOrderRepository  // Abstracción
{
    Task SaveAsync(Order order);
}

public interface IEmailService  // Abstracción
{
    Task SendConfirmationAsync(string email);
}

public class OrderService  // Alto nivel
{
    private readonly IOrderRepository repository;    // ✅ Depende de abstracción
    private readonly IEmailService emailService;     // ✅ Depende de abstracción

    public OrderService(IOrderRepository repository, IEmailService emailService)
    {
        this.repository = repository;
        this.emailService = emailService;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        await repository.SaveAsync(order);
        await emailService.SendConfirmationAsync(order.CustomerEmail);
    }
}

// Implementaciones concretas (bajo nivel)
public class MySqlOrderRepository : IOrderRepository  // Bajo nivel
{
    public async Task SaveAsync(Order order)
    {
        // MySQL specific implementation
    }
}

public class PostgreSqlOrderRepository : IOrderRepository  // Bajo nivel
{
    public async Task SaveAsync(Order order)
    {
        // PostgreSQL specific implementation
    }
}

public class SmtpEmailService : IEmailService  // Bajo nivel
{
    public async Task SendConfirmationAsync(string email)
    {
        // SMTP implementation
    }
}

public class SendGridEmailService : IEmailService  // Bajo nivel
{
    public async Task SendConfirmationAsync(string email)
    {
        // SendGrid implementation
    }
}

// DI Configuration
services.AddScoped<IOrderRepository, MySqlOrderRepository>();
services.AddScoped<IEmailService, SendGridEmailService>();
services.AddScoped<OrderService>();
```

**Beneficios de aplicar SOLID**

- **Mantenibilidad** Código más fácil de modificar
- **Testabilidad** Fácil crear unit tests con mocks
- **Extensibilidad** Agregar funcionalidad sin romper código existente
- **Reusabilidad** Componentes más reutilizables
- **Readabilidad** Código más claro y comprensible

### 34. Explica los patrones Repository y Unit of Work

**Contexto** Data access abstraction, testing, entity tracking.

**Respuesta detallada**

**Repository Pattern** abstrae el acceso a datos, proporcionando una interfaz similar a una colección en memoria.

**Repository básico**

```csharp
// Interfaz del repositorio
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

// Implementación genérica
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext context;
    protected readonly DbSet<T> dbSet;

    public Repository(ApplicationDbContext context)
    {
        this.context = context;
        this.dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public virtual async Task UpdateAsync(T entity)
    {
        dbSet.Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
    }

    public virtual async Task DeleteAsync(T entity)
    {
        if (context.Entry(entity).State == EntityState.Detached)
        {
            dbSet.Attach(entity);
        }
        dbSet.Remove(entity);
    }
}
```

**Repositorios específicos**

```csharp
// Interfaz específica para Order
public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
    Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Order?> GetOrderWithItemsAsync(int orderId);
    Task<decimal> GetTotalSalesByMonthAsync(int year, int month);
}

// Implementación específica
public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
    {
        return await dbSet
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await dbSet
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        return await dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<decimal> GetTotalSalesByMonthAsync(int year, int month)
    {
        return await dbSet
            .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month)
            .SumAsync(o => o.TotalAmount);
    }
}
```

**Unit of Work Pattern** gestiona transacciones y coordina múltiples repositorios:

```csharp
// Interfaz Unit of Work
public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    ICustomerRepository Customers { get; }
    IProductRepository Products { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// Implementación Unit of Work
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext context;
    private IDbContextTransaction? currentTransaction;

    // Lazy loading de repositorios
    private IOrderRepository? orderRepository;
    private ICustomerRepository? customerRepository;
    private IProductRepository? productRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        this.context = context;
    }

    public IOrderRepository Orders =>
        orderRepository ??= new OrderRepository(context);

    public ICustomerRepository Customers =>
        customerRepository ??= new CustomerRepository(context);

    public IProductRepository Products =>
        productRepository ??= new ProductRepository(context);

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        if (currentTransaction != null)
            throw new InvalidOperationException("Transaction already started");

        currentTransaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (currentTransaction == null)
            throw new InvalidOperationException("No transaction started");

        try
        {
            await SaveChangesAsync();
            await currentTransaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            currentTransaction?.Dispose();
            currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (currentTransaction != null)
        {
            await currentTransaction.RollbackAsync();
            currentTransaction.Dispose();
            currentTransaction = null;
        }
    }

    public void Dispose()
    {
        currentTransaction?.Dispose();
        context.Dispose();
    }
}
```

**Uso conjunto de Repository + Unit of Work**

```csharp
public class OrderService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<OrderService> logger;

    public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
    {
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            // Validar customer
            var customer = await unitOfWork.Customers.GetByIdAsync(request.CustomerId);
            if (customer == null)
                throw new ArgumentException("Customer not found");

            // Validar products y stock
            var products = new List<Product>();
            foreach (var item in request.Items)
            {
                var product = await unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new ArgumentException($"Product {item.ProductId} not found");

                if (product.Stock < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}");

                products.Add(product);
            }

            // Crear order
            var order = new Order
            {
                CustomerId = request.CustomerId,
                OrderDate = DateTime.UtcNow,
                OrderItems = request.Items.Select((item, index) => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = products[index].Price
                }).ToList()
            };

            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);

            // Actualizar stock
            foreach (var (item, product) in request.Items.Zip(products))
            {
                product.Stock -= item.Quantity;
                await unitOfWork.Products.UpdateAsync(product);
            }

            // Guardar order
            await unitOfWork.Orders.AddAsync(order);

            // Commit transacción
            await unitOfWork.CommitTransactionAsync();

            logger.LogInformation("Order {OrderId} created successfully", order.Id);
            return order;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "Failed to create order");
            throw;
        }
    }
}
```

**Testing con Repository + Unit of Work**

```csharp
[Test]
public async Task CreateOrder_Should_UpdateStockAndCreateOrder()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var mockOrderRepo = new Mock<IOrderRepository>();
    var mockCustomerRepo = new Mock<ICustomerRepository>();
    var mockProductRepo = new Mock<IProductRepository>();

    mockUnitOfWork.Setup(u => u.Orders).Returns(mockOrderRepo.Object);
    mockUnitOfWork.Setup(u => u.Customers).Returns(mockCustomerRepo.Object);
    mockUnitOfWork.Setup(u => u.Products).Returns(mockProductRepo.Object);

    var customer = new Customer { Id = 1, Name = "John Doe" };
    var product = new Product { Id = 1, Name = "Laptop", Price = 1000, Stock = 10 };

    mockCustomerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
    mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

    var service = new OrderService(mockUnitOfWork.Object, Mock.Of<ILogger<OrderService>>());

    var request = new CreateOrderRequest
    {
        CustomerId = 1,
        Items = new[] { new OrderItemRequest { ProductId = 1, Quantity = 2 } }
    };

    // Act
    var result = await service.CreateOrderAsync(request);

    // Assert
    Assert.AreEqual(2000, result.TotalAmount);
    mockProductRepo.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Stock == 8)), Times.Once);
    mockOrderRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
    mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
    mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
}
```

**Consideraciones y alternativas modernas**

```csharp
// ❌ Anti-pattern: Generic Repository with EF Core
// EF Core ya es un Repository/UoW pattern
public class AntiPatternService
{
    private readonly IRepository<Order> orderRepo; // Abstracción innecesaria

    public async Task<Order> GetOrderAsync(int id)
    {
        return await orderRepo.GetByIdAsync(id); // Solo delega a EF Core
    }
}

// ✅ Modern approach: Direct DbContext usage
public class ModernOrderService
{
    private readonly ApplicationDbContext context;

    public ModernOrderService(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<Order?> GetOrderWithItemsAsync(int id)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}

// ✅ Repository cuando añade valor real
public class CachedOrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext context;
    private readonly IMemoryCache cache;

    public async Task<Order?> GetByIdAsync(int id)
    {
        var cacheKey = $"order_{id}";

        if (cache.TryGetValue(cacheKey, out Order? cachedOrder))
            return cachedOrder;

        var order = await context.Orders.FindAsync(id);

        if (order != null)
        {
            cache.Set(cacheKey, order, TimeSpan.FromMinutes(5));
        }

        return order;
    }
}
```

### 35. ¿Cuál es la diferencia entre MVC, MVP y MVVM?

**Contexto** Separation of concerns, testability, UI frameworks.

**Respuesta detallada**

Los patrones **MVC**, **MVP** y **MVVM** separan la lógica de presentación de la lógica de negocio, pero difieren en sus responsabilidades y comunicación.

**MVC (Model-View-Controller)**

```csharp
// Model - datos y lógica de negocio
public class ProductModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public bool IsAvailable => Stock > 0;

    public void UpdateStock(int quantity)
    {
        if (Stock + quantity < 0)
            throw new InvalidOperationException("Insufficient stock");
        Stock += quantity;
    }
}

// View - UI y presentación
public interface IProductView
{
    void DisplayProducts(IEnumerable<ProductModel> products);
    void DisplayError(string message);
    void ShowSuccess(string message);

    event EventHandler<int> ProductRequested;
    event EventHandler<(int Id, int Quantity)> StockUpdateRequested;
}

// Controller - orquesta Model y View
public class ProductController
{
    private readonly IProductView view;
    private readonly IProductService productService;

    public ProductController(IProductView view, IProductService productService)
    {
        this.view = view;
        this.productService = productService;

        // Suscribirse a eventos del View
        view.ProductRequested += OnProductRequested;
        view.StockUpdateRequested += OnStockUpdateRequested;
    }

    public async Task LoadProductsAsync()
    {
        try
        {
            var products = await productService.GetAllProductsAsync();
            view.DisplayProducts(products);
        }
        catch (Exception ex)
        {
            view.DisplayError($"Error loading products: {ex.Message}");
        }
    }

    private async void OnProductRequested(object sender, int productId)
    {
        try
        {
            var product = await productService.GetProductAsync(productId);
            if (product != null)
            {
                view.DisplayProducts(new[] { product });
            }
        }
        catch (Exception ex)
        {
            view.DisplayError($"Error loading product: {ex.Message}");
        }
    }

    private async void OnStockUpdateRequested(object sender, (int Id, int Quantity) args)
    {
        try
        {
            await productService.UpdateStockAsync(args.Id, args.Quantity);
            view.ShowSuccess("Stock updated successfully");
            await LoadProductsAsync(); // Refresh view
        }
        catch (Exception ex)
        {
            view.DisplayError($"Error updating stock: {ex.Message}");
        }
    }
}

// ASP.NET Core MVC example
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase  // Controller
{
    private readonly IProductService productService;

    public ProductsController(IProductService productService)
    {
        this.productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductModel>>> GetProducts()
    {
        var products = await productService.GetAllProductsAsync();
        return Ok(products);  // View (JSON response)
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockRequest request)
    {
        try
        {
            await productService.UpdateStockAsync(id, request.Quantity);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

**MVP (Model-View-Presenter)**

```csharp
// Model - igual que en MVC
public class ProductModel { /* same as above */ }

// View - pasiva, no lógica
public interface IProductView
{
    // Properties para data binding
    IEnumerable<ProductModel> Products { get; set; }
    string ErrorMessage { get; set; }
    string SuccessMessage { get; set; }
    bool IsLoading { get; set; }

    // Events para user interactions
    event EventHandler LoadRequested;
    event EventHandler<int> ProductSelected;
    event EventHandler<(int Id, int Quantity)> StockUpdateRequested;
}

// Presenter - contiene toda la lógica de presentación
public class ProductPresenter
{
    private readonly IProductView view;
    private readonly IProductService productService;

    public ProductPresenter(IProductView view, IProductService productService)
    {
        this.view = view;
        this.productService = productService;

        // Wire up events
        view.LoadRequested += OnLoadRequested;
        view.ProductSelected += OnProductSelected;
        view.StockUpdateRequested += OnStockUpdateRequested;
    }

    private async void OnLoadRequested(object sender, EventArgs e)
    {
        view.IsLoading = true;
        view.ErrorMessage = string.Empty;

        try
        {
            var products = await productService.GetAllProductsAsync();
            view.Products = products;
        }
        catch (Exception ex)
        {
            view.ErrorMessage = ex.Message;
            view.Products = Enumerable.Empty<ProductModel>();
        }
        finally
        {
            view.IsLoading = false;
        }
    }

    private async void OnProductSelected(object sender, int productId)
    {
        // Presenter maneja toda la lógica
        var product = await productService.GetProductAsync(productId);

        if (product != null)
        {
            // Formatear datos para la vista
            view.Products = new[] { product };
        }
    }

    private async void OnStockUpdateRequested(object sender, (int Id, int Quantity) args)
    {
        try
        {
            await productService.UpdateStockAsync(args.Id, args.Quantity);
            view.SuccessMessage = "Stock updated successfully";

            // Reload data
            OnLoadRequested(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            view.ErrorMessage = ex.Message;
        }
    }
}

// WinForms implementation of IProductView
public partial class ProductForm : Form, IProductView
{
    public IEnumerable<ProductModel> Products
    {
        get => (IEnumerable<ProductModel>)dataGridView.DataSource;
        set => dataGridView.DataSource = value?.ToList();
    }

    public string ErrorMessage
    {
        get => lblError.Text;
        set => lblError.Text = value;
    }

    public string SuccessMessage
    {
        get => lblSuccess.Text;
        set => lblSuccess.Text = value;
    }

    public bool IsLoading
    {
        get => progressBar.Visible;
        set => progressBar.Visible = value;
    }

    public event EventHandler LoadRequested;
    public event EventHandler<int> ProductSelected;
    public event EventHandler<(int Id, int Quantity)> StockUpdateRequested;

    private void btnLoad_Click(object sender, EventArgs e)
    {
        LoadRequested?.Invoke(this, EventArgs.Empty);
    }

    private void dataGridView_SelectionChanged(object sender, EventArgs e)
    {
        if (dataGridView.SelectedRows.Count > 0)
        {
            var product = (ProductModel)dataGridView.SelectedRows[0].DataBoundItem;
            ProductSelected?.Invoke(this, product.Id);
        }
    }
}
```

**MVVM (Model-View-ViewModel)**

```csharp
// Model - igual que anteriores
public class ProductModel { /* same as above */ }

// ViewModel - binding properties y commands
public class ProductViewModel : INotifyPropertyChanged
{
    private readonly IProductService productService;
    private ObservableCollection<ProductModel> products;
    private string errorMessage;
    private string successMessage;
    private bool isLoading;
    private ProductModel selectedProduct;

    public ProductViewModel(IProductService productService)
    {
        this.productService = productService;
        Products = new ObservableCollection<ProductModel>();

        // Commands
        LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
        UpdateStockCommand = new AsyncRelayCommand<(int Id, int Quantity)>(UpdateStockAsync);
        SelectProductCommand = new RelayCommand<ProductModel>(SelectProduct);
    }

    // Properties with change notification
    public ObservableCollection<ProductModel> Products
    {
        get => products;
        set => SetProperty(ref products, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    public string SuccessMessage
    {
        get => successMessage;
        set => SetProperty(ref successMessage, value);
    }

    public bool IsLoading
    {
        get => isLoading;
        set => SetProperty(ref isLoading, value);
    }

    public ProductModel SelectedProduct
    {
        get => selectedProduct;
        set => SetProperty(ref selectedProduct, value);
    }

    // Commands para user interactions
    public IAsyncRelayCommand LoadProductsCommand { get; }
    public IAsyncRelayCommand<(int Id, int Quantity)> UpdateStockCommand { get; }
    public IRelayCommand<ProductModel> SelectProductCommand { get; }

    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var productList = await productService.GetAllProductsAsync();
            Products.Clear();

            foreach (var product in productList)
            {
                Products.Add(product);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task UpdateStockAsync((int Id, int Quantity) args)
    {
        try
        {
            await productService.UpdateStockAsync(args.Id, args.Quantity);
            SuccessMessage = "Stock updated successfully";
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void SelectProduct(ProductModel product)
    {
        SelectedProduct = product;
        // Additional logic for selection
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

// WPF View - declarative binding
/* ProductView.xaml
<UserControl x:Class="ProductView">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Commands -->
        <Button Grid.Row="0" Content="Load Products"
                Command="{Binding LoadProductsCommand}"/>

        <!-- Data -->
        <DataGrid Grid.Row="1"
                  ItemsSource="{Binding Products}"
                  SelectedItem="{Binding SelectedProduct}"/>

        <!-- Status -->
        <StackPanel Grid.Row="2">
            <TextBlock Text="{Binding ErrorMessage}" Foreground="Red"/>
            <TextBlock Text="{Binding SuccessMessage}" Foreground="Green"/>
            <ProgressBar IsIndeterminate="True"
                         Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"/>
        </StackPanel>
    </Grid>
</UserControl>
*/

// Code-behind minimal
public partial class ProductView : UserControl
{
    public ProductView()
    {
        InitializeComponent();
        // DataContext se asigna via DI o manualmente
        DataContext = App.ServiceProvider.GetService<ProductViewModel>();
    }
}
```

**Comparación de patrones**

| Aspecto          | MVC                       | MVP                      | MVVM                                |
| ---------------- | ------------------------- | ------------------------ | ----------------------------------- |
| **Coupling**     | View-Controller acoplados | View-Presenter acoplados | View-ViewModel débilmente acoplados |
| **Testability**  | Controller testeable      | Presenter muy testeable  | ViewModel muy testeable             |
| **View Logic**   | Mínima en View            | Cero en View             | Cero en View                        |
| **Data Binding** | Manual                    | Manual                   | Automático (two-way)                |
| **Platforms**    | Web (ASP.NET)             | WinForms, Web            | WPF, Xamarin, Blazor                |
| **Complexity**   | Moderada                  | Moderada                 | Alta (binding infrastructure)       |

### 36. Explica el patrón Command y sus casos de uso

**Contexto** CQRS, undo/redo, request processing.

**Respuesta detallada**

**Command Pattern** encapsula una petición como un objeto, permitiendo parametrizar, queuing, logging y undo operations.

**Command básico**

```csharp
// Interfaz base Command
public interface ICommand
{
    Task ExecuteAsync();
}

public interface ICommand<TResult>
{
    Task<TResult> ExecuteAsync();
}

// Interfaz para undo
public interface IUndoableCommand : ICommand
{
    Task UndoAsync();
    bool CanUndo { get; }
}

// Command concreto
public class CreateOrderCommand : ICommand<Order>
{
    private readonly Order order;
    private readonly IOrderRepository orderRepository;
    private readonly IEmailService emailService;

    public CreateOrderCommand(Order order, IOrderRepository orderRepository, IEmailService emailService)
    {
        this.order = order;
        this.orderRepository = orderRepository;
        this.emailService = emailService;
    }

    public async Task<Order> ExecuteAsync()
    {
        // Validar
        if (order.OrderItems?.Any() != true)
            throw new ArgumentException("Order must have items");

        // Ejecutar
        await orderRepository.AddAsync(order);
        await orderRepository.SaveChangesAsync();

        // Side effects
        await emailService.SendOrderConfirmationAsync(order);

        return order;
    }
}
```

**Command con Undo**

```csharp
public class TransferMoneyCommand : IUndoableCommand
{
    private readonly int fromAccountId;
    private readonly int toAccountId;
    private readonly decimal amount;
    private readonly IBankingService bankingService;
    private string? transactionId;

    public TransferMoneyCommand(int fromAccountId, int toAccountId, decimal amount, IBankingService bankingService)
    {
        this.fromAccountId = fromAccountId;
        this.toAccountId = toAccountId;
        this.amount = amount;
        this.bankingService = bankingService;
    }

    public bool CanUndo => !string.IsNullOrEmpty(transactionId);

    public async Task ExecuteAsync()
    {
        transactionId = await bankingService.TransferAsync(fromAccountId, toAccountId, amount);
    }

    public async Task UndoAsync()
    {
        if (!CanUndo)
            throw new InvalidOperationException("Cannot undo transfer");

        await bankingService.ReverseTransferAsync(transactionId);
        transactionId = null;
    }
}
```

**Command Invoker/Executor**

```csharp
public class CommandInvoker
{
    private readonly Stack<IUndoableCommand> history = new();
    private readonly ILogger<CommandInvoker> logger;

    public CommandInvoker(ILogger<CommandInvoker> logger)
    {
        this.logger = logger;
    }

    public async Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command)
    {
        logger.LogInformation("Executing command: {CommandType}", command.GetType().Name);

        try
        {
            var result = await command.ExecuteAsync();

            // Add to history if undoable
            if (command is IUndoableCommand undoableCommand)
            {
                history.Push(undoableCommand);
            }

            logger.LogInformation("Command executed successfully: {CommandType}", command.GetType().Name);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Command execution failed: {CommandType}", command.GetType().Name);
            throw;
        }
    }

    public async Task UndoLastAsync()
    {
        if (history.Count == 0)
            throw new InvalidOperationException("No commands to undo");

        var command = history.Pop();

        if (!command.CanUndo)
            throw new InvalidOperationException("Last command cannot be undone");

        logger.LogInformation("Undoing command: {CommandType}", command.GetType().Name);
        await command.UndoAsync();
    }

    public async Task UndoAllAsync()
    {
        while (history.Count > 0)
        {
            await UndoLastAsync();
        }
    }
}
```

**CQRS (Command Query Responsibility Segregation)**

```csharp
// Commands - modify state
public class CreateProductCommand : ICommand<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class UpdateProductPriceCommand : ICommand
{
    public int ProductId { get; set; }
    public decimal NewPrice { get; set; }
}

// Command Handlers
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IProductRepository repository;
    private readonly IEventBus eventBus;

    public CreateProductCommandHandler(IProductRepository repository, IEventBus eventBus)
    {
        this.repository = repository;
        this.eventBus = eventBus;
    }

    public async Task<int> HandleAsync(CreateProductCommand command)
    {
        var product = new Product
        {
            Name = command.Name,
            Price = command.Price,
            Stock = command.Stock
        };

        await repository.AddAsync(product);
        await repository.SaveChangesAsync();

        // Publish event
        await eventBus.PublishAsync(new ProductCreatedEvent(product.Id, product.Name));

        return product.Id;
    }
}

// Queries - read data
public class GetProductQuery : IQuery<ProductDto>
{
    public int ProductId { get; set; }
}

public class GetProductsQuery : IQuery<IEnumerable<ProductDto>>
{
    public string? NameFilter { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}

// Query Handlers
public class GetProductQueryHandler : IQueryHandler<GetProductQuery, ProductDto>
{
    private readonly IProductReadRepository repository;

    public GetProductQueryHandler(IProductReadRepository repository)
    {
        this.repository = repository;
    }

    public async Task<ProductDto> HandleAsync(GetProductQuery query)
    {
        var product = await repository.GetByIdAsync(query.ProductId);

        if (product == null)
            throw new EntityNotFoundException($"Product with ID {query.ProductId} not found");

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock
        };
    }
}
```

**Command Dispatcher/Mediator integration**

```csharp
// Using MediatR for command dispatching
public class ProductController : ControllerBase
{
    private readonly IMediator mediator;

    public ProductController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateProduct([FromBody] CreateProductCommand command)
    {
        var productId = await mediator.Send(command);
        return CreatedAtAction(nameof(GetProduct), new { id = productId }, productId);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var query = new GetProductQuery { ProductId = id };
        var product = await mediator.Send(query);
        return Ok(product);
    }

    [HttpPut("{id}/price")]
    public async Task<IActionResult> UpdatePrice(int id, [FromBody] decimal newPrice)
    {
        var command = new UpdateProductPriceCommand { ProductId = id, NewPrice = newPrice };
        await mediator.Send(command);
        return NoContent();
    }
}
```

**Macro Commands** (Composite Pattern):

```csharp
public class MacroCommand : IUndoableCommand
{
    private readonly List<IUndoableCommand> commands = new();
    private readonly List<IUndoableCommand> executedCommands = new();

    public bool CanUndo => executedCommands.Any();

    public void AddCommand(IUndoableCommand command)
    {
        commands.Add(command);
    }

    public async Task ExecuteAsync()
    {
        foreach (var command in commands)
        {
            try
            {
                await command.ExecuteAsync();
                executedCommands.Add(command);
            }
            catch
            {
                // Rollback executed commands
                await UndoAsync();
                throw;
            }
        }
    }

    public async Task UndoAsync()
    {
        // Undo in reverse order
        for (int i = executedCommands.Count - 1; i >= 0; i--)
        {
            if (executedCommands[i].CanUndo)
            {
                await executedCommands[i].UndoAsync();
            }
        }
        executedCommands.Clear();
    }
}

// Usage
var orderProcessing = new MacroCommand();
orderProcessing.AddCommand(new ReserveInventoryCommand(orderItems));
orderProcessing.AddCommand(new ChargePaymentCommand(payment));
orderProcessing.AddCommand(new CreateOrderCommand(order));
orderProcessing.AddCommand(new SendConfirmationEmailCommand(customer.Email));

await commandInvoker.ExecuteAsync(orderProcessing);
```

**Casos de uso del Command Pattern**

1. **CQRS** Separar commands (write) de queries (read)
2. **Undo/Redo** Implementar undo functionality
3. **Macro operations** Ejecutar múltiples operaciones como una unidad
4. **Queue processing** Commands como mensajes en cola
5. **Logging/Auditing** Log de todas las operaciones
6. **Transactional operations** Rollback en caso de error
7. **Remote operations** Serializar commands para RPC
8. **Wizard workflows** Multi-step processes

### 37. ¿Qué es Domain-Driven Design (DDD)?

**Contexto** Bounded contexts, aggregates, entities vs value objects.

**Respuesta detallada**

**Domain-Driven Design (DDD)** es un enfoque de desarrollo que coloca el dominio del negocio en el centro del diseño del software.

**Conceptos fundamentales de DDD**

**1. Ubiquitous Language** - Lenguaje común:

```csharp
// ❌ Términos técnicos, no del dominio
public class OrderRecord
{
    public int RecordId { get; set; }
    public DateTime CreatedTimestamp { get; set; }
    public decimal TotalValue { get; set; }
    public string Status { get; set; } // "1", "2", "3"
}

// ✅ Lenguaje del dominio
public class Order
{
    public OrderId Id { get; private set; }
    public DateTime OrderDate { get; private set; }
    public Money TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    // Métodos del dominio
    public void ConfirmOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        Status = OrderStatus.Confirmed;
        // Domain event
        AddDomainEvent(new OrderConfirmedEvent(Id));
    }

    public void CancelOrder(string reason)
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel delivered orders");

        Status = OrderStatus.Cancelled;
        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}
```

**2. Value Objects** - Objetos inmutables que representan conceptos:

```csharp
// Value Object para Money
public record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative");

        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    public static Money Zero(Currency currency) => new(0, currency);
}

// Value Object para Email
public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty");

        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format");

        Value = value.ToLowerInvariant();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static implicit operator string(Email email) => email.Value;
    public static explicit operator Email(string email) => new(email);
}

// Value Object para Address
public record Address(
    string Street,
    string City,
    string PostalCode,
    string Country)
{
    public Address(string street, string city, string postalCode, string country) : this(
        ValidateNotEmpty(street, nameof(street)),
        ValidateNotEmpty(city, nameof(city)),
        ValidateNotEmpty(postalCode, nameof(postalCode)),
        ValidateNotEmpty(country, nameof(country)))
    {
    }

    private static string ValidateNotEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be empty");
        return value;
    }

    public string FullAddress => $"{Street}, {City}, {PostalCode}, {Country}";
}
```

**3. Entities** - Objetos con identidad:

```csharp
// Entity base class
public abstract class Entity<TId>
{
    private readonly List<IDomainEvent> domainEvents = new();

    public TId Id { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other || GetType() != other.GetType())
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => Id?.GetHashCode() ?? 0;
}

// Customer Entity
public class Customer : Entity<CustomerId>
{
    public PersonalName Name { get; private set; }
    public Email Email { get; private set; }
    public Address? ShippingAddress { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private readonly List<Order> orders = new();

    public IReadOnlyCollection<Order> Orders => orders.AsReadOnly();

    private Customer() { } // For EF

    public Customer(CustomerId id, PersonalName name, Email email)
    {
        Id = id;
        Name = name;
        Email = email;
        Status = CustomerStatus.Active;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerCreatedEvent(id, email));
    }

    public void UpdateEmail(Email newEmail)
    {
        if (Email == newEmail) return;

        var oldEmail = Email;
        Email = newEmail;

        AddDomainEvent(new CustomerEmailChangedEvent(Id, oldEmail, newEmail));
    }

    public void SetShippingAddress(Address address)
    {
        ShippingAddress = address;
        AddDomainEvent(new CustomerAddressUpdatedEvent(Id, address));
    }

    public void DeactivateCustomer()
    {
        if (Status == CustomerStatus.Inactive)
            return;

        Status = CustomerStatus.Inactive;
        AddDomainEvent(new CustomerDeactivatedEvent(Id));
    }

    public Order PlaceOrder(IEnumerable<OrderItem> items)
    {
        if (Status != CustomerStatus.Active)
            throw new InvalidOperationException("Inactive customers cannot place orders");

        var order = new Order(new OrderId(Guid.NewGuid()), Id, items);
        orders.Add(order);

        return order;
    }
}
```

**4. Aggregates** - Consistencia boundaries:

```csharp
// Order Aggregate Root
public class Order : Entity<OrderId>, IAggregateRoot
{
    public CustomerId CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address? ShippingAddress { get; private set; }
    private readonly List<OrderItem> orderItems = new();

    public IReadOnlyCollection<OrderItem> OrderItems => orderItems.AsReadOnly();
    public Money TotalAmount => orderItems.Aggregate(
        Money.Zero(Currency.USD),
        (total, item) => total.Add(item.LineTotal));

    private Order() { } // For EF

    public Order(OrderId id, CustomerId customerId, IEnumerable<OrderItem> items)
    {
        Id = id;
        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;

        foreach (var item in items)
        {
            AddOrderItem(item);
        }

        if (!orderItems.Any())
            throw new ArgumentException("Order must have at least one item");

        AddDomainEvent(new OrderCreatedEvent(id, customerId, TotalAmount));
    }

    public void AddOrderItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed orders");

        var existingItem = orderItems.FirstOrDefault(oi => oi.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + item.Quantity);
        }
        else
        {
            orderItems.Add(item);
        }
    }

    public void RemoveOrderItem(ProductId productId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed orders");

        var item = orderItems.FirstOrDefault(oi => oi.ProductId == productId);
        if (item != null)
        {
            orderItems.Remove(item);
        }
    }

    public void SetShippingAddress(Address address)
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot change address of delivered order");

        ShippingAddress = address;
    }

    public void ConfirmOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        if (ShippingAddress == null)
            throw new InvalidOperationException("Shipping address is required");

        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedEvent(Id, TotalAmount));
    }
}

// OrderItem - Part of Order aggregate
public class OrderItem : Entity<OrderItemId>
{
    public ProductId ProductId { get; private set; }
    public string ProductName { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public Money LineTotal => UnitPrice.Multiply(Quantity);

    private OrderItem() { } // For EF

    public OrderItem(OrderItemId id, ProductId productId, string productName, Money unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        Id = id;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        Quantity = newQuantity;
    }
}
```

**5. Domain Services** - Lógica que no pertenece a una entidad:

```csharp
public interface IOrderDomainService
{
    Task<bool> CanPlaceOrderAsync(Customer customer, IEnumerable<OrderItem> items);
    Task<Money> CalculateShippingCostAsync(Address shippingAddress, IEnumerable<OrderItem> items);
}

public class OrderDomainService : IOrderDomainService
{
    private readonly IProductRepository productRepository;
    private readonly IShippingCalculator shippingCalculator;

    public OrderDomainService(IProductRepository productRepository, IShippingCalculator shippingCalculator)
    {
        this.productRepository = productRepository;
        this.shippingCalculator = shippingCalculator;
    }

    public async Task<bool> CanPlaceOrderAsync(Customer customer, IEnumerable<OrderItem> items)
    {
        // Business rules that span multiple aggregates
        if (customer.Status != CustomerStatus.Active)
            return false;

        // Check product availability
        foreach (var item in items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId);
            if (product == null || product.Stock < item.Quantity)
                return false;
        }

        // Check customer order limits
        var monthlyOrderTotal = customer.Orders
            .Where(o => o.OrderDate >= DateTime.UtcNow.AddDays(-30))
            .Sum(o => o.TotalAmount.Amount);

        var newOrderTotal = items.Sum(i => i.LineTotal.Amount);

        return monthlyOrderTotal + newOrderTotal <= 10000; // Business rule
    }

    public async Task<Money> CalculateShippingCostAsync(Address shippingAddress, IEnumerable<OrderItem> items)
    {
        var totalWeight = await CalculateTotalWeightAsync(items);
        return await shippingCalculator.CalculateAsync(shippingAddress, totalWeight);
    }

    private async Task<decimal> CalculateTotalWeightAsync(IEnumerable<OrderItem> items)
    {
        decimal totalWeight = 0;

        foreach (var item in items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId);
            totalWeight += product.Weight * item.Quantity;
        }

        return totalWeight;
    }
}
```

**6. Domain Events**

```csharp
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

public record OrderCreatedEvent(OrderId OrderId, CustomerId CustomerId, Money TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record OrderConfirmedEvent(OrderId OrderId, Money TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record CustomerCreatedEvent(CustomerId CustomerId, Email Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Domain Event Handlers
public class OrderConfirmedEventHandler : IDomainEventHandler<OrderConfirmedEvent>
{
    private readonly IEmailService emailService;
    private readonly IInventoryService inventoryService;

    public OrderConfirmedEventHandler(IEmailService emailService, IInventoryService inventoryService)
    {
        this.emailService = emailService;
        this.inventoryService = inventoryService;
    }

    public async Task HandleAsync(OrderConfirmedEvent domainEvent)
    {
        // Send confirmation email
        await emailService.SendOrderConfirmationAsync(domainEvent.OrderId);

        // Reserve inventory
        await inventoryService.ReserveInventoryAsync(domainEvent.OrderId);
    }
}
```

**7. Bounded Contexts**

```csharp
// Sales Context
namespace ECommerce.Sales.Domain
{
    public class Customer : Entity<CustomerId>
    {
        // Customer from sales perspective
        public PersonalName Name { get; private set; }
        public Email Email { get; private set; }
        public CustomerCreditLimit CreditLimit { get; private set; }
    }

    public class Order : Entity<OrderId>
    {
        // Order focused on sales process
        public CustomerId CustomerId { get; private set; }
        public Money TotalAmount { get; private set; }
        public OrderStatus Status { get; private set; }
    }
}

// Shipping Context
namespace ECommerce.Shipping.Domain
{
    public class Shipment : Entity<ShipmentId>
    {
        public OrderId OrderId { get; private set; }
        public ShippingAddress DeliveryAddress { get; private set; }
        public TrackingNumber TrackingNumber { get; private set; }
        public ShipmentStatus Status { get; private set; }

        public void MarkAsShipped(TrackingNumber trackingNumber)
        {
            if (Status != ShipmentStatus.Preparing)
                throw new InvalidOperationException("Only preparing shipments can be marked as shipped");

            TrackingNumber = trackingNumber;
            Status = ShipmentStatus.Shipped;

            AddDomainEvent(new ShipmentShippedEvent(Id, OrderId, trackingNumber));
        }
    }
}

// Inventory Context
namespace ECommerce.Inventory.Domain
{
    public class Product : Entity<ProductId>
    {
        public string Name { get; private set; }
        public SKU Sku { get; private set; }
        public int QuantityOnHand { get; private set; }
        public int ReservedQuantity { get; private set; }

        public int AvailableQuantity => QuantityOnHand - ReservedQuantity;

        public void ReserveStock(int quantity)
        {
            if (AvailableQuantity < quantity)
                throw new InsufficientStockException($"Cannot reserve {quantity} units. Available: {AvailableQuantity}");

            ReservedQuantity += quantity;
            AddDomainEvent(new StockReservedEvent(Id, quantity));
        }
    }
}
```

**Benefits of DDD**

- **Business alignment** Código que refleja el dominio real
- **Ubiquitous language** Comunicación clara entre técnicos y negocio
- **Maintainability** Lógica de negocio encapsulada y protegida
- **Testability** Domain logic separada de infrastructure
- **Scalability** Bounded contexts permiten equipos independientes

### 38. Explica el patrón Observer vs Event-driven architecture

**Contexto** Loose coupling, notifications, performance considerations.

**Respuesta detallada**

**Observer Pattern** y **Event-driven architecture** son patrones relacionados pero con diferentes alcances y complejidades.

**Observer Pattern clásico**

```csharp
// Subject interface
public interface IObservable<T>
{
    void Subscribe(IObserver<T> observer);
    void Unsubscribe(IObserver<T> observer);
    void NotifyObservers(T data);
}

// Observer interface
public interface IObserver<T>
{
    void Update(T data);
}

// Concrete Subject
public class StockPrice : IObservable<decimal>
{
    private readonly List<IObserver<decimal>> observers = new();
    private decimal currentPrice;

    public decimal CurrentPrice
    {
        get => currentPrice;
        set
        {
            if (currentPrice != value)
            {
                currentPrice = value;
                NotifyObservers(value);
            }
        }
    }

    public void Subscribe(IObserver<decimal> observer)
    {
        observers.Add(observer);
    }

    public void Unsubscribe(IObserver<decimal> observer)
    {
        observers.Remove(observer);
    }

    public void NotifyObservers(decimal price)
    {
        foreach (var observer in observers.ToList()) // ToList to avoid modification issues
        {
            try
            {
                observer.Update(price);
            }
            catch (Exception ex)
            {
                // Log error but continue with other observers
                Console.WriteLine($"Observer error: {ex.Message}");
            }
        }
    }
}

// Concrete Observers
public class PriceDisplayObserver : IObserver<decimal>
{
    private readonly string name;

    public PriceDisplayObserver(string name)
    {
        this.name = name;
    }

    public void Update(decimal price)
    {
        Console.WriteLine($"{name}: Stock price updated to ${price:F2}");
    }
}

public class PriceAlertObserver : IObserver<decimal>
{
    private readonly decimal threshold;
    private readonly IEmailService emailService;

    public PriceAlertObserver(decimal threshold, IEmailService emailService)
    {
        this.threshold = threshold;
        this.emailService = emailService;
    }

    public void Update(decimal price)
    {
        if (price >= threshold)
        {
            emailService.SendAlert($"Price alert: Stock reached ${price:F2}");
        }
    }
}

// Usage
var stockPrice = new StockPrice();
var display = new PriceDisplayObserver("Main Display");
var alert = new PriceAlertObserver(100m, new EmailService());

stockPrice.Subscribe(display);
stockPrice.Subscribe(alert);

stockPrice.CurrentPrice = 95m;  // Notifies all observers
stockPrice.CurrentPrice = 105m; // Notifies all observers + triggers alert
```

**Event-driven architecture moderna (.NET)**

```csharp
// Using .NET Events
public class OrderService
{
    // Event declarations
    public event EventHandler<OrderCreatedEventArgs>? OrderCreated;
    public event EventHandler<OrderCancelledEventArgs>? OrderCancelled;
    public event EventHandler<OrderStatusChangedEventArgs>? OrderStatusChanged;

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var order = new Order(request);
        await SaveOrderAsync(order);

        // Raise event
        OrderCreated?.Invoke(this, new OrderCreatedEventArgs(order));

        return order;
    }

    public async Task CancelOrderAsync(int orderId, string reason)
    {
        var order = await GetOrderAsync(orderId);
        order.Cancel(reason);
        await SaveOrderAsync(order);

        // Raise event
        OrderCancelled?.Invoke(this, new OrderCancelledEventArgs(order, reason));
    }

    protected virtual void OnOrderStatusChanged(Order order, OrderStatus oldStatus)
    {
        OrderStatusChanged?.Invoke(this, new OrderStatusChangedEventArgs(order, oldStatus));
    }
}

// Event argument classes
public class OrderCreatedEventArgs : EventArgs
{
    public Order Order { get; }
    public OrderCreatedEventArgs(Order order) => Order = order;
}

public class OrderCancelledEventArgs : EventArgs
{
    public Order Order { get; }
    public string Reason { get; }
    public OrderCancelledEventArgs(Order order, string reason)
    {
        Order = order;
        Reason = reason;
    }
}

// Event handlers
public class InventoryService
{
    public InventoryService(OrderService orderService)
    {
        orderService.OrderCreated += OnOrderCreated;
        orderService.OrderCancelled += OnOrderCancelled;
    }

    private async void OnOrderCreated(object sender, OrderCreatedEventArgs e)
    {
        await ReserveInventoryAsync(e.Order);
    }

    private async void OnOrderCancelled(object sender, OrderCancelledEventArgs e)
    {
        await ReleaseInventoryAsync(e.Order);
    }

    private async Task ReserveInventoryAsync(Order order) { /* implementation */ }
    private async Task ReleaseInventoryAsync(Order order) { /* implementation */ }
}
```

**Event Bus implementation**

```csharp
// Event interfaces
public interface IEvent
{
    DateTime OccurredAt { get; }
    Guid EventId { get; }
}

public interface IEventHandler<in T> where T : IEvent
{
    Task HandleAsync(T @event);
}

public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : IEvent;
    void Subscribe<T>(IEventHandler<T> handler) where T : IEvent;
    void Unsubscribe<T>(IEventHandler<T> handler) where T : IEvent;
}

// Event Bus implementation
public class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> handlers = new();
    private readonly ILogger<EventBus> logger;

    public EventBus(ILogger<EventBus> logger)
    {
        this.logger = logger;
    }

    public async Task PublishAsync<T>(T @event) where T : IEvent
    {
        var eventType = typeof(T);

        if (!handlers.TryGetValue(eventType, out var eventHandlers))
            return;

        var tasks = new List<Task>();

        foreach (var handler in eventHandlers.Cast<IEventHandler<T>>())
        {
            tasks.Add(HandleEventSafelyAsync(handler, @event));
        }

        await Task.WhenAll(tasks);
    }

    private async Task HandleEventSafelyAsync<T>(IEventHandler<T> handler, T @event) where T : IEvent
    {
        try
        {
            await handler.HandleAsync(@event);
            logger.LogInformation("Event {EventType} handled successfully by {HandlerType}",
                typeof(T).Name, handler.GetType().Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling event {EventType} with {HandlerType}",
                typeof(T).Name, handler.GetType().Name);
            // Don't throw - continue with other handlers
        }
    }

    public void Subscribe<T>(IEventHandler<T> handler) where T : IEvent
    {
        var eventType = typeof(T);
        handlers.AddOrUpdate(eventType,
            new List<object> { handler },
            (key, existing) => { existing.Add(handler); return existing; });
    }

    public void Unsubscribe<T>(IEventHandler<T> handler) where T : IEvent
    {
        var eventType = typeof(T);
        if (handlers.TryGetValue(eventType, out var eventHandlers))
        {
            eventHandlers.Remove(handler);
        }
    }
}

// Concrete events
public record OrderCreatedEvent(Guid OrderId, Guid CustomerId, decimal TotalAmount) : IEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}

public record PaymentProcessedEvent(Guid OrderId, decimal Amount, string PaymentMethod) : IEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}

// Event handlers
public class EmailNotificationHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IEmailService emailService;

    public EmailNotificationHandler(IEmailService emailService)
    {
        this.emailService = emailService;
    }

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        await emailService.SendOrderConfirmationAsync(@event.OrderId, @event.CustomerId);
    }
}

public class InventoryHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IInventoryService inventoryService;

    public InventoryHandler(IInventoryService inventoryService)
    {
        this.inventoryService = inventoryService;
    }

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        await inventoryService.ReserveStockAsync(@event.OrderId);
    }
}
```

**Distributed Event-driven architecture**

```csharp
// Message/Event for distributed systems
public class OrderCreatedMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// Message bus interface
public interface IMessageBus
{
    Task PublishAsync<T>(T message, string topic) where T : class;
    Task SubscribeAsync<T>(string topic, Func<T, Task> handler) where T : class;
}

// Azure Service Bus implementation
public class AzureServiceBusMessageBus : IMessageBus
{
    private readonly ServiceBusClient serviceBusClient;
    private readonly ILogger<AzureServiceBusMessageBus> logger;

    public AzureServiceBusMessageBus(ServiceBusClient serviceBusClient, ILogger<AzureServiceBusMessageBus> logger)
    {
        this.serviceBusClient = serviceBusClient;
        this.logger = logger;
    }

    public async Task PublishAsync<T>(T message, string topic) where T : class
    {
        var sender = serviceBusClient.CreateSender(topic);

        var json = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString(),
            Subject = typeof(T).Name
        };

        await sender.SendMessageAsync(serviceBusMessage);
        logger.LogInformation("Message {MessageType} published to topic {Topic}", typeof(T).Name, topic);
    }

    public async Task SubscribeAsync<T>(string topic, Func<T, Task> handler) where T : class
    {
        var processor = serviceBusClient.CreateProcessor(topic);

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var json = args.Message.Body.ToString();
                var message = JsonSerializer.Deserialize<T>(json);

                await handler(message);
                await args.CompleteMessageAsync(args.Message);

                logger.LogInformation("Message {MessageType} processed successfully", typeof(T).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message {MessageType}", typeof(T).Name);
                await args.AbandonMessageAsync(args.Message);
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "Error in message processor for topic {Topic}", topic);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync();
    }
}

// Usage in microservices
public class OrderService
{
    private readonly IMessageBus messageBus;

    public OrderService(IMessageBus messageBus)
    {
        this.messageBus = messageBus;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var order = new Order(request);
        await SaveOrderAsync(order);

        // Publish event to other microservices
        var message = new OrderCreatedMessage
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        };

        await messageBus.PublishAsync(message, "order-events");

        return order;
    }
}

// In Inventory microservice
public class InventoryEventHandler
{
    private readonly IInventoryService inventoryService;

    public InventoryEventHandler(IInventoryService inventoryService, IMessageBus messageBus)
    {
        this.inventoryService = inventoryService;

        // Subscribe to order events
        messageBus.SubscribeAsync<OrderCreatedMessage>("order-events", HandleOrderCreatedAsync);
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedMessage message)
    {
        await inventoryService.ReserveStockAsync(message.OrderId);

        // Publish inventory reserved event
        var inventoryMessage = new InventoryReservedMessage
        {
            OrderId = message.OrderId,
            ReservedAt = DateTime.UtcNow
        };

        await messageBus.PublishAsync(inventoryMessage, "inventory-events");
    }
}
```

**Comparación Observer vs Event-driven**

| Aspecto            | Observer Pattern          | Event-driven Architecture       |
| ------------------ | ------------------------- | ------------------------------- |
| **Scope**          | Single process            | Distributed systems             |
| **Coupling**       | Tight (direct references) | Loose (through messages)        |
| **Synchronous**    | Typically sync            | Can be async                    |
| **Error handling** | Affects all observers     | Isolated failures               |
| **Scalability**    | Limited                   | Highly scalable                 |
| **Persistence**    | In-memory only            | Can persist events              |
| **Ordering**       | Sequential                | May require ordering guarantees |

### 39. ¿Qué es Clean Architecture? ¿Cómo la implementarías en .NET? (CLEAN, API)

**Contexto** Layers, dependencies direction, testability.

**Respuesta detallada**

**Clean Architecture** organiza el código en capas concéntricas donde las dependencias apuntan hacia adentro, hacia el dominio.

**Estructura de capas**

```
┌─────────────────────────────────┐
│        Infrastructure           │  ← External concerns
├─────────────────────────────────┤
│        Adapters/Interface       │  ← Controllers, Presenters
├─────────────────────────────────┤
│        Application/Use Cases    │  ← Business rules
├─────────────────────────────────┤
│        Domain/Entities          │  ← Core business logic
└─────────────────────────────────┘
```

**1. Domain Layer (Centro)**

```csharp
// Domain/Entities/Order.cs
namespace CleanArchitecture.Domain.Entities
{
    public class Order : AuditableEntity
    {
        public int Id { get; private set; }
        public int CustomerId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public OrderStatus Status { get; private set; }
        public decimal TotalAmount { get; private set; }

        private readonly List<OrderItem> orderItems = new();
        public IReadOnlyCollection<OrderItem> OrderItems => orderItems.AsReadOnly();

        private Order() { } // EF Constructor

        public Order(int customerId, IEnumerable<OrderItem> items)
        {
            CustomerId = customerId;
            OrderDate = DateTime.UtcNow;
            Status = OrderStatus.Pending;

            foreach (var item in items)
            {
                AddOrderItem(item);
            }

            CalculateTotal();
        }

        public void AddOrderItem(OrderItem item)
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Cannot modify confirmed orders");

            orderItems.Add(item);
            CalculateTotal();
        }

        public void ConfirmOrder()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Order is already processed");

            if (!orderItems.Any())
                throw new InvalidOperationException("Cannot confirm empty order");

            Status = OrderStatus.Confirmed;
        }

        private void CalculateTotal()
        {
            TotalAmount = orderItems.Sum(item => item.Quantity * item.UnitPrice);
        }
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }
}

// Domain/ValueObjects/Money.cs
namespace CleanArchitecture.Domain.ValueObjects
{
    public record Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "USD")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative");
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required");

            Amount = amount;
            Currency = currency;
        }

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot add different currencies");

            return new Money(Amount + other.Amount, Currency);
        }

        public static Money Zero(string currency = "USD") => new(0, currency);
    }
}

// Domain/Interfaces/IOrderRepository.cs
namespace CleanArchitecture.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
        Task<Order> AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Order order);
        Task<bool> ExistsAsync(int id);
    }
}
```

**2. Application Layer (Use Cases)**

```csharp
// Application/DTOs/CreateOrderDto.cs
namespace CleanArchitecture.Application.DTOs
{
    public class CreateOrderDto
    {
        public int CustomerId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}

// Application/Interfaces/IOrderService.cs
namespace CleanArchitecture.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(int customerId);
        Task<OrderDto> ConfirmOrderAsync(int id);
        Task DeleteOrderAsync(int id);
    }
}

// Application/Services/OrderService.cs
namespace CleanArchitecture.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly ILogger<OrderService> logger;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IMapper mapper,
            ILogger<OrderService> logger)
        {
            this.orderRepository = orderRepository;
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            logger.LogInformation("Creating order for customer {CustomerId}", createOrderDto.CustomerId);

            // Validate products exist and have sufficient stock
            var orderItems = new List<OrderItem>();

            foreach (var itemDto in createOrderDto.Items)
            {
                var product = await productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    throw new NotFoundException($"Product with ID {itemDto.ProductId} not found");

                if (product.Stock < itemDto.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}");

                var orderItem = new OrderItem(itemDto.ProductId, product.Name, itemDto.Quantity, itemDto.UnitPrice);
                orderItems.Add(orderItem);
            }

            // Create order
            var order = new Order(createOrderDto.CustomerId, orderItems);

            // Save order
            var savedOrder = await orderRepository.AddAsync(order);

            logger.LogInformation("Order {OrderId} created successfully", savedOrder.Id);

            return mapper.Map<OrderDto>(savedOrder);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await orderRepository.GetByIdAsync(id);
            return order != null ? mapper.Map<OrderDto>(order) : null;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(int customerId)
        {
            var orders = await orderRepository.GetByCustomerIdAsync(customerId);
            return mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto> ConfirmOrderAsync(int id)
        {
            var order = await orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException($"Order with ID {id} not found");

            order.ConfirmOrder();
            await orderRepository.UpdateAsync(order);

            logger.LogInformation("Order {OrderId} confirmed", id);

            return mapper.Map<OrderDto>(order);
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException($"Order with ID {id} not found");

            await orderRepository.DeleteAsync(order);
            logger.LogInformation("Order {OrderId} deleted", id);
        }
    }
}

// Application/Exceptions/NotFoundException.cs
namespace CleanArchitecture.Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.") { }
    }
}
```

**3. Infrastructure Layer (External)**

```csharp
// Infrastructure/Data/ApplicationDbContext.cs
namespace CleanArchitecture.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}

// Infrastructure/Repositories/OrderRepository.cs
namespace CleanArchitecture.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext context;

        public OrderRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId)
        {
            return await context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<Order> AddAsync(Order order)
        {
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            context.Entry(order).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Order order)
        {
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Orders.AnyAsync(o => o.Id == id);
        }
    }
}

// Infrastructure/Mapping/OrderMappingProfile.cs
namespace CleanArchitecture.Infrastructure.Mapping
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<OrderItem, OrderItemDto>();

            CreateMap<CreateOrderDto, Order>()
                .ConstructUsing(src => new Order(src.CustomerId,
                    src.Items.Select(i => new OrderItem(i.ProductId, "", i.Quantity, i.UnitPrice))));
        }
    }
}
```

**4. Presentation Layer (Controllers)**

```csharp
// WebAPI/Controllers/OrdersController.cs
namespace CleanArchitecture.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService orderService;
        private readonly ILogger<OrdersController> logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            this.orderService = orderService;
            this.logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await orderService.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByCustomer(int customerId)
        {
            var orders = await orderService.GetOrdersByCustomerAsync(customerId);
            return Ok(orders);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                var order = await orderService.CreateOrderAsync(createOrderDto);
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/confirm")]
        public async Task<ActionResult<OrderDto>> ConfirmOrder(int id)
        {
            try
            {
                var order = await orderService.ConfirmOrderAsync(id);
                return Ok(order);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                await orderService.DeleteOrderAsync(id);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}
```

**5. Dependency Injection Configuration**

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application Services
builder.Services.AddScoped<IOrderService, OrderService>();

// Infrastructure
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(OrderMappingProfile));

// Logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Benefits of Clean Architecture**

- **Testability** Business logic isolated from external concerns
- **Independence** Database, UI, frameworks are details
- **Flexibility** Easy to change external dependencies
- **Maintainability** Clear separation of concerns
- **Scalability** Each layer can evolve independently

**Testing ejemplo**

```csharp
// Tests/Application/Services/OrderServiceTests.cs
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> mockOrderRepository;
    private readonly Mock<IProductRepository> mockProductRepository;
    private readonly Mock<IMapper> mockMapper;
    private readonly Mock<ILogger<OrderService>> mockLogger;
    private readonly OrderService orderService;

    public OrderServiceTests()
    {
        mockOrderRepository = new Mock<IOrderRepository>();
        mockProductRepository = new Mock<IProductRepository>();
        mockMapper = new Mock<IMapper>();
        mockLogger = new Mock<ILogger<OrderService>>();

        orderService = new OrderService(
            mockOrderRepository.Object,
            mockProductRepository.Object,
            mockMapper.Object,
            mockLogger.Object);
    }

    [Test]
    public async Task CreateOrderAsync_ValidOrder_ReturnsOrderDto()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 10m }
            }
        };

        var product = new Product { Id = 1, Name = "Test Product", Stock = 10 };
        var order = new Order(1, new List<OrderItem>());
        var orderDto = new OrderDto { Id = 1, CustomerId = 1 };

        mockProductRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>())).ReturnsAsync(order);
        mockMapper.Setup(m => m.Map<OrderDto>(order)).Returns(orderDto);

        // Act
        var result = await orderService.CreateOrderAsync(createOrderDto);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Id);
        mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
    }
}
```

<br>
**EJEMPLO DE ARQUITECTURA CLEAN**
Ejemplo en .NET 8 Clean Architecture para un microservicio de Pedidos:
<br><br>
**Estructura de carpetas**
src/
<br>
Orders.Api → capa de entrada (controllers, endpoints, middlewares)
<br>
Orders.Application → casos de uso (CQRS: commands, queries, handlers)
<br>
Orders.Domain → entidades, value objects, lógica de negocio, interfaces
<br>
Orders.Infrastructure → EF Core, repositorios, mensajería, implementaciones técnicas
<br>
<br>
Dominio (Orders.Domain/Entities/Order.cs)

```csharp
public class Order
{
    public Guid Id { get; private set; }
    public decimal Total { get; private set; }
    public bool Paid { get; private set; }

    public Order(decimal total)
    {
        Id = Guid.NewGuid();
        Total = total;
        Paid = false;
    }

    public void MarkAsPaid()
    {
        if (Paid) throw new InvalidOperationException("Order already paid");
        Paid = true;
    }
}
```

<br>
Caso de uso (Application → Command)
<br>
Orders.Application/Orders/Commands/PlaceOrderCommand.cs

```csharp
public record PlaceOrderCommand(decimal Total) : IRequest<Guid>;

public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    private readonly IOrderRepository _repository;

    public PlaceOrderHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(request.Total);
        await _repository.AddAsync(order);
        return order.Id;
    }
}
```

<br>
Infraestructura (Repository)
<br>
Orders.Infrastructure/Persistence/OrderRepository.cs

```csharp
public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }
}
```

<br>
API (Controller)
<br>
Orders.Api/Controllers/OrdersController.cs

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] decimal total)
    {
        var id = await _mediator.Send(new PlaceOrderCommand(total));
        return Ok(new { OrderId = id });
    }
}
```

<br>
**Este ejemplo muestra la separación**
Domain: entidades y lógica pura.
<br>
Application: orquesta casos de uso con CQRS.
<br>
Infrastructure: detalles técnicos (EF, repositorios, mensajería).
<br>
Api: punto de entrada HTTP.

### 40. Explica el patrón Factory y Abstract Factory

**Contexto** Object creation, dependency management, extensibility.

**Respuesta detallada**

**Factory Pattern** encapsula la creación de objetos, proporcionando una interfaz común para crear familias de objetos relacionados sin especificar sus clases concretas.

**Factory Method**

- Permite crear objetos sin especificar la clase exacta
- Define un método para crear objetos, pero las subclases deciden qué clase instanciar
- Útil cuando el tipo de objeto a crear se determina en tiempo de ejecución
- Ejemplos: creación de conexiones de base de datos, logging providers, payment processors

**Abstract Factory**

- Proporciona una interfaz para crear familias de objetos relacionados
- Garantiza que los objetos creados sean compatibles entre sí
- Útil para soportar múltiples plataformas o configuraciones
- Ejemplos: UI controls para diferentes sistemas operativos, drivers de base de datos

**Beneficios**

- **Desacoplamiento** El código cliente no depende de clases concretas
- **Extensibilidad** Fácil agregar nuevos tipos sin modificar código existente
- **Consistencia** Abstract Factory garantiza compatibilidad entre productos
- **Testabilidad** Facilita el uso de mocks y stubs

**Casos de uso comunes**

- Sistemas multi-tenant con diferentes configuraciones
- Aplicaciones multiplataforma
- Plugins y extensiones
- Configuración de dependency injection containers

### 41. ¿Qué es el patrón Mediator? ¿Cuándo usarías MediatR?

**Contexto** Decoupling, CQRS, request/response handling.

**Respuesta detallada**

**Mediator Pattern** define cómo un conjunto de objetos interactúan entre sí, promoviendo el bajo acoplamiento al evitar que los objetos se refieran explícitamente unos a otros.

**MediatR** es una implementación del patrón Mediator para .NET que facilita:

- **Request/Response patterns** Manejo de comandos y queries
- **Command dispatching** Enrutamiento automático de requests a handlers
- **Notification patterns** Publicación de eventos a múltiples handlers
- **Pipeline behaviors** Cross-cutting concerns como validación, logging, caching

**Beneficios del patrón Mediator**

- **Desacoplamiento** Reduce dependencias directas entre objetos
- **Single Responsibility** Cada handler maneja una responsabilidad específica
- **Extensibilidad** Fácil agregar nuevos handlers sin modificar existentes
- **Testabilidad** Handlers individuales son fáciles de testear

**Cuándo usar MediatR**

- **CQRS implementations** Separar commands de queries
- **Clean Architecture** Mediator en Application layer
- **Complex business logic** Múltiples pasos de procesamiento
- **Cross-cutting concerns** Validación, logging, autorización
- **Event-driven architectures** Publicación de domain events

**Trade-offs**

- **Overhead** Capa adicional de abstracción
- **Debugging** Más difícil seguir el flujo de ejecución
- **Performance** Overhead mínimo por reflection/DI
- **Learning curve** Requiere entender el patrón correctamente

### 42. Explica el patrón Strategy y su implementación en C#

**Contexto** Algorithm selection, polymorphism, configuration.

**Respuesta detallada**

**Strategy Pattern** permite seleccionar algoritmos en tiempo de ejecución encapsulando cada uno en clases separadas e intercambiables.

**Componentes del patrón**

- **Strategy interface** Define la interfaz común para todos los algoritmos
- **Concrete strategies** Implementaciones específicas de algoritmos
- **Context** Mantiene referencia a una strategy y delega el trabajo

**Casos de uso comunes**

- **Payment processing** Diferentes métodos de pago (credit card, PayPal, bank transfer)
- **Compression algorithms** ZIP, RAR, 7Z según necesidades
- **Sorting algorithms** QuickSort, MergeSort, BubbleSort según tamaño de datos
- **Pricing strategies** Regular, discount, premium pricing
- **Validation rules** Diferentes reglas según contexto
- **Caching strategies** Memory, Redis, SQL según performance needs

**Implementación en C#**

- **Interfaces** Definir contrato común para strategies
- **Dependency Injection** Inyectar strategies apropiadas
- **Factory pattern** Crear strategies basado en configuración
- **Enum-based selection** Usar enums para seleccionar strategy

**Beneficios**

- **Open/Closed Principle** Abierto para extensión, cerrado para modificación
- **Runtime flexibility** Cambiar comportamiento sin recompilar
- **Testability** Strategies individuales fáciles de testear
- **Maintainability** Algoritmos aislados y focalizados

**Consideraciones de performance**

- **Strategy caching** Reutilizar instances cuando sea apropiado
- **Lazy loading** Crear strategies solo cuando se necesiten
- **Memory usage** Considerar lifetime de strategy objects

### 43. ¿Qué es Event Sourcing y cuáles son sus trade-offs?

**Contexto** Audit trails, temporal queries, complexity.

**Respuesta detallada**

**Event Sourcing** es un patrón donde el estado de la aplicación se determina por una secuencia de eventos que han ocurrido, en lugar de almacenar solo el estado actual.

**Conceptos fundamentales**

- **Events** Representan cambios que han ocurrido en el dominio
- **Event Store** Base de datos append-only que almacena todos los eventos
- **Event Streams** Secuencia de eventos para una entidad específica
- **Snapshots** Estado precalculado en momentos específicos para performance
- **Projections** Vistas materializadas creadas desde event streams

**Ventajas del Event Sourcing**

- **Complete audit trail** Historial completo de todos los cambios
- **Temporal queries** Consultar estado en cualquier momento del pasado
- **Replay capability** Reconstruir estado desde eventos
- **Debugging** Facilita debug al mostrar exactamente qué pasó
- **Event-driven architecture** Natural fit para sistemas event-driven
- **Business intelligence** Rica fuente de datos para analytics

**Desventajas y complejidad**

- **Learning curve** Paradigma diferente requiere new mental models
- **Query complexity** Queries complejas requieren projections
- **Storage overhead** Más almacenamiento que state-based approaches
- **Performance** Reconstruir estado puede ser costoso
- **Event versioning** Evolución de eventos requiere migration strategies
- **Eventual consistency** Projections pueden estar out-of-sync

**Cuándo usar Event Sourcing**

- **Audit requirements** Compliance, financial systems, medical records
- **Complex business domains** Domains con rich business logic
- **Temporal analysis** Necesidad de analizar trends over time
- **Event-driven systems** Ya usando events extensively
- **Debugging needs** Sistemas donde debugging es crítico

**Cuándo NO usar Event Sourcing**

- **Simple CRUD applications** Overhead innecesario
- **Performance-critical reads** Cuando read performance es crítica
- **Simple domains** Domains sin complex business rules
- **Team readiness** Cuando el equipo no está preparado para la complejidad

**Consideraciones de implementación**

- **Event design** Events deben ser immutable y well-versioned
- **Snapshot strategy** Cuándo y cómo crear snapshots
- **Projection management** Cómo mantener projections up-to-date
- **Error handling** Qué hacer cuando event replay falla

### 44. Explica el patrón Saga para manejar transacciones distribuidas

**Contexto** Microservices, compensation, eventual consistency.

**Respuesta detallada**

**Saga Pattern** maneja transacciones distribuidas coordinando una secuencia de transacciones locales, cada una actualizada por un servicio participante.

**Tipos de Saga**

**Choreography-based Saga**

- Cada servicio publica eventos que disparan acciones en otros servicios
- No hay coordinador central
- Services reaccionan a eventos y publican sus propios eventos
- Más resiliente pero más difícil de trackear

**Orchestration-based Saga**

- Un coordinador central (orchestrator) maneja la saga
- El orchestrator envía comandos a servicios participantes
- Centraliza la lógica de coordinación
- Más fácil de monitorear pero punto único de fallo

**Componentes clave**

- **Saga Orchestrator** Coordina la secuencia de transacciones
- **Compensation Actions** Acciones para deshacer transacciones parciales
- **Saga Log** Track del progreso y estado de la saga
- **Timeouts** Manejar servicios que no responden

**Casos de uso típicos**

- **E-commerce order processing** Payment, inventory, shipping coordination
- **Travel booking** Flight, hotel, car rental reservations
- **Financial transactions** Multi-step money transfers
- **User onboarding** Account creation across multiple services

**Ventajas**

- **No 2PC** Evita two-phase commit y sus problemas
- **Autonomy** Services mantienen autonomía
- **Scalability** Mejor scalability que distributed transactions
- **Resilience** Can handle partial failures gracefully

**Desventajas**

- **Complexity** More complex than local transactions
- **Eventual consistency** No immediate consistency guarantees
- **Compensation logic** Need to implement undo operations
- **Debugging** Harder to trace distributed flows

**Consideraciones de diseño**

- **Idempotency** All operations must be idempotent
- **Compensation design** Not all operations can be undone
- **Timeout handling** How long to wait for responses
- **Error handling** What to do when compensation fails
- **Monitoring** Need comprehensive saga monitoring

**Alternativas**

- **Event Sourcing** Combined with saga for complete audit trail
- **CQRS** Separate read/write models for saga state
- **Two-Phase Commit** Para scenarios donde strong consistency es requerida

### 45. ¿Cuál es la diferencia entre Layered Architecture y Onion Architecture?

**Contexto** Dependencies, testability, maintainability.

**Respuesta detallada**

**Layered Architecture** y **Onion Architecture** son patrones arquitectónicos que organizan código, pero difieren en dirección de dependencias y testabilidad.

**Layered Architecture (Traditional N-Tier)**

- **Presentation Layer** UI, Controllers, Web APIs
- **Business Logic Layer** Services, domain logic
- **Data Access Layer** Repositories, ORM, database access
- **Database Layer** Physical data storage

**Características de Layered Architecture**

- Dependencies flow downward (Presentation → Business → Data → Database)
- Each layer can only call layers below it
- Data layer often drives the design
- Database is often the foundation

**Onion Architecture (Clean Architecture)**

- **Core/Domain** Entities, value objects, business rules
- **Application** Use cases, services, interfaces
- **Infrastructure** Databases, external services, frameworks
- **Presentation** Controllers, UI, delivery mechanisms

**Características de Onion Architecture**

- Dependencies point inward toward the domain
- Domain layer has no dependencies on external concerns
- Infrastructure depends on domain, not vice versa
- Inversion of control through dependency injection

**Diferencias clave**

| Aspecto                  | Layered Architecture              | Onion Architecture               |
| ------------------------ | --------------------------------- | -------------------------------- |
| **Dependency Direction** | Top-down (UI → Data)              | Inward (Infrastructure → Domain) |
| **Domain Focus**         | Data-driven                       | Domain-driven                    |
| **Testability**          | Difficult (database dependencies) | High (isolated domain)           |
| **Framework Coupling**   | High coupling to frameworks       | Low coupling to frameworks       |
| **Database Centricity**  | Database-centric                  | Domain-centric                   |
| **Flexibility**          | Limited (tightly coupled)         | High (loosely coupled)           |

**Ventajas de Layered Architecture**

- **Simplicity** Easy to understand and implement
- **Familiar** Well-known pattern in industry
- **Quick development** Faster initial development
- **Tool support** Good tooling support

**Desventajas de Layered Architecture**

- **Tight coupling** High coupling between layers
- **Testing difficulty** Hard to unit test business logic
- **Framework lock-in** Difficult to change frameworks
- **Database dependency** Business logic depends on database

**Ventajas de Onion Architecture**

- **High testability** Domain logic completely isolated
- **Framework independence** Easy to change frameworks
- **Domain focus** Business logic is central
- **Flexibility** Easy to change external dependencies

**Desventajas de Onion Architecture**

- **Complexity** More complex setup and understanding
- **Over-engineering** Can be overkill for simple applications
- **Learning curve** Requires understanding of DI and inversion of control
- **Initial overhead** More upfront design work

**Cuándo usar cada una**

**Layered Architecture**

- Simple CRUD applications
- Tight deadlines and quick delivery
- Small teams with limited experience
- Applications where database schema drives design

**Onion Architecture**

- Complex business domains
- Long-term maintainability is important
- High testability requirements
- Applications that need to support multiple UI or delivery mechanisms
- Teams experienced with DI and clean architecture principles

---

## 🌐 **APIs Y MICROSERVICIOS (10 preguntas)**

### 46. ¿Cuáles son los principios REST? ¿Qué hace a una API RESTful?

**Contexto** HTTP verbs, statelessness, resource identification.

**Respuesta detallada**

**REST (Representational State Transfer)** es un estilo arquitectónico para sistemas distribuidos basado en seis principios fundamentales:

**1. Client-Server Architecture**

- Separación clara entre cliente y servidor
- Cliente maneja UI y user state
- Servidor maneja data storage y business logic
- Permite evolución independiente de ambos lados

**2. Statelessness**

- Cada request debe contener toda la información necesaria
- Servidor no mantiene contexto del cliente entre requests
- Mejora escalabilidad y reliability
- Facilita load balancing y caching

**3. Cacheability**

- Responses deben estar marcadas como cacheable o non-cacheable
- Reduce latencia y mejora performance
- Cache-Control headers para controlar caching behavior
- ETags para conditional requests

**4. Uniform Interface**

- **Resource identification** URLs únicos para cada resource
- **Resource manipulation** HTTP verbs (GET, POST, PUT, DELETE)
- **Self-descriptive messages** Headers indican content type
- **HATEOAS** Hypermedia as the Engine of Application State

**5. Layered System**

- Arquitectura puede tener múltiples layers (proxy, gateway, cache)
- Cliente no sabe si está conectado directamente al servidor
- Permite load balancers, CDNs, security layers

**6. Code on Demand (Optional)**

- Servidor puede enviar código ejecutable al cliente
- JavaScript, applets, etc.
- Extiende funcionalidad del cliente

**Qué hace una API RESTful**

- **Resource-based URLs** `/users/123` no `/getUserById?id=123`
- **HTTP verbs correctos** GET para read, POST para create, etc.
- **Status codes apropiados** 200, 201, 404, 500, etc.
- **Content negotiation** Accept/Content-Type headers
- **Consistent naming** Plural nouns, consistent patterns
- **Idempotency** GET, PUT, DELETE son idempotent

### 47. ¿Cuál es la diferencia entre PUT, POST, PATCH? ¿Cuándo usar cada uno?

**Contexto** Idempotency, resource creation vs updates.

**Respuesta detallada**

**POST (Create)**

- **Propósito** Crear nuevos recursos o procesar data
- **Idempotency** NO idempotent
- **Target** Collection endpoint (`/users`)
- **Response** 201 Created con Location header
- **Body** Datos para crear el nuevo recurso
- **Side effects** Puede tener side effects

**Cuándo usar POST**

- Crear recursos cuando server asigna el ID
- Operaciones que no son idempotent
- Procesar formularios o data
- Operaciones complejas que no mapean a CRUD

**PUT (Create or Replace)**

- **Propósito** Crear o reemplazar completamente un recurso
- **Idempotency** SÍ idempotent
- **Target** Specific resource endpoint (`/users/123`)
- **Response** 201 Created (new) o 200 OK (updated)
- **Body** Representación completa del recurso
- **Behavior** Replace entire resource

**Cuándo usar PUT**

- Client conoce el resource identifier
- Quieres reemplazar el recurso completo
- Crear recurso con ID conocido
- Operaciones que deben ser idempotent

**PATCH (Partial Update)**

- **Propósito** Actualización parcial de un recurso
- **Idempotency** Puede ser idempotent (depends on implementation)
- **Target** Specific resource endpoint (`/users/123`)
- **Response** 200 OK o 204 No Content
- **Body** Solo los campos a actualizar
- **Behavior** Modify specific fields

**Cuándo usar PATCH**

- Actualizar solo algunos campos
- Large resources donde PUT sería wasteful
- Operaciones atómicas en campos específicos
- Better performance para updates pequeños

**Diferencias clave**

| Aspecto         | POST              | PUT                | PATCH           |
| --------------- | ----------------- | ------------------ | --------------- |
| **Idempotency** | No                | Sí                 | Depende         |
| **Target**      | Collection        | Resource           | Resource        |
| **Purpose**     | Create/Process    | Replace            | Modify          |
| **Body**        | Partial data      | Complete resource  | Partial updates |
| **Creation**    | Server assigns ID | Client provides ID | No creation     |

**Consideraciones de diseño**

- **Validation** PATCH requiere validación más compleja
- **Conflict resolution** Optimistic locking con ETags
- **Partial validation** Validar solo campos enviados en PATCH
- **JSON Patch** Estándar RFC 6902 para PATCH operations

### 48. Explica diferentes estrategias de versionado de APIs

**Contexto** Header-based, URL-based, content negotiation.

**Respuesta detallada**

**API Versioning** es crítico para evolucionar APIs sin romper clientes existentes.

**1. URL Path Versioning**

- Versión en la URL: `/api/v1/users`, `/api/v2/users`
- **Ventajas** Explícito, fácil de debuggear, cacheable
- **Desventajas** Proliferación de URLs, coupling con versión
- **Uso** Más común, fácil de implementar

**2. Query Parameter Versioning**

- Versión como query parameter: `/api/users?version=1`
- **Ventajas** Same URL path, flexible
- **Desventajas** Easy to forget, cache complications
- **Uso** Menos común, puede confundir caching

**3. Header Versioning**

- Versión en HTTP header: `Api-Version: 1` o `X-Version: 2`
- **Ventajas** Clean URLs, separation of concerns
- **Desventajas** Invisible, harder to test manually
- **Uso** RESTful puristas, cuando URLs deben ser stable

**4. Content Negotiation (Accept Header)**

- Usar Accept header: `Accept: application/vnd.api+json;version=1`
- **Ventajas** True REST, media type evolution
- **Desventajas** Complex, browser limitations
- **Uso** APIs muy RESTful, cuando format también cambia

**5. Subdomain Versioning**

- Diferentes subdominios: `v1.api.company.com`, `v2.api.company.com`
- **Ventajas** Complete separation, different deployments
- **Desventajas** DNS complexity, certificate management
- **Uso** Major architectural changes

**Estrategias de mantenimiento**

- **Sunset policy** Comunicar deprecation timeline
- **Backward compatibility** Mantener compatibilidad cuando posible
- **Migration guides** Documentar cambios y migration paths
- **Versioning strategy** Semantic versioning (major.minor.patch)

**Best practices**

- **Default version** Tener versión por defecto
- **Documentation** Version cada endpoint claramente
- **Testing** Test all supported versions
- **Deprecation warnings** Headers para indicate deprecation

### 49. ¿Qué es GraphQL y cuándo preferirías sobre REST?

**Contexto** Over-fetching, type safety, complexity trade-offs.

**Respuesta detallada**

**GraphQL** es un query language y runtime para APIs que permite a clientes solicitar exactamente los datos que necesitan.

**Características principales**

- **Single endpoint** Una URL para todas las operaciones
- **Flexible queries** Clientes especifican qué data necesitan
- **Type system** Schema strongly typed
- **Real-time subscriptions** Built-in support para real-time updates
- **Introspection** Schema es self-documenting

**Ventajas de GraphQL**

- **No over-fetching** Solo get data que necesitas
- **No under-fetching** Get related data en single request
- **Strongly typed** Compile-time error checking
- **Developer experience** Excellent tooling y documentation
- **Frontend flexibility** UI teams no blocked por backend changes
- **Real-time** Built-in subscription support

**Desventajas de GraphQL**

- **Learning curve** New concepts y patterns
- **Caching complexity** Harder to cache than REST
- **Query complexity** Can create expensive queries
- **Security concerns** Need query depth limiting
- **Tooling maturity** Less mature ecosystem than REST
- **HTTP features** Loses some HTTP benefits (caching, status codes)

**Cuándo preferir GraphQL sobre REST**

- **Mobile applications** Bandwidth y battery optimization
- **Complex data relationships** Many interconnected entities
- **Rapid frontend development** Multiple UI teams
- **Data aggregation** Combining múltiples data sources
- **Real-time requirements** Chat, notifications, live updates
- **Developer experience** Strong typing y tooling important

**Cuándo preferir REST sobre GraphQL**

- **Simple APIs** CRUD operations on few entities
- **Caching important** Heavy caching requirements
- **File uploads** Large file handling
- **Team expertise** Team familiar with REST
- **HTTP features** Need full HTTP feature set
- **Microservices** Service boundaries bien defined

**Consideraciones de implementación**

- **N+1 problem** Use DataLoader para batch requests
- **Query complexity** Implement query depth y complexity limits
- **Caching** Use field-level caching strategies
- **Security** Rate limiting y query analysis
- **Performance** Monitor query performance y optimize

### 50. ¿Cómo manejarías la autenticación y autorización en microservicios?

**Contexto** JWT, OAuth2, API Gateway, service-to-service auth.

**Respuesta detallada**

**Authentication vs Authorization** en microservicios presenta desafíos únicos de distributed security.

**Estrategias de Authentication**

**1. JWT (JSON Web Tokens)**

- **Stateless** No requiere server-side storage
- **Self-contained** Claims incluidos en token
- **Scalable** No database lookups para validation
- **Considerations** Token size, expiration strategy, revocation

**2. API Gateway Authentication**

- **Centralized** Gateway maneja authentication
- **Token conversion** External tokens → internal tokens
- **Rate limiting** Combined con authentication
- **Benefits** Single point of authentication logic

**3. OAuth2/OpenID Connect**

- **Standard protocols** Industry standard implementation
- **Third-party integration** Social login, enterprise SSO
- **Scope-based** Granular permissions
- **Token types** Access tokens, refresh tokens, ID tokens

**Service-to-Service Authentication**

**1. Mutual TLS (mTLS)**

- **Certificate-based** Each service has client certificate
- **Strong security** Cryptographic identity verification
- **Infrastructure heavy** Certificate management complexity
- **Performance** TLS handshake overhead

**2. Service Mesh (Istio, Linkerd)**

- **Automatic mTLS** Transparent encryption entre services
- **Policy enforcement** Centralized security policies
- **Traffic management** Security + routing + observability
- **Complexity** Additional infrastructure layer

**3. API Keys/Service Tokens**

- **Simple** Easy to implement y manage
- **Rotation** Need automated key rotation
- **Scope limitation** Limited granularity
- **Secret management** Secure storage y distribution

**Authorization Strategies**

**1. RBAC (Role-Based Access Control)**

- **Role assignment** Users assigned to roles
- **Permission inheritance** Roles have permissions
- **Simple model** Easy to understand y implement
- **Scalability issues** Role explosion en complex systems

**2. ABAC (Attribute-Based Access Control)**

- **Fine-grained** Decisions based on attributes
- **Flexible** Complex rules y conditions
- **Context-aware** Time, location, device-based decisions
- **Complexity** More complex to implement y debug

**3. Claims-Based Authorization**

- **Token claims** Authorization info en JWT claims
- **Distributed** No central authorization service needed
- **Performance** Fast authorization decisions
- **Token size** Limited by token size constraints

**Best Practices**

- **Principle of least privilege** Minimal necessary permissions
- **Token expiration** Short-lived tokens con refresh mechanism
- **Audit logging** Log all authentication y authorization events
- **Circuit breakers** Handle auth service failures gracefully
- **Security headers** HTTPS, HSTS, CSP headers
- **Rate limiting** Prevent brute force attacks

### 51. Explica el patrón API Gateway y sus responsabilidades

**Contexto** Routing, authentication, rate limiting, aggregation.

**Respuesta detallada**

**API Gateway** actúa como single entry point para todas las client requests en una arquitectura de microservicios.

**Responsabilidades principales**

**1. Request Routing**

- **Service discovery** Route requests al service correcto
- **Load balancing** Distribute load entre service instances
- **Path-based routing** Route based on URL patterns
- **Header-based routing** Route based on headers o parameters

**2. Authentication & Authorization**

- **Token validation** Validate JWT tokens o API keys
- **OAuth integration** Handle OAuth flows
- **Claims transformation** Convert external claims a internal format
- **Security policies** Enforce security rules centrally

**3. Rate Limiting & Throttling**

- **Client-based limits** Different limits per client
- **API-based limits** Limits per endpoint
- **Time windows** Sliding window, fixed window algorithms
- **Quota management** Monthly, daily usage quotas

**4. Request/Response Transformation**

- **Data aggregation** Combine multiple service responses
- **Format conversion** JSON ↔ XML, GraphQL ↔ REST
- **Request modification** Add headers, modify payloads
- **Response filtering** Remove sensitive data

**5. Monitoring & Analytics**

- **Request logging** Log all API calls
- **Metrics collection** Response times, error rates
- **Health monitoring** Service health checks
- **Analytics** Usage patterns, popular endpoints

**6. Caching**

- **Response caching** Cache frequent responses
- **Cache invalidation** Smart cache invalidation strategies
- **Edge caching** CDN integration
- **Personalized caching** User-specific cache strategies

**Beneficios del API Gateway**

- **Simplified clients** Clients only conocen gateway endpoint
- **Cross-cutting concerns** Centralized handling de security, logging
- **Service evolution** Backend changes no affect clients directly
- **Protocol translation** Different protocols para different clients
- **Reduced complexity** Less client-side logic

**Desventajas y consideraciones**

- **Single point of failure** Gateway failure affects all services
- **Performance bottleneck** All traffic through gateway
- **Complexity** Gateway itself becomes complex component
- **Latency** Additional network hop
- **Development bottleneck** Changes may require gateway updates

**Patterns y estrategias**

- **Backend for Frontend (BFF)** Different gateways para different client types
- **Circuit breaker** Handle downstream service failures
- **Bulkhead isolation** Isolate different API flows
- **Strangler fig** Gradually migrate from monolith

**Implementation considerations**

- **High availability** Multiple gateway instances
- **Configuration management** Dynamic routing configuration
- **Security** Gateway is high-value attack target
- **Observability** Comprehensive monitoring y tracing

### 52. ¿Qué estrategias usarías para manejar la comunicación entre microservicios?

**Contexto** Synchronous vs asynchronous, message queues, service mesh.

**Respuesta detallada**

**Communication patterns** en microservicios requieren careful consideration de consistency, latency, y reliability.

**Synchronous Communication**

**HTTP/REST APIs**

- **Direct calls** Service A calls Service B directly
- **Advantages** Simple, immediate response, familiar
- **Disadvantages** Tight coupling, cascade failures, latency accumulation
- **Use cases** Real-time queries, user-facing operations

**gRPC**

- **Protocol Buffers** Efficient binary serialization
- **HTTP/2** Multiplexing, streaming support
- **Code generation** Strong typing across languages
- **Performance** Faster than REST para internal communication

**Asynchronous Communication**

**Message Queues**

- **Point-to-point** One producer, one consumer
- **Decoupling** Services no need to be online simultaneously
- **Reliability** Message persistence y retry mechanisms
- **Examples** RabbitMQ, AWS SQS, Azure Service Bus

**Event Streaming**

- **Publish-Subscribe** One producer, multiple consumers
- **Event sourcing** Stream of events represents state changes
- **Real-time processing** Apache Kafka, AWS Kinesis
- **Scalability** Handle high-throughput scenarios

**Message Brokers vs Event Streams**

- **Message Brokers** Focus on message delivery guarantees
- **Event Streams** Focus on event ordering y replay capabilities
- **Durability** Events can be replayed, messages typically consumed once
- **Scalability** Event streams typically more scalable

**Service Mesh**

- **Infrastructure layer** Handles service-to-service communication
- **Features** Load balancing, security, observability, traffic management
- **Examples** Istio, Linkerd, Consul Connect
- **Benefits** Consistent communication policies, no application code changes

**Communication Patterns**

**Request-Response**

- **Synchronous** Direct HTTP calls
- **Best for** User-facing operations, immediate consistency needed
- **Challenges** Error handling, timeouts, circuit breakers

**Fire-and-Forget**

- **Asynchronous** Send message y continue processing
- **Best for** Notifications, audit logging, non-critical operations
- **Benefits** Low latency, high throughput

**Request-Acknowledge**

- **Async with confirmation** Send message y wait for acknowledgment
- **Best for** Important operations que need confirmation
- **Reliability** Better than fire-and-forget

**Publish-Subscribe**

- **Event-driven** Services react to events
- **Decoupling** Publishers don't know about subscribers
- **Scalability** Easy to add new event consumers

**Best Practices**

- **Idempotency** All operations should be idempotent
- **Circuit breakers** Prevent cascade failures
- **Timeouts** Set appropriate timeouts para all calls
- **Retry policies** Exponential backoff con jitter
- **Dead letter queues** Handle poison messages
- **Message ordering** Consider ordering requirements
- **Data consistency** Choose appropriate consistency model

### 53. ¿Cómo implementarías Circuit Breaker pattern?

**Contexto** Fault tolerance, cascading failures, Polly library.

**Respuesta detallada**

**Circuit Breaker Pattern** previene cascade failures al monitorear fallas y "abrir el circuito" cuando failure rate excede threshold.

**Estados del Circuit Breaker**

**1. Closed (Normal Operation)**

- Requests pass through normalmente
- Monitor failure rate y response times
- Count successful y failed requests
- When failure rate exceeds threshold → Open

**2. Open (Failing Fast)**

- Immediately return error sin calling downstream service
- Prevents resource waste y cascade failures
- After timeout period → Half-Open
- Can return cached data o default response

**3. Half-Open (Testing)**

- Allow limited requests to test service recovery
- If requests succeed → Closed
- If requests fail → Open again
- Use small number of test requests

**Implementación considerations**

**Failure Detection**

- **HTTP status codes** 5xx errors, timeouts
- **Exceptions** Network errors, timeouts
- **Response times** Slow responses counted as failures
- **Custom criteria** Business-specific failure conditions

**Threshold Configuration**

- **Failure percentage** e.g., 50% failure rate
- **Request volume** Minimum requests before evaluating
- **Time window** Rolling window para failure calculation
- **Recovery timeout** How long to stay Open

**Polly Library Features**

- **Circuit breaker policies** Built-in circuit breaker implementation
- **Policy combinations** Combine con retry, timeout policies
- **Async support** Full async/await support
- **Result handling** Handle both exceptions y specific results
- **Event callbacks** OnBreak, OnReset, OnHalfOpen events

**Advanced Features**

**Bulkhead Pattern**

- **Resource isolation** Separate thread pools para different operations
- **Failure isolation** Failure en one area no affects others
- **Resource limits** Limit concurrent operations

**Fallback Mechanisms**

- **Cached responses** Return stale data when service unavailable
- **Default values** Return sensible defaults
- **Degraded functionality** Reduced feature set
- **Alternative services** Route to backup service

**Monitoring y Observability**

- **Circuit state metrics** Track state changes over time
- **Failure rate monitoring** Monitor failure patterns
- **Response time tracking** Detect degradation early
- **Alerting** Alert on circuit breaker state changes

**Best Practices**

- **Health checks** Regular health checks para faster recovery
- **Gradual recovery** Slowly increase traffic after recovery
- **Configuration management** Externalize thresholds para tuning
- **Testing** Test circuit breaker behavior en different scenarios
- **Documentation** Clear documentation de fallback behavior

### 54. Explica diferentes estrategias de deployment para microservicios

**Contexto** Blue-green, canary, rolling deployments, containers.

**Respuesta detallada**

**Deployment strategies** para microservicios deben balance speed, safety, y minimal downtime.

**Rolling Deployment**

- **Process** Gradually replace old instances con new ones
- **Advantages** Zero downtime, automatic rollback capability
- **Disadvantages** Mixed versions during deployment, slower rollout
- **Use cases** Standard deployments con backward compatibility
- **Implementation** Kubernetes rolling updates, load balancer rotation

**Blue-Green Deployment**

- **Process** Two identical environments (blue/green), switch traffic atomically
- **Advantages** Instant rollback, testing en production-like environment
- **Disadvantages** Double infrastructure cost, data synchronization
- **Use cases** Critical applications, major releases
- **Considerations** Database migrations, stateful services

**Canary Deployment**

- **Process** Deploy to small subset of users, gradually increase traffic
- **Advantages** Risk mitigation, real user feedback, gradual rollout
- **Disadvantages** Complex traffic management, monitoring requirements
- **Strategies** Percentage-based, user-based, geography-based
- **Monitoring** Key metrics, error rates, user feedback

**A/B Testing Deployment**

- **Process** Run multiple versions simultaneously para different user segments
- **Purpose** Feature testing, performance comparison, business metrics
- **Duration** Longer-running than canary deployments
- **Analytics** Statistical significance, conversion rates, user behavior

**Recreate Deployment**

- **Process** Stop all instances, deploy new version, start instances
- **Advantages** Simple, clean state
- **Disadvantages** Downtime during deployment
- **Use cases** Development environments, non-critical services

**Container-Specific Strategies**

**Docker Swarm Mode**

- **Rolling updates** Built-in support para rolling deployments
- **Health checks** Container health verification
- **Rollback** Automatic rollback en case of failures

**Kubernetes Deployments**

- **ReplicaSets** Manage desired state of pod replicas
- **Rolling strategy** Configure maxUnavailable y maxSurge
- **Readiness probes** Ensure pods ready before receiving traffic
- **Liveness probes** Restart unhealthy pods

**Service Mesh Deployments**

- **Traffic splitting** Granular traffic control entre versions
- **Circuit breaking** Automatic failure handling
- **Observability** Detailed metrics y tracing
- **Security** Automatic mTLS entre services

**Database Migration Strategies**

- **Backward compatible changes** Add columns, tables
- **Two-phase migrations** Compatible change, then cleanup
- **Database per service** Independent database evolution
- **Event sourcing** Append-only changes

**Best Practices**

- **Automated testing** Comprehensive test suites antes deployment
- **Monitoring** Real-time monitoring durante deployments
- **Rollback plans** Clear rollback procedures
- **Feature flags** Decouple deployment from feature release
- **Infrastructure as Code** Version-controlled infrastructure
- **Deployment pipelines** Automated, repeatable processes

### 55. ¿Qué es Service Discovery y cómo lo implementarías?

**Contexto** Dynamic environments, load balancing, health checks.

**Respuesta detallada**

**Service Discovery** permite services encontrar y comunicarse con otros services en dynamic environments donde locations change frequently.

**Problemas que resuelve**

- **Dynamic IPs** Services move entre hosts, IP addresses change
- **Scaling** Services scale up/down, instances come y go
- **Health management** Unhealthy instances need removal
- **Load balancing** Distribute traffic entre healthy instances

**Patterns de Service Discovery**

**Client-Side Discovery**

- **Process** Client queries service registry y choose instance
- **Advantages** Simple, client controls load balancing
- **Disadvantages** Client complexity, language-specific logic
- **Examples** Netflix Eureka con Ribbon

**Server-Side Discovery**

- **Process** Client calls load balancer, which queries registry
- **Advantages** Simple clients, centralized logic
- **Disadvantages** Load balancer complexity, potential bottleneck
- **Examples** AWS ELB, Kubernetes Services

**Service Registry Patterns**

**Self-Registration**

- **Process** Services register themselves at startup
- **Advantages** Simple, no external coordination
- **Disadvantages** Service complexity, failure handling
- **Implementation** Heartbeat mechanism, graceful shutdown

**Third-Party Registration**

- **Process** External system registers services
- **Advantages** Services stay simple, centralized management
- **Disadvantages** Additional infrastructure component
- **Examples** Kubernetes automatically registers pods

**Popular Service Discovery Solutions**

**Consul (HashiCorp)**

- **Features** Service registry, health checking, KV store, service mesh
- **Health checks** HTTP, TCP, script-based checks
- **Multi-datacenter** Built-in support para multiple datacenters
- **Security** ACLs, encryption

**etcd**

- **Distributed** Strongly consistent, distributed key-value store
- **Kubernetes** Used by Kubernetes para service discovery
- **Watch API** Real-time notifications de changes
- **Clustering** Raft consensus algorithm

**Zookeeper**

- **Mature** Long-established service discovery solution
- **Consistency** Strong consistency guarantees
- **Complexity** More complex setup y maintenance
- **Use cases** Kafka, Hadoop ecosystems

**Kubernetes Service Discovery**

- **DNS-based** Services discoverable via DNS names
- **Environment variables** Service info injected as env vars
- **Service types** ClusterIP, NodePort, LoadBalancer
- **Endpoints** Automatic management of service endpoints

**Cloud Provider Solutions**

- **AWS Service Discovery** Cloud Map, ECS service discovery
- **Azure Service Fabric** Built-in service discovery
- **GCP** Cloud Load Balancing, service directory

**Implementation Considerations**

**Health Checking**

- **Health check types** HTTP, TCP, custom scripts
- **Check frequency** Balance responsiveness con resource usage
- **Failure thresholds** How many failures before removing service
- **Recovery** How quickly to re-add recovered services

**Load Balancing Algorithms**

- **Round-robin** Simple, even distribution
- **Least connections** Route to service con fewest connections
- **Weighted** Different weights para different instances
- **Geographic** Route based on client location

**Caching y Performance**

- **Client-side caching** Cache service locations para performance
- **TTL strategies** Appropriate cache expiration
- **Cache invalidation** Handle service changes quickly
- **Fallback mechanisms** What to do when discovery fails

**Security**

- **Authentication** Secure access to service registry
- **Authorization** Control which services can register
- **Encryption** Encrypt communication con registry
- **Network policies** Restrict network access entre services

---

## 🗄️ **BASE DE DATOS Y ORM (10 preguntas)**

### 56. ¿Cuáles son las diferencias entre Entity Framework Core y Dapper?

**Contexto** Performance, features, learning curve, use cases.

**Respuesta detallada**

**Entity Framework Core (EF Core)** y **Dapper** son dos enfoques diferentes para data access en .NET, cada uno con sus propias ventajas y trade-offs.

**Entity Framework Core**

- **Full ORM** Complete object-relational mapping solution
- **Code First** Define models en código, generate database schema
- **Change Tracking** Automatic tracking de entity changes
- **LINQ Support** Rich querying capabilities con LINQ
- **Migrations** Database schema versioning y evolution
- **Lazy Loading** Automatic loading de related data
- **Convention over Configuration** Sensible defaults con customization options

**Dapper**

- **Micro ORM** Lightweight, focused on performance
- **SQL First** Write raw SQL queries, map to objects
- **No Change Tracking** Manual change management
- **Performance** Minimal overhead, close to raw ADO.NET
- **Flexibility** Full control over SQL queries
- **Simple** Easy to understand y debug

**Performance Comparison**

- **Dapper** ~2-3x faster than EF Core para simple queries
- **EF Core** Acceptable performance con optimizations
- **Memory Usage** Dapper uses less memory (no change tracking)
- **Query Optimization** EF Core can generate suboptimal queries

**Learning Curve**

- **EF Core** Steeper learning curve, more concepts to master
- **Dapper** Easier to learn, especially con SQL background
- **SQL Knowledge** Dapper requires strong SQL skills

**Use Cases para EF Core**

- Complex business applications con rich domain models
- Rapid application development
- Teams con limited SQL expertise
- Applications requiring extensive data relationships
- When development speed matters more than raw performance

**Use Cases para Dapper**

- High-performance applications
- Existing databases con complex schemas
- Teams con strong SQL skills
- Simple data access patterns
- Microservices con specific data access needs

### 57. Explica Code First vs Database First en Entity Framework

**Contexto** Migrations, development workflow, team collaboration.

**Respuesta detallada**

**Code First** y **Database First** representan diferentes workflows para developing database-driven applications con Entity Framework.

**Code First Approach**

- **Model Definition** Define entities en C# classes
- **Database Generation** EF creates database schema from models
- **Migrations** Track schema changes over time
- **Version Control** Model changes tracked en source control
- **Team Collaboration** Better para team collaboration

**Ventajas de Code First**

- **Developer Focused** Natural workflow para developers
- **Version Control** Schema changes versioned con código
- **Refactoring** Easy to refactor models y propagate changes
- **Testing** Easy to create test databases
- **Cross Platform** Works across different database providers

**Database First Approach**

- **Existing Database** Start con existing database schema
- **Reverse Engineering** Generate models from database
- **Designer Support** Visual database designer tools
- **DBA Collaboration** Natural fit para DBA-driven environments

**Ventajas de Database First**

- **Legacy Systems** Work con existing databases
- **DBA Control** DBAs maintain schema control
- **Visual Design** Database diagrams y visual tools
- **Performance** DBAs can optimize schema directly

**Migration Strategies**

- **Automatic Migrations** EF handles migrations automatically
- **Code-Based Migrations** Developer-written migration código
- **Custom SQL** Include custom SQL en migrations
- **Data Seeding** Populate initial data during migrations

**Best Practices**

- **Migration Naming** Descriptive names para migrations
- **Rollback Planning** Consider rollback scenarios
- **Production Deployment** Careful migration deployment strategies
- **Team Coordination** Coordinate migrations entre team members

### 58. ¿Qué son las Navigation Properties y cómo funcionan en EF?

**Contexto** Lazy loading, eager loading, explicit loading.

**Respuesta detallada**

**Navigation Properties** representan relationships entre entities en Entity Framework, permitiendo navigate from one entity to related entities.

**Types of Navigation Properties**

- **Reference Navigation** Single related entity (one-to-one, many-to-one)
- **Collection Navigation** Multiple related entities (one-to-many, many-to-many)
- **Inverse Navigation** Navigation property en related entity

**Loading Strategies**

**Lazy Loading**

- **On-Demand** Related data loaded when accessed
- **Proxy Creation** EF creates proxy classes para enable lazy loading
- **Virtual Properties** Navigation properties must be virtual
- **N+1 Problem** Can cause performance issues
- **DbContext Lifetime** Requires active DbContext

**Eager Loading**

- **Include Method** Explicitly specify related data to load
- **Query Time** Related data loaded con initial query
- **Performance** Single database query, but potentially large result set
- **Cartesian Product** Can cause data duplication

**Explicit Loading**

- **Manual Loading** Explicitly load related data when needed
- **Load Method** Use Load() method para related entities
- **Query Method** Use Query() method para filtering related data
- **Control** Full control over when y how data is loaded

**Configuration Options**

- **Fluent API** Configure relationships using Fluent API
- **Data Annotations** Use attributes para simple configurations
- **Foreign Keys** Configure foreign key properties
- **Cascade Delete** Configure delete behavior

**Performance Considerations**

- **Select N+1** Avoid multiple queries for related data
- **Projection** Use Select() para load only needed data
- **Split Queries** Use AsSplitQuery() para complex includes
- **Tracking** Consider NoTracking para read-only scenarios

### 59. ¿Cómo optimizarías queries en Entity Framework?

**Contexto** N+1 problem, projection, compiled queries, raw SQL.

**Respuesta detallada**

**Query Optimization** en Entity Framework requiere understanding de how EF generates SQL y strategic use de available features.

**Common Performance Issues**

**N+1 Query Problem**

- **Problem** Loading related data en loop causes multiple queries
- **Solution** Use Include() para eager loading
- **Alternative** Use projection para specific fields
- **Batch Loading** Load related data en single query

**Unnecessary Data Loading**

- **Problem** Loading entire entities when only few properties needed
- **Solution** Use projection con Select()
- **Anonymous Types** Project to anonymous types
- **DTOs** Project to specific DTO classes

**Change Tracking Overhead**

- **Problem** Change tracking adds memory y CPU overhead
- **Solution** Use AsNoTracking() para read-only queries
- **Global Setting** Configure no-tracking globally when appropriate
- **Split Responsibility** Separate read y write operations

**Optimization Techniques**

**Compiled Queries**

- **Pre-compilation** Compile query plan once, reuse multiple times
- **Performance** Eliminates query compilation overhead
- **Use Cases** Frequently executed queries con parameters
- **Limitations** Limited flexibility, static query structure

**Raw SQL Queries**

- **Complex Queries** Use raw SQL para complex scenarios
- **Stored Procedures** Execute stored procedures directly
- **Performance** Optimal performance para specific use cases
- **Maintainability** Balance performance con maintainability

**Query Splitting**

- **Multiple Queries** Split complex includes into multiple queries
- **Cartesian Product** Avoid cartesian product issues
- **AsSplitQuery()** EF Core method para automatic splitting
- **Performance** Better performance para complex includes

**Projection Optimization**

- **Select Only Needed** Project only required fields
- **Flattening** Flatten nested objects en projections
- **GroupBy Operations** Optimize grouping y aggregations
- **Distinct Operations** Use Distinct efficiently

**Indexing Strategies**

- **Database Indexes** Ensure appropriate indexes exist
- **Composite Indexes** Multi-column indexes para complex queries
- **Query Analysis** Analyze execution plans
- **Index Hints** Use index hints cuando necessary

### 60. Explica el concepto de Change Tracking en EF

**Contexto** DbContext lifecycle, performance implications, NoTracking.

**Respuesta detallada**

**Change Tracking** es el mecanismo que Entity Framework usa para monitor changes to entities y determine what updates to send to the database.

**Entity States**

- **Unchanged** Entity exists en context y hasn't been modified
- **Added** New entity que will be inserted to database
- **Modified** Existing entity que has been changed
- **Deleted** Existing entity que will be deleted from database
- **Detached** Entity not being tracked by context

**Change Detection**

- **Snapshot Change Tracking** Default mechanism, compares current values con original values
- **Dynamic Proxies** Create proxy classes que notify changes immediately
- **Change Notifications** Entities implement INotifyPropertyChanged
- **Manual Detection** Call DetectChanges() manually

**Performance Implications**

- **Memory Usage** Tracking requires storing original values
- **CPU Overhead** Change detection requires value comparisons
- **Large Result Sets** Tracking many entities can be expensive
- **Long-running Contexts** Memory usage grows over time

**NoTracking Queries**

- **Read-Only Scenarios** When entities won't be modified
- **Performance Benefit** Eliminates tracking overhead
- **AsNoTracking()** Query-level no tracking
- **Global Setting** Configure no tracking globally

**Change Tracking Strategies**

- **Automatic** EF automatically detects changes
- **Manual** Call SaveChanges() to persist changes
- **Unit of Work** DbContext acts as unit of work
- **Transaction Management** Changes saved en single transaction

**Best Practices**

- **Short-lived Contexts** Keep DbContext lifetime short
- **Projection** Use projection para read-only data
- **Bulk Operations** Use bulk operations para large datasets
- **Memory Management** Dispose contexts appropriately

### 61. ¿Cuál es la diferencia entre `Add`, `Update`, `Attach` en EF?

**Contexto** Entity states, tracking, performance considerations.

**Respuesta detallada**

**Add**, **Update**, y **Attach** son métodos en Entity Framework para managing entity states y change tracking.

**Add Method**

- **Purpose** Mark entity as Added estado
- **Behavior** Entity will be inserted to database on SaveChanges()
- **Key Handling** Database-generated keys set after insert
- **Related Entities** Can cascade add to related entities
- **Use Cases** Creating new entities

**Update Method**

- **Purpose** Mark entity y all properties as Modified
- **Behavior** All properties updated en database, regardless of actual changes
- **Performance** Can be inefficient para partial updates
- **Concurrency** May overwrite concurrent changes
- **Use Cases** When you want to update entire entity

**Attach Method**

- **Purpose** Begin tracking entity en Unchanged estado
- **Behavior** No database operation until entity is modified
- **Change Detection** EF detects subsequent changes to attached entity
- **Related Entities** Can attach object graphs
- **Use Cases** Working con entities from other contexts

**Entity State Management**

- **Entry Method** Access EntityEntry para fine-grained control
- **State Property** Directly set entity state
- **Property Tracking** Track changes to specific properties
- **Original Values** Access original values para comparison

**Scenarios y Best Practices**

**Disconnected Scenarios**

- **Web Applications** Entities travel to client y back
- **Attach Pattern** Attach entity, modify, save changes
- **Update Pattern** Use Update cuando you know entity changed
- **Tracking** Consider tracking strategies para disconnected scenarios

**Performance Considerations**

- **Attach + Modify** More efficient para partial updates
- **Update** Simpler but less efficient para partial updates
- **Bulk Operations** Consider bulk operations para many entities
- **Change Detection** Balance accuracy con performance

**Related Entity Handling**

- **Cascade Behavior** How related entities are affected
- **Navigation Properties** Impact on related entity states
- **Graph Updates** Updating entire object graphs
- **Foreign Keys** Managing foreign key relationships

### 62. ¿Cómo manejarías concurrency en aplicaciones .NET?

**Contexto** Optimistic vs pessimistic locking, row versions, timestamps.

**Respuesta detallada**

**Concurrency Control** manages simultaneous access to shared resources para maintain data consistency.

**Optimistic Concurrency**

- **Assumption** Conflicts are rare
- **Check at Commit** Verify data hasn't changed before updating
- **Row Versions** Use timestamp/version columns
- **Conflict Detection** Compare original values con current database values
- **User Experience** Better user experience, no blocking

**Pessimistic Concurrency**

- **Assumption** Conflicts are likely
- **Lock Resources** Lock data during entire transaction
- **Blocking** Other users wait para resource access
- **Deadlock Risk** Potential para deadlocks
- **Use Cases** Critical data updates, financial transactions

**Implementation Strategies**

**Row Version/Timestamp**

- **Automatic Versioning** Database automatically updates version
- **Conflict Detection** Compare version before update
- **Entity Framework** Built-in support con [Timestamp] attribute
- **Performance** Minimal overhead

**Concurrency Tokens**

- **Custom Fields** Use specific fields as concurrency tokens
- **Business Logic** Check business-relevant fields para changes
- **Multiple Fields** Combine multiple fields para concurrency checking
- **Flexibility** More control over what constitutes a conflict

**Database-Level Locking**

- **Row Locking** Lock specific rows durante transaction
- **Table Locking** Lock entire tables (rarely used)
- **Read Locks** Shared locks para reading
- **Write Locks** Exclusive locks para writing

**Application-Level Strategies**

- **Distributed Locks** Redis, database-based distributed locks
- **Message Queues** Serialize operations through queues
- **Actor Model** Single-threaded actors manage state
- **Event Sourcing** Append-only, natural concurrency handling

**Best Practices**

- **Granular Locking** Lock smallest necessary scope
- **Timeout Handling** Set appropriate timeouts
- **Retry Logic** Implement retry con exponential backoff
- **User Communication** Clear messaging about conflicts
- **Conflict Resolution** Provide mechanisms para resolving conflicts

### 63. Explica different isolation levels en SQL Server

**Contexto** Read committed, snapshot, deadlocks, performance.

**Respuesta detallada**

**Transaction Isolation Levels** determine how much one transaction is isolated from changes made by other concurrent transactions.

**Read Uncommitted**

- **Dirty Reads** Can read uncommitted changes from other transactions
- **Performance** Highest performance, lowest isolation
- **Consistency** Lowest consistency guarantees
- **Use Cases** Reporting donde approximate data is acceptable
- **Risks** Reading data que may be rolled back

**Read Committed (Default)**

- **Dirty Read Prevention** Cannot read uncommitted data
- **Shared Locks** Places shared locks para reading
- **Lock Duration** Locks released immediately after read
- **Phantom Reads** Possible between reads en same transaction
- **Performance** Good balance of performance y consistency

**Repeatable Read**

- **Consistent Reads** Same data returned para repeated reads
- **Shared Locks** Locks held until transaction commits
- **Phantom Reads** Still possible (new rows can appear)
- **Deadlock Risk** Higher risk due to longer lock duration
- **Use Cases** Financial calculations requiring consistency

**Serializable**

- **Highest Isolation** Complete isolation from other transactions
- **Range Locks** Prevents phantom reads
- **Performance** Lowest performance due to extensive locking
- **Consistency** Highest consistency guarantees
- **Use Cases** Critical operations requiring absolute consistency

**Snapshot Isolation**

- **Row Versioning** Uses version store instead of locks
- **No Blocking** Readers don't block writers
- **Consistent View** Transaction sees consistent snapshot
- **Write Conflicts** Update conflicts detected at commit
- **tempdb Usage** Requires additional tempdb space

**Read Committed Snapshot**

- **Default Enhancement** Enhanced version of Read Committed
- **Version Store** Uses row versioning para reads
- **Writer Blocking** Writers still block writers
- **Performance** Better read performance than standard Read Committed
- **Upgrade Path** Easy upgrade from Read Committed

**Deadlock Handling**

- **Detection** SQL Server automatically detects deadlocks
- **Victim Selection** Chooses transaction to rollback
- **Prevention** Design transactions para minimize deadlock risk
- **Monitoring** Use deadlock monitoring tools
- **Retry Logic** Implement application-level retry logic

**Performance Considerations**

- **Lock Overhead** Higher isolation = more locking overhead
- **Blocking** Higher isolation = more potential blocking
- **tempdb Impact** Snapshot isolation affects tempdb
- **Index Design** Proper indexing reduces lock contention

### 64. ¿Qué son los Value Converters en Entity Framework?

**Contexto** Data transformation, custom types, JSON columns.

**Respuesta detallada**

**Value Converters** en Entity Framework allow conversion between types used en your application y types stored en the database.

**Purpose y Use Cases**

- **Type Conversion** Convert between .NET types y database types
- **Enum Handling** Store enums as strings instead of integers
- **Custom Types** Support para custom value types
- **JSON Storage** Store complex objects as JSON
- **Encryption** Encrypt sensitive data before storage

**Built-in Converters**

- **Enum to String** Convert enums to string representation
- **Boolean Conversions** Store booleans as 'Y'/'N' or 1/0
- **DateTime Conversions** Handle different datetime formats
- **String Conversions** Trim strings, case conversions
- **Number Conversions** Convert between different numeric types

**Custom Value Converters**

- **Implementation** Implement conversion logic entre types
- **Bidirectional** Convert both to database y from database
- **Performance** Consider performance implications
- **Null Handling** Handle null values appropriately
- **Validation** Validate converted values

**JSON Column Support**

- **Complex Objects** Store entire objects as JSON
- **Nested Properties** Query into JSON properties
- **Array Support** Store y query arrays within JSON
- **Performance** Balance convenience con query performance
- **Database Support** Requires JSON support en database

**Configuration Options**

- **Fluent API** Configure converters using fluent API
- **Global Converters** Apply converters globally
- **Property-Specific** Apply converters to specific properties
- **Conditional** Apply converters based on conditions

**Best Practices**

- **Performance Testing** Test performance impact of converters
- **Database Compatibility** Ensure database supports target types
- **Indexing** Consider indexing implications
- **Migration** Plan migrations cuando changing converters
- **Documentation** Document custom converter behavior

### 65. ¿Cómo implementarías una estrategia de caching para datos?

**Contexto** In-memory, distributed cache, cache invalidation, Redis.

**Respuesta detallada**

**Caching Strategy** requires careful consideration de data patterns, consistency requirements, y performance goals.

**Cache Types**

**In-Memory Caching**

- **Local Storage** Data stored en application memory
- **Fast Access** Fastest possible access times
- **Scalability Limits** Limited to single application instance
- **Memory Constraints** Limited by available memory
- **Use Cases** Application configuration, reference data, frequently accessed data

**Distributed Caching**

- **Shared Storage** Cache shared entre multiple application instances
- **Scalability** Scales across multiple servers
- **Network Overhead** Slight network latency
- **Consistency** Shared cache ensures consistency
- **Use Cases** User sessions, shared application data

**Cache Providers**

**Redis**

- **Feature Rich** Rich data structures (strings, lists, sets, sorted sets)
- **Persistence** Optional data persistence
- **Clustering** Built-in clustering support
- **Performance** High performance, sub-millisecond latency
- **Use Cases** Session storage, real-time analytics, pub/sub

**Memcached**

- **Simple** Simple key-value store
- **Memory Only** Pure in-memory storage
- **Performance** Very fast, minimal overhead
- **Simplicity** Easy to understand y deploy
- **Use Cases** Simple caching scenarios, web page caching

**Cache Patterns**

**Cache-Aside (Lazy Loading)**

- **On-Demand** Load data into cache only when requested
- **Cache Miss** Load from database cuando not en cache
- **Application Control** Application manages cache
- **Consistency** Potential staleness issues
- **Use Cases** Read-heavy workloads

**Write-Through**

- **Immediate Sync** Write to cache y database simultaneously
- **Consistency** Strong consistency between cache y database
- **Performance** Write performance impact
- **Complexity** More complex error handling
- **Use Cases** Critical data requiring consistency

**Write-Behind (Write-Back)**

- **Async Writes** Write to cache immediately, database later
- **Performance** Better write performance
- **Risk** Potential data loss
- **Complexity** Complex implementation
- **Use Cases** High-write workloads

**Cache Invalidation Strategies**

**TTL (Time To Live)**

- **Expiration** Data expires after specified time
- **Simple** Easy to implement y understand
- **Accuracy** May serve stale data
- **Tuning** Requires careful TTL tuning
- **Use Cases** Data con predictable change patterns

**Event-Driven Invalidation**

- **Real-Time** Invalidate cuando data changes
- **Accuracy** More accurate than TTL
- **Complexity** More complex implementation
- **Dependencies** Requires event infrastructure
- **Use Cases** Critical data requiring freshness

**Tag-Based Invalidation**

- **Grouping** Tag related cache entries
- **Bulk Invalidation** Invalidate entire groups
- **Flexibility** Flexible invalidation strategies
- **Management** Requires tag management
- **Use Cases** Related data que changes together

**Performance Considerations**

- **Cache Hit Ratio** Monitor y optimize hit ratios
- **Cache Size** Balance memory usage con hit ratio
- **Serialization** Efficient serialization/deserialization
- **Network Latency** Consider network overhead para distributed caches
- **Monitoring** Comprehensive cache performance monitoring

---

## 🧪 **PRUEBAS Y CALIDAD (10 preguntas)**

### 66. ¿Cuál es la diferencia entre Unit Tests, Integration Tests y End-to-End Tests?

**Contexto** Test pyramid, scope, execution speed, maintenance.

**Respuesta detallada**

**Test Pyramid** representa la distribución ideal de diferentes tipos de tests en una application, con unit tests en la base y E2E tests en la cima.

**Unit Tests**

- **Scope** Test individual units of code (methods, classes) in isolation
- **Dependencies** Mock or stub external dependencies
- **Speed** Very fast execution (milliseconds)
- **Maintenance** Easy to maintain, focused failures
- **Coverage** High coverage possible, test edge cases
- **Cost** Low cost to write and maintain
- **Feedback** Immediate feedback on code changes

**Integration Tests**

- **Scope** Test interaction between multiple components
- **Dependencies** Use real implementations of some dependencies
- **Speed** Moderate execution time (seconds)
- **Types** Component integration, API integration, database integration
- **Environment** May require test environment setup
- **Complexity** More complex setup and teardown
- **Value** Catch integration issues unit tests miss

**End-to-End Tests**

- **Scope** Test complete user workflows through entire system
- **Environment** Production-like environment required
- **Speed** Slow execution (minutes)
- **Maintenance** High maintenance cost, brittle
- **Value** Highest confidence in system functionality
- **Automation** Often involve UI automation tools
- **Flakiness** More prone to intermittent failures

**Test Pyramid Ratios**

- **70% Unit Tests** Fast, reliable, cheap to maintain
- **20% Integration Tests** Cover component interactions
- **10% E2E Tests** Cover critical user journeys

**Anti-patterns**

- **Ice Cream Cone** Too many E2E tests, few unit tests
- **Hourglass** Many unit and E2E tests, few integration tests
- **Testing Trophy** Alternative model emphasizing integration tests

### 67. Explica los conceptos de Mocking y Stubbing

**Contexto** Test doubles, frameworks (Moq, NSubstitute), isolation.

**Respuesta detallada**

**Test Doubles** son objetos que reemplazan dependencies reales durante testing para achieve isolation y control.

**Types of Test Doubles**

**Dummy Objects**

- **Purpose** Objects que se pasan pero nunca se usan
- **Behavior** No functional behavior
- **Use Case** Fill parameter lists
- **Example** Passing null or empty objects as parameters

**Fake Objects**

- **Purpose** Working implementation con simplified behavior
- **Behavior** Functional but not suitable for production
- **Use Case** In-memory databases, simple implementations
- **Example** In-memory repository instead of database repository

**Stubs**

- **Purpose** Provide predetermined responses to method calls
- **Behavior** Return canned responses
- **State Verification** Focus on state verification
- **Use Case** Control indirect inputs to system under test
- **Characteristics** No logic, just return predefined values

**Mocks**

- **Purpose** Verify interactions between objects
- **Behavior** Can have expectations about how they're called
- **Behavior Verification** Focus on behavior verification
- **Use Case** Verify that certain methods were called with expected parameters
- **Characteristics** Can fail tests if expectations not met

**Spies**

- **Purpose** Record information about method calls
- **Behavior** Partial mocks that can verify interactions
- **Use Case** Verify method calls while allowing real implementation
- **Characteristics** Hybrid between real objects and mocks

**Mock Frameworks**

**Moq Features**

- **Fluent API** Readable, expressive syntax
- **Verification** Verify method calls, parameters, call counts
- **Callbacks** Execute custom logic during method calls
- **Properties** Mock properties and auto-properties
- **Events** Raise events from mocks

**NSubstitute Features**

- **Simple Syntax** More concise than Moq
- **Argument Matching** Flexible argument matching
- **Return Values** Easy configuration of return values
- **Received Calls** Simple verification syntax
- **Partial Substitutes** Mix real and mocked behavior

**Best Practices**

- **Mock Roles, Not Objects** Mock interfaces and abstractions
- **Verify Behavior** Focus on verifying important interactions
- **Don't Over-Mock** Avoid mocking value objects or simple data structures
- **Test State** Prefer testing state changes over interactions when possible
- **Mock Boundaries** Mock at architectural boundaries

### 68. ¿Qué es TDD (Test-Driven Development)? ¿Cuáles son sus beneficios?

**Contexto** Red-Green-Refactor cycle, design implications, challenges.

**Respuesta detallada**

**Test-Driven Development (TDD)** es una software development practice donde tests se escriben antes del production code.

**Red-Green-Refactor Cycle**

**Red Phase**

- **Write Failing Test** Write a test for next piece of functionality
- **Run Test** Verify test fails (confirms test is working)
- **Focus** Define what needs to be built
- **Duration** Short, focused on single requirement

**Green Phase**

- **Make Test Pass** Write minimal code to make test pass
- **No Optimization** Focus on making test pass, not perfect code
- **Incremental** Add functionality incrementally
- **Quick Feedback** Fast validation that code works

**Refactor Phase**

- **Improve Code** Clean up code while keeping tests green
- **Design Patterns** Apply design patterns and principles
- **Remove Duplication** Eliminate code duplication
- **Maintain Tests** Ensure tests remain green throughout

**Benefits of TDD**

**Design Benefits**

- **Better Design** Forces thinking about API design first
- **Loose Coupling** Natural tendency toward loosely coupled code
- **SOLID Principles** Encourages following SOLID principles
- **Testable Code** Code is naturally more testable

**Quality Benefits**

- **High Test Coverage** Typically achieves high test coverage
- **Fewer Bugs** Studies show reduction in bug density
- **Regression Safety** Comprehensive test suite catches regressions
- **Documentation** Tests serve as living documentation

**Development Benefits**

- **Faster Feedback** Quick feedback on code changes
- **Confidence** High confidence when refactoring
- **Focus** Helps maintain focus on requirements
- **Incremental** Supports incremental development

**Challenges and Limitations**

**Learning Curve**

- **Paradigm Shift** Requires different thinking approach
- **Skill Development** Takes time to write good tests
- **Tool Knowledge** Need to learn testing frameworks and tools

**Time Investment**

- **Initial Slowdown** Can slow initial development
- **Test Maintenance** Tests require maintenance over time
- **Complex Scenarios** Some scenarios difficult to test-drive

**Situational Limitations**

- **Legacy Code** Difficult to apply to existing untested code
- **UI Testing** Challenging for complex UI interactions
- **Integration** Less effective for integration testing
- **Prototyping** May not be suitable for throw-away prototypes

**Best Practices**

- **Small Steps** Take very small steps in each cycle
- **Baby Steps** Write minimal test, minimal code
- **Refactor Regularly** Don't skip refactoring phase
- **Test Names** Use descriptive test names that explain behavior

### 69. ¿Cómo testearías métodos async/await?

**Contexto** Task testing, async test methods, deadlock prevention.

**Respuesta detallada**

**Testing async methods** requiere special considerations para handle asynchronous execution, timing issues, y potential deadlocks.

**Async Test Methods**

- **Test Method Signature** Mark test methods con async keyword
- **Return Type** Return Task instead of void
- **Await Operations** Await async operations within tests
- **Framework Support** Most testing frameworks support async tests

**Common Patterns**

**Testing Task Return Values**

- **Await Results** Await async method y assert on result
- **Exception Testing** Use Assert.ThrowsAsync para async exceptions
- **Cancellation** Test cancellation token handling
- **Timeout Handling** Test timeout scenarios

**Testing Void Async Methods**

- **Completion Testing** Verify method completes without exceptions
- **Side Effect Testing** Test side effects of async operations
- **State Changes** Verify state changes after async completion

**Deadlock Prevention**

- **ConfigureAwait(false)** Use ConfigureAwait(false) en library code
- **Task.Run** Avoid Task.Run en async methods
- **SynchronizationContext** Be aware of synchronization context issues
- **Test Environment** Ensure test environment doesn't have problematic sync context

**Mock Async Dependencies**

- **Task.FromResult** Return completed tasks para synchronous results
- **Task.CompletedTask** Return completed task para void methods
- **Task.FromException** Return faulted tasks para testing error handling
- **Delay Simulation** Use Task.Delay para simulate long-running operations

**Testing Patterns**

**Result Verification**

- **Direct Awaiting** Await method y verify result
- **Task Properties** Check task status, result, exception
- **Multiple Awaits** Test multiple async operations
- **Parallel Execution** Test concurrent async operations

**Exception Handling**

- **Async Exception Testing** Use Assert.ThrowsAsync
- **AggregateException** Handle wrapped exceptions en Task
- **Unobserved Exceptions** Be aware of unobserved task exceptions

**Performance Testing**

- **Execution Time** Measure async operation timing
- **Concurrency** Test behavior under concurrent load
- **Resource Usage** Monitor resource usage during async operations

**Best Practices**

- **Avoid Async Void** Never return async void except para event handlers
- **Test Cancellation** Always test cancellation token behavior
- **Exception Propagation** Verify exceptions propagate correctly
- **Resource Disposal** Ensure proper resource disposal en async methods

### 70. Explica el patrón AAA (Arrange, Act, Assert) en unit testing

**Contexto** Test structure, readability, maintainability.

**Respuesta detallada**

**AAA Pattern** (Arrange, Act, Assert) es una estructura standard para organizing unit tests que improves readability y maintainability.

**Arrange Phase**

- **Setup** Prepare all necessary preconditions y inputs
- **Test Data** Create test data y objects needed
- **Mock Configuration** Configure mocks y stubs
- **State Initialization** Set up initial state of system under test
- **Dependencies** Prepare all dependencies

**Act Phase**

- **Single Action** Execute the specific behavior being tested
- **Method Call** Usually a single method call
- **Event Trigger** Or trigger a specific event
- **Focus** Should be focused on one specific behavior
- **Minimal** Keep this phase as minimal as possible

**Assert Phase**

- **Verification** Verify expected behavior occurred
- **State Verification** Check final state of objects
- **Behavior Verification** Verify method calls on mocks
- **Exception Verification** Verify expected exceptions
- **Multiple Assertions** Can have multiple related assertions

**Benefits of AAA Pattern**

**Readability**

- **Clear Structure** Easy to understand test structure
- **Logical Flow** Natural flow from setup to action to verification
- **Consistency** Consistent structure across all tests
- **Documentation** Tests serve as clear documentation

**Maintainability**

- **Isolated Changes** Changes typically affect only one section
- **Easy Debugging** Easy to identify which part fails
- **Refactoring** Easier to refactor when structure is clear
- **Code Review** Easier to review structured tests

**Variations y Extensions**

**Given-When-Then (BDD)**

- **Given** Equivalent to Arrange (preconditions)
- **When** Equivalent to Act (action)
- **Then** Equivalent to Assert (expected outcome)
- **Natural Language** More natural language approach

**Setup-Exercise-Verify-Teardown**

- **Setup** Arrange phase
- **Exercise** Act phase
- **Verify** Assert phase
- **Teardown** Cleanup phase (often automated)

**Common Anti-patterns**

**Multiple Acts**

- **Problem** Testing multiple behaviors en single test
- **Solution** Split into separate tests
- **Impact** Harder to understand what's being tested

**Arrange in Act**

- **Problem** Setting up data during action phase
- **Solution** Move setup to arrange phase
- **Impact** Unclear test structure

**Missing Arrange**

- **Problem** Not setting up necessary preconditions
- **Solution** Explicit arrange phase even if minimal
- **Impact** Unclear test dependencies

**Best Practices**

- **Empty Lines** Use empty lines to separate phases visually
- **Comments** Consider phase comments para complex tests
- **Single Concept** Test one concept per test method
- **Minimal Arrange** Keep arrange phase as simple as possible
- **Clear Asserts** Use descriptive assertion messages

### 71. ¿Qué es Code Coverage y cuáles son sus limitaciones?

**Contexto** Metrics interpretation, quality vs quantity, mutation testing.

**Respuesta detallada**

**Code Coverage** measures what percentage of code is executed during test runs, pero no necessarily indicates test quality.

**Types of Coverage**

**Line Coverage**

- **Measurement** Percentage of lines executed
- **Simple** Easiest to understand y measure
- **Limitations** Doesn't account para partial line execution
- **Use** Good starting point para coverage analysis

**Branch Coverage**

- **Measurement** Percentage of decision branches taken
- **Control Flow** Covers if/else, switch statements
- **More Thorough** Better than line coverage
- **Edge Cases** Helps identify untested edge cases

**Function/Method Coverage**

- **Measurement** Percentage of functions called
- **Coarse Grained** High-level view of coverage
- **Entry Points** Ensures all entry points tested
- **Limitations** Doesn't indicate thoroughness of testing

**Statement Coverage**

- **Measurement** Percentage of statements executed
- **Granular** More granular than line coverage
- **Multiple Statements** Handles multiple statements per line
- **Accuracy** More accurate than line coverage

**Limitations of Code Coverage**

**Quality vs Quantity**

- **False Confidence** High coverage doesn't guarantee good tests
- **Test Quality** Doesn't measure assertion quality
- **Edge Cases** May miss important edge cases
- **Business Logic** Doesn't validate business requirements

**Coverage Gaming**

- **Meaningless Tests** Tests written just to increase coverage
- **No Assertions** Tests que execute code but don't verify behavior
- **Trivial Tests** Testing getters/setters instead of business logic
- **Metrics Obsession** Focus on metrics instead of quality

**Technical Limitations**

- **Generated Code** Includes generated code en metrics
- **Dead Code** Identifies dead code but not its importance
- **Integration** Doesn't cover integration scenarios
- **Performance** Doesn't indicate performance characteristics

**Alternative Approaches**

**Mutation Testing**

- **Concept** Introduce bugs y verify tests catch them
- **Quality Measure** Actually measures test effectiveness
- **Tools** Stryker.NET para .NET applications
- **Expensive** More computationally expensive than coverage

**Property-Based Testing**

- **Random Inputs** Test con randomly generated inputs
- **Edge Cases** Better at finding edge cases
- **Comprehensive** More comprehensive than example-based tests
- **Frameworks** FsCheck, AutoFixture para .NET

**Behavior-Driven Development**

- **Requirements Focus** Focus on business requirements
- **User Stories** Tests derived from user stories
- **Communication** Better communication con stakeholders
- **Quality** Higher quality tests aligned con business value

**Best Practices**

- **Coverage Goals** Aim para 80-90% coverage as guideline, not rule
- **Quality First** Focus on test quality before coverage metrics
- **Critical Paths** Ensure critical business paths are well tested
- **Integration** Combine coverage con other quality metrics
- **Review** Regularly review both covered y uncovered code

### 72. ¿Cómo testearías componentes que dependen de external services?

**Contexto** Test doubles, integration testing, test containers.

**Respuesta detallada**

**Testing components con external dependencies** requires strategies para isolate tests while ensuring realistic behavior.

**Test Doubles Strategy**

**Mock External Services**

- **Unit Testing** Replace external services con mocks
- **Control** Full control over responses y behavior
- **Speed** Fast execution, no network calls
- **Isolation** True unit testing isolation
- **Limitations** May not catch integration issues

**Stub HTTP Responses**

- **HTTP Clients** Use tools like WireMock para stub HTTP responses
- **Realistic** More realistic than simple mocks
- **Scenarios** Test various response scenarios (success, errors, timeouts)
- **Flexible** Easy to configure different responses

**Integration Testing Approaches**

**Test Containers**

- **Real Services** Run real services en Docker containers
- **Isolation** Each test gets fresh container
- **Realistic** Most realistic testing environment
- **Performance** Slower than mocks but more thorough
- **Examples** TestContainers para databases, message queues

**Embedded Services**

- **In-Memory** Use in-memory versions when available
- **Examples** In-memory databases, embedded message brokers
- **Fast** Faster than full external services
- **Limitations** May behave differently than real services

**Shared Test Environment**

- **Dedicated Environment** Shared test environment para integration tests
- **Data Management** Careful test data management required
- **Isolation** Tests may interfere con each other
- **Maintenance** Requires maintenance y monitoring

**Testing Strategies by Service Type**

**Database Dependencies**

- **In-Memory Database** SQLite in-memory para simple cases
- **Test Database** Dedicated test database instance
- **Repository Pattern** Mock repository interface
- **Database Containers** Use database containers para realistic testing

**HTTP API Dependencies**

- **HTTP Client Mocking** Mock HttpClient responses
- **Integration Tests** Test against real APIs en test environment
- **Contract Testing** Use contract testing tools
- **Service Virtualization** Create virtual services

**Message Queue Dependencies**

- **In-Memory Queues** Use in-memory implementations
- **Test Brokers** Embedded test message brokers
- **Mock Publishers** Mock message publishing
- **End-to-End** Test complete message flow

**File System Dependencies**

- **In-Memory File System** Use in-memory file system implementations
- **Temporary Directories** Create y cleanup temporary test directories
- **Mock File Operations** Mock file system operations
- **Test Fixtures** Use test fixture files

**Best Practices**

**Test Pyramid Application**

- **Unit Tests** Mock external dependencies
- **Integration Tests** Test con real services occasionally
- **Contract Tests** Verify interface contracts
- **End-to-End Tests** Full system testing sparingly

**Error Scenario Testing**

- **Network Failures** Test network timeout y connection failures
- **Service Unavailable** Test when external service is down
- **Malformed Responses** Test handling of unexpected responses
- **Rate Limiting** Test rate limiting scenarios

**Test Data Management**

- **Isolated Data** Each test uses isolated test data
- **Cleanup** Proper cleanup after tests
- **Seed Data** Consistent seed data para tests
- **Data Builders** Use builder pattern para test data creation

### 73. Explica el concepto de Dependency Injection en testing

**Contexto** TestHost, WebApplicationFactory, service replacement.

**Respuesta detallada**

**Dependency Injection en testing** enables replacing production dependencies con test doubles para achieve isolation y control durante testing.

**DI Container en Testing**

**Service Registration**

- **Test Services** Register test-specific implementations
- **Override Production** Override production services con test versions
- **Scoped Services** Control service lifetimes durante tests
- **Configuration** Separate test configuration from production

**Service Replacement Strategies**

- **Mock Services** Replace services con mocks
- **Fake Implementations** Use simplified test implementations
- **Test Doubles** Various types of test doubles
- **In-Memory Services** Use in-memory versions cuando available

**ASP.NET Core Testing**

**TestHost**

- **In-Memory Server** Run application en in-memory test server
- **No Network** No actual network calls required
- **Fast** Very fast execution
- **Isolation** Completely isolated test environment

**WebApplicationFactory**

- **Test Server** Creates test server para integration testing
- **Service Override** Easy service replacement
- **Configuration Override** Override configuration para testing
- **Custom Startup** Use custom startup para tests

**Integration Test Setup**

- **ConfigureTestServices** Override services specifically para tests
- **Test Database** Configure test database connections
- **Authentication** Configure test authentication
- **Logging** Configure test logging

**Testing Patterns**

**Constructor Injection Testing**

- **Direct Instantiation** Create objects directly con test dependencies
- **Control** Full control over dependencies
- **Simple** Simple y straightforward
- **Unit Testing** Perfect para unit testing

**Factory Pattern Testing**

- **Test Factories** Create factories que return test implementations
- **Flexibility** Easy to switch between different implementations
- **Configuration** Configure factories para different test scenarios

**Builder Pattern Testing**

- **Test Builders** Build test objects con specific configurations
- **Fluent API** Readable test setup
- **Default Values** Sensible defaults con override capability
- **Complex Objects** Handle complex object creation

**Mock Framework Integration**

**Service Mocking**

- **Mock Registration** Register mocks en DI container
- **Behavior Configuration** Configure mock behavior en test setup
- **Verification** Verify interactions after test execution
- **Shared Mocks** Share mocks across related tests

**Partial Mocking**

- **Real Implementation** Use real implementation para some services
- **Mock Dependencies** Mock only specific dependencies
- **Hybrid Approach** Balance entre realism y control

**Best Practices**

**Test Organization**

- **Base Test Classes** Create base classes para common test setup
- **Test Fixtures** Use fixtures para shared test infrastructure
- **Setup/Teardown** Proper setup y teardown of test services
- **Resource Management** Proper disposal of test resources

**Service Lifecycle**

- **Scoped Services** Use appropriate service scopes
- **Stateful Services** Handle stateful services carefully
- **Singleton Services** Be careful con singleton services en tests
- **Disposal** Ensure proper disposal of services

**Configuration Management**

- **Test Configuration** Separate test configuration files
- **Environment Variables** Use environment variables para test settings
- **Secrets** Handle test secrets appropriately
- **Dynamic Configuration** Support dynamic test configuration

### 74. ¿Qué son los Property-based tests?

**Contexto** FsCheck, randomized testing, test case generation.

**Respuesta detallada**

**Property-based testing** verifies that certain properties hold true para a wide range of inputs, rather than testing specific examples.

**Core Concepts**

**Properties**

- **Invariants** Conditions que should always be true
- **Relationships** Relationships between inputs y outputs
- **Behavior** Expected behavior regardless of input
- **Examples** Sorting preserves length, encryption/decryption roundtrip

**Generators**

- **Random Input** Generate random test inputs
- **Diverse Coverage** Cover wide range of input space
- **Edge Cases** Include edge cases automatically
- **Constraints** Generate inputs meeting specific constraints

**Shrinking**

- **Minimal Failing Case** Find smallest input que causes failure
- **Debugging** Easier debugging con minimal examples
- **Automatic** Framework automatically shrinks failing inputs
- **Efficiency** More efficient than manual test case creation

**Benefits over Example-based Testing**

**Coverage**

- **Input Space** Tests much larger input space
- **Edge Cases** Automatically finds edge cases
- **Unexpected Scenarios** Discovers scenarios developers didn't consider
- **Comprehensive** More comprehensive than hand-written examples

**Maintenance**

- **Less Maintenance** Fewer test cases to maintain
- **Generative** Tests generate themselves
- **Robust** More robust against code changes
- **Documentation** Properties serve as documentation

**FsCheck Framework**

**Property Definition**

- **Functional Style** Define properties using functional syntax
- **Arbitrary Generators** Built-in generators para common types
- **Custom Generators** Create custom generators para domain objects
- **Combinators** Combine generators para complex scenarios

**Integration**

- **NUnit Integration** Integrates con NUnit testing framework
- **xUnit Integration** Works con xUnit.net
- **MSTest Support** Can be used con MSTest
- **Standalone** Can run independently

**Common Property Patterns**

**Roundtrip Properties**

- **Serialization** Serialize then deserialize equals original
- **Encoding** Encode then decode equals original
- **Transformation** Apply transformation then inverse equals original
- **Example** JSON serialization roundtrip

**Invariant Properties**

- **Data Structure** Properties que should always hold
- **Business Rules** Business invariants
- **Mathematical** Mathematical properties
- **Example** List length after adding element

**Metamorphic Properties**

- **Related Inputs** Relationship between different inputs
- **Output Relationships** Expected relationships between outputs
- **Transformations** How output changes con input transformation
- **Example** Sorting twice equals sorting once

**Best Practices**

**Property Design**

- **Simple Properties** Start con simple, obvious properties
- **Multiple Properties** Test multiple properties para same function
- **Business Properties** Focus on business-relevant properties
- **Clear Intent** Properties should clearly express intent

**Generator Design**

- **Realistic Data** Generate realistic test data
- **Edge Cases** Include important edge cases
- **Performance** Consider performance of generator
- **Constraints** Respect domain constraints

**Integration Strategy**

- **Complement Examples** Use junto con example-based tests
- **Critical Functions** Focus on critical business functions
- **Complex Logic** Especially valuable para complex algorithms
- **Regression Testing** Excellent para regression testing

### 75. ¿Cómo implementarías testing para aplicaciones que usan Entity Framework?

**Contexto** In-memory database, test databases, repository pattern.

**Respuesta detallada**

**Testing Entity Framework applications** requires careful consideration de database dependencies, test isolation, y performance.

**Testing Strategies**

**In-Memory Database Provider**

- **Fast Execution** Very fast test execution
- **No Setup** No database setup required
- **Isolation** Perfect isolation between tests
- **Limitations** Different behavior from real database
- **Use Cases** Unit testing, simple integration testing

**SQLite In-Memory**

- **Real Database** More realistic than EF in-memory provider
- **SQL Features** Supports more SQL features
- **Lightweight** Still lightweight y fast
- **Limitations** Some SQL Server features not supported
- **Setup** Requires connection management

**Test Database**

- **Realistic** Most realistic testing environment
- **Full Features** All database features available
- **Slower** Slower than in-memory options
- **Isolation** Requires careful test isolation
- **CI/CD** May require database en CI/CD pipeline

**Repository Pattern Testing**

**Mock Repository**

- **Unit Testing** Pure unit testing approach
- **Fast** Very fast execution
- **Control** Full control over data
- **Isolation** Perfect isolation
- **Limitations** Doesn't test actual data access

**Fake Repository**

- **In-Memory Collections** Use collections para fake implementation
- **Realistic Behavior** More realistic than mocks
- **Testing Logic** Test business logic without database
- **Simple** Simple to implement y maintain

**Test Data Management**

**Database Seeding**

- **Consistent State** Ensure consistent starting state
- **Test Data** Create relevant test data
- **Performance** Balance comprehensive data con performance
- **Cleanup** Proper cleanup after tests

**Data Builders**

- **Object Creation** Fluent API para creating test objects
- **Default Values** Sensible defaults con override capability
- **Relationships** Handle related entities properly
- **Maintainable** Maintainable test data creation

**Transaction Management**

- **Test Transactions** Wrap tests en transactions
- **Rollback** Rollback transactions after tests
- **Isolation** Ensure test isolation
- **Performance** Better performance than recreating database

**Testing Patterns**

**DbContext Testing**

- **Context Per Test** New context para each test
- **Shared Context** Shared context with careful management
- **Factory Pattern** Use factory para context creation
- **Disposal** Proper disposal of contexts

**Migration Testing**

- **Up Migrations** Test migrations apply correctly
- **Down Migrations** Test migration rollbacks
- **Data Migration** Test data transformations
- **Schema Validation** Validate final schema

**Query Testing**

- **LINQ Translation** Verify LINQ queries translate correctly
- **SQL Generation** Check generated SQL quality
- **Performance** Test query performance
- **Result Correctness** Verify query results

**Configuration Testing**

- **Entity Configuration** Test entity configurations
- **Relationships** Test relationship configurations
- **Constraints** Test database constraints
- **Indexes** Verify index configurations

**Best Practices**

**Test Organization**

- **Test Categories** Categorize tests by speed y dependencies
- **Integration Tests** Separate integration tests from unit tests
- **Base Classes** Use base classes para common setup
- **Test Fixtures** Share expensive setup cuando appropriate

**Performance Considerations**

- **Test Speed** Balance realism con test speed
- **Parallel Execution** Consider parallel test execution
- **Resource Usage** Monitor test resource usage
- **CI/CD Impact** Consider impact on build pipeline

**Maintenance**

- **Schema Changes** Handle schema changes en tests
- **Test Data Evolution** Evolve test data con application
- **Refactoring** Refactor tests along con production code
- **Documentation** Document test infrastructure

---

## ⚡ **PERFORMANCE Y OPTIMIZACIÓN (10 preguntas)**

### 76. ¿Cuáles son las principales causas de memory leaks en .NET?

**Contexto** Event subscriptions, static references, unmanaged resources.

**Respuesta detallada**

**Memory leaks** en .NET ocurren cuando objects que ya no se necesitan permanecen reachable para el Garbage Collector, impidiendo su liberación.

**Principales causas de memory leaks**

**Event Subscriptions sin Unsubscribe**

- **Problema** Objects permanecen vivos debido a event handlers
- **Scenario** Publisher mantiene referencias a subscribers
- **Impacto** Subscribers no pueden ser garbage collected
- **Solución** Explicit unsubscription o weak event patterns
- **Ejemplo** Static events que mantienen referencias a instances

**Static References**

- **Problema** Static fields mantienen referencias a objects
- **Lifetime** Static references viven durante toda la application lifetime
- **Collections** Static collections que crecen indefinidamente
- **Caches** Caches estáticos sin eviction policies
- **Singletons** Singletons mal implementados que acumulan state

**Unmanaged Resources**

- **File Handles** Streams, file handles no disposed
- **Database Connections** Connections no cerradas apropiadamente
- **Graphics Resources** Bitmaps, fonts, drawing objects
- **Native Memory** P/Invoke calls que allocate native memory
- **COM Objects** COM interop objects sin proper release

**Circular References con External Resources**

- **Problem** Circular references involving unmanaged resources
- **Timer Objects** Timers que referencian objects y prevent collection
- **Delegates** Long-lived delegates pointing to short-lived objects
- **Finalizers** Improper finalizer implementation

**Large Object Heap (LOH) Issues**

- **Large Arrays** Arrays mayores a 85KB van al LOH
- **Fragmentation** LOH fragmentation causa memory waste
- **Long-lived Objects** Objects que se mueven al LOH y persisten
- **Generation 2** LOH objects promote to Gen 2 quickly

**Threading Related Leaks**

- **Thread Static** ThreadStatic fields en long-running threads
- **ThreadLocal** ThreadLocal objects en thread pools
- **Concurrent Collections** Improper use of concurrent collections
- **Lock Objects** Objects used as lock targets

**Prevention Strategies**

**Proper Resource Management**

- **Using Statements** Use using statements para IDisposable objects
- **Finally Blocks** Cleanup resources en finally blocks
- **Dispose Pattern** Implement IDisposable correctly
- **Weak References** Use weak references cuando appropriate

**Event Management**

- **Unsubscription** Always unsubscribe from events
- **Weak Event Pattern** Use weak event patterns para long-lived publishers
- **Event Aggregators** Use event aggregators para decoupling
- **Automatic Cleanup** Implement automatic cleanup mechanisms

**Memory Monitoring**

- **Profiling Tools** Use memory profilers regularly
- **Performance Counters** Monitor memory usage metrics
- **Diagnostic Tools** Use diagnostic tools para leak detection
- **Load Testing** Test under realistic load conditions

### 77. Explica diferentes generation del Garbage Collector

**Contexto** Gen 0, 1, 2, LOH, performance implications.

**Respuesta detallada**

**Generational Garbage Collection** en .NET organiza objects en generations based on their age y usage patterns para optimize collection performance.

**Generation Structure**

**Generation 0 (Gen 0)**

- **New Objects** All newly allocated objects start here
- **Size** Smallest generation (typically 256KB - 4MB)
- **Collection Frequency** Most frequent collections
- **Performance** Fastest collections
- **Survival** Objects que survive get promoted to Gen 1
- **Typical Content** Short-lived objects, temporary variables

**Generation 1 (Gen 1)**

- **Middle Generation** Buffer between Gen 0 y Gen 2
- **Size** Medium size (typically few MB)
- **Purpose** Temporary storage para objects que survived one collection
- **Collection** Collected less frequently than Gen 0
- **Promotion** Surviving objects move to Gen 2
- **Function** Acts as buffer to protect Gen 2 from frequent collections

**Generation 2 (Gen 2)**

- **Long-lived Objects** Contains long-lived application objects
- **Size** Largest generation (can be very large)
- **Collection Frequency** Least frequent, most expensive
- **Performance** Slowest collections
- **Content** Application state, caches, long-lived references
- **Full Collection** Triggers full garbage collection

**Large Object Heap (LOH)**

- **Size Threshold** Objects >= 85KB go directly to LOH
- **Generation** Treated as part of Gen 2
- **Collection** Only collected during Gen 2 collections
- **Compaction** Not compacted by default (fragmentation risk)
- **Performance** Can cause performance issues due to size

**Collection Process**

**Generational Hypothesis**

- **Young Objects** Young objects are likely to die young
- **Old Objects** Old objects are likely to live longer
- **Efficiency** Focus collection efforts on younger generations
- **Performance** Avoid scanning long-lived objects frequently

**Collection Triggers**

- **Memory Pressure** When generation thresholds are reached
- **Explicit Request** GC.Collect() calls (not recommended)
- **Low Memory** System memory pressure
- **Application Shutdown** During application termination

**Collection Types**

- **Gen 0 Collection** Only collects Gen 0
- **Gen 1 Collection** Collects Gen 0 y Gen 1
- **Gen 2 Collection** Collects all generations + LOH
- **Full Collection** Complete heap cleanup

**Performance Implications**

**Collection Costs**

- **Gen 0** Very fast (microseconds)
- **Gen 1** Fast (few milliseconds)
- **Gen 2** Expensive (tens to hundreds of milliseconds)
- **LOH** Can be very expensive due to size

**Application Impact**

- **Pause Times** Gen 2 collections cause longer pauses
- **Throughput** Frequent Gen 2 collections reduce throughput
- **Latency** Unpredictable pause times affect latency-sensitive applications
- **CPU Usage** GC work competes with application work

**Optimization Strategies**

- **Object Lifetime** Design para appropriate object lifetimes
- **Pooling** Use object pooling para frequently allocated objects
- **Large Objects** Avoid large object allocation cuando possible
- **Promotion** Avoid unnecessary promotion to higher generations

### 78. ¿Qué herramientas usarías para profiling de aplicaciones .NET?

**Contexto** dotTrace, PerfView, Application Insights, custom metrics.

**Respuesta detallada**

**Performance Profiling** requires different tools depending on the type of analysis needed y the environment being profiled.

**Memory Profilers**

**dotMemory (JetBrains)**

- **Features** Memory usage analysis, leak detection, heap snapshots
- **Capabilities** Compare snapshots, analyze object retention
- **Integration** Integrates con Visual Studio y ReSharper
- **Use Cases** Memory leak investigation, optimization
- **Cost** Commercial license required

**PerfView (Microsoft)**

- **Free Tool** Free Microsoft tool para advanced profiling
- **ETW Based** Uses Event Tracing para Windows
- **Capabilities** CPU, memory, ETW events analysis
- **Advanced** Very powerful but requires learning curve
- **GC Analysis** Excellent para garbage collection analysis

**Application Insights**

- **Cloud Native** Azure-based APM solution
- **Real-time** Real-time performance monitoring
- **Distributed** Supports distributed application monitoring
- **Integration** Easy integration con .NET applications
- **Cost** Usage-based pricing model

**Performance Profilers**

**dotTrace (JetBrains)**

- **CPU Profiling** Detailed CPU usage analysis
- **Methods** Timeline, sampling, tracing, memory profiling
- **Hotspots** Identify performance bottlenecks
- **Call Trees** Analyze call hierarchies y execution times
- **Integration** Visual Studio integration

**Visual Studio Diagnostic Tools**

- **Built-in** Integrated en Visual Studio
- **Real-time** Real-time CPU y memory usage
- **Debugging** Available during debugging sessions
- **Events** Custom events y breakpoint analysis
- **Free** Included con Visual Studio

**Specialized Tools**

**BenchmarkDotNet**

- **Micro-benchmarking** Precise micro-benchmark measurements
- **Statistical** Statistical analysis of performance
- **Multiple Frameworks** Test across different .NET versions
- **Methodology** Scientific benchmarking methodology
- **Open Source** Free y open source

**NBomber**

- **Load Testing** Load testing framework para .NET
- **Performance** Application performance under load
- **Scenarios** Complex load testing scenarios
- **Reporting** Detailed performance reports
- **Integration** Integrates con CI/CD pipelines

**Custom Metrics**

**Performance Counters**

- **Windows Counters** Built-in Windows performance counters
- **Custom Counters** Create application-specific counters
- **Real-time** Real-time monitoring capabilities
- **Historical** Historical data collection
- **Integration** Easy integration con monitoring tools

**Structured Logging**

- **Serilog** Structured logging con performance metrics
- **Custom Metrics** Application-specific performance metrics
- **Correlation** Correlate performance con business metrics
- **Analysis** Analyze performance trends over time

**Monitoring Integration**

**APM Solutions**

- **Application Insights** Microsoft's APM solution
- **New Relic** Third-party APM para .NET
- **Datadog** Comprehensive monitoring platform
- **AppDynamics** Enterprise APM solution

**Production Monitoring**

- **Health Checks** Built-in health check endpoints
- **Custom Metrics** Application-specific metrics
- **Alerting** Performance-based alerting
- **Dashboards** Real-time performance dashboards

**Best Practices**

- **Multiple Tools** Use different tools para different analysis types
- **Production Safe** Ensure profiling tools are production-safe
- **Baseline** Establish performance baselines
- **Continuous** Implement continuous performance monitoring
- **Correlation** Correlate performance con business metrics

### 79. ¿Cuál es la diferencia entre `StringBuilder` y string concatenation?

**Contexto** Immutability, memory allocation, performance scenarios.

**Respuesta detallada**

**String vs StringBuilder** performance differences stem from string immutability y memory allocation patterns en .NET.

**String Immutability**

**String Characteristics**

- **Immutable** Strings cannot be modified after creation
- **New Objects** Each concatenation creates new string object
- **Memory Allocation** Each operation allocates new memory
- **Garbage Collection** Old strings become garbage immediately
- **Reference Semantics** String variables hold references to string objects

**Concatenation Performance**

- **Multiple Allocations** Each + operation creates new string
- **Memory Copying** Content must be copied to new memory location
- **Quadratic Complexity** O(n²) para multiple concatenations
- **GC Pressure** Creates pressure on garbage collector
- **Memory Fragmentation** Can cause heap fragmentation

**StringBuilder Characteristics**

**Mutable Buffer**

- **Internal Buffer** Maintains internal character buffer
- **Capacity Growth** Buffer grows exponentially cuando needed
- **In-place Modification** Modifies content without creating new objects
- **Single Allocation** Fewer memory allocations overall
- **Linear Complexity** O(n) para multiple append operations

**Buffer Management**

- **Initial Capacity** Can specify initial buffer size
- **Growth Strategy** Doubles capacity cuando buffer fills
- **Memory Efficiency** Reduces total memory allocations
- **Reusability** Same buffer used para multiple operations

**Performance Comparison**

**Few Concatenations (< 5)**

- **String** Acceptable performance para small numbers
- **StringBuilder** Overhead may not be justified
- **Compiler Optimization** Compiler may optimize simple concatenations
- **Recommendation** String concatenation is fine

**Many Concatenations (> 10)**

- **String** Performance degrades significantly
- **StringBuilder** Consistent performance
- **Memory Usage** StringBuilder uses much less memory
- **Recommendation** Always use StringBuilder

**Loop Concatenations**

- **String** Extremely poor performance en loops
- **StringBuilder** Excellent performance en loops
- **Scalability** StringBuilder scales linearly
- **Best Practice** Never concatenate strings en loops

**Use Case Guidelines**

**Use String Concatenation When**

- **Few Operations** 2-4 simple concatenations
- **Known Values** All values known at compile time
- **String Interpolation** Using string interpolation syntax
- **Readability** Code readability is more important

**Use StringBuilder When**

- **Many Operations** Multiple concatenation operations
- **Loops** Building strings en loops
- **Dynamic Content** Content determined at runtime
- **Performance Critical** Performance is important

**Memory Considerations**

**String Concatenation Memory**

- **Temporary Objects** Creates many temporary string objects
- **Memory Pressure** Increases GC pressure
- **Fragmentation** Can cause heap fragmentation
- **Peak Usage** High peak memory usage

**StringBuilder Memory**

- **Efficient** More memory efficient overall
- **Buffer Size** Consider initial capacity para optimization
- **Reuse** Can clear y reuse same StringBuilder
- **Capacity Management** Monitor capacity vs length

**Best Practices**

- **Initial Capacity** Set appropriate initial capacity cuando known
- **Clear vs New** Clear existing StringBuilder instead of creating new
- **Capacity Monitoring** Monitor capacity usage para optimization
- **String Interpolation** Use string interpolation para simple cases
- **Performance Testing** Measure actual performance en your scenarios

### 80. Explica el concepto de Object Pooling

**Contexto** ArrayPool, ObjectPool, HTTP clients, database connections.

**Respuesta detallada**

**Object Pooling** is a performance optimization technique que reuses objects instead of creating y destroying them repeatedly.

**Core Concepts**

**Why Object Pooling**

- **Allocation Cost** Avoid expensive object allocation
- **GC Pressure** Reduce garbage collection pressure
- **Initialization** Avoid repeated initialization costs
- **Resource Management** Efficient resource utilization
- **Performance** Improve overall application performance

**Pool Management**

- **Pool Size** Optimal pool size balances memory y performance
- **Eviction Policy** Strategy para removing unused objects
- **Thread Safety** Ensure thread-safe pool operations
- **Object Lifecycle** Manage object state y validation

**Built-in Pooling en .NET**

**ArrayPool<T>**

- **Purpose** Pool arrays to avoid allocation
- **Shared Instance** ArrayPool<T>.Shared para common usage
- **Rent/Return** Rent arrays y return when done
- **Size Management** Handles different array sizes efficiently
- **Thread Safety** Thread-safe operations

**ObjectPool<T>**

- **Generic Pooling** Generic object pooling framework
- **Custom Objects** Pool any type of object
- **Policy-based** Configurable pooling policies
- **DI Integration** Integrates con dependency injection
- **ASP.NET Core** Built-in support en ASP.NET Core

**HttpClientFactory**

- **HTTP Client Pooling** Manages HttpClient instances
- **Connection Pooling** Underlying connection pooling
- **DNS Refresh** Handles DNS refresh issues
- **Configuration** Configure client behavior
- **Lifetime Management** Manages client lifetimes

**Database Connection Pooling**

**ADO.NET Connection Pooling**

- **Automatic** Automatic connection pooling
- **Connection Strings** Configured via connection strings
- **Pool Size** Min/max pool size configuration
- **Timeout** Connection timeout management
- **Validation** Connection validation before use

**Entity Framework**

- **DbContext Pooling** Pool DbContext instances
- **Performance** Significant performance improvement
- **Configuration** Configure pool size y behavior
- **State Management** Reset context state between uses

**Custom Object Pooling**

**Implementation Patterns**

- **Queue-based** Use ConcurrentQueue para thread safety
- **Stack-based** Use ConcurrentStack para LIFO behavior
- **Ring Buffer** Circular buffer implementation
- **Partitioned** Partition pool by thread para better performance

**Pool Policies**

- **Maximum Size** Limit maximum pool size
- **Idle Timeout** Remove objects after idle period
- **Validation** Validate objects before returning to pool
- **Reset Strategy** How to reset object state

**Object State Management**

- **Reset Method** Implement method to reset object state
- **Validation** Validate object state when returning to pool
- **Immutable Objects** Consider immutable objects para pooling
- **Stateless Objects** Prefer stateless objects cuando possible

**Use Cases**

**High-Frequency Allocations**

- **Temporary Objects** Objects created y destroyed frequently
- **Request Processing** Objects used durante request processing
- **Computation** Objects used en computational scenarios
- **Data Processing** Objects used para data transformation

**Expensive Objects**

- **Initialization Cost** Objects expensive to initialize
- **Resource Allocation** Objects que allocate expensive resources
- **Network Objects** Objects que establish network connections
- **Large Objects** Large objects que impact GC

**Best Practices**

- **Measure First** Profile before implementing pooling
- **Thread Safety** Ensure pool implementation is thread-safe
- **Pool Size** Tune pool size based on actual usage
- **Object Validation** Validate objects before reuse
- **Memory Leaks** Monitor para memory leaks en pooled objects
- **State Reset** Properly reset object state
- **Exception Safety** Handle exceptions during object reset

### 81. ¿Qué son las Compile-time optimizations en .NET?

**Contexto** JIT optimizations, PGO, ReadyToRun, AOT.

**Respuesta detallada**

**Compile-time optimizations** en .NET include various techniques to improve application performance through compilation strategies.

**JIT (Just-In-Time) Optimizations**

**Basic JIT Optimizations**

- **Method Inlining** Inline small methods para reduce call overhead
- **Dead Code Elimination** Remove unreachable code
- **Constant Folding** Evaluate constants at compile time
- **Loop Optimizations** Optimize loop structures y unrolling
- **Register Allocation** Optimize CPU register usage

**Tiered Compilation**

- **Tier 0** Quick initial compilation para fast startup
- **Tier 1** Optimized compilation after method becomes hot
- **Hot Methods** Profile-guided optimization para frequently called methods
- **Dynamic Optimization** Continuous optimization based on runtime behavior
- **Startup vs Throughput** Balance startup time y steady-state performance

**Profile-Guided Optimization (PGO)**

**Dynamic PGO**

- **Runtime Profiling** Collect profiling data during execution
- **Hot Path Optimization** Optimize frequently executed code paths
- **Type Speculation** Specialize code para common types
- **Devirtualization** Convert virtual calls to direct calls cuando possible
- **Branch Prediction** Optimize branches based on actual usage patterns

**Static PGO**

- **Training Runs** Use training data to guide optimizations
- **Offline Analysis** Analyze code patterns offline
- **Feedback Data** Use feedback data para optimize subsequent compilations
- **Reproducible** More predictable than dynamic PGO

**ReadyToRun (R2R)**

**Pre-compilation Benefits**

- **Startup Performance** Faster application startup
- **Reduced JIT** Less JIT compilation needed at runtime
- **Code Sharing** Share compiled code across processes
- **Deterministic** More predictable performance characteristics

**R2R Limitations**

- **Platform Specific** Must target specific platform
- **Size Overhead** Larger application size
- **Optimization Limits** Cannot perform some runtime optimizations
- **Version Binding** Tied to specific runtime versions

**Ahead-of-Time (AOT) Compilation**

**Native AOT**

- **Single File** Compile to single native executable
- **No Runtime** No need para .NET runtime on target machine
- **Fast Startup** Very fast startup times
- **Small Footprint** Smaller memory footprint
- **Deployment** Simplified deployment model

**AOT Limitations**

- **Reflection Limitations** Limited reflection support
- **Dynamic Code** Cannot generate code at runtime
- **Feature Subset** Only subset of .NET features supported
- **Size Trade-offs** May result en larger executables

**Compiler Optimizations**

**Roslyn Optimizations**

- **Constant Propagation** Propagate constants through code
- **Unreachable Code** Remove unreachable code paths
- **Expression Optimization** Optimize expression trees
- **Pattern Matching** Optimize pattern matching constructs

**IL-level Optimizations**

- **IL Rewriting** Optimize intermediate language
- **Metadata Optimization** Optimize assembly metadata
- **Assembly Trimming** Remove unused code from assemblies
- **Linker Optimizations** Tree-shaking y dead code elimination

**Performance Considerations**

**Compilation Time**

- **Build Performance** Optimizations increase build time
- **Incremental Builds** Impact on incremental build performance
- **CI/CD** Consider impact on build pipelines
- **Development** Balance optimization con development productivity

**Runtime Performance**

- **Startup Time** Different optimizations affect startup differently
- **Steady State** Focus on steady-state performance para long-running applications
- **Memory Usage** Some optimizations trade memory para CPU performance
- **Predictability** Consider performance predictability requirements

**Best Practices**

- **Profile First** Understand actual performance bottlenecks
- **Measure Impact** Measure actual impact of optimizations
- **Appropriate Strategy** Choose strategy based on application type
- **Testing** Test thoroughly con optimizations enabled
- **Monitoring** Monitor production performance
- **Incremental** Apply optimizations incrementally

### 82. ¿Cómo optimizarías una aplicación web ASP.NET Core?

**Contexto** Response caching, compression, bundling, CDN, async patterns.

**Respuesta detallada**

**ASP.NET Core optimization** involves multiple layers from infrastructure to application-level improvements.

**Response Caching**

**Output Caching**

- **Full Page** Cache entire page responses
- **Partial Views** Cache partial view outputs
- **Conditional** Cache based on conditions
- **Vary Headers** Cache different versions based on headers
- **Edge Cases** Handle cache invalidation scenarios

**Response Caching Middleware**

- **HTTP Headers** Use standard HTTP caching headers
- **Cache Profiles** Define reusable caching profiles
- **Memory Cache** In-memory response caching
- **Distributed Cache** Share cache across multiple instances
- **Custom Logic** Implement custom caching logic

**Compression**

**Response Compression**

- **Gzip** Standard compression algorithm
- **Brotli** Modern compression con better ratios
- **Dynamic** Compress responses dynamically
- **Static Files** Pre-compress static files
- **Mime Types** Configure which content types to compress

**Content Optimization**

- **Minification** Minify CSS, JavaScript, HTML
- **Bundling** Bundle multiple files together
- **Tree Shaking** Remove unused code
- **Image Optimization** Optimize image sizes y formats
- **WebP Support** Use modern image formats

**Static File Optimization**

**Static File Middleware**

- **ETag Support** Use ETags para conditional requests
- **Last-Modified** Set appropriate Last-Modified headers
- **Cache Headers** Set long cache headers para static content
- **Content-Type** Proper content type detection
- **Range Requests** Support partial content requests

**CDN Integration**

- **Asset Distribution** Distribute static assets via CDN
- **Geographic Distribution** Serve content from edge locations
- **Cache Control** Implement proper cache control headers
- **Fallback** Provide fallback mechanisms
- **SSL/TLS** Ensure secure content delivery

**Database Optimization**

**Entity Framework Optimization**

- **Connection Pooling** Use connection pooling
- **Query Optimization** Optimize database queries
- **Async Patterns** Use async database operations
- **Batch Operations** Batch database operations
- **Read Replicas** Use read replicas para read-heavy workloads

**Caching Strategies**

- **Memory Cache** In-memory caching para frequently accessed data
- **Distributed Cache** Redis/SQL Server distributed cache
- **Cache-Aside** Implement cache-aside pattern
- **Write-Through** Use write-through caching cuando appropriate

**Async Patterns**

**Async Controllers**

- **Async Actions** Use async action methods
- **Task-based** Return Task/Task<T> from actions
- **ConfigureAwait** Use ConfigureAwait(false) appropriately
- **Cancellation** Support cancellation tokens

**Parallel Processing**

- **Parallel Operations** Use Parallel.ForEach para CPU-bound work
- **Task.WhenAll** Execute multiple async operations concurrently
- **Channel** Use System.Threading.Channels para producer-consumer scenarios
- **Background Services** Use background services para long-running tasks

**Memory Management**

**Object Pooling**

- **ArrayPool** Use ArrayPool para temporary arrays
- **ObjectPool** Pool expensive objects
- **StringBuilder** Pool StringBuilder instances
- **HttpClient** Use HttpClientFactory para HTTP clients

**Garbage Collection**

- **Server GC** Enable server GC para multi-core servers
- **Allocation Patterns** Minimize allocations en hot paths
- **Large Objects** Avoid large object heap allocations
- **Memory Pressure** Monitor memory usage patterns

**Middleware Optimization**

**Pipeline Efficiency**

- **Minimal Middleware** Use only necessary middleware
- **Order Matters** Optimize middleware order
- **Short Circuiting** Short-circuit cuando possible
- **Conditional Middleware** Apply middleware conditionally

**Custom Middleware**

- **Efficient Implementation** Write efficient middleware
- **Async Patterns** Use async patterns correctly
- **Error Handling** Efficient error handling
- **Logging** Minimize logging overhead en hot paths

**Configuration y Deployment**

**Hosting Configuration**

- **Kestrel Tuning** Tune Kestrel server settings
- **Thread Pool** Configure thread pool settings
- **Connection Limits** Set appropriate connection limits
- **Request Limits** Configure request size limits

**Environment Configuration**

- **Production Settings** Optimize settings para production
- **Logging Levels** Set appropriate logging levels
- **Error Handling** Efficient error handling en production
- **Health Checks** Implement efficient health checks

**Monitoring y Profiling**

- **Application Insights** Monitor performance metrics
- **Custom Metrics** Track application-specific metrics
- **Profiling** Regular performance profiling
- **Load Testing** Test under realistic load conditions

### 83. Explica el concepto de CPU vs I/O bound operations

**Contexto** Threading strategies, async patterns, Task.Run usage.

**Respuesta detallada**

**CPU-bound vs I/O-bound operations** require different optimization strategies debido a their fundamentally different performance characteristics.

**CPU-bound Operations**

**Characteristics**

- **Processor Intensive** Operations que primarily use CPU resources
- **Computational** Mathematical calculations, data processing
- **Memory Access** Frequent memory access patterns
- **Blocking** Block thread until computation completes
- **Examples** Image processing, encryption, complex algorithms

**Performance Bottlenecks**

- **CPU Utilization** Limited by available CPU cores
- **Parallelization** Benefits from parallel processing
- **Cache Efficiency** Memory cache utilization affects performance
- **Algorithm Complexity** Big O complexity directly impacts performance

**Optimization Strategies**

- **Parallel Processing** Use Parallel.ForEach y PLINQ
- **Task.Run** Offload work to thread pool
- **Partitioning** Partition work efficiently across cores
- **Data Structures** Use cache-friendly data structures
- **SIMD** Use Single Instruction Multiple Data cuando available

**I/O-bound Operations**

**Characteristics**

- **External Resources** Wait para external resources (disk, network, database)
- **Latency** High latency, low CPU utilization
- **Non-blocking Potential** Can be made non-blocking con async
- **Throughput** Limited by I/O subsystem throughput
- **Examples** File operations, HTTP requests, database queries

**Performance Bottlenecks**

- **Latency** Network y disk latency
- **Bandwidth** Available bandwidth limits
- **Connection Limits** Number of concurrent connections
- **Resource Contention** Contention para shared resources

**Optimization Strategies**

- **Async/Await** Use async patterns to avoid blocking threads
- **Connection Pooling** Pool expensive connections
- **Batching** Batch I/O operations cuando possible
- **Caching** Cache I/O results to avoid repeated operations
- **Compression** Reduce data transfer overhead

**Threading Strategies**

**CPU-bound Threading**

- **Thread Pool** Use thread pool para CPU-bound work
- **Parallel.ForEach** Parallelize CPU-intensive loops
- **PLINQ** Parallel LINQ para data processing
- **Task.Run** Move CPU work off UI thread
- **Degree of Parallelism** Limit parallelism to available cores

**I/O-bound Threading**

- **Async/Await** Primary pattern para I/O-bound operations
- **No Task.Run** Avoid Task.Run para I/O-bound work
- **ConfigureAwait** Use ConfigureAwait(false) en libraries
- **Cancellation** Support cancellation tokens
- **Completion Ports** Let I/O completion ports handle threading

**Task.Run Usage Guidelines**

**When to Use Task.Run**

- **CPU-bound Work** Offload CPU-intensive work from UI thread
- **Blocking Operations** Wrap blocking operations cuando async alternative unavailable
- **Synchronous APIs** Bridge synchronous APIs en async context
- **Long-running Computation** Prevent blocking calling thread

**When NOT to Use Task.Run**

- **I/O-bound Operations** Use native async methods instead
- **Already Async** Don't wrap already async operations
- **Server Applications** Avoid en ASP.NET Core controllers
- **Library Code** Generally avoid en library implementations

**Async Patterns**

**Async Best Practices**

- **Async All the Way** Use async throughout call stack
- **ValueTask** Use ValueTask para frequently synchronous results
- **AsyncEnumerable** Use IAsyncEnumerable para streaming data
- **Channels** Use System.Threading.Channels para producer-consumer

**Common Pitfalls**

- **Sync over Async** Avoid .Result y .Wait() on async operations
- **Thread Pool Starvation** Don't block thread pool threads
- **SynchronizationContext** Be aware of sync context capture
- **Exception Handling** Proper async exception handling

**Performance Considerations**

**CPU-bound Performance**

- **Core Utilization** Maximize CPU core utilization
- **Memory Bandwidth** Consider memory bandwidth limitations
- **Cache Locality** Optimize para CPU cache efficiency
- **Algorithm Choice** Choose appropriate algorithms y data structures

**I/O-bound Performance**

- **Concurrency** Maximize concurrent I/O operations
- **Connection Management** Efficient connection management
- **Buffer Sizes** Optimize buffer sizes para I/O operations
- **Batching** Batch operations to reduce overhead

**Monitoring y Measurement**

- **CPU Metrics** Monitor CPU utilization y thread usage
- **I/O Metrics** Track I/O latency y throughput
- **Thread Pool** Monitor thread pool usage
- **Performance Counters** Use relevant performance counters
- **Profiling** Profile to identify actual bottlenecks

### 84. ¿Qué es Lock Contention y cómo evitarla?

**Contexto** Concurrent collections, lock-free algorithms, partitioning.

**Respuesta detallada**

**Lock Contention** occurs cuando multiple threads compete para the same lock, causing performance degradation y reduced scalability.

**Understanding Lock Contention**

**What Causes Contention**

- **Shared Resources** Multiple threads accessing same resource
- **Critical Sections** Long-running code en critical sections
- **Fine-grained Locking** Too many locks causing coordination overhead
- **Hot Spots** Frequently accessed shared data
- **Poor Lock Design** Inappropriate locking strategies

**Performance Impact**

- **Thread Blocking** Threads wait para lock availability
- **Context Switching** Expensive context switches between threads
- **Reduced Throughput** Overall system throughput decreases
- **Scalability Issues** Performance doesn't improve con more cores
- **Deadlock Risk** Increased risk of deadlocks

**Avoiding Lock Contention**

**Lock-Free Data Structures**

- **ConcurrentQueue<T>** Lock-free queue implementation
- **ConcurrentStack<T>** Lock-free stack implementation
- **ConcurrentDictionary<T>** Highly concurrent dictionary
- **Atomic Operations** Use Interlocked class para atomic operations
- **Compare-and-Swap** Use CAS operations cuando appropriate

**Concurrent Collections**

- **Thread-Safe** Built-in thread safety
- **Performance** Optimized para concurrent access
- **Scalability** Scale better than manual locking
- **API Design** Designed para concurrent scenarios
- **Internal Optimization** Use advanced techniques internally

**Lock Elimination Strategies**

**Immutable Data**

- **No Locks Needed** Immutable objects don't need locking
- **Functional Programming** Use immutable data structures
- **Copy-on-Write** Create new instances instead of modifying
- **Read-Heavy Scenarios** Excellent para read-heavy workloads

**Thread-Local Storage**

- **ThreadLocal<T>** Each thread has its own instance
- **ThreadStatic** Static variables per thread
- **Elimination** Eliminates contention completely
- **Aggregation** Aggregate results cuando needed

**Partitioning**

- **Data Partitioning** Split data across multiple partitions
- **Hash-based** Use hash functions para partition assignment
- **Range-based** Partition based on data ranges
- **Reduced Contention** Each partition has separate lock

**Lock Optimization Techniques**

**Reader-Writer Locks**

- **ReaderWriterLockSlim** Allow multiple readers, single writer
- **Read-Heavy Scenarios** Optimize para read-heavy workloads
- **Upgrade/Downgrade** Support lock upgrades y downgrades
- **Fairness** Consider fairness policies

**Lock-Free Algorithms**

- **Compare-and-Swap** Use CAS operations
- **Optimistic Concurrency** Assume no conflicts, retry on conflict
- **Hazard Pointers** Manage memory en lock-free structures
- **Memory Ordering** Consider memory ordering requirements

**Advanced Techniques**

**Actor Model**

- **Message Passing** Communicate via messages instead of shared state
- **Isolation** Each actor has isolated state
- **No Shared State** Eliminates need para locking
- **Frameworks** Use actor frameworks like Akka.NET

**Software Transactional Memory**

- **Transactions** Group operations into transactions
- **Optimistic** Optimistic concurrency control
- **Retry** Automatic retry on conflicts
- **Composability** Transactions can be composed

**Design Patterns**

**Producer-Consumer**

- **Channels** Use System.Threading.Channels
- **Bounded Queues** Control memory usage con bounded queues
- **Backpressure** Handle backpressure scenarios
- **Multiple Producers/Consumers** Support multiple participants

**Event-Driven Architecture**

- **Loose Coupling** Reduce direct dependencies
- **Async Processing** Process events asynchronously
- **Event Sourcing** Store events instead of current state
- **CQRS** Separate read y write models

**Measurement y Monitoring**

**Detecting Contention**

- **Performance Counters** Monitor lock contention counters
- **Profiling Tools** Use profilers to identify hot locks
- **Custom Metrics** Track lock hold times
- **Thread Dumps** Analyze thread dumps para blocking

**Metrics to Track**

- **Lock Acquisition Time** Time spent waiting para locks
- **Lock Hold Time** Time locks are held
- **Contention Rate** Frequency of lock contention
- **Thread Utilization** How effectively threads are utilized

**Best Practices**

- **Measure First** Identify actual contention before optimizing
- **Start Simple** Begin con simple locking, optimize based on measurements
- **Lock Hierarchy** Establish lock ordering to prevent deadlocks
- **Minimize Critical Sections** Keep locked code sections small
- **Consider Trade-offs** Balance complexity y performance
- **Test Under Load** Test concurrent scenarios under realistic load

### 85. ¿Cómo manejarías high-throughput scenarios en .NET?

**Contexto** Connection pooling, batching, async patterns, hardware utilization.

**Respuesta detallada**

**High-throughput scenarios** require careful architecture y optimization to handle large volumes of requests efficiently.

**Connection Pooling**

**Database Connection Pooling**

- **ADO.NET Pooling** Built-in connection pooling
- **Pool Size Tuning** Optimize min/max pool sizes
- **Connection Lifetime** Configure connection lifetime appropriately
- **Pool Monitoring** Monitor pool health y utilization
- **Multiple Pools** Use separate pools para different databases

**HTTP Connection Pooling**

- **HttpClientFactory** Manage HTTP client instances
- **Connection Limits** Configure connection limits appropriately
- **DNS Refresh** Handle DNS refresh scenarios
- **Keep-Alive** Use HTTP keep-alive para connection reuse
- **Certificate Validation** Optimize certificate validation

**Batching Strategies**

**Database Batching**

- **Bulk Operations** Use bulk insert/update operations
- **Batch Size Optimization** Find optimal batch sizes
- **Transaction Management** Batch operations within transactions
- **Parallel Batching** Process batches en parallel
- **Error Handling** Handle partial batch failures

**Message Batching**

- **Queue Batching** Batch messages before processing
- **Temporal Batching** Batch based on time windows
- **Size-based Batching** Batch based on size thresholds
- **Priority Batching** Batch based on message priority

**Async Patterns**

**Scalable Async Design**

- **Async All the Way** Use async throughout the stack
- **Task-based APIs** Prefer Task-based asynchronous patterns
- **ConfigureAwait** Use ConfigureAwait(false) appropriately
- **Cancellation** Support cancellation throughout
- **Resource Management** Proper resource disposal en async code

**Parallel Processing**

- **Parallel.ForEach** Para CPU-bound parallel work
- **Task.WhenAll** Execute multiple async operations concurrently
- **Partitioner** Use custom partitioners para load balancing
- **Degree of Parallelism** Limit parallelism appropriately

**Hardware Utilization**

**CPU Optimization**

- **Multi-core Usage** Utilize all available CPU cores
- **Thread Pool Tuning** Optimize thread pool settings
- **CPU Affinity** Consider CPU affinity para specific workloads
- **NUMA Awareness** Be aware of NUMA topology
- **Workload Distribution** Distribute work evenly across cores

**Memory Optimization**

- **Object Pooling** Pool frequently allocated objects
- **Memory-mapped Files** Use memory-mapped files para large datasets
- **Span<T> y Memory<T>** Use para zero-allocation scenarios
- **Large Object Heap** Manage LOH allocations carefully
- **GC Tuning** Tune garbage collection settings

**I/O Optimization**

**Disk I/O**

- **Async File Operations** Use async file I/O
- **Sequential Access** Optimize para sequential access patterns
- **Buffer Sizes** Optimize buffer sizes para I/O operations
- **SSD Optimization** Take advantage of SSD characteristics
- **Memory-mapped Files** Use para large file processing

**Network I/O**

- **Keep-Alive Connections** Reuse network connections
- **Compression** Use compression para network traffic
- **Multiplexing** Use HTTP/2 multiplexing
- **CDN** Use CDNs para static content delivery

**Caching Strategies**

**Multi-level Caching**

- **L1 Cache** In-memory application cache
- **L2 Cache** Distributed cache (Redis)
- **L3 Cache** CDN y edge caching
- **Cache Hierarchy** Design effective cache hierarchy
- **Cache Invalidation** Efficient cache invalidation strategies

**Cache Optimization**

- **Hit Ratio** Optimize cache hit ratios
- **Eviction Policies** Choose appropriate eviction policies
- **Cache Warming** Pre-populate caches strategically
- **Cache Partitioning** Partition caches para better locality

**Architecture Patterns**

**Microservices Architecture**

- **Service Decomposition** Break monoliths into services
- **Independent Scaling** Scale services independently
- **Technology Diversity** Use optimal technology para each service
- **Fault Isolation** Isolate failures to specific services

**Event-Driven Architecture**

- **Loose Coupling** Reduce direct service dependencies
- **Async Processing** Process events asynchronously
- **Event Streaming** Use event streaming platforms
- **CQRS** Separate read y write models

**Load Balancing**

**Application Load Balancing**

- **Round Robin** Distribute requests evenly
- **Weighted** Weight based on server capacity
- **Health Checks** Route only to healthy instances
- **Session Affinity** Handle stateful applications

**Database Load Balancing**

- **Read Replicas** Scale read operations
- **Sharding** Partition data across databases
- **Write Splitting** Separate read y write workloads
- **Connection Pooling** Pool connections effectively

**Monitoring y Observability**

**Performance Metrics**

- **Throughput** Requests/transactions per second
- **Latency** Response time percentiles
- **Error Rates** Error frequency y types
- **Resource Utilization** CPU, memory, disk, network usage

**Application Performance Monitoring**

- **Real-time Monitoring** Monitor performance en real-time
- **Alerting** Set up performance-based alerts
- **Distributed Tracing** Trace requests across services
- **Custom Metrics** Track business-specific metrics

**Best Practices**

- **Measure Everything** Comprehensive monitoring y measurement
- **Bottleneck Identification** Identify y address bottlenecks systematically
- **Gradual Optimization** Make incremental improvements
- **Load Testing** Test under realistic high-throughput scenarios
- **Capacity Planning** Plan para growth y peak loads
- **Documentation** Document performance characteristics y optimizations

---

## 🔒 **SEGURIDAD (8 preguntas)**

### 86. ¿Cómo prevendrías SQL Injection en aplicaciones .NET?

**Contexto** Parameterized queries, ORM usage, input validation.

**Respuesta detallada**

**SQL Injection** es una de las vulnerabilidades más críticas en aplicaciones web que permite a atacantes ejecutar comandos SQL maliciosos.

**Parameterized Queries**

**ADO.NET Parameters**

- **SqlParameter** Use parámetros SQL en lugar de concatenación de strings
- **Type Safety** Los parámetros SQL proporcionan type safety
- **Automatic Escaping** El motor de base de datos escapa automáticamente los valores
- **Performance** Los queries parametrizados pueden ser cached y reutilizados
- **Best Practice** Nunca concatenar user input directamente en SQL strings

**Entity Framework Protection**

- **LINQ Queries** LINQ to Entities genera automáticamente queries parametrizados
- **FromSqlRaw** Use parámetros cuando utilice raw SQL
- **Interpolation** String interpolation en FromSqlInterpolated es seguro
- **Dynamic Queries** Evite construir dynamic queries con user input

**Input Validation**

**Validation Strategies**

- **Whitelist Validation** Validar contra lista de valores permitidos
- **Data Type Validation** Verificar tipos de datos esperados
- **Length Validation** Limitar longitud de inputs
- **Format Validation** Validar formatos específicos (email, phone, etc.)
- **Business Rules** Implementar reglas de negocio en validation

**ASP.NET Core Validation**

- **Model Validation** Use Data Annotations para model validation
- **Custom Validators** Implement custom validation attributes
- **Fluent Validation** Use FluentValidation para validation logic compleja
- **Server-side Validation** Siempre validate en server-side

**Additional Security Measures**

**Least Privilege Principle**

- **Database Permissions** Use minimal database permissions
- **Stored Procedures** Consider stored procedures para complex operations
- **Database Users** Use separate database users para different application components
- **Connection Strings** Secure connection string storage

**Error Handling**

- **Generic Error Messages** No expose database structure en error messages
- **Logging** Log security events sin expose sensitive data
- **Error Pages** Use generic error pages en production
- **Stack Traces** No expose stack traces to end users

**Code Review y Testing**

- **Static Analysis** Use static analysis tools para detect SQL injection
- **Security Testing** Include security testing en development process
- **Penetration Testing** Regular penetration testing
- **Code Reviews** Focus on security during code reviews

### 87. Explica diferentes tipos de authentication en ASP.NET Core

**Contexto** JWT, cookies, OAuth2, Identity framework.

**Respuesta detallada**

**Authentication en ASP.NET Core** proporciona múltiples esquemas para verificar la identidad de usuarios.

**Cookie Authentication**

**Traditional Cookies**

- **Server-side Sessions** Store session data en server
- **Automatic Management** ASP.NET Core maneja cookie lifecycle
- **CSRF Protection** Requiere CSRF protection
- **Scalability** Challenges con multiple server instances
- **Use Cases** Traditional web applications, server-rendered pages

**Configuration**

- **Cookie Settings** Configure cookie name, expiration, security settings
- **Login Path** Configure login y logout paths
- **Access Denied** Configure access denied path
- **Sliding Expiration** Configure sliding expiration behavior

**JWT (JSON Web Tokens)**

**Token Structure**

- **Header** Algorithm y token type information
- **Payload** Claims about the user
- **Signature** Cryptographic signature para verification
- **Stateless** No server-side storage required
- **Self-contained** All information contained en token

**JWT Benefits**

- **Scalability** Stateless nature supports horizontal scaling
- **Cross-domain** Works across different domains
- **Mobile Apps** Ideal para mobile y SPA applications
- **Microservices** Easy to share entre microservices
- **Performance** No server-side lookup required

**OAuth2 y OpenID Connect**

**OAuth2 Framework**

- **Authorization** Framework para authorization, not authentication
- **Third-party Access** Allow third-party access to resources
- **Scopes** Fine-grained permissions through scopes
- **Grant Types** Different flows para different scenarios
- **Resource Protection** Protect APIs y resources

**OpenID Connect**

- **Authentication Layer** Authentication layer built on OAuth2
- **Identity Token** Provides identity information
- **User Info** Standardized user information endpoint
- **Discovery** Automatic discovery of provider capabilities
- **Integration** Easy integration con identity providers

**ASP.NET Core Identity**

**Identity Framework**

- **User Management** Comprehensive user management system
- **Password Management** Password hashing, validation, reset
- **Role Management** Role-based authorization
- **Claims Support** Claims-based identity
- **External Providers** Integration con external identity providers

**Identity Features**

- **Two-Factor Authentication** Built-in 2FA support
- **Account Lockout** Automatic account lockout
- **Password Policies** Configurable password policies
- **Email Confirmation** Email verification workflows
- **Phone Confirmation** SMS verification support

**External Authentication**

**Social Providers**

- **Google** Google OAuth integration
- **Facebook** Facebook Login integration
- **Microsoft** Microsoft Account integration
- **Twitter** Twitter OAuth integration
- **GitHub** GitHub OAuth integration

**Enterprise Providers**

- **Azure Active Directory** Enterprise identity provider
- **SAML** SAML 2.0 support
- **WS-Federation** Windows Federation support
- **LDAP** LDAP integration capabilities

**Multi-factor Authentication**

**MFA Implementation**

- **TOTP** Time-based One-Time Passwords
- **SMS** SMS-based verification
- **Email** Email-based verification
- **Hardware Tokens** Hardware security keys
- **Biometric** Biometric authentication support

**Best Practices**

- **Secure Storage** Secure storage of authentication tokens
- **Token Expiration** Appropriate token expiration policies
- **Refresh Tokens** Implement refresh token mechanism
- **Security Headers** Use appropriate security headers
- **HTTPS** Always use HTTPS para authentication

### 88. ¿Qué es CSRF y cómo prevenirlo?

**Contexto** Anti-forgery tokens, SameSite cookies, CORS.

**Respuesta detallada**

**Cross-Site Request Forgery (CSRF)** es un attack donde un malicious website tricks users into executing unwanted actions on a trusted website.

**How CSRF Works**

**Attack Scenario**

- **Authenticated User** User is logged into legitimate website
- **Malicious Site** User visits malicious website
- **Hidden Request** Malicious site sends request to legitimate site
- **Browser Cookies** Browser automatically includes authentication cookies
- **Unintended Action** Legitimate site processes request as if user intended it

**Vulnerable Operations**

- **State-changing Actions** POST, PUT, DELETE operations
- **Financial Transactions** Money transfers, purchases
- **Account Changes** Password changes, email updates
- **Administrative Actions** User management, configuration changes

**Anti-forgery Tokens**

**ASP.NET Core Anti-forgery**

- **Token Generation** Generate unique tokens para each user session
- **Token Validation** Validate tokens on state-changing requests
- **Automatic Protection** Automatically protect forms y AJAX requests
- **Token Rotation** Rotate tokens periodically
- **Cryptographic Security** Use cryptographically secure tokens

**Implementation**

- **ValidateAntiForgeryToken** Attribute para action methods
- **@Html.AntiForgeryToken()** Razor helper para forms
- **RequestVerificationToken** Hidden form field
- **AJAX Headers** Include token en AJAX request headers

**SameSite Cookies**

**SameSite Attribute**

- **Strict** Cookie only sent para same-site requests
- **Lax** Cookie sent para top-level navigation
- **None** Cookie sent para all requests (requires Secure)
- **Browser Support** Modern browser support
- **Default Behavior** Browser default is Lax

**Configuration**

- **Cookie Policy** Configure SameSite policy globally
- **Authentication Cookies** Set SameSite para authentication cookies
- **Session Cookies** Configure session cookie SameSite
- **Custom Cookies** Set SameSite para custom application cookies

**CORS Configuration**

**Cross-Origin Resource Sharing**

- **Origin Validation** Validate origin headers
- **Allowed Origins** Specify allowed origins explicitly
- **Credentials** Control whether credentials are allowed
- **Preflight Requests** Handle preflight OPTIONS requests
- **Headers** Control allowed headers y methods

**CORS Policies**

- **Named Policies** Create named CORS policies
- **Per-endpoint** Apply different policies to different endpoints
- **Dynamic Origins** Dynamically determine allowed origins
- **Security** Balance functionality con security

**Additional Protection**

**Double Submit Cookies**

- **Two Tokens** Send token en both cookie y request parameter
- **Comparison** Compare tokens on server side
- **Domain Isolation** Works even without server-side storage
- **Implementation** Simpler than traditional anti-forgery tokens

**Custom Headers**

- **X-Requested-With** Require custom headers para AJAX requests
- **Origin Validation** Validate Origin header
- **Referer Validation** Validate Referer header (less reliable)
- **Content-Type** Require specific content types

**Best Practices**

- **Defense in Depth** Use multiple protection mechanisms
- **State-changing Operations** Protect all state-changing operations
- **User Education** Educate users about security risks
- **Regular Testing** Test CSRF protection regularly
- **Security Headers** Use additional security headers

### 89. ¿Cómo manejarías sensitive data en aplicaciones .NET?

**Contexto** Configuration secrets, encryption, key management.

**Respuesta detallada**

**Sensitive Data Management** requires careful handling throughout the application lifecycle, from development to production.

**Configuration Secrets**

**Azure Key Vault**

- **Centralized Storage** Centralized secret storage
- **Access Control** Fine-grained access control
- **Audit Logging** Complete audit trail
- **Rotation** Automatic secret rotation
- **Integration** Easy integration con .NET applications

**Environment Variables**

- **Development** Use environment variables para local development
- **Production** Set environment variables en production environment
- **Container Deployment** Configure secrets en container orchestration
- **Security** Ensure environment variables are not logged
- **Scope** Limit scope of environment variables

**User Secrets (Development)**

- **Development Only** Only para development environment
- **Local Storage** Stored locally, not en source control
- **Easy Management** Simple to add y manage secrets
- **Security** Not suitable para production
- **Integration** Built-in Visual Studio integration

**Encryption Strategies**

**Data at Rest Encryption**

- **Database Encryption** Use database-level encryption features
- **File System Encryption** Encrypt files y directories
- **Column-level Encryption** Encrypt specific database columns
- **Transparent Encryption** Use transparent data encryption
- **Backup Encryption** Encrypt database backups

**Data in Transit Encryption**

- **HTTPS/TLS** Use HTTPS para all communications
- **Certificate Management** Proper SSL certificate management
- **Perfect Forward Secrecy** Use protocols supporting PFS
- **Certificate Pinning** Implement certificate pinning donde appropriate
- **VPN** Use VPNs para internal communications

**Application-level Encryption**

- **Symmetric Encryption** Use AES para bulk data encryption
- **Asymmetric Encryption** Use RSA para key exchange
- **Hashing** Use secure hashing algorithms (SHA-256, bcrypt)
- **Salt** Always use salts con password hashing
- **Key Derivation** Use key derivation functions para passwords

**Key Management**

**Key Storage**

- **Hardware Security Modules** Use HSMs para key storage
- **Key Vault Services** Use cloud key vault services
- **Key Separation** Separate keys from encrypted data
- **Key Rotation** Implement regular key rotation
- **Key Backup** Secure key backup y recovery

**Key Lifecycle**

- **Generation** Secure key generation processes
- **Distribution** Secure key distribution mechanisms
- **Usage** Control y monitor key usage
- **Rotation** Regular key rotation procedures
- **Destruction** Secure key destruction

**Secure Coding Practices**

**Memory Management**

- **SecureString** Use SecureString para sensitive data en memory
- **Zero Memory** Clear sensitive data from memory after use
- **Garbage Collection** Be aware of GC implications
- **Memory Dumps** Prevent sensitive data en memory dumps
- **Swapping** Consider memory swapping implications

**Logging y Monitoring**

- **No Sensitive Data** Never log sensitive information
- **Masked Logging** Mask sensitive data en logs
- **Audit Trails** Maintain audit trails para access
- **Monitoring** Monitor para unauthorized access attempts
- **Alerts** Set up alerts para security events

**Data Classification**

**Sensitivity Levels**

- **Public** No protection required
- **Internal** Limited access within organization
- **Confidential** Restricted access, encryption required
- **Secret** Highest protection level
- **Regulatory** Special handling para regulatory compliance

**Handling Procedures**

- **Access Controls** Implement appropriate access controls
- **Data Minimization** Collect y store only necessary data
- **Retention Policies** Implement data retention policies
- **Disposal** Secure data disposal procedures
- **Documentation** Document data handling procedures

**Compliance Considerations**

- **GDPR** European data protection regulation
- **HIPAA** Healthcare data protection
- **PCI DSS** Payment card industry standards
- **SOX** Financial reporting compliance
- **Industry Standards** Follow industry-specific standards

### 90. Explica el concepto de Claims-based authentication

**Contexto** Identity, authorization policies, role-based vs claims-based.

**Respuesta detallada**

**Claims-based Authentication** es un modelo donde identity information se representa como claims que describe attributes of the user.

**Claims Fundamentals**

**What are Claims**

- **Key-Value Pairs** Claims son key-value pairs describing user attributes
- **Type y Value** Each claim has a type y a value
- **Standard Claims** Predefined claim types (name, email, role)
- **Custom Claims** Application-specific claim types
- **Multiple Values** A claim type can have multiple values

**Claims vs Roles**

- **Granularity** Claims provide more granular information than roles
- **Flexibility** Claims are more flexible y extensible
- **Context** Claims can include contextual information
- **Scalability** Claims scale better than traditional role-based systems
- **Interoperability** Claims work well en federated scenarios

**Claims Principal**

**ClaimsPrincipal Structure**

- **Principal** Represents the user
- **Identities** Can contain multiple identities
- **Claims** Each identity contains claims
- **Authentication Type** How the user was authenticated
- **IsAuthenticated** Whether the user is authenticated

**Claims Identity**

- **Authentication Method** How identity was authenticated
- **Name Claim** Primary name claim
- **Role Claims** Role information as claims
- **Custom Claims** Application-specific claims
- **Label** Optional label para identity

**Authorization Policies**

**Policy-based Authorization**

- **Requirements** Define what must be true para access
- **Policies** Named sets of requirements
- **Handlers** Logic to evaluate requirements
- **Context** Authorization context information
- **Multiple Requirements** Combine multiple requirements

**Policy Configuration**

- **Startup Configuration** Configure policies en Startup.cs
- **Authorize Attribute** Apply policies using [Authorize] attribute
- **Resource-based** Authorization based on specific resources
- **Dynamic Policies** Create policies dynamically
- **Fallback** Configure fallback authorization policies

**Implementation Patterns**

**Claims Transformation**

- **Claims Transformer** Transform claims during authentication
- **Additional Claims** Add claims from external sources
- **Claim Mapping** Map external claims to internal claims
- **Enrichment** Enrich claims con additional information
- **Caching** Cache transformed claims para performance

**Custom Authorization**

- **Custom Requirements** Create custom authorization requirements
- **Custom Handlers** Implement custom authorization handlers
- **Resource Authorization** Authorize access to specific resources
- **Multiple Handlers** Multiple handlers para same requirement
- **Async Authorization** Asynchronous authorization logic

**External Identity Integration**

**Federated Identity**

- **Identity Providers** Integration con external identity providers
- **SAML** SAML 2.0 integration
- **OpenID Connect** OIDC integration
- **Azure AD** Azure Active Directory integration
- **Claims Mapping** Map external claims to internal representation

**Social Authentication**

- **Google** Google OAuth claims
- **Facebook** Facebook Login claims
- **Microsoft** Microsoft Account claims
- **Custom Providers** Custom external providers
- **Claim Normalization** Normalize claims from different providers

**Best Practices**

**Security Considerations**

- **Claim Validation** Validate claims thoroughly
- **Least Privilege** Apply least privilege principle
- **Claim Encryption** Encrypt sensitive claims cuando necessary
- **Audit Trail** Maintain audit trail of authorization decisions
- **Secure Storage** Store claims securely

**Performance Optimization**

- **Claim Caching** Cache claims para performance
- **Minimal Claims** Include only necessary claims
- **Lazy Loading** Load additional claims on demand
- **Claim Compression** Compress large claim sets
- **Database Optimization** Optimize claim storage y retrieval

**Design Guidelines**

- **Claim Design** Design meaningful claim types
- **Consistency** Maintain consistent claim naming
- **Documentation** Document claim types y their usage
- **Versioning** Plan para claim type evolution
- **Interoperability** Design para interoperability

### 91. ¿Qué son las Security Headers y cuáles implementarías?

**Contexto** HTTPS, CSP, HSTS, XSS protection.

**Respuesta detallada**

**Security Headers** proporcionan an additional layer of security by instructing browsers how to handle content y protect against various attacks.

**Essential Security Headers**

**HTTP Strict Transport Security (HSTS)**

- **Purpose** Enforce HTTPS connections
- **max-age** Specify how long to remember HTTPS requirement
- **includeSubDomains** Apply to all subdomains
- **preload** Submit domain para browser preload lists
- **Protection** Prevents SSL stripping attacks
- **Implementation** `Strict-Transport-Security: max-age=31536000; includeSubDomains; preload`

**Content Security Policy (CSP)**

- **Purpose** Prevent XSS y data injection attacks
- **Directives** Control sources para different content types
- **script-src** Control JavaScript sources
- **style-src** Control CSS sources
- **img-src** Control image sources
- **Nonce** Use nonces para inline scripts y styles
- **Report** Report violations para monitoring

**X-Frame-Options**

- **Purpose** Prevent clickjacking attacks
- **DENY** Prevent framing completely
- **SAMEORIGIN** Allow framing only from same origin
- **ALLOW-FROM** Allow framing from specific URI
- **Modern Alternative** Use CSP frame-ancestors directive
- **Implementation** `X-Frame-Options: SAMEORIGIN`

**Additional Protection Headers**

**X-Content-Type-Options**

- **Purpose** Prevent MIME type sniffing
- **nosniff** Disable MIME type sniffing
- **Protection** Prevents executing scripts when wrong MIME type
- **Browser Support** Widely supported
- **Implementation** `X-Content-Type-Options: nosniff`

**X-XSS-Protection**

- **Purpose** Enable browser XSS filtering
- **Enabled** `1; mode=block` enables protection
- **Deprecated** Being phased out en favor of CSP
- **Legacy Support** Still useful para older browsers
- **Implementation** `X-XSS-Protection: 1; mode=block`

**Referrer-Policy**

- **Purpose** Control referrer information sent
- **Policies** Various policies para different scenarios
- **Privacy** Protect user privacy
- **Security** Prevent information leakage
- **Implementation** `Referrer-Policy: strict-origin-when-cross-origin`

**Advanced Security Headers**

**Feature-Policy / Permissions-Policy**

- **Purpose** Control browser features y APIs
- **Granular Control** Fine-grained control over features
- **Privacy** Protect user privacy
- **Performance** Disable unused features
- **Examples** camera, microphone, geolocation permissions

**Expect-CT**

- **Purpose** Certificate Transparency monitoring
- **Enforcement** Enforce CT requirements
- **Reporting** Report CT violations
- **max-age** Specify policy duration
- **Implementation** Monitor certificate transparency

**Implementation en ASP.NET Core**

**Middleware Approach**

- **Custom Middleware** Create custom middleware para headers
- **Built-in Middleware** Use built-in security middleware
- **Configuration** Configure headers en Startup.cs
- **Conditional** Apply headers conditionally based on environment
- **Performance** Minimize overhead

**Third-party Libraries**

- **NWebsec** Comprehensive security header library
- **Configuration** Easy configuration y management
- **Templates** Pre-configured security profiles
- **Integration** Easy integration con ASP.NET Core
- **Flexibility** Flexible configuration options

**Content Security Policy Implementation**

**CSP Directives**

- **default-src** Default source para all content types
- **script-src** JavaScript source restrictions
- **style-src** CSS source restrictions
- **img-src** Image source restrictions
- **connect-src** AJAX, WebSocket, EventSource restrictions
- **font-src** Font source restrictions

**CSP Strategies**

- **Whitelist** Whitelist trusted sources
- **Nonce-based** Use nonces para inline content
- **Hash-based** Use hashes para specific inline content
- **Strict** Implement strict CSP policies
- **Report-only** Test policies using report-only mode

**Monitoring y Reporting**

**Violation Reporting**

- **report-uri** Deprecated reporting mechanism
- **report-to** Modern reporting mechanism
- **Monitoring** Monitor security header violations
- **Analysis** Analyze violation reports
- **Alerting** Set up alerts para security violations

**Security Header Testing**

- **Browser Dev Tools** Test headers en browser
- **Online Tools** Use online security header testing tools
- **Automation** Automate security header testing
- **CI/CD Integration** Include testing en CI/CD pipeline
- **Regular Audits** Regular security header audits

**Best Practices**

- **Gradual Deployment** Deploy headers gradually
- **Testing** Thorough testing before production
- **Monitoring** Monitor para broken functionality
- **Documentation** Document header configurations
- **Regular Review** Regularly review y update headers

### 92. ¿Cómo implementarías Rate Limiting?

**Contexto** DDoS protection, API quotas, sliding windows.

**Respuesta detallada**

**Rate Limiting** controla the number of requests a client can make to prevent abuse y ensure fair resource usage.

**Rate Limiting Algorithms**

**Token Bucket**

- **Bucket Capacity** Maximum number of tokens
- **Refill Rate** Rate at which tokens are added
- **Token Consumption** Each request consumes tokens
- **Burst Handling** Allows bursts up to bucket capacity
- **Smooth Traffic** Smooths traffic over time

**Sliding Window**

- **Time Windows** Fixed time windows para counting requests
- **Window Sliding** Windows slide continuously
- **Request Counting** Count requests within current window
- **Memory Usage** Higher memory usage than fixed window
- **Accuracy** More accurate than fixed window

**Fixed Window**

- **Time Periods** Fixed time periods (minute, hour)
- **Request Counter** Count requests within current period
- **Reset** Counter resets at period boundary
- **Simple Implementation** Easy to implement
- **Burst Issues** Can allow bursts at window boundaries

**Leaky Bucket**

- **Constant Rate** Processes requests at constant rate
- **Queue** Queues excess requests
- **Overflow** Drops requests when queue is full
- **Smooth Output** Ensures smooth output rate
- **Latency** May introduce latency due to queuing

**Implementation Strategies**

**In-Memory Rate Limiting**

- **MemoryCache** Use IMemoryCache para storing rate limit data
- **Performance** Fast access times
- **Scalability** Limited to single server instance
- **Data Loss** Data lost on server restart
- **Use Cases** Single-server applications, development

**Distributed Rate Limiting**

- **Redis** Use Redis para distributed rate limiting
- **Shared State** Share rate limit state across servers
- **Lua Scripts** Use Lua scripts para atomic operations
- **Performance** Network latency considerations
- **Scalability** Scales across multiple servers

**Database Rate Limiting**

- **Persistent Storage** Store rate limit data en database
- **Durability** Survives server restarts
- **Performance** Slower than memory-based solutions
- **Complexity** More complex implementation
- **Use Cases** Long-term rate limiting, audit requirements

**ASP.NET Core Implementation**

**Built-in Rate Limiting (.NET 7+)**

- **RateLimitingMiddleware** Built-in middleware
- **Multiple Algorithms** Support para various algorithms
- **Policy Configuration** Configure different policies
- **Flexibility** Flexible configuration options
- **Performance** Optimized implementation

**Custom Middleware**

- **Custom Logic** Implement custom rate limiting logic
- **Flexibility** Complete control over behavior
- **Integration** Easy integration con existing code
- **Maintenance** Requires ongoing maintenance
- **Testing** Requires thorough testing

**Third-party Libraries**

- **AspNetCoreRateLimit** Popular rate limiting library
- **Configuration** Rich configuration options
- **Features** Multiple algorithms y storage options
- **Community** Active community support

**Rate Limiting Strategies**

**User-based Rate Limiting**

- **User Identification** Identify users by authentication
- **Personal Quotas** Different limits para different users
- **Premium Users** Higher limits para premium users
- **Anonymous Users** Special handling para anonymous users

**IP-based Rate Limiting**

- **IP Address** Use client IP address para identification
- **Geographic** Different limits based on geography
- **Proxy Considerations** Handle proxy y CDN scenarios
- **IPv6** Consider IPv6 address ranges

**API Key Rate Limiting**

- **API Keys** Use API keys para identification
- **Per-key Limits** Different limits para different API keys
- **Key Management** Manage API key lifecycle
- **Monitoring** Monitor API key usage patterns

**Advanced Features**

**Dynamic Rate Limiting**

- **Load-based** Adjust limits based on system load
- **Time-based** Different limits at different times
- **User Behavior** Adjust limits based on user behavior
- **Machine Learning** Use ML para dynamic adjustment

**Rate Limit Headers**

- **X-RateLimit-Limit** Total request limit
- **X-RateLimit-Remaining** Remaining requests
- **X-RateLimit-Reset** Time when limit resets
- **Retry-After** When client can retry after rate limit hit
- **Standards** Follow industry standards

**Graceful Degradation**

- **Queuing** Queue requests instead of rejecting
- **Prioritization** Prioritize important requests
- **Fallback** Provide fallback responses
- **Circuit Breaker** Implement circuit breaker patterns

**Monitoring y Analytics**

**Metrics Collection**

- **Request Rates** Monitor request rates per client
- **Rejection Rates** Track rate limit violations
- **Performance Impact** Monitor performance impact
- **Resource Usage** Track resource usage patterns

**Alerting**

- **Threshold Alerts** Alert when thresholds are exceeded
- **Pattern Detection** Detect unusual traffic patterns
- **Attack Detection** Identify potential attacks
- **Capacity Planning** Alert when approaching capacity

**Best Practices**

- **Fair Limits** Set reasonable y fair limits
- **Clear Communication** Clearly communicate limits to clients
- **Gradual Implementation** Implement gradually
- **Testing** Test thoroughly under various conditions
- **Documentation** Document rate limiting policies
- **Regular Review** Regularly review y adjust limits

### 93. Explica el principio de Least Privilege en aplicaciones

**Contexto** Authorization policies, role design, security boundaries.

**Respuesta detallada**

**Principle of Least Privilege** establece que users y processes should only have the minimum access rights necessary to perform their intended functions.

**Core Concepts**

**Minimal Access Rights**

- **Need-to-Know Basis** Access only to information actually needed
- **Minimal Permissions** Minimum permissions required para functionality
- **Time-limited Access** Access rights limited en time cuando possible
- **Specific Resources** Access to specific resources, not broad categories
- **Regular Review** Regular review y adjustment of access rights

**Security Boundaries**

- **Clear Boundaries** Well-defined security boundaries
- **Isolation** Isolate different components y data
- **Compartmentalization** Separate sensitive operations
- **Defense en Depth** Multiple layers of security
- **Fail Secure** Default to deny access cuando unsure

**Application-Level Implementation**

**Role-Based Access Control (RBAC)**

- **Minimal Roles** Create roles con minimal necessary permissions
- **Role Hierarchy** Establish clear role hierarchy
- **Role Assignment** Assign users to appropriate roles only
- **Role Review** Regular review of role assignments
- **Temporary Roles** Temporary role assignments cuando needed

**Attribute-Based Access Control (ABAC)**

- **Fine-grained Control** More granular control than RBAC
- **Dynamic Decisions** Access decisions based on attributes
- **Context Awareness** Consider context en access decisions
- **Policy Engine** Centralized policy engine
- **Flexible Rules** Complex y flexible access rules

**Authorization Policies**

**ASP.NET Core Policies**

- **Policy-based Authorization** Define specific authorization policies
- **Requirements** Specific requirements para access
- **Resource-based** Authorization based on specific resources
- **Claims-based** Use claims para fine-grained authorization
- **Custom Logic** Custom authorization logic cuando needed

**Policy Design**

- **Granular Policies** Create specific policies para specific operations
- **Composable Policies** Combine simple policies into complex ones
- **Readable Policies** Make policies easy to understand y maintain
- **Testable Policies** Ensure policies can be tested effectively
- **Auditable Policies** Policy decisions should be auditable

**Database Access Control**

**Database Permissions**

- **Minimal Database Rights** Grant minimal database permissions
- **Schema-level Security** Control access at schema level
- **Row-level Security** Implement row-level security cuando appropriate
- **Column-level Security** Restrict access to sensitive columns
- **Dynamic Data Masking** Mask sensitive data para non-privileged users

**Connection Security**

- **Service Accounts** Use dedicated service accounts
- **Connection Pooling** Secure connection pooling configuration
- **Credential Management** Secure credential storage y rotation
- **Network Security** Restrict network access to databases
- **Audit Logging** Log all database access

**API Security**

**Endpoint Protection**

- **Authentication Required** Require authentication para all endpoints
- **Operation-specific Authorization** Different permissions para different operations
- **Resource-specific Access** Control access to specific resources
- **HTTP Method Security** Consider HTTP method en authorization
- **Parameter Validation** Validate all input parameters

**API Gateway Security**

- **Centralized Authorization** Centralize authorization decisions
- **Rate Limiting** Implement rate limiting policies
- **Request Filtering** Filter malicious requests
- **API Key Management** Manage API keys securely
- **Monitoring** Monitor API usage patterns

**Service-to-Service Communication**

**Microservices Security**

- **Service Identity** Each service has unique identity
- **Inter-service Authentication** Authenticate service-to-service calls
- **Service Mesh** Use service mesh para security policies
- **Network Segmentation** Segment network between services
- **Zero Trust** Implement zero trust principles

**Message Security**

- **Message Encryption** Encrypt messages en transit
- **Message Signing** Sign messages para integrity
- **Access Control** Control access to message queues
- **Topic Security** Control access to specific topics
- **Audit Trail** Maintain audit trail of message exchanges

**Implementation Strategies**

**Progressive Authorization**

- **Step-up Authentication** Require additional authentication para sensitive operations
- **Just-in-Time Access** Grant access only when needed
- **Time-based Access** Limit access duration
- **Approval Workflows** Require approval para sensitive access
- **Emergency Access** Controlled emergency access procedures

**Defense en Depth**

- **Multiple Layers** Multiple authorization layers
- **Redundant Controls** Overlapping security controls
- **Fail-Safe Defaults** Default to deny access
- **Error Handling** Secure error handling
- **Security Monitoring** Continuous security monitoring

**Monitoring y Compliance**

**Access Monitoring**

- **Access Logging** Log all access attempts
- **Failed Access** Monitor failed access attempts
- **Privilege Escalation** Detect privilege escalation attempts
- **Unusual Patterns** Identify unusual access patterns
- **Real-time Alerts** Real-time security alerts

**Compliance Reporting**

- **Regular Audits** Regular access rights audits
- **Compliance Reports** Generate compliance reports
- **Access Certification** Periodic access certification
- **Violation Tracking** Track policy violations
- **Remediation** Quick remediation of violations

**Best Practices**

- **Regular Review** Regularly review y update access rights
- **Automated Enforcement** Automate policy enforcement donde possible
- **Clear Documentation** Document access policies clearly
- **Training** Train users on security principles
- **Incident Response** Have clear incident response procedures
- **Continuous Improvement** Continuously improve security posture

### 93. Explica el principio de Least Privilege en aplicaciones

**Contexto** Authorization policies, role design, security boundaries.

---

## 🚀 **DEVOPS Y CI/CD (7 preguntas)**

### 94. ¿Cómo configurarías un pipeline de CI/CD para aplicaciones .NET?

**Contexto** Build, test, deploy stages, artifact management.

**Respuesta detallada**

**CI/CD Pipeline Structure** para aplicaciones .NET requiere stages bien definidos que automation build, testing, y deployment processes.

**Build Stage**

**Source Control Integration**

- **Git Integration** Integration con Git repositories (GitHub, Azure DevOps, GitLab)
- **Branch Policies** Configure branch protection y pull request policies
- **Trigger Configuration** Configure triggers para automatic builds
- **Webhook Setup** Set up webhooks para real-time integration
- **Multi-branch Support** Support para feature branches y main branches

**Build Process**

- **Restore Dependencies** Restore NuGet packages y dependencies
- **Compilation** Compile source code con appropriate configuration
- **Version Management** Automatic version number generation
- **Build Artifacts** Generate build artifacts para deployment
- **Build Validation** Validate build success y quality

**Test Stage**

**Automated Testing**

- **Unit Tests** Run comprehensive unit test suite
- **Integration Tests** Execute integration tests
- **Code Coverage** Measure y report code coverage
- **Quality Gates** Set quality gates based on test results
- **Test Reporting** Generate detailed test reports

**Quality Assurance**

- **Static Code Analysis** Run static analysis tools (SonarQube, CodeQL)
- **Security Scanning** Perform security vulnerability scanning
- **Dependency Checking** Check para known vulnerabilities en dependencies
- **Code Quality Metrics** Measure y track code quality metrics
- **Performance Testing** Run performance tests cuando appropriate

**Artifact Management**

**Package Creation**

- **NuGet Packages** Create NuGet packages para libraries
- **Container Images** Build Docker container images
- **Deployment Packages** Create deployment-ready packages
- **Versioning Strategy** Implement consistent versioning strategy
- **Metadata** Include metadata en artifacts

**Artifact Storage**

- **Artifact Repository** Store artifacts en secure repository
- **Retention Policies** Implement artifact retention policies
- **Access Control** Control access to artifacts
- **Download Optimization** Optimize artifact download process
- **Backup Strategy** Implement artifact backup strategy

**Deployment Stages**

**Environment Promotion**

- **Development** Automatic deployment to development environment
- **Staging** Controlled deployment to staging environment
- **Production** Controlled deployment to production environment
- **Approval Gates** Implement approval processes para production
- **Rollback Strategy** Implement automatic rollback capabilities

**Deployment Strategies**

- **Blue-Green Deployment** Zero-downtime deployment strategy
- **Rolling Deployment** Gradual replacement of instances
- **Canary Deployment** Gradual traffic shifting to new version
- **Feature Flags** Use feature flags para controlled rollouts
- **Database Migrations** Handle database schema changes safely

**Platform-Specific Implementation**

**Azure DevOps**

- **Azure Pipelines** YAML-based pipeline definition
- **Build Agents** Self-hosted o Microsoft-hosted agents
- **Service Connections** Secure connections to external services
- **Variable Groups** Manage environment-specific variables
- **Release Management** Sophisticated release management features

**GitHub Actions**

- **Workflow Files** YAML workflow definitions
- **Actions Marketplace** Reusable actions from marketplace
- **Secrets Management** Secure secrets management
- **Matrix Builds** Test across multiple environments
- **Scheduled Workflows** Scheduled y event-driven workflows

**Jenkins**

- **Jenkinsfile** Pipeline as code using Jenkinsfile
- **Plugin Ecosystem** Rich plugin ecosystem
- **Master-Slave Architecture** Distributed build architecture
- **Blue Ocean** Modern user interface
- **Pipeline Libraries** Shared pipeline libraries

**Best Practices**

- **Pipeline as Code** Store pipeline definitions en source control
- **Fail Fast** Fail early when issues are detected
- **Parallel Execution** Run independent tasks en parallel
- **Monitoring** Monitor pipeline performance y reliability
- **Security** Implement security best practices throughout
- **Documentation** Document pipeline processes y procedures

### 95. Explica containerización con Docker para aplicaciones .NET

**Contexto** Multi-stage builds, base images, optimization.

**Respuesta detallada**

**Docker Containerization** para .NET applications provides consistent deployment environments y improved scalability.

**Base Images**

**Microsoft Official Images**

- **mcr.microsoft.com/dotnet/runtime** Runtime-only images para deployment
- **mcr.microsoft.com/dotnet/aspnet** ASP.NET Core runtime images
- **mcr.microsoft.com/dotnet/sdk** Full SDK images para building
- **Alpine Variants** Smaller images based on Alpine Linux
- **Ubuntu/Debian** Full Linux distribution-based images

**Image Selection Criteria**

- **Size Considerations** Balance between functionality y image size
- **Security** Choose images con regular security updates
- **Compatibility** Ensure compatibility con application requirements
- **Performance** Consider performance implications
- **Support** Use officially supported images

**Multi-stage Builds**

**Build Stage**

- **SDK Image** Use full SDK image para building application
- **Source Copy** Copy source code into container
- **Dependency Restoration** Restore NuGet packages
- **Compilation** Compile application
- **Test Execution** Run tests during build

**Runtime Stage**

- **Runtime Image** Use minimal runtime image para final container
- **Artifact Copy** Copy compiled artifacts from build stage
- **Configuration** Configure runtime environment
- **Entry Point** Define application entry point
- **Optimization** Optimize para runtime performance

**Container Optimization**

**Image Size Optimization**

- **Layer Caching** Optimize Dockerfile para better layer caching
- **Minimal Base Images** Use minimal base images cuando possible
- **Dependency Management** Remove unnecessary dependencies
- **Multi-stage Benefits** Leverage multi-stage builds para smaller images
- **Image Scanning** Scan images para vulnerabilities y bloat

**Performance Optimization**

- **Startup Time** Optimize application startup time
- **Memory Usage** Configure appropriate memory limits
- **CPU Allocation** Set appropriate CPU limits
- **Networking** Optimize container networking
- **Storage** Use appropriate storage configurations

**Dockerfile Best Practices**

**Security Considerations**

- **Non-root User** Run application as non-root user
- **Minimal Permissions** Use minimal file permissions
- **Secrets Management** Avoid hardcoding secrets en Dockerfile
- **Base Image Updates** Regularly update base images
- **Vulnerability Scanning** Scan containers para security vulnerabilities

**Development Workflow**

- **Local Development** Use Docker para local development
- **Docker Compose** Orchestrate multiple services locally
- **Hot Reload** Configure hot reload para development
- **Debugging** Enable debugging en containerized applications
- **Environment Parity** Maintain parity between environments

**Container Orchestration**

**Docker Compose**

- **Service Definition** Define multiple services
- **Networking** Configure service-to-service communication
- **Volume Management** Manage persistent data
- **Environment Configuration** Configure environment-specific settings
- **Development Orchestration** Orchestrate development environments

**Kubernetes Integration**

- **Deployment Manifests** Create Kubernetes deployment manifests
- **Service Discovery** Configure service discovery
- **Load Balancing** Implement load balancing
- **Scaling** Configure horizontal y vertical scaling
- **Health Checks** Implement health check endpoints

**CI/CD Integration**

- **Automated Builds** Automate container image builds
- **Registry Integration** Push images to container registry
- **Security Scanning** Integrate security scanning en pipeline
- **Deployment Automation** Automate container deployments
- **Rollback Capabilities** Implement rollback mechanisms

### 96. ¿Qué estrategias de logging implementarías?

**Contexto** Structured logging, log levels, centralized logging, correlation IDs.

**Respuesta detallada**

**Logging Strategy** es critical para monitoring, debugging, y maintaining .NET applications en production.

**Structured Logging**

**Benefits of Structured Logging**

- **Queryable Data** Logs become structured data que can be queried
- **Consistent Format** Consistent log message format
- **Rich Context** Include rich contextual information
- **Machine Readable** Easily parsed by log analysis tools
- **Correlation** Better correlation between related log entries

**Implementation con Serilog**

- **Structured Templates** Use structured message templates
- **Property Enrichment** Enrich logs con additional properties
- **Contextual Logging** Add context throughout request pipeline
- **Sink Configuration** Configure multiple output sinks
- **Performance** High-performance logging implementation

**Log Levels**

**Standard Log Levels**

- **Trace** Most detailed information, typically only en development
- **Debug** Detailed information para debugging
- **Information** General information about application flow
- **Warning** Unexpected situations que don't stop application
- **Error** Error events que still allow application to continue
- **Critical** Critical errors que might cause application to abort

**Level Usage Guidelines**

- **Production Levels** Typically Information y above en production
- **Performance Impact** Higher levels reduce performance impact
- **Environment Configuration** Different levels para different environments
- **Dynamic Configuration** Support runtime log level changes
- **Monitoring** Monitor log level distribution

**Centralized Logging**

**Log Aggregation**

- **ELK Stack** Elasticsearch, Logstash, y Kibana
- **Azure Monitor** Azure's logging y monitoring solution
- **Splunk** Enterprise log management platform
- **Fluentd** Open-source data collector
- **Grafana Loki** Horizontally scalable log aggregation

**Benefits**

- **Single View** Single view of all application logs
- **Search Capabilities** Powerful search y filtering
- **Real-time Monitoring** Real-time log monitoring
- **Alerting** Set up alerts based on log patterns
- **Historical Analysis** Analyze historical log data

**Correlation IDs**

**Request Correlation**

- **Trace ID** Unique identifier para each request
- **Span ID** Unique identifier para each operation within request
- **Propagation** Propagate correlation IDs across service boundaries
- **HTTP Headers** Include correlation IDs en HTTP headers
- **Database Queries** Include correlation IDs en database operations

**Implementation**

- **Middleware** Implement correlation ID middleware
- **Dependency Injection** Inject correlation context
- **Async Context** Maintain correlation across async operations
- **External Services** Include correlation IDs en external service calls
- **Logging Integration** Automatically include correlation IDs en logs

**Performance Considerations**

**Asynchronous Logging**

- **Background Processing** Process logs en background threads
- **Buffering** Buffer log entries para batch processing
- **Non-blocking** Ensure logging doesn't block application threads
- **Queue Management** Manage log queue overflow scenarios
- **Backpressure** Handle backpressure situations

**Log Sampling**

- **High-volume Scenarios** Sample logs en high-volume scenarios
- **Intelligent Sampling** Sample based on log content importance
- **Error Preservation** Always preserve error y warning logs
- **Configuration** Configurable sampling rates
- **Monitoring** Monitor sampling effectiveness

**Security y Compliance**

**Sensitive Data Protection**

- **Data Masking** Mask sensitive information en logs
- **PII Protection** Protect personally identifiable information
- **Security Events** Log security-related events appropriately
- **Access Control** Control access to log data
- **Retention Policies** Implement appropriate log retention policies

**Compliance Requirements**

- **Audit Logs** Maintain audit logs para compliance
- **Immutability** Ensure log immutability donde required
- **Encryption** Encrypt logs en transit y at rest
- **Geographic Restrictions** Comply con geographic data restrictions
- **Legal Hold** Support legal hold requirements

**Monitoring y Alerting**

**Log-based Monitoring**

- **Error Rate Monitoring** Monitor application error rates
- **Performance Metrics** Extract performance metrics from logs
- **Business Metrics** Track business-relevant metrics
- **Anomaly Detection** Detect anomalies en log patterns
- **Trend Analysis** Analyze trends over time

**Alerting Strategy**

- **Real-time Alerts** Set up real-time alerts para critical issues
- **Threshold-based** Alert cuando metrics exceed thresholds
- **Pattern-based** Alert on suspicious log patterns
- **Escalation** Implement alert escalation procedures
- **Alert Fatigue** Minimize false positives y alert fatigue

**Best Practices**

- **Consistent Format** Maintain consistent log message format
- **Meaningful Messages** Write clear y meaningful log messages
- **Appropriate Level** Use appropriate log levels
- **Performance Testing** Test logging performance impact
- **Regular Review** Regularly review y optimize logging strategy

### 97. ¿Cómo monitorizarías una aplicación .NET en producción?

**Contexto** APM tools, health checks, metrics, alerting.

**Respuesta detallada**

**Production Monitoring** es essential para maintaining application health, performance, y user experience.

**Application Performance Monitoring (APM)**

**APM Tools**

- **Application Insights** Microsoft's APM solution para Azure
- **New Relic** Comprehensive APM platform
- **Datadog** Full-stack monitoring platform
- **AppDynamics** Enterprise APM solution
- **Dynatrace** AI-powered monitoring platform

**Key Metrics**

- **Response Time** Application response time percentiles
- **Throughput** Requests per second y transaction volume
- **Error Rate** Application error rates y error types
- **Availability** Application uptime y availability metrics
- **Resource Utilization** CPU, memory, y disk usage

**Health Checks**

**Built-in Health Checks**

- **ASP.NET Core Health Checks** Built-in health check framework
- **Dependency Health** Check health of dependencies (database, external APIs)
- **Custom Health Checks** Implement custom health check logic
- **Health Check UI** Provide health check dashboard
- **Health Check Endpoints** Expose health check endpoints para monitoring

**Health Check Types**

- **Liveness Checks** Verify application is running
- **Readiness Checks** Verify application is ready to serve requests
- **Startup Checks** Verify application started correctly
- **Dependency Checks** Verify external dependencies are available
- **Resource Checks** Verify adequate resources are available

**Custom Metrics**

**Business Metrics**

- **User Activity** Track user engagement y activity
- **Business Transactions** Monitor business-critical transactions
- **Revenue Metrics** Track revenue-related metrics
- **Performance KPIs** Monitor key performance indicators
- **Customer Experience** Measure customer experience metrics

**Technical Metrics**

- **Database Performance** Database query performance y connection pool usage
- **Cache Hit Rates** Monitor cache effectiveness
- **Queue Lengths** Monitor message queue lengths
- **External API Performance** Monitor external API call performance
- **Security Events** Track security-related events

**Infrastructure Monitoring**

**Server Metrics**

- **CPU Usage** Monitor CPU utilization patterns
- **Memory Usage** Track memory consumption y garbage collection
- **Disk I/O** Monitor disk read/write performance
- **Network I/O** Track network bandwidth usage
- **Process Metrics** Monitor application process health

**Container Monitoring**

- **Container Health** Monitor container health y lifecycle
- **Resource Limits** Track resource usage against limits
- **Container Orchestration** Monitor Kubernetes o Docker Swarm metrics
- **Image Vulnerabilities** Monitor para security vulnerabilities
- **Registry Health** Monitor container registry availability

**Distributed Tracing**

**Trace Implementation**

- **OpenTelemetry** Industry-standard tracing framework
- **Correlation IDs** Track requests across service boundaries
- **Span Creation** Create spans para significant operations
- **Trace Sampling** Implement intelligent trace sampling
- **Cross-service Tracing** Trace requests across microservices

**Benefits**

- **Request Flow Visibility** Understand complete request flow
- **Performance Bottlenecks** Identify performance bottlenecks
- **Error Attribution** Attribute errors to specific services
- **Dependency Mapping** Map service dependencies
- **Latency Analysis** Analyze latency contributions

**Alerting Strategy**

**Alert Types**

- **Threshold Alerts** Alert cuando metrics exceed thresholds
- **Anomaly Detection** Alert on unusual patterns
- **Compound Alerts** Combine multiple conditions
- **Trend Alerts** Alert on concerning trends
- **Availability Alerts** Alert on service unavailability

**Alert Management**

- **Alert Prioritization** Prioritize alerts by severity
- **Alert Routing** Route alerts to appropriate teams
- **Escalation Procedures** Implement escalation workflows
- **Alert Suppression** Suppress duplicate o related alerts
- **Alert Fatigue Prevention** Minimize false positives

**Observability Stack**

**Metrics Collection**

- **Prometheus** Open-source metrics collection
- **Grafana** Metrics visualization y dashboards
- **InfluxDB** Time-series database para metrics
- **Custom Collectors** Custom metric collection agents
- **Cloud Native** Use cloud provider monitoring services

**Log Management**

- **Centralized Logging** Aggregate logs from all sources
- **Log Analytics** Analyze logs para insights
- **Real-time Processing** Process logs en real-time
- **Long-term Storage** Archive logs para compliance
- **Search Capabilities** Powerful log search y filtering

**Dashboard y Visualization**

**Dashboard Design**

- **Executive Dashboards** High-level business metrics
- **Operational Dashboards** Technical operational metrics
- **Service Dashboards** Service-specific metrics
- **Real-time Dashboards** Real-time monitoring views
- **Mobile Dashboards** Mobile-friendly monitoring views

**Visualization Best Practices**

- **Meaningful Metrics** Display actionable metrics
- **Appropriate Timeframes** Show relevant time ranges
- **Visual Hierarchy** Organize information by importance
- **Color Coding** Use consistent color coding
- **Drill-down Capability** Enable detailed investigation

**Incident Management**

- **Incident Detection** Rapid incident detection
- **Incident Response** Coordinated incident response procedures
- **Post-incident Review** Conduct thorough post-mortems
- **Knowledge Management** Maintain incident knowledge base
- **Continuous Improvement** Improve monitoring based on incidents

### 98. Explica Infrastructure as Code para aplicaciones .NET

**Contexto** ARM templates, Terraform, environment consistency.

**Respuesta detallada**

**Infrastructure as Code (IaC)** enables managing y provisioning infrastructure through code rather than manual processes.

**Benefits of IaC**

**Consistency y Repeatability**

- **Environment Parity** Ensure consistency across environments
- **Reproducible Deployments** Recreate infrastructure reliably
- **Reduced Human Error** Eliminate manual configuration errors
- **Documentation** Infrastructure code serves as documentation
- **Version Control** Track infrastructure changes over time

**Scalability y Efficiency**

- **Rapid Provisioning** Quickly provision new environments
- **Auto-scaling** Implement automatic scaling policies
- **Resource Optimization** Optimize resource allocation
- **Cost Management** Better cost control y tracking
- **Disaster Recovery** Rapid disaster recovery capabilities

**ARM Templates (Azure)**

**Template Structure**

- **Parameters** Input parameters para template customization
- **Variables** Calculated values y reusable expressions
- **Resources** Azure resources to be created
- **Outputs** Values returned after deployment
- **Functions** Built-in functions para dynamic values

**Best Practices**

- **Modular Templates** Create reusable template modules
- **Linked Templates** Compose complex deployments from smaller templates
- **Parameter Files** Use separate parameter files para different environments
- **Template Validation** Validate templates before deployment
- **Incremental Deployment** Use incremental deployment mode

**Terraform**

**Terraform Advantages**

- **Multi-cloud Support** Support para multiple cloud providers
- **Rich Provider Ecosystem** Extensive provider ecosystem
- **State Management** Sophisticated state management
- **Plan y Apply** Plan changes before applying
- **Module System** Powerful module system para reusability

**Terraform Structure**

- **Providers** Configure cloud providers
- **Resources** Define infrastructure resources
- **Variables** Input variables para customization
- **Outputs** Return values from modules
- **Data Sources** Query existing infrastructure

**Configuration Management**

**Ansible**

- **Agentless** No agent required on target systems
- **Playbooks** YAML-based configuration playbooks
- **Idempotent** Safe to run multiple times
- **Inventory Management** Manage target host inventory
- **Role System** Reusable configuration roles

**PowerShell DSC**

- **Windows Focus** Windows-focused configuration management
- **Declarative** Declarative configuration syntax
- **Push/Pull** Support both push y pull modes
- **Built-in Resources** Rich set of built-in resources
- **Custom Resources** Create custom DSC resources

**Environment Management**

**Environment Promotion**

- **Development** Development environment configuration
- **Staging** Staging environment matching production
- **Production** Production environment configuration
- **Feature Environments** Temporary feature testing environments
- **DR Environments** Disaster recovery environments

**Configuration Strategies**

- **Environment-specific Parameters** Different parameters para each environment
- **Conditional Resources** Create resources conditionally
- **Environment Tagging** Tag resources by environment
- **Resource Naming** Consistent resource naming conventions
- **Access Control** Environment-specific access controls

**CI/CD Integration**

**Pipeline Integration**

- **Infrastructure Pipeline** Separate pipeline para infrastructure changes
- **Application Pipeline** Application deployment pipeline
- **Approval Gates** Require approvals para infrastructure changes
- **Testing** Test infrastructure changes before deployment
- **Rollback** Implement infrastructure rollback capabilities

**GitOps Approach**

- **Git as Source of Truth** Git repository as single source of truth
- **Pull-based Deployment** Agents pull changes from Git
- **Automatic Sync** Automatic synchronization con desired state
- **Drift Detection** Detect y correct configuration drift
- **Audit Trail** Complete audit trail of changes

**Monitoring y Compliance**

**Infrastructure Monitoring**

- **Resource Health** Monitor infrastructure resource health
- **Cost Monitoring** Track infrastructure costs
- **Compliance Monitoring** Ensure compliance con policies
- **Performance Monitoring** Monitor infrastructure performance
- **Security Monitoring** Monitor security configurations

**Policy Enforcement**

- **Azure Policy** Enforce organizational policies
- **Terraform Sentinel** Policy as code para Terraform
- **Compliance Frameworks** Implement compliance frameworks
- **Security Baselines** Enforce security baselines
- **Automated Remediation** Automatic policy remediation

**Best Practices**

- **Version Control** Store all infrastructure code en version control
- **Modular Design** Design modular y reusable infrastructure components
- **Testing** Test infrastructure code thoroughly
- **Documentation** Document infrastructure architecture y decisions
- **Security** Implement security best practices throughout
- **Monitoring** Monitor infrastructure health y performance continuously

### 99. ¿Cómo manejarías configuration management en diferentes environments?

**Contexto** appsettings.json, environment variables, Azure Key Vault.

**Respuesta detallada**

**Configuration Management** across different environments requires secure, flexible, y maintainable approaches to handle varying requirements.

**Hierarchical Configuration**

**ASP.NET Core Configuration System**

- **appsettings.json** Base application settings
- **appsettings.{Environment}.json** Environment-specific overrides
- **Environment Variables** System-level configuration
- **Command Line Arguments** Runtime parameter overrides
- **User Secrets** Development-time secrets (local only)

**Configuration Precedence**

- **Order of Priority** Later sources override earlier ones
- **Flexible Overrides** Allow environment-specific customization
- **Default Values** Provide sensible defaults en base configuration
- **Validation** Validate configuration at startup
- **Hot Reload** Support configuration hot reload donde appropriate

**Environment-Specific Strategies**

**Development Environment**

- **User Secrets** Store sensitive data securely during development
- **Local Configuration** Use local configuration files
- **Default Settings** Provide developer-friendly defaults
- **Mock Services** Configure mock external services
- **Debug Settings** Enable detailed logging y debugging

**Staging Environment**

- **Production-like** Mirror production configuration donde possible
- **Test Data** Use test data y test external services
- **Performance Testing** Configure para performance testing
- **Security Testing** Enable security testing features
- **Monitoring** Enable comprehensive monitoring

**Production Environment**

- **Security First** Prioritize security en all configuration
- **Performance Optimized** Optimize para production performance
- **Minimal Logging** Reduce logging overhead
- **External Services** Configure real external service endpoints
- **Backup y Recovery** Configure backup y recovery settings

**Secrets Management**

**Azure Key Vault**

- **Centralized Storage** Centralized secret storage
- **Access Control** Fine-grained access control policies
- **Audit Logging** Complete audit trail of secret access
- **Rotation** Automatic secret rotation capabilities
- **Integration** Easy integration con .NET applications

**Implementation**

- **Managed Identity** Use managed identity para authentication
- **Configuration Provider** Use Key Vault configuration provider
- **Caching** Cache secrets appropriately para performance
- **Fallback** Provide fallback mechanisms
- **Monitoring** Monitor secret access y usage

**Alternative Secret Stores**

- **HashiCorp Vault** Enterprise secret management
- **AWS Secrets Manager** AWS secret storage service
- **Kubernetes Secrets** Container orchestration secrets
- **Docker Secrets** Docker Swarm secret management
- **Environment Variables** Simple environment variable storage

**Configuration Patterns**

**Options Pattern**

- **Strongly Typed** Create strongly-typed configuration classes
- **Validation** Implement configuration validation
- **Dependency Injection** Inject configuration through DI
- **Change Notifications** React to configuration changes
- **Scoped Configuration** Different configuration scopes

**Feature Flags**

- **Runtime Control** Control features at runtime
- **Gradual Rollout** Gradually roll out new features
- **A/B Testing** Support A/B testing scenarios
- **Emergency Toggles** Quick feature disabling en emergencies
- **User Segmentation** Target features to specific user segments

**Configuration as Code**

**Infrastructure Integration**

- **Terraform Variables** Integrate con Terraform variables
- **ARM Template Parameters** Use ARM template parameters
- **Ansible Variables** Integrate con Ansible variable systems
- **GitOps** Store configuration en Git repositories
- **Version Control** Track configuration changes

**Deployment Integration**

- **Build-time Configuration** Set configuration during build
- **Deploy-time Configuration** Configure during deployment
- **Runtime Configuration** Support runtime configuration changes
- **Blue-Green Deployment** Handle configuration during blue-green deployments
- **Canary Deployment** Gradual configuration rollout

**Security Considerations**

**Access Control**

- **Principle of Least Privilege** Minimal necessary access
- **Role-based Access** Role-based configuration access
- **Environment Isolation** Isolate configuration by environment
- **Audit Trail** Complete audit trail of configuration access
- **Regular Review** Regular access review y cleanup

**Encryption**

- **Encryption at Rest** Encrypt configuration data at rest
- **Encryption en Transit** Encrypt configuration data en transit
- **Key Management** Proper cryptographic key management
- **Certificate Management** Manage certificates securely
- **Hardware Security** Use hardware security modules donde appropriate

**Monitoring y Observability**

**Configuration Monitoring**

- **Change Detection** Detect unauthorized configuration changes
- **Drift Monitoring** Monitor configuration drift
- **Performance Impact** Monitor configuration change impact
- **Error Tracking** Track configuration-related errors
- **Usage Analytics** Analyze configuration usage patterns

**Alerting**

- **Configuration Changes** Alert on significant configuration changes
- **Security Events** Alert on security-related configuration events
- **Performance Degradation** Alert on performance impacts
- **Compliance Violations** Alert on compliance violations
- **Unauthorized Access** Alert on unauthorized configuration access

**Best Practices**

- **Documentation** Document all configuration options y their purposes
- **Validation** Validate configuration at multiple levels
- **Testing** Test configuration changes thoroughly
- **Backup** Backup configuration data regularly
- **Recovery** Have configuration recovery procedures
- **Automation** Automate configuration management donde possible

### 100. ¿Qué consideraciones tendrías para deployments en Kubernetes?

**Contexto** Resource limits, health checks, service discovery, scaling.

**Respuesta detallada**

**Kubernetes Deployments** para .NET applications require careful consideration of container orchestration, resource management, y operational requirements.

**Resource Management**

**Resource Limits y Requests**

- **CPU Requests** Minimum CPU resources guaranteed
- **CPU Limits** Maximum CPU resources allowed
- **Memory Requests** Minimum memory resources guaranteed
- **Memory Limits** Maximum memory resources allowed
- **Quality of Service** QoS classes based on resource specification

**Resource Planning**

- **Application Profiling** Profile application resource usage
- **Load Testing** Test resource usage under load
- **Monitoring** Monitor actual resource usage en production
- **Right-sizing** Continuously optimize resource allocation
- **Cost Optimization** Balance performance y cost

**Health Checks**

**Kubernetes Probes**

- **Liveness Probe** Restart container if application is unhealthy
- **Readiness Probe** Remove traffic from unhealthy containers
- **Startup Probe** Allow extra time para slow-starting applications
- **Probe Configuration** Configure timeouts, intervals, y thresholds
- **Probe Types** HTTP, TCP, y exec probe types

**Health Check Implementation**

- **ASP.NET Core Health Checks** Use built-in health check middleware
- **Dependency Health** Check health of dependencies
- **Custom Health Logic** Implement application-specific health checks
- **Performance Considerations** Ensure health checks are lightweight
- **Graceful Degradation** Handle partial system failures

**Service Discovery y Networking**

**Service Types**

- **ClusterIP** Internal cluster communication
- **NodePort** Expose service on each node
- **LoadBalancer** Cloud provider load balancer integration
- **Ingress** HTTP/HTTPS routing y SSL termination
- **Headless Services** Direct pod-to-pod communication

**Network Policies**

- **Security** Control network traffic between pods
- **Ingress Rules** Control incoming traffic
- **Egress Rules** Control outgoing traffic
- **Namespace Isolation** Isolate traffic by namespace
- **Default Deny** Default deny policies para security

**Scaling Strategies**

**Horizontal Pod Autoscaler (HPA)**

- **CPU-based Scaling** Scale based on CPU utilization
- **Memory-based Scaling** Scale based on memory utilization
- **Custom Metrics** Scale based on custom application metrics
- **Multiple Metrics** Combine multiple metrics para scaling decisions
- **Scaling Policies** Configure scale-up y scale-down behavior

**Vertical Pod Autoscaler (VPA)**

- **Resource Optimization** Automatically adjust resource requests
- **Right-sizing** Continuously optimize pod resource allocation
- **Recommendation Mode** Provide resource recommendations
- **Auto Mode** Automatically apply resource changes
- **Update Policies** Control how resource changes are applied

**Deployment Strategies**

**Rolling Updates**

- **Zero Downtime** Update applications without downtime
- **Gradual Rollout** Gradually replace old pods con new ones
- **Rollback** Quick rollback to previous version
- **MaxUnavailable** Control how many pods can be unavailable
- **MaxSurge** Control how many extra pods can be created

**Blue-Green Deployment**

- **Complete Environment** Maintain two complete environments
- **Traffic Switching** Instant traffic switching between environments
- **Quick Rollback** Instant rollback capability
- **Resource Usage** Higher resource usage during deployment
- **Testing** Thorough testing en blue environment before switching

**Configuration Management**

**ConfigMaps y Secrets**

- **ConfigMaps** Store non-sensitive configuration data
- **Secrets** Store sensitive configuration data
- **Volume Mounts** Mount configuration as files
- **Environment Variables** Inject configuration as environment variables
- **Hot Reload** Support configuration hot reload donde appropriate

**External Configuration**

- **External Secrets** Integrate con external secret stores
- **Cloud Provider Integration** Use cloud provider configuration services
- **Vault Integration** Integrate con HashiCorp Vault
- **Key Rotation** Support automatic key rotation
- **Security** Encrypt secrets at rest y en transit

**Monitoring y Observability**

**Application Monitoring**

- **Prometheus Integration** Expose metrics para Prometheus
- **Custom Metrics** Export application-specific metrics
- **Distributed Tracing** Implement distributed tracing
- **Log Aggregation** Aggregate logs from all pods
- **Performance Monitoring** Monitor application performance

**Cluster Monitoring**

- **Resource Usage** Monitor cluster resource usage
- **Node Health** Monitor node health y availability
- **Network Performance** Monitor network performance
- **Storage Performance** Monitor storage performance
- **Security Events** Monitor security-related events

**Security Considerations**

**Pod Security**

- **Security Contexts** Configure pod security contexts
- **Non-root Users** Run containers as non-root users
- **Read-only Filesystems** Use read-only root filesystems
- **Privilege Escalation** Disable privilege escalation
- **Capabilities** Drop unnecessary Linux capabilities

**Network Security**

- **Network Policies** Implement network segmentation
- **Service Mesh** Use service mesh para additional security
- **TLS Encryption** Encrypt traffic between services
- **Certificate Management** Manage TLS certificates
- **Ingress Security** Secure ingress controllers

**Operational Considerations**

**Backup y Recovery**

- **Persistent Volume Backup** Backup persistent volumes
- **Configuration Backup** Backup Kubernetes configurations
- **Disaster Recovery** Implement disaster recovery procedures
- **Cross-region Replication** Replicate across regions para availability
- **Recovery Testing** Regularly test recovery procedures

**Maintenance**

- **Cluster Updates** Plan y execute cluster updates
- **Node Maintenance** Handle node maintenance windows
- **Application Updates** Coordinate application updates
- **Security Patches** Apply security patches promptly
- **Capacity Planning** Plan cluster capacity growth

**Best Practices**

- **Namespace Organization** Organize workloads using namespaces
- **Resource Tagging** Tag resources consistently
- **Documentation** Document deployment configurations
- **Automation** Automate deployment processes
- **Testing** Test deployments thoroughly
- **Monitoring** Implement comprehensive monitoring
- **Security** Follow security best practices consistently

---

## 📚 **Recursos Adicionales**

### Fuentes y Referencias:

- **Microsoft Learn** Documentación oficial y learning paths
- **Stack Overflow Developer Survey 2023** Tendencias y tecnologías más utilizadas
- **GitHub .NET Community** Discusiones y mejores prácticas
- **Pluralsight/Udemy** Cursos especializados en .NET
- **Martin Fowler's Blog** Patrones de arquitectura y diseño
- **Clean Code / Clean Architecture** Robert C. Martin
- **Enterprise Integration Patterns** Gregor Hohpe

### Categorización por Nivel:

- **⭐ Básico** Preguntas 1-30 (Fundamentos y C# básico)
- **⭐⭐ Intermedio** Preguntas 31-70 (Arquitectura, APIs, Datos)
- **⭐⭐⭐ Avanzado** Preguntas 71-100 (Performance, Seguridad, DevOps)

### Preparación Recomendada:

1. **Práctica hands-on** con cada concepto
2. **Proyectos personales** que demuestren los conceptos
3. **Contribuciones open source** en proyectos .NET
4. **Certificaciones Microsoft** (AZ-204, AZ-400)
5. **Preparación de ejemplos de código** para demostrar conceptos

---

_Documento compilado basándose en análisis de múltiples fuentes de la industria y experiencias reales de entrevistas técnicas para posiciones .NET Senior en 2024-2025._
