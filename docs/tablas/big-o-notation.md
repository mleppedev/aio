# Contexto y Propósito

## ¿Qué es?
La notación Big O es una forma de describir cómo crece el tiempo o el espacio que requiere un algoritmo a medida que aumenta el tamaño de los datos. No mide tiempos exactos, sino el comportamiento asintótico. En .NET es clave para elegir colecciones, diseñar algoritmos y anticipar cuellos de botella en producción.

## ¿Por qué?
Porque escribir código que "funcione" no basta: debe escalar. Un algoritmo O(n²) puede ser aceptable con 100 elementos pero desastroso con 1 millón. En mi experiencia, identificar estas diferencias marcó la diferencia entre sistemas que soportaban picos de carga en retail o banca, y otros que colapsaban ante volúmenes de datos moderados.

## ¿Para qué?
- **Evaluar rendimiento** de algoritmos en escenarios reales de negocio.  
- **Seleccionar estructuras de datos .NET** (Dictionary, List, LinkedList, etc.) según patrones de acceso.  
- **Optimizar código crítico** en APIs, microservicios y motores de análisis.  
- **Comunicar decisiones técnicas** con un marco común entre arquitectos y desarrolladores.  

## Valor agregado desde la experiencia
- Migrar búsquedas de listas a **Dictionary<TKey, TValue> O(1)** redujo tiempos de respuesta en APIs de scoring financiero.  
- Aplicar **HashSet O(1)** en validaciones masivas de datos municipales eliminó problemas de latencia.  
- Reemplazar concatenaciones de strings O(n²) por **StringBuilder O(n)** redujo el consumo de CPU en reportes de gran escala.  

# Big O Notation for .NET

**Guía completa de complejidad algorítmica con ejemplos específicos en C# para análisis de rendimiento y optimización.**
Este documento cubre desde notaciones básicas hasta análisis práctico de algoritmos con estructuras de datos .NET.
Fundamental para escribir código eficiente, optimizar rendimiento y tomar decisiones arquitectónicas informadas en aplicaciones empresariales.

## Big O Definitions

**Definiciones concisas de cada notación Big O con su significado práctico en el desarrollo de software.**
Esta sección proporciona una comprensión rápida del comportamiento de cada complejidad temporal.
Esencial para identificar inmediatamente el tipo de rendimiento que puedes esperar de un algoritmo.

| **Notación**   | **Definición**                                       | **Significado Práctico**                                                                        |
| -------------- | ---------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| **O(1)**       | Tiempo constante independiente del tamaño de entrada | El algoritmo siempre toma el mismo tiempo, sin importar si tienes 10 o 10 millones de elementos |
| **O(log n)**   | Tiempo crece logarítmicamente                        | Cada vez que duplicas los datos, solo agregas un paso más (muy eficiente)                       |
| **O(n)**       | Tiempo crece linealmente con el tamaño               | Si duplicas los datos, el tiempo también se duplica (proporcional)                              |
| **O(n log n)** | Tiempo crece linealmente multiplicado por logaritmo  | Ligeramente peor que lineal, típico de algoritmos de ordenamiento eficientes                    |
| **O(n²)**      | Tiempo crece cuadráticamente                         | Si duplicas los datos, el tiempo se multiplica por 4 (puede volverse lento)                     |
| **O(2^n)**     | Tiempo se duplica por cada elemento adicional        | Crece extremadamente rápido, solo viable para datasets muy pequeños                             |
| **O(n!)**      | Tiempo crece factorialmente                          | El peor caso posible, impracticable para más de ~10-15 elementos                                |

## Common Time Complexities

**Tabla de complejidades temporales más comunes con ejemplos prácticos en C# y casos de uso reales.**
Esta referencia muestra el crecimiento de tiempo de ejecución según el tamaño de entrada con implementaciones concretas.
Esencial para elegir algoritmos apropiados y predecir el comportamiento de rendimiento en diferentes escalas de datos.

| **Notación**   | **Nombre**   | **Ejemplo C#**                   | **Caso de Uso**        | **N=10** | **N=100** | **N=1000** |
| -------------- | ------------ | -------------------------------- | ---------------------- | -------- | --------- | ---------- |
| **O(1)**       | Constante    | `dict[key]`, `array[index]`      | Acceso directo         | 1        | 1         | 1          |
| **O(log n)**   | Logarítmico  | `Array.BinarySearch()`           | Búsqueda binaria       | 3        | 7         | 10         |
| **O(n)**       | Lineal       | `foreach`, `list.Contains()`     | Búsqueda secuencial    | 10       | 100       | 1000       |
| **O(n log n)** | Linearítmico | `Array.Sort()`, `list.OrderBy()` | Ordenamiento eficiente | 33       | 664       | 9966       |
| **O(n²)**      | Cuadrático   | Bubble sort, nested loops        | Comparaciones pares    | 100      | 10K       | 1M         |
| **O(2^n)**     | Exponencial  | Fibonacci recursivo              | Backtracking           | 1024     | ~10^30    | ~10^301    |
| **O(n!)**      | Factorial    | Permutaciones completas          | Traveling salesman     | 3.6M     | ~10^157   | ~10^2567   |

## Space Complexities

**Análisis de complejidad espacial para algoritmos y estructuras de datos en aplicaciones .NET.**
Esta tabla muestra el uso de memoria adicional requerido por diferentes operaciones y estructuras.
Crítico para optimizar el consumo de memoria y evitar problemas de escalabilidad en sistemas distribuidos.

| **Notación**   | **Estructura/Algoritmo** | **Uso de Memoria**       | **Ejemplo .NET**          |
| -------------- | ------------------------ | ------------------------ | ------------------------- |
| **O(1)**       | Variables simples        | Espacio constante        | `int x`, `bool flag`      |
| **O(log n)**   | Binary search recursivo  | Stack de llamadas        | Recursión con división    |
| **O(n)**       | Arrays, listas           | Proporcional a elementos | `List<T>`, `T[]`          |
| **O(n log n)** | Merge sort               | Arrays temporales        | Ordenamiento estable      |
| **O(n²)**      | Matrices 2D              | Tabla completa           | `int[,]`, `List<List<T>>` |

## .NET Collections Performance

**Rendimiento de las colecciones .NET más utilizadas con análisis de operaciones CRUD.**
Esta tabla compara las complejidades de operaciones básicas en diferentes tipos de colecciones.
Fundamental para elegir la estructura de datos correcta según los patrones de acceso de la aplicación.

| **Colección**                   | **Acceso** | **Inserción** | **Eliminación** | **Búsqueda** | **Mejor Para**         |
| ------------------------------- | ---------- | ------------- | --------------- | ------------ | ---------------------- |
| **Array**                       | O(1)       | O(n)          | O(n)            | O(n)         | Acceso por índice      |
| **List&lt;T&gt;**               | O(1)       | O(1)\*        | O(n)            | O(n)         | Colección general      |
| **LinkedList&lt;T&gt;**         | O(n)       | O(1)          | O(1)            | O(n)         | Inserciones frecuentes |
| **Dictionary&lt;K,V&gt;**       | O(1)       | O(1)          | O(1)            | O(1)         | Búsquedas por clave    |
| **HashSet&lt;T&gt;**            | N/A        | O(1)          | O(1)            | O(1)         | Conjuntos únicos       |
| **SortedDictionary&lt;K,V&gt;** | O(log n)   | O(log n)      | O(log n)        | O(log n)     | Orden mantenido        |
| **Queue&lt;T&gt;**              | O(1)       | O(1)          | O(1)            | O(n)         | FIFO operations        |
| **Stack&lt;T&gt;**              | O(1)       | O(1)          | O(1)            | O(n)         | LIFO operations        |

\*O(1) amortizado, O(n) en el peor caso cuando se redimensiona

## Algorithm Analysis Examples

**Ejemplos prácticos de análisis de complejidad con implementaciones reales en C#.**
Estos ejemplos muestran cómo calcular y optimizar la complejidad de algoritmos comunes.
Esencial para identificar cuellos de botella y mejorar el rendimiento de aplicaciones críticas.

```csharp
// O(n²) - Nested loops para encontrar duplicados
public bool HasDuplicates(int[] array)
{
    for (int i = 0; i < array.Length; i++)
    {
        for (int j = i + 1; j < array.Length; j++)
        {
            if (array[i] == array[j])
                return true; // O(n²) en el peor caso
        }
    }
    return false;
}

// O(n) - Optimizado con HashSet
public bool HasDuplicatesOptimized(int[] array)
{
    var seen = new HashSet<int>();
    foreach (int item in array)
    {
        if (!seen.Add(item))
            return true; // O(n) tiempo, O(n) espacio
    }
    return false;
}
```

## Sorting Algorithms Comparison

**Comparación de algoritmos de ordenamiento con sus complejidades temporales y espaciales.**
Esta tabla analiza los algoritmos más importantes con casos promedio, mejor y peor escenario.
Crítico para elegir el algoritmo de ordenamiento apropiado según el contexto y tamaño de datos.

| **Algoritmo**       | **Mejor Caso** | **Caso Promedio** | **Peor Caso** | **Espacio** | **Estable** |
| ------------------- | -------------- | ----------------- | ------------- | ----------- | ----------- |
| **Bubble Sort**     | O(n)           | O(n²)             | O(n²)         | O(1)        | Sí          |
| **Selection Sort**  | O(n²)          | O(n²)             | O(n²)         | O(1)        | No          |
| **Insertion Sort**  | O(n)           | O(n²)             | O(n²)         | O(1)        | Sí          |
| **Merge Sort**      | O(n log n)     | O(n log n)        | O(n log n)    | O(n)        | Sí          |
| **Quick Sort**      | O(n log n)     | O(n log n)        | O(n²)         | O(log n)    | No          |
| **Heap Sort**       | O(n log n)     | O(n log n)        | O(n log n)    | O(1)        | No          |
| **Tim Sort (.NET)** | O(n)           | O(n log n)        | O(n log n)    | O(n)        | Sí          |

## Search Algorithms Performance

**Análisis de rendimiento de algoritmos de búsqueda con implementaciones .NET específicas.**
Esta tabla compara diferentes estrategias de búsqueda y sus trade-offs de tiempo vs espacio.
Fundamental para optimizar operaciones de consulta en bases de datos y estructuras de datos en memoria.

| **Algoritmo**     | **Complejidad** | **Prerrequisitos**    | **Implementación .NET**              |
| ----------------- | --------------- | --------------------- | ------------------------------------ |
| **Linear Search** | O(n)            | Ninguno               | `Array.IndexOf()`, `List.Contains()` |
| **Binary Search** | O(log n)        | Array ordenado        | `Array.BinarySearch()`               |
| **Hash Table**    | O(1) promedio   | Hash function         | `Dictionary<K,V>`                    |
| **Tree Search**   | O(log n)        | Estructura balanceada | `SortedDictionary<K,V>`              |
| **Interpolation** | O(log log n)    | Distribución uniforme | Implementación manual                |

## Performance Optimization Rules

**Reglas prácticas para optimización de rendimiento basadas en análisis Big O.**
Estas guidelines ayudan a tomar decisiones informadas sobre optimización prematura vs necesaria.
Esencial para balancear legibilidad del código con eficiencia computacional en aplicaciones empresariales.

| **Escenario**        | **Umbral Crítico** | **Acción Recomendada**   | **Técnica de Optimización** |
| -------------------- | ------------------ | ------------------------ | --------------------------- |
| **N < 100**          | Cualquier O(n²)    | No optimizar             | Priorizar legibilidad       |
| **100 ≤ N < 10K**    | O(n³) o mayor      | Considerar optimización  | Algoritmos O(n log n)       |
| **N ≥ 10K**          | O(n²)              | Optimización obligatoria | Estructuras eficientes      |
| **N ≥ 1M**           | O(n log n)         | Análisis profundo        | Algoritmos especializados   |
| **Memoria limitada** | O(n) espacio       | Optimizar memoria        | Algoritmos in-place         |

## Common Pitfalls and Solutions

**Errores comunes en análisis de complejidad y sus soluciones en contexto .NET.**
Esta tabla identifica anti-patrones frecuentes y proporciona alternativas optimizadas.
Crítico para evitar degradación de rendimiento en aplicaciones de producción y sistemas distribuidos.

```csharp
// ❌ MALO: O(n²) - String concatenation en loop
public string ConcatenateStrings(string[] strings)
{
    string result = "";
    foreach (string s in strings)
    {
        result += s; // Cada += crea nuevo string
    }
    return result;
}

// ✅ BUENO: O(n) - StringBuilder
public string ConcatenateStringsOptimized(string[] strings)
{
    var sb = new StringBuilder();
    foreach (string s in strings)
    {
        sb.Append(s); // O(1) amortizado
    }
    return sb.ToString();
}

// ❌ MALO: O(n) en cada Contains - Total O(n²)
public List<int> RemoveDuplicates(List<int> input)
{
    var result = new List<int>();
    foreach (int item in input)
    {
        if (!result.Contains(item)) // O(n) cada vez
            result.Add(item);
    }
    return result;
}

// ✅ BUENO: O(n) total con HashSet
public List<int> RemoveDuplicatesOptimized(List<int> input)
{
    return input.Distinct().ToList(); // O(n) con hash interno
}
```

## Big O Decision Tree

**Árbol de decisión para seleccionar algoritmos basado en restricciones de tiempo y espacio.**
Este diagrama guía la elección de algoritmos según los requisitos específicos del proyecto.
Fundamental para tomar decisiones arquitectónicas informadas en el diseño de sistemas de alto rendimiento.

````mermaid
graph TD
    A[Que operacion necesitas?] --> B{Busqueda}
    A --> C{Ordenamiento}
    A --> D{Insercion/Eliminacion}

    B --> E{Datos ordenados?}
    E -->|Si| F[Binary Search O log n]
    E -->|No| G{Busquedas frecuentes?}
    G -->|Si| H[Dictionary O 1]
    G -->|No| I[Linear Search O n]

    C --> J{Tamano de datos?}
    J -->|menor 50| K[Insertion Sort O n2]
    J -->|50-10K| L[Quick Sort O n log n]
    J -->|mayor 10K| M[Tim Sort O n log n]

    D --> N{Posicion especifica?}
    N -->|Inicio/Fin| O[List T O 1]
    N -->|Medio| P[LinkedList T O 1]
    N -->|Por clave| Q[Dictionary O 1]

    classDef optimal fill:#14532d,stroke:#4ade80,stroke-width:3px,color:#ffffff
    classDef good fill:#c2410c,stroke:#fb923c,stroke-width:3px,color:#ffffff
    classDef efficient fill:#1e3a8a,stroke:#60a5fa,stroke-width:3px,color:#ffffff
    classDef decision fill:#581c87,stroke:#c084fc,stroke-width:3px,color:#ffffff
    
    class F,H optimal
    class L,M good
    class O,P efficient
    style Q fill:#4f8ff7
```## Memory vs Speed Trade-offs

**Análisis de trade-offs entre memoria y velocidad en algoritmos y estructuras de datos .NET.**
Esta tabla ayuda a balancear el uso de recursos según las restricciones del sistema.
Esencial para optimizar aplicaciones con limitaciones específicas de memoria o procesamiento.

| **Técnica**       | **Ganancia en Velocidad** | **Costo en Memoria** | **Cuándo Usar**                |
| ----------------- | ------------------------- | -------------------- | ------------------------------ |
| **Memoization**   | O(exponencial) → O(n)     | O(n) adicional       | Recursión con overlap          |
| **Hash Tables**   | O(n) → O(1)               | O(n) para hash       | Búsquedas frecuentes           |
| **Índices DB**    | O(n) → O(log n)           | 10-15% adicional     | Consultas repetitivas          |
| **Caching**       | O(red/disk) → O(1)        | RAM disponible       | Datos accedidos frecuentemente |
| **Preprocessing** | O(n²) → O(n)              | O(n) para resultados | Cálculos repetitivos           |

## Real-World Performance Metrics

**Métricas de rendimiento del mundo real para diferentes tamaños de datos en aplicaciones .NET.**
Estos benchmarks proporcionan referencias concretas para planificación de capacidad y optimización.
Crítico para establecer SLAs realistas y dimensionar correctamente la infraestructura de aplicaciones.

| **Operación**          | **1K elementos** | **100K elementos** | **1M elementos** | **Tiempo Aceptable** |
| ---------------------- | ---------------- | ------------------ | ---------------- | -------------------- |
| **Dictionary lookup**  | < 1μs            | < 1μs              | < 1μs            | < 10μs               |
| **List.Contains()**    | ~50μs            | ~5ms               | ~50ms            | < 100ms              |
| **Array.Sort()**       | ~100μs           | ~20ms              | ~250ms           | < 1s                 |
| **LINQ complex query** | ~1ms             | ~100ms             | ~1s              | < 5s                 |
| **Database query**     | ~1ms             | ~50ms              | ~500ms           | < 2s                 |
| **JSON serialization** | ~500μs           | ~50ms              | ~500ms           | < 1s                 |
````
