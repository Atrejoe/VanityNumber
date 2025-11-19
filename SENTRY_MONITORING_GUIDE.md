# Sentry Monitoring Integration Guide

## Overview

This project includes optional Sentry monitoring for both the **Blazor WebAssembly front-end** and **ASP.NET Core backend API**. Sentry provides real-time error tracking, performance monitoring, and application health insights.

## Why Separate DSNs?

Following Sentry best practices, we use **separate DSN (Data Source Name)** for front-end and backend because:

1. **Different Error Contexts**: Front-end errors (browser, client-side) vs. backend errors (server, API)
2. **Separate Quotas**: Independent event limits and performance monitoring
3. **Better Organization**: Filter and analyze errors by service in Sentry dashboard
4. **Security**: Front-end DSN is public (in browser), backend DSN remains private
5. **Different Sampling Rates**: Client-side can have higher sampling than server-side

## Setup Instructions

### 1. Create Sentry Projects

Go to [sentry.io](https://sentry.io) and create **two projects**:

1. **Project 1: Vanity Number Web** (JavaScript/Blazor)
   - Platform: Blazor or JavaScript
   - Copy the DSN (looks like: `https://abc123@o123456.ingest.sentry.io/7654321`)

2. **Project 2: Vanity Number API** (ASP.NET Core)
   - Platform: ASP.NET Core
   - Copy the DSN (looks like: `https://xyz789@o123456.ingest.sentry.io/7654322`)

### 2. Configure Front-End (Blazor WebAssembly)

#### Development
Edit `VanityNumber.Web/wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "http://localhost:8081/",
  "Sentry": {
    "Dsn": "https://your-frontend-dsn@sentry.io/project-id",
    "Environment": "Development",
    "TracesSampleRate": 1.0,
    "Debug": false
  }
}
```

#### Production
Edit `VanityNumber.Web/wwwroot/appsettings.Production.json`:

```json
{
  "ApiBaseUrl": "https://your-production-url/",
  "Sentry": {
    "Dsn": "https://your-frontend-dsn@sentry.io/project-id",
    "Environment": "Production",
    "TracesSampleRate": 0.1,
    "Debug": false
  }
}
```

**Important**: The front-end DSN is **public** and will be visible in the browser. This is normal and expected for client-side monitoring.

### 3. Configure Backend (ASP.NET Core API)

#### Development
Edit `VanityNumber.Api/appsettings.json`:

```json
{
  "Sentry": {
    "Dsn": "https://your-backend-dsn@sentry.io/project-id",
    "Environment": "Development",
    "TracesSampleRate": 1.0,
    "SendDefaultPii": false,
    "Debug": false,
    "AttachStacktrace": true,
    "MaxBreadcrumbs": 50
  }
}
```

#### Production
Edit `VanityNumber.Api/appsettings.Production.json`:

```json
{
  "Sentry": {
    "Dsn": "https://your-backend-dsn@sentry.io/project-id",
    "Environment": "Production",
    "TracesSampleRate": 0.1,
    "SendDefaultPii": false,
    "Debug": false,
    "AttachStacktrace": true,
    "MaxBreadcrumbs": 50
  }
}
```

### 4. Environment Variables (Alternative Configuration)

Instead of editing JSON files, you can use environment variables:

#### For API (Docker/Kubernetes)
```bash
Sentry__Dsn=https://your-backend-dsn@sentry.io/project-id
Sentry__Environment=Production
Sentry__TracesSampleRate=0.1
```

#### For Docker Compose
Edit `docker-compose.yml`:

```yaml
services:
  vanity-api:
    environment:
      - Sentry__Dsn=https://your-backend-dsn@sentry.io/project-id
      - Sentry__Environment=Production
      - Sentry__TracesSampleRate=0.1
```

**Note**: For Blazor WASM front-end, Sentry configuration must be in `appsettings.Production.json` at build time (cannot use runtime environment variables).

## Configuration Options

### Common Options (Both Front-End & Backend)

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Dsn` | string | `""` | Sentry Data Source Name. Leave empty to disable Sentry. |
| `Environment` | string | `"Development"` | Environment name (e.g., Production, Staging, Development) |
| `TracesSampleRate` | double | `1.0` | Performance monitoring sample rate (0.0 to 1.0) |
| `Debug` | bool | `false` | Enable debug logging for Sentry SDK |

### Backend-Specific Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `SendDefaultPii` | bool | `false` | Send Personally Identifiable Information (user IPs, etc.) |
| `AttachStacktrace` | bool | `true` | Include stack traces with events |
| `MaxBreadcrumbs` | int | `50` | Maximum number of breadcrumbs to keep |

## Testing Sentry Integration

### 1. Test Front-End Error Tracking

Add a test button to trigger an error in `Home.razor`:

```razor
<button @onclick="() => throw new Exception("Test Sentry Front-End Error")">
    Test Sentry Error
</button>
```

### 2. Test Backend Error Tracking

Make an API call that throws an exception, or add a test endpoint:

```csharp
[HttpGet("test-sentry")]
public IActionResult TestSentry()
{
    throw new Exception("Test Sentry Backend Error");
}
```

### 3. Verify in Sentry Dashboard

1. Go to your Sentry dashboard
2. Check the **Issues** tab for both projects
3. You should see the test errors appear within seconds

## Performance Monitoring

### TracesSampleRate Recommendations

- **Development**: `1.0` (100% - capture all transactions)
- **Staging**: `0.5` (50% - good balance for testing)
- **Production**: `0.1` (10% - sufficient for most apps, reduces quota usage)
- **High Traffic**: `0.01` (1% - for apps with millions of requests)

### What Gets Tracked?

**Front-End**:
- Unhandled JavaScript/Blazor exceptions
- Page load times
- Component render times
- HTTP request performance
- User navigation patterns

**Backend**:
- Unhandled API exceptions
- HTTP request/response times
- Database query performance
- Dependency calls
- Custom transactions

## Disabling Sentry

To disable Sentry monitoring:

### Option 1: Remove DSN
Set `Dsn` to empty string in configuration files:

```json
{
  "Sentry": {
    "Dsn": ""
  }
}
```

### Option 2: Comment Out Environment Variables
In `docker-compose.yml`, comment out Sentry variables:

```yaml
# - Sentry__Dsn=https://...
```

When DSN is empty or not provided, Sentry initialization is **skipped entirely** with no performance impact.

## Best Practices

### 1. Don't Commit DSNs to Git (Optional)

Create `.env` file for local development:

```bash
# .env
SENTRY_WEB_DSN=https://your-frontend-dsn@sentry.io/project-id
SENTRY_API_DSN=https://your-backend-dsn@sentry.io/project-id
```

Add to `.gitignore`:
```
.env
appsettings.Production.json
```

### 2. Use Different Environments

Configure different environments in Sentry:
- `Development` - Local development
- `Staging` - Pre-production testing
- `Production` - Live production

### 3. Set Up Alerts

In Sentry dashboard:
1. Go to **Alerts**
2. Create rules for critical errors
3. Configure email/Slack notifications

### 4. Release Tracking

The integration automatically sets release versions based on assembly version. To see releases in Sentry:

1. Ensure your `*.csproj` files have version numbers
2. Releases will appear in Sentry automatically
3. Associate commits with releases for better tracking

### 5. Filter Sensitive Data

Backend is configured with `SendDefaultPii: false` by default. To further filter sensitive data:

```csharp
options.SetBeforeSend((sentryEvent, hint) =>
{
    // Remove sensitive headers
    if (sentryEvent.Request?.Headers != null)
    {
        sentryEvent.Request.Headers.Remove("Authorization");
        sentryEvent.Request.Headers.Remove("Cookie");
    }
    return sentryEvent;
});
```

## Troubleshooting

### Front-End: Sentry Not Capturing Errors

1. **Check browser console** for Sentry initialization messages
2. **Verify DSN** is not empty in `appsettings.json`
3. **Check browser network tab** - look for requests to `sentry.io`
4. **Ensure HTTPS** - Sentry requires HTTPS in production

### Backend: Sentry Not Capturing Errors

1. **Check logs** for Sentry initialization
2. **Verify DSN** format is correct
3. **Test with explicit error** using test endpoint
4. **Check firewall** - ensure outbound HTTPS to `sentry.io` is allowed

### Quota Exceeded

If you hit Sentry quota limits:
1. **Reduce TracesSampleRate** (e.g., from 0.1 to 0.01)
2. **Filter noisy errors** using `BeforeSend`
3. **Upgrade Sentry plan** if needed

## Cost Considerations

Sentry pricing is based on:
- **Events** (errors captured)
- **Transactions** (performance monitoring samples)

### Free Tier
- 5,000 errors/month
- 10,000 performance units/month
- 1 project
- 1 user

### Recommendations
- Start with **TracesSampleRate: 0.1** in production
- Monitor usage in Sentry dashboard
- Adjust sampling rate based on traffic

## Advanced Features

### Custom Context

Add custom data to Sentry events:

```csharp
// Backend
SentrySdk.ConfigureScope(scope =>
{
    scope.SetTag("user-type", "premium");
    scope.SetExtra("order-id", orderId);
});

// Front-End (Blazor)
SentrySdk.ConfigureScope(scope =>
{
    scope.User = new Sentry.User
    {
        Id = userId,
        Username = username
    };
});
```

### Custom Transactions

Track custom performance metrics:

```csharp
var transaction = SentrySdk.StartTransaction("process-payment", "payment");
try
{
    // Your code here
    transaction.Finish(SpanStatus.Ok);
}
catch (Exception ex)
{
    transaction.Finish(ex);
    throw;
}
```

## Resources

- [Sentry Documentation](https://docs.sentry.io/)
- [Sentry ASP.NET Core Guide](https://docs.sentry.io/platforms/dotnet/guides/aspnetcore/)
- [Sentry Blazor Guide](https://docs.sentry.io/platforms/javascript/guides/blazor/)
- [Sentry Best Practices](https://docs.sentry.io/product/best-practices/)

## Files Modified

- `VanityNumber.Web/VanityNumber.Web.csproj` - Added Sentry package
- `VanityNumber.Web/Program.cs` - Added Sentry initialization
- `VanityNumber.Web/wwwroot/appsettings.json` - Added Sentry config
- `VanityNumber.Web/wwwroot/appsettings.Production.json` - Production Sentry config
- `VanityNumber.Api/VanityNumber.Api.csproj` - Added Sentry package
- `VanityNumber.Api/Program.cs` - Added Sentry initialization
- `VanityNumber.Api/appsettings.json` - Added Sentry config
- `VanityNumber.Api/appsettings.Production.json` - Production Sentry config
- `docker-compose.yml` - Added Sentry environment variables

## Support

For issues or questions:
1. Check Sentry dashboard for error details
2. Review Sentry SDK logs (`Debug: true`)
3. Consult [Sentry Community Forum](https://forum.sentry.io/)
