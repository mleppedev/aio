# Biblia Técnica de Entrevista

## 🎯 Descripción

Una aplicación web moderna para estudiar conceptos técnicos de entrevistas, especialmente orientada a desarrolladores .NET, Azure y arquitectura de microservicios.

## ✨ Características Mejoradas

### 🎨 UI/UX Moderna

- **Diseño Dark Theme Premium**: Colores más sofisticados y gradientes modernos
- **Tipografía Mejorada**: Inter font con características avanzadas
- **Animaciones Sutiles**: Transiciones suaves y efectos hover
- **Layout Responsivo**: Adaptado para escritorio y móvil
- **Iconos Contextuales**: Emojis para categorización visual

### 🏗️ Arquitectura Estructurada

- **Datos en JSON**: Migración desde markdown embebido a estructura JSON
- **Separación de Responsabilidades**: Datos, lógica y presentación separados
- **Renderizado Dinámico**: Generación de HTML desde estructura de datos
- **Sistema de Tipos**: Clasificación de contenido (texto, lista, Q&A, etc.)

### 🔍 Búsqueda Inteligente

- **Búsqueda en Tiempo Real**: Resultados instantáneos mientras escribes
- **Sistema de Puntuación**: Relevancia por título, contenido y palabras clave
- **Resaltado Contextual**: Términos encontrados highlighted automáticamente
- **Atajos de Teclado**: `Ctrl+/` o `/` para buscar, `Esc` para limpiar

### 📱 Experiencia Móvil

- **Navegación Adaptativa**: TOC como overlay en pantallas pequeñas
- **Touch Optimizado**: Botones y áreas de toque apropiadas
- **Performance**: Carga rápida y renderizado eficiente

## 🗂️ Estructura de Archivos

```
docs/site/
├── biblia.html          # Aplicación principal con fallback embebido
├── biblia.json          # Datos estructurados (opcional)
├── servidor.bat         # Script para levantar servidor HTTP local
└── README.md           # Esta documentación
```

## 📊 Formato de Datos JSON

```json
{
  "title": "Título principal",
  "description": "Descripción del contenido",
  "sections": [
    {
      "id": "seccion-id",
      "title": "Título de la Sección",
      "level": 2,
      "type": "ordered_list|list|text|qa",
      "content": "string o array",
      "subsections": [...]
    }
  ]
}
```

### Tipos de Contenido Soportados

- **`text`**: Párrafos de texto con markdown inline
- **`list`**: Listas con viñetas (ul)
- **`ordered_list`**: Listas numeradas con diseño especial tipo card
- **`qa`**: Formato pregunta-respuesta

## 🚀 Características Técnicas

### Performance

- **Carga Asíncrona**: Fetch de datos no bloquea la UI
- **Búsqueda Optimizada**: Índice pre-calculado con debounce
- **Lazy Rendering**: Secciones se animan gradualmente
- **Memory Efficient**: Reutilización de elementos DOM

### Accesibilidad

- **Contraste Alto**: Colores que cumplen WCAG
- **Navegación por Teclado**: Todos los elementos accesibles
- **Texto Descriptivo**: Labels y placeholders informativos
- **Focus Management**: Estados de foco visibles

### SEO & Semántica

- **HTML Semántico**: nav, main, section, article apropiados
- **Meta Tags**: Viewport y charset configurados
- **URLs Fragmentadas**: Navegación con anchors (#)

## 🛠️ Uso y Desarrollo

### Para Probar Localmente

**Opción 1: Servidor HTTP (Recomendado)**

```bash
# Ejecutar el script incluido
servidor.bat

# O manualmente con Python
python -m http.server 8000
# Luego abrir: http://localhost:8000/biblia.html
```

**Opción 2: Archivo Directo**

- Abrir `biblia.html` directamente (usa fallback embebido)
- El contenido se carga automáticamente desde contenido embebido

### Para Añadir Contenido

1. **Con servidor**: Editar `biblia.json` con la nueva estructura
2. **Sin servidor**: Editar el contenido embebido en `parseEmbeddedContent()`
3. La aplicación se actualizará automáticamente al recargar

### Arquitectura de Carga Inteligente

- **Prioridad 1**: Intenta cargar desde `biblia.json` (cuando hay servidor HTTP)
- **Fallback**: Usa contenido embebido si falla la carga JSON
- **Resultado**: Funciona tanto con servidor como sin él

### Para Modificar Estilos

- Variables CSS en `:root` para temas
- Clases modulares para componentes específicos
- Media queries para responsive

### Para Extender Funcionalidad

- Sistema modular de funciones JavaScript
- Event delegation para performance
- API clara para añadir nuevos tipos de contenido

## 🎮 Atajos de Teclado

| Tecla           | Acción            |
| --------------- | ----------------- |
| `/` o `Ctrl+/`  | Enfocar búsqueda  |
| `Esc`           | Limpiar búsqueda  |
| Click en índice | Navegar a sección |

## 📈 Métricas de Mejora

### Antes vs Después

- **Tiempo de Carga**: Markdown embebido → JSON asíncrono
- **Mantenibilidad**: Código HTML mezclado → Estructura separada
- **Experiencia Visual**: Diseño básico → UI moderna y atractiva
- **Búsqueda**: Simple coincidencia → Sistema de relevancia
- **Responsive**: Básico → Experiencia móvil optimizada

## 🔧 Personalización

### Cambiar Tema

Editar variables CSS en `:root`:

```css
:root {
  --bg: #nuevo-fondo;
  --accent: #nuevo-acento;
  --text: #nuevo-texto;
}
```

### Añadir Nuevo Tipo de Contenido

1. Definir el tipo en JSON
2. Añadir lógica de renderizado en `renderFromJSON()`
3. Crear estilos CSS correspondientes

## 📝 Próximas Mejoras Sugeridas

- [ ] Modo claro/oscuro toggle
- [ ] Export a PDF
- [ ] Favoritos/Bookmarks
- [ ] Historial de búsquedas
- [ ] Compartir secciones específicas
- [ ] Búsqueda con normalización de acentos
- [ ] Categorías colapsables
- [ ] Modo presentación

---

**Autor**: GitHub Copilot  
**Versión**: 2.0 (JSON-based)  
**Última actualización**: Agosto 2025
