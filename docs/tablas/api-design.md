# API Design for .NET

**Guía completa de diseño de APIs REST en .NET con patrones, versionado y mejores prácticas.**
Este documento cubre desde principios fundamentales hasta implementaciones avanzadas de seguridad y escalabilidad.
Fundamental para crear APIs robustas, mantenibles y que cumplan estándares de la industria.

## REST API Design Principles

**Principios fundamentales para diseñar APIs REST consistentes y predecibles.**
Esta tabla establece las bases para una arquitectura API sólida siguiendo estándares HTTP y REST.
Esencial para crear interfaces que sean intuitivas para desarrolladores y fáciles de consumir.

| **Principio**           | **Descripción**                                 | **Buena Práctica**      | **Evitar**               |
| ----------------------- | ----------------------------------------------- | ----------------------- | ------------------------ |
| **Resource-Based URLs** | URLs representan recursos, no acciones          | `/api/users/123`        | `/api/getUser?id=123`    |
| **HTTP Methods**        | Usar verbos HTTP correctamente                  | GET, POST, PUT, DELETE  | Solo GET/POST            |
| **Status Codes**        | Códigos de estado HTTP semánticamente correctos | 200, 201, 400, 404, 500 | Siempre 200              |
| **Stateless**           | Cada request es independiente                   | JWT tokens, headers     | Sessions server-side     |
| **Cacheable**           | Responses marcadas apropiadamente               | Cache-Control headers   | Sin headers de caché     |
| **Uniform Interface**   | Consistencia en toda la API                     | Convenciones uniformes  | Endpoints inconsistentes |

## HTTP Status Codes for APIs

**Códigos de estado HTTP estándar organizados por categoría para APIs REST.**
Esta referencia asegura respuestas semánticamente correctas que faciliten debugging y integración.
Fundamental para comunicar claramente el resultado de operaciones a clientes de la API.

| **Código** | **Significado**       | **Cuándo Usar**                        | **Response Body**                |
| ---------- | --------------------- | -------------------------------------- | -------------------------------- |
| **200**    | OK                    | GET exitoso, operación completada      | Datos solicitados                |
| **201**    | Created               | POST exitoso, recurso creado           | Recurso creado + Location header |
| **204**    | No Content            | DELETE exitoso, PUT sin contenido      | Sin body                         |
| **400**    | Bad Request           | Request malformado, validación fallida | Error details                    |
| **401**    | Unauthorized          | Falta autenticación                    | Error message                    |
| **403**    | Forbidden             | Sin permisos para el recurso           | Error message                    |
| **404**    | Not Found             | Recurso no existe                      | Error message                    |
| **409**    | Conflict              | Conflicto de estado (ej: duplicado)    | Error details                    |
| **422**    | Unprocessable Entity  | Validación de negocio fallida          | Validation errors                |
| **500**    | Internal Server Error | Error no controlado del servidor       | Generic error                    |

## RESTful API Implementation

**Implementación completa de un controlador REST siguiendo mejores prácticas.**
Este ejemplo demuestra validación, manejo de errores, paginación y documentación automática.
Ideal como template para controladores empresariales que requieren robustez y mantenibilidad.

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IMapper mapper,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of products
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page (max 100)</param>
    /// <param name="category">Optional category filter</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductDto>), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "pageNumber", "pageSize", "category", "searchTerm" })]
    public async Task<ActionResult<PagedResponse<ProductDto>>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1)
                return BadRequest(new ApiError("PageNumber must be greater than 0"));

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(new ApiError("PageSize must be between 1 and 100"));

            var filter = new ProductFilter
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Category = category,
                SearchTerm = searchTerm
            };

            var products = await _productService.GetProductsAsync(filter);
            var productDtos = _mapper.Map<List<ProductDto>>(products.Items);

            var response = new PagedResponse<ProductDto>
            {
                Items = productDtos,
                PageNumber = products.PageNumber,
                PageSize = products.PageSize,
                TotalItems = products.TotalItems,
                TotalPages = products.TotalPages,
                HasNextPage = products.HasNextPage,
                HasPreviousPage = products.HasPreviousPage
            };

            // Add pagination links
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            response.Links = GeneratePaginationLinks(baseUrl, filter, products.TotalPages);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products with filter: {@Filter}", filter);
            return StatusCode(500, new ApiError("An error occurred while retrieving products"));
        }
    }

    /// <summary>
    /// Retrieves a specific product by ID
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailDto), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ResponseCache(Duration = 600, VaryByHeader = "Accept-Language")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound(new ApiError($"Product with ID {id} not found"));
        }

        var productDto = _mapper.Map<ProductDetailDto>(product);

        // Add HATEOAS links
        productDto.Links = new Dictionary<string, Link>
        {
            ["self"] = new Link { Href = Url.Action(nameof(GetProduct), new { id }) },
            ["update"] = new Link { Href = Url.Action(nameof(UpdateProduct), new { id }), Method = "PUT" },
            ["delete"] = new Link { Href = Url.Action(nameof(DeleteProduct), new { id }), Method = "DELETE" },
            ["reviews"] = new Link { Href = $"/api/v1/products/{id}/reviews" }
        };

        return Ok(productDto);
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="productDto">Product data</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(typeof(ApiError), 409)]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto productDto)
    {
        try
        {
            // Additional business validation
            if (await _productService.ProductExistsAsync(productDto.Sku))
            {
                return Conflict(new ApiError($"Product with SKU '{productDto.Sku}' already exists"));
            }

            var product = _mapper.Map<Product>(productDto);
            var createdProduct = await _productService.CreateProductAsync(product);
            var resultDto = _mapper.Map<ProductDto>(createdProduct);

            var location = Url.Action(nameof(GetProduct), new { id = createdProduct.Id });

            _logger.LogInformation("Product created successfully: {ProductId}", createdProduct.Id);

            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, resultDto);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiError(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {@ProductDto}", productDto);
            return StatusCode(500, new ApiError("An error occurred while creating the product"));
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <param name="productDto">Updated product data</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto productDto)
    {
        try
        {
            var existingProduct = await _productService.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound(new ApiError($"Product with ID {id} not found"));
            }

            // Check for optimistic concurrency
            if (productDto.Version != existingProduct.Version)
            {
                return Conflict(new ApiError("Product has been modified by another user. Please refresh and try again."));
            }

            _mapper.Map(productDto, existingProduct);
            var updatedProduct = await _productService.UpdateProductAsync(existingProduct);
            var resultDto = _mapper.Map<ProductDto>(updatedProduct);

            _logger.LogInformation("Product updated successfully: {ProductId}", id);

            return Ok(resultDto);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiError(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}: {@ProductDto}", id, productDto);
            return StatusCode(500, new ApiError("An error occurred while updating the product"));
        }
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 409)]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        try
        {
            var exists = await _productService.ProductExistsByIdAsync(id);
            if (!exists)
            {
                return NotFound(new ApiError($"Product with ID {id} not found"));
            }

            // Check for dependencies
            if (await _productService.HasActiveOrdersAsync(id))
            {
                return Conflict(new ApiError("Cannot delete product with active orders"));
            }

            await _productService.DeleteProductAsync(id);

            _logger.LogInformation("Product deleted successfully: {ProductId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, new ApiError("An error occurred while deleting the product"));
        }
    }

    private Dictionary<string, Link> GeneratePaginationLinks(string baseUrl, ProductFilter filter, int totalPages)
    {
        var links = new Dictionary<string, Link>
        {
            ["self"] = new Link { Href = $"{baseUrl}?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}" }
        };

        if (filter.PageNumber > 1)
        {
            links["first"] = new Link { Href = $"{baseUrl}?pageNumber=1&pageSize={filter.PageSize}" };
            links["prev"] = new Link { Href = $"{baseUrl}?pageNumber={filter.PageNumber - 1}&pageSize={filter.PageSize}" };
        }

        if (filter.PageNumber < totalPages)
        {
            links["next"] = new Link { Href = $"{baseUrl}?pageNumber={filter.PageNumber + 1}&pageSize={filter.PageSize}" };
            links["last"] = new Link { Href = $"{baseUrl}?pageNumber={totalPages}&pageSize={filter.PageSize}" };
        }

        return links;
    }
}
```

## API Versioning Strategies

**Estrategias de versionado de APIs para mantener compatibilidad hacia atrás.**
Esta tabla compara diferentes enfoques de versionado con sus ventajas y casos de uso.
Fundamental para evolucionar APIs sin romper integraciones existentes.

| **Estrategia**      | **Implementación**                           | **Ventajas**         | **Desventajas** | **Cuándo Usar**   |
| ------------------- | -------------------------------------------- | -------------------- | --------------- | ----------------- |
| **URL Path**        | `/api/v1/products`                           | Simple, explícito    | URLs múltiples  | APIs públicas     |
| **Query Parameter** | `/api/products?version=1`                    | Flexible             | Fácil omitir    | APIs internas     |
| **Header**          | `API-Version: 1.0`                           | URLs limpias         | Menos visible   | APIs REST puras   |
| **Accept Header**   | `Accept: application/vnd.api+json;version=1` | Estándar HTTP        | Complejo        | APIs hipermedia   |
| **Subdomain**       | `v1.api.company.com`                         | Aislamiento completo | Infraestructura | Versiones mayores |

### URL Path Versioning Implementation

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersV1Controller : ControllerBase
{
    [HttpGet("{id}")]
    [ApiVersion("1.0")]
    public async Task<UserV1Dto> GetUserV1(int id)
    {
        // V1 implementation - basic user info
        var user = await _userService.GetUserAsync(id);
        return new UserV1Dto
        {
            Id = user.Id,
            Name = user.FullName,
            Email = user.Email
        };
    }
}

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersV2Controller : ControllerBase
{
    [HttpGet("{id}")]
    [ApiVersion("2.0")]
    public async Task<UserV2Dto> GetUserV2(int id)
    {
        // V2 implementation - enhanced user info
        var user = await _userService.GetUserAsync(id);
        return new UserV2Dto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Profile = new UserProfileDto
            {
                Avatar = user.AvatarUrl,
                Bio = user.Biography,
                Location = user.Location
            }
        };
    }
}
```

### Header-based Versioning

```csharp
public class ApiVersionMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Default to latest version if not specified
        if (!context.Request.Headers.ContainsKey("API-Version"))
        {
            context.Request.Headers.Add("API-Version", "2.0");
        }

        await _next(context);
    }
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id}")]
    [MapToApiVersion("1.0")]
    public async Task<ProductV1Dto> GetProductV1(int id)
    {
        // Version 1.0 implementation
    }

    [HttpGet("{id}")]
    [MapToApiVersion("2.0")]
    public async Task<ProductV2Dto> GetProductV2(int id)
    {
        // Version 2.0 implementation
    }
}
```

## Error Handling and Problem Details

**Manejo estandarizado de errores siguiendo RFC 7807 Problem Details.**
Esta implementación proporciona respuestas de error consistentes y debugeables.
Esencial para APIs que requieren información de error detallada para troubleshooting.

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            ValidationException validationEx => new ValidationProblemDetails(validationEx.Errors)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path
            },

            NotFoundException notFoundEx => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Resource not found",
                Status = StatusCodes.Status404NotFound,
                Detail = notFoundEx.Message,
                Instance = context.Request.Path
            },

            BusinessRuleException businessEx => new ProblemDetails
            {
                Type = "https://example.com/problems/business-rule-violation",
                Title = "Business rule violation",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = businessEx.Message,
                Instance = context.Request.Path
            },

            UnauthorizedAccessException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "Authentication is required to access this resource",
                Instance = context.Request.Path
            },

            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "An error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Detail = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred",
                Instance = context.Request.Path
            }
        };

        // Add correlation ID for tracing
        problemDetails.Extensions["correlationId"] = context.TraceIdentifier;

        // Log error with appropriate level
        var logLevel = GetLogLevel(exception);
        _logger.Log(logLevel, exception, "Exception handled by middleware: {ExceptionType}", exception.GetType().Name);

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private LogLevel GetLogLevel(Exception exception)
    {
        return exception switch
        {
            ValidationException => LogLevel.Warning,
            NotFoundException => LogLevel.Warning,
            BusinessRuleException => LogLevel.Warning,
            UnauthorizedAccessException => LogLevel.Warning,
            _ => LogLevel.Error
        };
    }
}

// Custom exception types
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
```

## Authentication and Authorization

**Implementación de autenticación JWT y autorización basada en políticas.**
Esta sección cubre desde configuración básica hasta autorización granular con claims y policies.
Fundamental para APIs que requieren seguridad robusta y control de acceso detallado.

```csharp
// JWT Configuration
public class JwtConfiguration
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string SecretKey { get; set; }
    public int ExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}

// Authentication Service
public class AuthenticationService
{
    private readonly JwtConfiguration _jwtConfig;
    private readonly IUserService _userService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<AuthenticationService> _logger;

    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        var user = await _userService.GetByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Authentication failed: User not found for email {Email}", request.Email);
            return AuthenticationResult.Failed("Invalid email or password");
        }

        var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordVerification != PasswordVerificationResult.Success)
        {
            _logger.LogWarning("Authentication failed: Invalid password for user {UserId}", user.Id);
            return AuthenticationResult.Failed("Invalid email or password");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Authentication failed: User {UserId} is not active", user.Id);
            return AuthenticationResult.Failed("Account is not active");
        }

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Store refresh token
        await _userService.UpdateRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationDays));

        _logger.LogInformation("User {UserId} authenticated successfully", user.Id);

        return AuthenticationResult.Success(accessToken, refreshToken, user);
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Email, user.Email),
            new("user_id", user.Id.ToString()),
            new("tenant_id", user.TenantId.ToString())
        };

        // Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        // Add permission claims
        var permissions = user.Roles.SelectMany(r => r.Permissions).Distinct();
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission.Name));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Authorization Policies
public static class AuthorizationPolicies
{
    public const string RequireAdminRole = "RequireAdminRole";
    public const string RequireManagerRole = "RequireManagerRole";
    public const string RequireProductWrite = "RequireProductWrite";
    public const string RequireOrderRead = "RequireOrderRead";

    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        options.AddPolicy(RequireAdminRole, policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy(RequireManagerRole, policy =>
            policy.RequireRole("Admin", "Manager"));

        options.AddPolicy(RequireProductWrite, policy =>
            policy.RequireClaim("permission", "products:write"));

        options.AddPolicy(RequireOrderRead, policy =>
            policy.RequireClaim("permission", "orders:read"));

        // Resource-based authorization
        options.AddPolicy("RequireResourceOwner", policy =>
            policy.Requirements.Add(new ResourceOwnerRequirement()));
    }
}

// Custom Authorization Requirement
public class ResourceOwnerRequirement : IAuthorizationRequirement
{
}

public class ResourceOwnerHandler : AuthorizationHandler<ResourceOwnerRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = context.User.FindFirst("user_id")?.Value;

        // Get resource ID from route
        var resourceId = httpContext.Request.RouteValues["id"]?.ToString();

        // Check if user owns the resource (implementation specific)
        if (IsResourceOwner(userId, resourceId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private bool IsResourceOwner(string userId, string resourceId)
    {
        // Implementation specific logic
        return true;
    }
}

// Usage in Controllers
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RequireOrderRead)]
    public async Task<ActionResult<List<OrderDto>>> GetOrders()
    {
        // Implementation
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "RequireResourceOwner")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        // Implementation
    }

    [HttpPost]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto orderDto)
    {
        // Implementation
    }
}
```

## API Documentation with OpenAPI

**Documentación automática de APIs usando OpenAPI/Swagger con ejemplos y validaciones.**
Esta configuración genera documentación interactiva completa con ejemplos de uso.
Esencial para APIs que requieren documentación detallada para desarrolladores externos.

| **Anotación**                | **Propósito**              | **Ejemplo**                                       | **Beneficio**                |
| ---------------------------- | -------------------------- | ------------------------------------------------- | ---------------------------- |
| **[ProducesResponseType]**   | Define tipos de respuesta  | `[ProducesResponseType(typeof(ProductDto), 200)]` | Tipado fuerte en Swagger     |
| **[SwaggerOperation]**       | Documentación de operación | `OperationId = "GetProduct"`                      | Mejor generación de clientes |
| **[SwaggerResponse]**        | Respuesta personalizada    | `Description = "Product found"`                   | Documentación clara          |
| **[SwaggerRequestExample]**  | Ejemplo de request         | Ejemplo JSON estructurado                         | Testing simplificado         |
| **[SwaggerResponseExample]** | Ejemplo de response        | Ejemplo JSON de salida                            | Comprensión mejorada         |

```csharp
// Swagger Configuration
public class SwaggerConfiguration
{
    public static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Product API",
                Version = "v1",
                Description = "API for managing products and inventory",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "api-support@company.com",
                    Url = new Uri("https://company.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            c.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "Product API",
                Version = "v2",
                Description = "Enhanced API for managing products and inventory"
            });

            // JWT Authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            // Custom schema filters
            c.SchemaFilter<EnumSchemaFilter>();
            c.OperationFilter<SwaggerDefaultValues>();
            c.DocumentFilter<SwaggerExcludeFilter>();
        });

        services.AddSwaggerExamplesFromAssemblyOf<ProductRequestExample>();
    }
}

// Request/Response Examples
public class ProductRequestExample : IExamplesProvider<CreateProductDto>
{
    public CreateProductDto GetExamples()
    {
        return new CreateProductDto
        {
            Name = "Gaming Laptop",
            Description = "High-performance laptop for gaming and professional work",
            Sku = "LAPTOP-GAMING-001",
            Price = 1299.99m,
            CategoryId = 1,
            Brand = "TechBrand",
            Specifications = new Dictionary<string, string>
            {
                ["CPU"] = "Intel i7-11800H",
                ["RAM"] = "16GB DDR4",
                ["Storage"] = "512GB NVMe SSD",
                ["GPU"] = "NVIDIA RTX 3060"
            },
            Tags = new[] { "gaming", "laptop", "high-performance" }
        };
    }
}

public class ProductResponseExample : IExamplesProvider<ProductDto>
{
    public ProductDto GetExamples()
    {
        return new ProductDto
        {
            Id = 1,
            Name = "Gaming Laptop",
            Description = "High-performance laptop for gaming and professional work",
            Sku = "LAPTOP-GAMING-001",
            Price = 1299.99m,
            Category = new CategoryDto { Id = 1, Name = "Electronics" },
            Brand = "TechBrand",
            InStock = true,
            StockQuantity = 25,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Version = 1
        };
    }
}

// Enhanced Controller Documentation
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
[SwaggerTag("Products", "Operations related to product management")]
public class ProductsController : ControllerBase
{
    /// <summary>
    /// Creates a new product in the catalog
    /// </summary>
    /// <param name="productDto">Product information</param>
    /// <returns>The created product with assigned ID</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/products
    ///     {
    ///         "name": "Gaming Laptop",
    ///         "description": "High-performance laptop",
    ///         "sku": "LAPTOP-001",
    ///         "price": 1299.99,
    ///         "categoryId": 1
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="409">Product with same SKU already exists</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new product",
        Description = "Creates a new product in the catalog with the provided information",
        OperationId = "CreateProduct",
        Tags = new[] { "Products" }
    )]
    [SwaggerRequestExample(typeof(CreateProductDto), typeof(ProductRequestExample))]
    [SwaggerResponseExample(201, typeof(ProductDto), typeof(ProductResponseExample))]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductDto productDto)
    {
        // Implementation
    }
}
```
