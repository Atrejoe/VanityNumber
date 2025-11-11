# Vanity Number Generator - Complete Setup Guide

## 🎉 What's New

A modern, mobile-friendly **Blazor WebAssembly** front-end application has been added to the Vanity Number API project!

## 📁 Project Structure

```
naamnummers/
├── VanityNumberApi/              # Backend API
├── VanityNumberApi.Core/         # Core business logic
├── VanityNumberApi.Tests/        # API tests
├── VanityNumberApi.Web/          # ⭐ NEW: Blazor WebAssembly Frontend
├── VanityNumberApi.DictionarySanitizer/
└── VanityNumberApi.DictionarySanitizer.Tool/
```

## 🚀 Quick Start (Running Both API and Web App)

### Option 1: Two Terminals (Recommended for Development)

**Terminal 1 - Start the API:**
```bash
cd VanityNumberApi
dotnet run
```
The API will be available at: `https://localhost:7001`

**Terminal 2 - Start the Blazor App:**
```bash
cd VanityNumberApi.Web
dotnet run
```
The web app will be available at: `https://localhost:7002`

### Option 2: Visual Studio

1. Right-click on the solution
2. Set "Multiple Startup Projects"
3. Set both `VanityNumberApi` and `VanityNumberApi.Web` to "Start"
4. Press F5

### Option 3: Using dotnet watch (Hot Reload)

**Terminal 1:**
```bash
cd VanityNumberApi
dotnet watch run
```

**Terminal 2:**
```bash
cd VanityNumberApi.Web
dotnet watch run
```

## 🎨 Features of the Web App

### ✅ Mobile-First Design
- **Responsive Layout**: Works perfectly on phones, tablets, and desktops
- **Touch-Optimized**: Large, easy-to-tap buttons and controls
- **Adaptive UI**: Layout adjusts for screen size
- **Fast Loading**: Optimized for mobile networks

### 🎯 User Experience
- **Clean Interface**: Material Design-inspired UI
- **Real-Time Validation**: Instant feedback on form inputs
- **Loading States**: Visual indicators during API calls
- **Error Handling**: Clear, helpful error messages
- **Dark Mode**: Automatic support based on system preference

### 🔧 Functionality
- **Phone Number Input**: Accepts various formats (555-1234, 0612345678, etc.)
- **Dictionary Selection**: Choose Dutch, English, or Urban dictionaries
- **Leet Speak Mode**: Optional leet speak mappings (0=O, 1=I/L, 4=A, 5=S, 7=T, 8=B)
- **Configurable Results**: Set maximum number of results (1-100)
- **Detailed Results**: Shows vanity number, word, dictionaries, position, and length

## 📱 Screenshots & Examples

### Example Usage

1. **Enter Phone Number**: `227646`
2. **Select Dictionaries**: Dutch ✓, English ✓
3. **Enable Leet Speak**: Yes
4. **Click Generate**

**Results:**
- `ba7m4n` (BATMAN) - uses leet speak: 7=T, 4=A
- Position: 0, Length: 6 digits
- Found in: English dictionary

## 🔒 Security & CORS

The API has been configured with CORS to allow the Blazor app to communicate:

```csharp
// Allowed origins for Blazor app
- https://localhost:7002
- http://localhost:5002
- https://localhost:5001
- http://localhost:5000
```

## 🎨 Customization

### Changing Colors

Edit `VanityNumberApi.Web/wwwroot/css/app.css`:

```css
:root {
    --primary-color: #6366f1;    /* Indigo */
    --primary-dark: #4f46e5;
    --primary-light: #818cf8;
    /* ... change to your preferred colors */
}
```

### Changing API URL

Edit `VanityNumberApi.Web/wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://your-api-url.com/"
}
```

## 🏗️ Architecture

### Technology Stack
- **Framework**: Blazor WebAssembly (.NET 10)
- **UI**: Custom CSS (no heavy frameworks)
- **API Communication**: HttpClient with typed models
- **Patterns**: Component-based architecture

### Key Components

1. **Models** (`Models/VanityNumberModels.cs`)
   - `VanityNumberRequest` - API request model
   - `VanityNumberResult` - API response model
   - `DictionaryType` - Enum for dictionary selection

2. **Services** (`Services/VanityNumberApiService.cs`)
   - API communication layer
   - Error handling
   - JSON serialization

3. **Pages** (`Pages/Home.razor`)
   - Main vanity number generator
   - Form handling
   - Result display

4. **Styles** (`wwwroot/css/app.css`)
   - Responsive CSS
   - Mobile-first approach
   - Dark mode support

## 📊 Performance

- **Bundle Size**: Optimized Blazor WebAssembly bundle
- **Load Time**: Fast initial load with caching
- **API Calls**: Efficient HTTP client with error handling
- **Rendering**: Client-side rendering for instant UI updates

## 🧪 Testing the Setup

### 1. Test API Endpoint
```bash
curl https://localhost:7001/api/VanityNumber/dictionaries
```

### 2. Test Web App
1. Navigate to `https://localhost:7002`
2. Enter phone number: `2255`
3. Click "Generate Vanity Numbers"
4. Should see results like "CALL"

### 3. Test CORS
Open browser console and check for CORS errors. Should see none.

## 🐛 Troubleshooting

### Issue: "Failed to connect to the API"
**Solution**: Make sure the API is running on `https://localhost:7001`

### Issue: CORS errors in browser console
**Solution**: Check that the API's CORS policy includes your web app's origin

### Issue: No results found
**Solution**: 
- Try a different phone number
- Enable more dictionaries
- Try enabling leet speak mode

### Issue: Styles not loading
**Solution**: 
- Clear browser cache
- Check that `wwwroot/css/app.css` exists
- Rebuild the project

## 📝 Development Notes

### Adding New Features

1. **New API Endpoint**: 
   - Add to `VanityNumberApiService.cs`
   - Add models to `Models/VanityNumberModels.cs`

2. **New Page**:
   - Create new `.razor` file in `Pages/`
   - Add `@page` directive
   - Update navigation if needed

3. **Styling**:
   - Use existing CSS classes in `app.css`
   - Follow mobile-first approach
   - Test on multiple screen sizes

## 🚢 Deployment

### Publishing for Production

```bash
cd VanityNumberApi.Web
dotnet publish -c Release -o ./publish
```

The output in `publish/wwwroot/` can be deployed to:
- Azure Static Web Apps
- GitHub Pages
- Netlify
- Any static hosting service

**Remember to update the API URL in production!**

## 📚 Additional Resources

- [Blazor WebAssembly Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [.NET 10 Release Notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [CSS Variables Guide](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)

## 🎯 Next Steps

1. ✅ API is running
2. ✅ Web app is running
3. ✅ Try generating some vanity numbers!
4. 🎨 Customize the colors/styling
5. 🚀 Deploy to production
6. 📱 Share with users

## 💡 Tips

- Use Chrome DevTools to test mobile responsiveness (F12 → Device Toolbar)
- Check the browser console for any errors
- The app works offline after initial load (PWA capabilities can be added)
- Test with different phone numbers and dictionary combinations

## 🤝 Contributing

Feel free to:
- Report issues
- Suggest improvements
- Submit pull requests
- Share feedback

---

**Enjoy using the Vanity Number Generator! 📞✨**
