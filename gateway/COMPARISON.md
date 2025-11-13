# Gateway Options Comparison

## Three Approaches

### Option 1: Nginx (Current Implementation)
**Pros:**
- ✅ Smallest image: 54MB
- ✅ Lowest memory: ~15MB
- ✅ No build step needed
- ✅ Battle-tested, industry standard
- ✅ Pure configuration

**Cons:**
- ❌ Nginx config syntax (learning curve for .NET devs)
- ❌ Separate technology stack from API/Web
- ❌ Less integration with .NET ecosystem
- ❌ No built-in .NET diagnostics/logging integration

**Use Case:** Simple path-based routing, minimal resource usage

---

### Option 2: Pre-built YARP Image (mcr.microsoft.com/dotnet/nightly/yarp)
**Status:** ❌ **Does not exist**

Microsoft doesn't publish a ready-to-use YARP container image with configuration support. YARP is a library, not a standalone application.

---

### Option 3: Minimal YARP Application (Custom Build)
**Pros:**
- ✅ Same technology stack as API (.NET)
- ✅ Unified logging/monitoring with .NET ecosystem
- ✅ Configuration in appsettings.json (familiar to .NET devs)
- ✅ Can extend with .NET middleware if needed
- ✅ OpenTelemetry integration built-in
- ✅ Better for teams already using .NET

**Cons:**
- ❌ Larger image: ~200MB (but acceptable for .NET apps)
- ❌ Higher memory: ~50-60MB
- ❌ Requires build step
- ❌ Need to maintain .NET version updates

**Use Case:** .NET-first teams, future extensibility needs

---

## Recommendation by Scenario

### Choose **Nginx** if:
- You want absolute minimal resource usage
- Simple routing is all you need
- Team comfortable with nginx config
- Running many gateway replicas (cost savings)
- **This is our current situation** ✅

### Choose **Custom YARP** if:
- You're a .NET-first team
- You might need custom routing logic later
- You want unified .NET telemetry/logging
- Image size isn't a concern
- You value consistency over minimal footprint

### There is **NO** pre-built YARP image option
Microsoft doesn't provide `mcr.microsoft.com/dotnet/nightly/yarp` or similar. YARP must be built into a custom application.

---

## Size Comparison

| Component | Image Size | Memory Usage |
|-----------|-----------|--------------|
| **Nginx Gateway** | 54 MB | ~15 MB |
| **YARP Gateway** | ~220 MB | ~50-60 MB |
| **Difference** | **4x larger** | **3-4x more** |

For 2 replicas in Kubernetes:
- Nginx: ~108 MB storage, ~30 MB RAM
- YARP: ~440 MB storage, ~120 MB RAM

---

## Current Implementation

We're using **nginx** because:
1. Our routing needs are simple (path-based only)
2. Resource efficiency matters
3. No need for .NET-specific features
4. Standard solution for this use case

If requirements change (custom auth, dynamic routing, .NET integration), we can switch to YARP by replacing the gateway deployment.

---

## Code Comparison

### Nginx Configuration (Current)
```nginx
upstream api_backend {
    server vanity-api:8080;
}

location /api/ {
    proxy_pass http://api_backend;
}
```
**Lines:** ~90 in nginx.conf  
**Build:** None needed  
**Technology:** nginx config syntax

### YARP Configuration
```json
{
  "ReverseProxy": {
    "Routes": {
      "api-route": {
        "ClusterId": "api-cluster",
        "Match": { "Path": "/api/{**catch-all}" }
      }
    },
    "Clusters": {
      "api-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://vanity-api:8080/"
          }
        }
      }
    }
  }
}
```
**Plus Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();
app.MapReverseProxy();
await app.RunAsync();
```
**Lines:** ~50 in appsettings.json + ~10 in Program.cs  
**Build:** `dotnet publish` required  
**Technology:** JSON + C#

---

## Decision

**Stick with nginx** for now because:
1. No pre-built YARP image exists anyway
2. Our needs are simple
3. Resource efficiency is valuable
4. Easy to switch to YARP later if needed

The gateway is isolated - changing it later won't affect API or Web components.
