# Sentry Quick Start

## 🚀 Enable Sentry in 3 Steps

### Step 1: Get Your DSN(s)
1. Go to [sentry.io](https://sentry.io)
2. Create two projects:
   - **Vanity Number Web** (JavaScript/Blazor)
   - **Vanity Number API** (ASP.NET Core)
3. Copy both DSN values

### Step 2: Configure Front-End
Edit `VanityNumber.Web/wwwroot/appsettings.Production.json`:

```json
{
  "Sentry": {
    "Dsn": "https://YOUR-WEB-DSN@sentry.io/PROJECT-ID"
  }
}
```

### Step 3: Configure Backend
Edit `VanityNumber.Api/appsettings.Production.json`:

```json
{
  "Sentry": {
    "Dsn": "https://YOUR-API-DSN@sentry.io/PROJECT-ID"
  }
}
```

### Step 4: Build & Deploy
```bash
dotnet build
dotnet publish -c Release
```

Or with Docker:
```bash
docker-compose up --build
```

## 🧪 Test It Works

### Front-End Test
Add to `Home.razor`:
```razor
<button @onclick="() => throw new Exception("Test Error")">Test Sentry</button>
```

### Backend Test
```bash
curl https://your-api/api/test-endpoint-that-errors
```

Check Sentry dashboard - errors should appear within seconds!

## ⚙️ Recommended Settings

### Development
```json
{
  "Sentry": {
    "Dsn": "your-dsn",
    "Environment": "Development",
    "TracesSampleRate": 1.0
  }
}
```

### Production
```json
{
  "Sentry": {
    "Dsn": "your-dsn",
    "Environment": "Production",
    "TracesSampleRate": 0.1
  }
}
```

## 🔒 Security Note

- ✅ **Front-End DSN is public** - This is normal for browser apps
- ✅ **Backend DSN is private** - Keep it in environment variables
- ✅ **PII disabled by default** - No user data sent without consent

## 📊 What You Get

**Error Tracking**:
- Real-time error notifications
- Stack traces with source code
- User impact analysis

**Performance Monitoring**:
- API response times
- Slow database queries
- Page load performance

**Breadcrumbs**:
- User actions before error
- HTTP requests
- Console logs

## 💡 Pro Tips

1. **Start small**: Use `TracesSampleRate: 0.1` in production
2. **Monitor quota**: Check Sentry dashboard weekly
3. **Set up alerts**: Get notified of critical errors
4. **Use environments**: Separate dev/staging/production

## 🚫 Disable Sentry

Leave DSN empty:
```json
{
  "Sentry": {
    "Dsn": ""
  }
}
```

Sentry is completely disabled when DSN is empty - zero overhead!

## 📖 Full Documentation

See [SENTRY_MONITORING_GUIDE.md](./SENTRY_MONITORING_GUIDE.md) for:
- Advanced configuration
- Custom context
- Filtering sensitive data
- Troubleshooting
- Cost optimization

## ❓ Common Questions

**Q: Do I need two DSNs?**  
A: Yes - separate front-end and backend for better organization and security.

**Q: Will this slow down my app?**  
A: No - Sentry runs asynchronously with minimal overhead.

**Q: What if I hit quota limits?**  
A: Reduce `TracesSampleRate` or filter noisy errors.

**Q: Is the front-end DSN really public?**  
A: Yes - it's visible in browser code. This is normal and safe for client-side SDKs.

## 🆘 Getting Help

- Check [Sentry Docs](https://docs.sentry.io/)
- Review error details in Sentry dashboard
- Enable `Debug: true` to see SDK logs
