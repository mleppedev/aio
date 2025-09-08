# Contexto y Propósito

## ¿Qué es?
HTTP (HyperText Transfer Protocol) es el protocolo base de la web y de APIs REST. Los códigos de estado HTTP comunican el resultado de las operaciones cliente-servidor (ej. 200 OK, 404 Not Found, 500 Internal Server Error). En .NET, su uso correcto es esencial para APIs claras y predecibles.

## ¿Por qué?
Porque una API no solo debe funcionar, también debe comunicar correctamente errores y estados. En mi experiencia, un uso inconsistente de códigos HTTP llevó a confusión de clientes y mayores tickets de soporte. Aplicar buenas prácticas redujo fricción y mejoró interoperabilidad.

## ¿Para qué?
- **Establecer convenciones claras** en respuestas API.  
- **Reducir ambigüedad** en manejo de errores.  
- **Facilitar debugging** con códigos precisos.  
- **Cumplir estándares REST** que esperan integradores externos.  

## Valor agregado desde la experiencia
- Usar **422 Unprocessable Entity** en validaciones mejoró feedback en formularios bancarios.  
- Aplicar **202 Accepted** en operaciones asíncronas evitó timeouts en retail.  
- Estándarizar **Problem Details (RFC 7807)** en errores redujo soporte en municipalidades.  
- Con **429 Too Many Requests**, implementamos rate limiting que protegió APIs expuestas al público.  

# HTTP Status Codes for .NET APIs

**Códigos de estado HTTP esenciales para APIs REST en .NET con ejemplos de uso específicos y mejores prácticas.**
Esta tabla presenta los códigos más importantes para APIs, cuándo usarlos y cómo implementarlos correctamente.
Fundamental para diseñar APIs REST que comuniquen correctamente el resultado de las operaciones a los clientes.

| **Código**                     | **Nombre**                    | **Uso típico en APIs .NET**                                                                  |
| ------------------------------ | ----------------------------- | -------------------------------------------------------------------------------------------- |
| **200 OK**                     | Éxito                         | Respuesta estándar a una petición GET/POST exitosa.                                          |
| **201 Created**                | Recurso creado                | Usado al crear un recurso (`POST /orders`). Incluye `Location` con la URL del nuevo recurso. |
| **202 Accepted**               | Aceptado para procesamiento   | La petición fue aceptada pero se procesará asíncronamente. Útil en colas/eventos.            |
| **204 No Content**             | Sin contenido                 | Éxito sin cuerpo de respuesta. Usado en `DELETE` o `PUT` exitoso.                            |
| **301 Moved Permanently**      | Redirección permanente        | Indica que la URL cambió definitivamente.                                                    |
| **302 Found**                  | Redirección temporal          | Recurso disponible en otra URL temporalmente.                                                |
| **304 Not Modified**           | No modificado                 | Respuesta a `GET` condicional (cache).                                                       |
| **400 Bad Request**            | Petición inválida             | Datos malformados o validación fallida en el request body.                                   |
| **401 Unauthorized**           | No autenticado                | El cliente no envió credenciales válidas (ej. JWT inválido).                                 |
| **403 Forbidden**              | Prohibido                     | Autenticado pero sin permisos suficientes.                                                   |
| **404 Not Found**              | No encontrado                 | Recurso no existe (ej. `GET /orders/999`).                                                   |
| **405 Method Not Allowed**     | Método no soportado           | Se usó un verbo HTTP incorrecto (ej. `PUT` donde solo se permite `GET`).                     |
| **409 Conflict**               | Conflicto                     | Estado inconsistente (ej. crear un recurso que ya existe).                                   |
| **415 Unsupported Media Type** | Tipo de contenido inválido    | El `Content-Type` no es soportado por el endpoint.                                           |
| **422 Unprocessable Entity**   | Entidad no procesable         | Request correcto pero reglas de negocio inválidas (ej. pago rechazado).                      |
| **429 Too Many Requests**      | Límite de peticiones superado | Protege contra abusos o DoS (rate limiting).                                                 |
| **500 Internal Server Error**  | Error interno                 | Excepción no controlada en el servidor.                                                      |
| **502 Bad Gateway**            | Error de gateway              | Proxy o gateway no pudo comunicarse con servicio downstream.                                 |
| **503 Service Unavailable**    | Servicio no disponible        | El servicio está caído o en mantenimiento.                                                   |
| **504 Gateway Timeout**        | Timeout en gateway            | El servicio downstream no respondió a tiempo.                                                |
