# Authentication & Authorization Patterns

**Patrones de autenticación y autorización en .NET con JWT, OAuth2, RBAC y Zero Trust.**
Este documento cubre implementación de identity providers, policy-based authorization y security patterns.
Fundamental para proteger APIs y aplicaciones con authentication moderna y fine-grained authorization.

## Authentication Patterns

**Patrones fundamentales de autenticación en aplicaciones .NET modernas.**
Esta tabla compara diferentes mecanismos de autenticación con sus casos de uso y características.
Esencial para elegir la estrategia correcta según el tipo de aplicación y requisitos de seguridad.

| **Patrón**            | **Uso Principal**            | **Ventajas**              | **Desafíos**               | **Casos de Uso**           |
| --------------------- | ---------------------------- | ------------------------- | -------------------------- | -------------------------- |
| **JWT Bearer**        | API stateless authentication | Escalable, cross-domain   | Token management, refresh  | REST APIs, microservices   |
| **OAuth2 + OIDC**     | Third-party authentication   | Standards-based, SSO      | Complexity, flow selection | Web apps, mobile apps      |
| **API Keys**          | Service-to-service auth      | Simple, fast              | Limited scope, management  | Internal APIs, webhooks    |
| **Certificate Auth**  | High-security scenarios      | Strong cryptography       | PKI complexity             | B2B, device authentication |
| **SAML**              | Enterprise SSO               | Federation, attributes    | XML complexity, legacy     | Enterprise applications    |
| **Multi-factor Auth** | Enhanced security            | Additional security layer | UX complexity              | Sensitive operations       |

## JWT Authentication Implementation

**Implementación completa de autenticación JWT con refresh tokens y security best practices.**
Esta sección demuestra generación de tokens, validación y manejo seguro de credentials.
Fundamental para APIs que requieren authentication stateless y escalable.

```csharp
// JWT authentication service
public class JwtAuthenticationService
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly JwtOptions _jwtOptions;
    private readonly IUserService _userService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<JwtAuthenticationService> _logger;

    public JwtAuthenticationService(
        IOptions<JwtOptions> jwtOptions,
        IUserService userService,
        IRefreshTokenService refreshTokenService,
        ILogger<JwtAuthenticationService> logger)
    {
        _tokenHandler = new JwtSecurityTokenHandler();
        _jwtOptions = jwtOptions.Value;
        _userService = userService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        _logger.LogInformation("Authenticating user {Email}", request.Email);

        try
        {
            // Validate user credentials
            var user = await _userService.ValidateCredentialsAsync(request.Email, request.Password);

            if (user == null)
            {
                _logger.LogWarning("Authentication failed for user {Email}: Invalid credentials", request.Email);
                return AuthenticationResult.Failure("Invalid credentials");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed for user {Email}: Account inactive", request.Email);
                return AuthenticationResult.Failure("Account is inactive");
            }

            // Check for account lockout
            if (user.IsLockedOut)
            {
                _logger.LogWarning("Authentication failed for user {Email}: Account locked", request.Email);
                return AuthenticationResult.Failure($"Account is locked until {user.LockoutEnd}");
            }

            // Generate tokens
            var accessToken = GenerateAccessToken(user);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id);

            // Update last login
            await _userService.UpdateLastLoginAsync(user.Id);

            _logger.LogInformation("Successfully authenticated user {UserId} ({Email})", user.Id, user.Email);

            return AuthenticationResult.Success(new AuthTokens
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresIn = _jwtOptions.AccessTokenExpirationMinutes * 60,
                TokenType = "Bearer"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error for user {Email}", request.Email);
            return AuthenticationResult.Failure("Authentication failed");
        }
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogDebug("Refreshing access token");

        try
        {
            var tokenData = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken);

            if (tokenData == null)
            {
                _logger.LogWarning("Invalid refresh token provided");
                return AuthenticationResult.Failure("Invalid refresh token");
            }

            var user = await _userService.GetByIdAsync(tokenData.UserId);

            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Refresh token user {UserId} not found or inactive", tokenData.UserId);
                await _refreshTokenService.RevokeTokenAsync(refreshToken);
                return AuthenticationResult.Failure("User not found or inactive");
            }

            // Generate new tokens
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken);

            _logger.LogDebug("Successfully refreshed tokens for user {UserId}", user.Id);

            return AuthenticationResult.Success(new AuthTokens
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresIn = _jwtOptions.AccessTokenExpirationMinutes * 60,
                TokenType = "Bearer"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh error");
            return AuthenticationResult.Failure("Token refresh failed");
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        try
        {
            await _refreshTokenService.RevokeTokenAsync(refreshToken);
            _logger.LogInformation("Successfully revoked refresh token");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke refresh token");
            return false;
        }
    }

    private string GenerateAccessToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_jwtOptions.SecretKey);
        var claims = BuildClaims(user);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),

            // Additional security claims
            NotBefore = DateTime.UtcNow,
            IssuedAt = DateTime.UtcNow
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    private List<Claim> BuildClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),

            // Custom claims
            new("tenant_id", user.TenantId?.ToString() ?? ""),
            new("user_type", user.UserType.ToString()),
            new("email_verified", user.EmailConfirmed.ToString().ToLower())
        };

        // Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permission claims
        foreach (var permission in user.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        // Add custom claims from user profile
        if (user.CustomClaims?.Any() == true)
        {
            foreach (var customClaim in user.CustomClaims)
            {
                claims.Add(new Claim(customClaim.Key, customClaim.Value));
            }
        }

        return claims;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var key = Encoding.ASCII.GetBytes(_jwtOptions.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5), // Allow 5 minutes clock skew
                RequireExpirationTime = true
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Additional validation
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }
}

// Refresh token service
public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repository;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IRefreshTokenRepository repository,
        IOptions<JwtOptions> jwtOptions,
        ILogger<RefreshTokenService> logger)
    {
        _repository = repository;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(refreshToken);

        _logger.LogDebug("Generated refresh token for user {UserId}", userId);

        return refreshToken;
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _repository.GetByTokenAsync(token);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token not found");
            return null;
        }

        if (refreshToken.IsRevoked)
        {
            _logger.LogWarning("Attempted use of revoked refresh token for user {UserId}", refreshToken.UserId);
            return null;
        }

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired refresh token used for user {UserId}", refreshToken.UserId);
            await _repository.RevokeAsync(token);
            return null;
        }

        return refreshToken;
    }

    public async Task<RefreshToken> RotateRefreshTokenAsync(string oldToken)
    {
        var oldRefreshToken = await ValidateRefreshTokenAsync(oldToken);

        if (oldRefreshToken == null)
        {
            throw new SecurityException("Invalid refresh token");
        }

        // Revoke old token
        await _repository.RevokeAsync(oldToken);

        // Generate new token
        return await GenerateRefreshTokenAsync(oldRefreshToken.UserId);
    }

    public async Task RevokeTokenAsync(string token)
    {
        await _repository.RevokeAsync(token);
        _logger.LogDebug("Revoked refresh token");
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        await _repository.RevokeAllUserTokensAsync(userId);
        _logger.LogInformation("Revoked all refresh tokens for user {UserId}", userId);
    }

    private string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

// JWT authentication middleware
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtAuthenticationService _authService;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(
        RequestDelegate next,
        JwtAuthenticationService authService,
        ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _authService = authService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractTokenFromHeader(context.Request);

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var principal = _authService.ValidateToken(token);

                if (principal != null)
                {
                    context.User = principal;

                    // Add user context for logging
                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;

                    using var scope = _logger.BeginScope(new Dictionary<string, object>
                    {
                        ["UserId"] = userId ?? "unknown",
                        ["UserEmail"] = userEmail ?? "unknown"
                    });

                    await _next(context);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed");
            }
        }

        await _next(context);
    }

    private string? ExtractTokenFromHeader(HttpRequest request)
    {
        var authHeader = request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        return authHeader["Bearer ".Length..].Trim();
    }
}

// Configuration and models
public class JwtOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public Guid? TenantId { get; set; }
    public UserType UserType { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, string>? CustomClaims { get; set; }
}

public enum UserType
{
    Regular,
    Admin,
    Service
}

public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class AuthTokens
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

public class AuthenticationResult
{
    public bool Success { get; set; }
    public AuthTokens? Tokens { get; set; }
    public string? ErrorMessage { get; set; }

    public static AuthenticationResult Success(AuthTokens tokens) =>
        new() { Success = true, Tokens = tokens };

    public static AuthenticationResult Failure(string error) =>
        new() { Success = false, ErrorMessage = error };
}

// Service interfaces
public interface IUserService
{
    Task<User?> ValidateCredentialsAsync(string email, string password);
    Task<User?> GetByIdAsync(Guid userId);
    Task UpdateLastLoginAsync(Guid userId);
}

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
    Task<RefreshToken> RotateRefreshTokenAsync(string oldToken);
    Task RevokeTokenAsync(string token);
    Task RevokeAllUserTokensAsync(Guid userId);
}

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeAsync(string token);
    Task RevokeAllUserTokensAsync(Guid userId);
}
```

## Policy-Based Authorization

**Implementación de autorización basada en políticas con requirements y handlers personalizados.**
Esta sección demuestra RBAC, ABAC y authorization policies complejas con múltiples criterios.
Fundamental para implementar fine-grained access control y business rules de seguridad.

```csharp
// Authorization policies configuration
public static class AuthorizationPolicies
{
    public const string RequireAdminRole = "RequireAdminRole";
    public const string RequireManagerRole = "RequireManagerRole";
    public const string RequireVerifiedEmail = "RequireVerifiedEmail";
    public const string RequireTenantAccess = "RequireTenantAccess";
    public const string RequireResourceOwnership = "RequireResourceOwnership";
    public const string RequireBusinessHours = "RequireBusinessHours";
    public const string RequireMinimumAge = "RequireMinimumAge";

    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        // Role-based policies
        options.AddPolicy(RequireAdminRole, policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy(RequireManagerRole, policy =>
            policy.RequireRole("Manager", "Admin"));

        // Email verification requirement
        options.AddPolicy(RequireVerifiedEmail, policy =>
            policy.RequireClaim("email_verified", "true"));

        // Tenant-based access
        options.AddPolicy(RequireTenantAccess, policy =>
            policy.AddRequirements(new TenantAccessRequirement()));

        // Resource ownership
        options.AddPolicy(RequireResourceOwnership, policy =>
            policy.AddRequirements(new ResourceOwnershipRequirement()));

        // Business hours restriction
        options.AddPolicy(RequireBusinessHours, policy =>
            policy.AddRequirements(new BusinessHoursRequirement()));

        // Age-based restriction
        options.AddPolicy(RequireMinimumAge, policy =>
            policy.AddRequirements(new MinimumAgeRequirement(18)));

        // Complex permission-based policy
        options.AddPolicy("CanManageOrders", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("email_verified", "true");
            policy.AddRequirements(
                new PermissionRequirement("orders.manage"),
                new TenantAccessRequirement()
            );
        });

        // Conditional access based on resource and action
        options.AddPolicy("ConditionalResourceAccess", policy =>
        {
            policy.AddRequirements(new ConditionalAccessRequirement());
        });
    }
}

// Custom authorization requirements
public class TenantAccessRequirement : IAuthorizationRequirement
{
    public bool AllowCrossTenantAccess { get; set; } = false;
}

public class ResourceOwnershipRequirement : IAuthorizationRequirement
{
    public string ResourceIdParameterName { get; set; } = "id";
}

public class BusinessHoursRequirement : IAuthorizationRequirement
{
    public TimeSpan StartTime { get; set; } = new(9, 0, 0); // 9 AM
    public TimeSpan EndTime { get; set; } = new(17, 0, 0); // 5 PM
    public List<DayOfWeek> AllowedDays { get; set; } = new()
    {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday
    };
}

public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public int MinimumAge { get; }

    public MinimumAgeRequirement(int minimumAge)
    {
        MinimumAge = minimumAge;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class ConditionalAccessRequirement : IAuthorizationRequirement
{
    // This requirement will be evaluated by conditional access handler
}

// Authorization handlers
public class TenantAccessHandler : AuthorizationHandler<TenantAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantAccessHandler> _logger;

    public TenantAccessHandler(
        IHttpContextAccessor httpContextAccessor,
        ITenantService tenantService,
        ILogger<TenantAccessHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantService = tenantService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantAccessRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }

        var userTenantId = context.User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(userTenantId))
        {
            _logger.LogWarning("User {UserId} has no tenant claim",
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            context.Fail();
            return;
        }

        // Get requested tenant from route or header
        var requestedTenantId = httpContext.Request.RouteValues["tenantId"]?.ToString() ??
                               httpContext.Request.Headers["X-Tenant-ID"].FirstOrDefault();

        if (string.IsNullOrEmpty(requestedTenantId))
        {
            // If no specific tenant requested, allow access to user's own tenant
            context.Succeed(requirement);
            return;
        }

        // Check if user has access to requested tenant
        if (userTenantId == requestedTenantId)
        {
            context.Succeed(requirement);
            return;
        }

        // Check cross-tenant access if allowed
        if (requirement.AllowCrossTenantAccess)
        {
            var hasAccess = await _tenantService.HasCrossTenantAccessAsync(
                Guid.Parse(userTenantId),
                Guid.Parse(requestedTenantId));

            if (hasAccess)
            {
                _logger.LogInformation("Cross-tenant access granted for user {UserId} to tenant {TenantId}",
                    context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, requestedTenantId);
                context.Succeed(requirement);
                return;
            }
        }

        _logger.LogWarning("Tenant access denied for user {UserId} to tenant {TenantId}",
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, requestedTenantId);
        context.Fail();
    }
}

public class ResourceOwnershipHandler : AuthorizationHandler<ResourceOwnershipRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IResourceOwnershipService _ownershipService;
    private readonly ILogger<ResourceOwnershipHandler> _logger;

    public ResourceOwnershipHandler(
        IHttpContextAccessor httpContextAccessor,
        IResourceOwnershipService ownershipService,
        ILogger<ResourceOwnershipHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _ownershipService = ownershipService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnershipRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        var resourceId = httpContext.Request.RouteValues[requirement.ResourceIdParameterName]?.ToString();
        if (string.IsNullOrEmpty(resourceId))
        {
            _logger.LogWarning("Resource ID not found in route for ownership check");
            context.Fail();
            return;
        }

        var resourceType = GetResourceTypeFromEndpoint(httpContext);
        var isOwner = await _ownershipService.IsResourceOwnerAsync(userId, resourceType, resourceId);

        if (isOwner)
        {
            _logger.LogDebug("Resource ownership confirmed for user {UserId} and resource {ResourceId}",
                userId, resourceId);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Resource ownership denied for user {UserId} and resource {ResourceId}",
                userId, resourceId);
            context.Fail();
        }
    }

    private string GetResourceTypeFromEndpoint(HttpContext httpContext)
    {
        var endpoint = httpContext.GetEndpoint();
        var controllerName = endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor>()?.ControllerName;
        return controllerName?.ToLowerInvariant() ?? "unknown";
    }
}

public class BusinessHoursHandler : AuthorizationHandler<BusinessHoursRequirement>
{
    private readonly ILogger<BusinessHoursHandler> _logger;

    public BusinessHoursHandler(ILogger<BusinessHoursHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BusinessHoursRequirement requirement)
    {
        var now = DateTime.Now;
        var currentTime = now.TimeOfDay;
        var currentDay = now.DayOfWeek;

        if (!requirement.AllowedDays.Contains(currentDay))
        {
            _logger.LogWarning("Access denied outside allowed days. Current day: {Day}", currentDay);
            context.Fail();
            return Task.CompletedTask;
        }

        if (currentTime < requirement.StartTime || currentTime > requirement.EndTime)
        {
            _logger.LogWarning("Access denied outside business hours. Current time: {Time}", currentTime);
            context.Fail();
            return Task.CompletedTask;
        }

        _logger.LogDebug("Business hours check passed for time {Time} on {Day}", currentTime, currentDay);
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionHandler> _logger;

    public PermissionHandler(ILogger<PermissionHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissions = context.User.FindAll("permission").Select(c => c.Value);

        if (permissions.Contains(requirement.Permission))
        {
            _logger.LogDebug("Permission {Permission} found for user {UserId}",
                requirement.Permission,
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Permission {Permission} not found for user {UserId}",
                requirement.Permission,
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

public class ConditionalAccessHandler : AuthorizationHandler<ConditionalAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRiskAssessmentService _riskService;
    private readonly ILogger<ConditionalAccessHandler> _logger;

    public ConditionalAccessHandler(
        IHttpContextAccessor httpContextAccessor,
        IRiskAssessmentService riskService,
        ILogger<ConditionalAccessHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _riskService = riskService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ConditionalAccessRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        // Assess risk based on various factors
        var riskContext = new RiskAssessmentContext
        {
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            RequestPath = httpContext.Request.Path,
            RequestMethod = httpContext.Request.Method,
            Timestamp = DateTime.UtcNow
        };

        var riskLevel = await _riskService.AssessRiskAsync(riskContext);

        switch (riskLevel)
        {
            case RiskLevel.Low:
                _logger.LogDebug("Low risk access granted for user {UserId}", userId);
                context.Succeed(requirement);
                break;

            case RiskLevel.Medium:
                // Check if user has recent MFA
                var lastMfa = context.User.FindFirst("last_mfa")?.Value;
                if (!string.IsNullOrEmpty(lastMfa) &&
                    DateTime.TryParse(lastMfa, out var mfaTime) &&
                    DateTime.UtcNow - mfaTime < TimeSpan.FromHours(1))
                {
                    _logger.LogInformation("Medium risk access granted with recent MFA for user {UserId}", userId);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning("Medium risk access denied - MFA required for user {UserId}", userId);
                    context.Fail(new AuthorizationFailureReason(this, "MFA required for medium risk access"));
                }
                break;

            case RiskLevel.High:
                _logger.LogWarning("High risk access denied for user {UserId} from IP {IpAddress}", userId, ipAddress);
                context.Fail(new AuthorizationFailureReason(this, "Access denied due to high risk"));
                break;
        }
    }
}

// Supporting services and models
public interface ITenantService
{
    Task<bool> HasCrossTenantAccessAsync(Guid userTenantId, Guid requestedTenantId);
}

public interface IResourceOwnershipService
{
    Task<bool> IsResourceOwnerAsync(string userId, string resourceType, string resourceId);
}

public interface IRiskAssessmentService
{
    Task<RiskLevel> AssessRiskAsync(RiskAssessmentContext context);
}

public class RiskAssessmentContext
{
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum RiskLevel
{
    Low,
    Medium,
    High
}

// Authorization attribute for easier usage
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"Permission_{permission}";
    }
}

// Usage in controllers
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RequireTenantAccess)]
    public async Task<IActionResult> GetOrders()
    {
        // Implementation
        return Ok();
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireResourceOwnership)]
    public async Task<IActionResult> GetOrder(int id)
    {
        // Implementation
        return Ok();
    }

    [HttpPost]
    [RequirePermission("orders.create")]
    [Authorize(Policy = AuthorizationPolicies.RequireVerifiedEmail)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Implementation
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ConditionalResourceAccess")]
    [Authorize(Policy = AuthorizationPolicies.RequireResourceOwnership)]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        // Implementation
        return Ok();
    }
}

public class CreateOrderRequest
{
    // Order creation properties
}
```

## OAuth2 and OpenID Connect

**Integración OAuth2 y OpenID Connect para autenticación federada y SSO enterprise.**
Esta sección demuestra configuración de identity providers, scopes y token handling.
Fundamental para aplicaciones que requieren integración con Azure AD, Google, o identity providers externos.

| **Componente**           | **Responsabilidad** | **Configuración**             | **Casos de Uso**       |
| ------------------------ | ------------------- | ----------------------------- | ---------------------- |
| **Authorization Server** | Emitir tokens       | Client credentials, scopes    | SSO, API access        |
| **Resource Server**      | Validar tokens      | JWT validation, introspection | Protected APIs         |
| **Client Application**   | Consume APIs        | Flow selection, token storage | Web apps, SPAs         |
| **Identity Provider**    | User authentication | Claims mapping, protocols     | Enterprise integration |

```csharp
// OAuth2 configuration
public static class OAuth2Configuration
{
    public static void ConfigureOAuth2(WebApplicationBuilder builder)
    {
        var oauthSection = builder.Configuration.GetSection("OAuth2");

        builder.Services.Configure<OAuth2Options>(oauthSection);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var oauth2Options = oauthSection.Get<OAuth2Options>();

            options.Authority = oauth2Options.Authority;
            options.Audience = oauth2Options.Audience;
            options.RequireHttpsMetadata = oauth2Options.RequireHttps;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(context.Exception, "JWT authentication failed");
                    return Task.CompletedTask;
                },

                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var userId = context.Principal?.FindFirst("sub")?.Value;
                    logger.LogDebug("JWT token validated for user {UserId}", userId);
                    return Task.CompletedTask;
                }
            };
        })
        .AddOpenIdConnect("oidc", options =>
        {
            var oauth2Options = oauthSection.Get<OAuth2Options>();

            options.Authority = oauth2Options.Authority;
            options.ClientId = oauth2Options.ClientId;
            options.ClientSecret = oauth2Options.ClientSecret;
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.RequireHttpsMetadata = oauth2Options.RequireHttps;

            // Add required scopes
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Scope.Add("api.access");

            options.Events = new OpenIdConnectEvents
            {
                OnAuthorizationCodeReceived = async context =>
                {
                    // Custom token processing if needed
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogDebug("Authorization code received for user");
                },

                OnTokenValidated = async context =>
                {
                    // Process claims and map to application user
                    var userService = context.HttpContext.RequestServices.GetRequiredService<IUserMappingService>();
                    await userService.ProcessExternalUserAsync(context.Principal);
                }
            };
        });
    }
}

public class OAuth2Options
{
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool RequireHttps { get; set; } = true;
    public List<string> Scopes { get; set; } = new();
}

// External user mapping service
public interface IUserMappingService
{
    Task ProcessExternalUserAsync(ClaimsPrincipal externalUser);
    Task<User?> MapExternalUserAsync(ClaimsPrincipal externalUser);
}

public class UserMappingService : IUserMappingService
{
    private readonly IUserService _userService;
    private readonly ILogger<UserMappingService> _logger;

    public UserMappingService(IUserService userService, ILogger<UserMappingService> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task ProcessExternalUserAsync(ClaimsPrincipal externalUser)
    {
        var externalUserId = externalUser.FindFirst("sub")?.Value;
        var email = externalUser.FindFirst("email")?.Value;

        if (string.IsNullOrEmpty(externalUserId) || string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("External user missing required claims");
            return;
        }

        var existingUser = await _userService.GetByExternalIdAsync(externalUserId);

        if (existingUser == null)
        {
            // Create new user from external identity
            var newUser = await MapExternalUserAsync(externalUser);
            if (newUser != null)
            {
                await _userService.CreateUserAsync(newUser);
                _logger.LogInformation("Created new user {UserId} from external identity {ExternalId}",
                    newUser.Id, externalUserId);
            }
        }
        else
        {
            // Update existing user with latest claims
            await UpdateUserFromExternalClaims(existingUser, externalUser);
        }
    }

    public async Task<User?> MapExternalUserAsync(ClaimsPrincipal externalUser)
    {
        var externalUserId = externalUser.FindFirst("sub")?.Value;
        var email = externalUser.FindFirst("email")?.Value;
        var name = externalUser.FindFirst("name")?.Value ??
                  externalUser.FindFirst("given_name")?.Value ??
                  email;

        if (string.IsNullOrEmpty(externalUserId) || string.IsNullOrEmpty(email))
        {
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name ?? string.Empty,
            ExternalId = externalUserId,
            ExternalProvider = GetProviderName(externalUser),
            IsActive = true,
            EmailConfirmed = true, // Trust external provider
            UserType = UserType.Regular
        };

        // Map roles from external claims
        var roleClaims = externalUser.FindAll("role").Select(c => c.Value).ToList();
        if (roleClaims.Any())
        {
            user.Roles = MapExternalRoles(roleClaims);
        }

        // Map custom claims
        user.CustomClaims = ExtractCustomClaims(externalUser);

        return user;
    }

    private async Task UpdateUserFromExternalClaims(User user, ClaimsPrincipal externalUser)
    {
        var name = externalUser.FindFirst("name")?.Value;
        if (!string.IsNullOrEmpty(name) && name != user.Name)
        {
            user.Name = name;
        }

        // Update roles if they've changed
        var externalRoles = externalUser.FindAll("role").Select(c => c.Value).ToList();
        if (externalRoles.Any())
        {
            var mappedRoles = MapExternalRoles(externalRoles);
            if (!user.Roles.SequenceEqual(mappedRoles))
            {
                user.Roles = mappedRoles;
                _logger.LogInformation("Updated roles for user {UserId}: {Roles}",
                    user.Id, string.Join(", ", mappedRoles));
            }
        }

        await _userService.UpdateUserAsync(user);
    }

    private string GetProviderName(ClaimsPrincipal user)
    {
        var issuer = user.FindFirst("iss")?.Value;

        return issuer switch
        {
            var i when i?.Contains("login.microsoftonline.com") == true => "AzureAD",
            var i when i?.Contains("accounts.google.com") == true => "Google",
            var i when i?.Contains("github.com") == true => "GitHub",
            _ => "Unknown"
        };
    }

    private List<string> MapExternalRoles(List<string> externalRoles)
    {
        // Map external provider roles to application roles
        var mappedRoles = new List<string>();

        foreach (var externalRole in externalRoles)
        {
            var mappedRole = externalRole.ToLowerInvariant() switch
            {
                "global administrator" => "Admin",
                "user administrator" => "Manager",
                "directory readers" => "User",
                "admin" => "Admin",
                "manager" => "Manager",
                "user" => "User",
                _ => "User" // Default role
            };

            if (!mappedRoles.Contains(mappedRole))
            {
                mappedRoles.Add(mappedRole);
            }
        }

        return mappedRoles;
    }

    private Dictionary<string, string> ExtractCustomClaims(ClaimsPrincipal user)
    {
        var customClaims = new Dictionary<string, string>();

        // Extract specific claims we care about
        var claimsToExtract = new[]
        {
            "department", "job_title", "office_location",
            "employee_id", "cost_center", "manager"
        };

        foreach (var claimType in claimsToExtract)
        {
            var claim = user.FindFirst(claimType);
            if (claim != null && !string.IsNullOrEmpty(claim.Value))
            {
                customClaims[claimType] = claim.Value;
            }
        }

        return customClaims;
    }
}
```
