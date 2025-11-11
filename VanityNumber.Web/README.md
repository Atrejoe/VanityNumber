# Vanity Number Generator - Web Frontend

A modern, mobile-friendly Blazor WebAssembly application for generating vanity numbers from phone numbers.

## Features

✨ **Clean, Modern UI**
- Responsive design that works on desktop, tablet, and mobile devices
- Material Design-inspired interface with smooth animations
- Dark mode support (automatic based on system preference)

📱 **Mobile-First Design**
- Touch-friendly controls
- Optimized for small screens
- Fast loading and responsive interactions

🎨 **User-Friendly Features**
- Real-time form validation
- Loading indicators
- Clear error messages
- Dictionary selection (Dutch, English, Urban)
- Optional leet speak mode
- Configurable result limits

## Getting Started

### Prerequisites

- .NET 10 SDK
- VanityNumber backend running (default: https://localhost:7001)

### Running the Application

1. **Start the API Backend** (in separate terminal):
   ```bash
   cd VanityNumber
   dotnet run
   ```

2. **Start the Blazor App**:
   ```bash
   cd VanityNumber.Web
   dotnet run
   ```

3. **Open in Browser**:
   - Navigate to `https://localhost:7002` or `http://localhost:5002`

### Configuration

Update the API base URL in `wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7001/"
}
```

## Usage

1. Enter a phone number (e.g., "555-1234" or "0612345678")
2. Select which dictionaries to search (Dutch, English, Urban)
3. Optionally enable leet speak for additional mappings
4. Click "Generate Vanity Numbers"
5. View results with word matches and dictionary sources

## Example Results

For phone number `227646`:

- **Standard**: `catmap`, `batman`
- **With Leet Speak**: `ba7m4n` (BATMAN using 7=T, 4=A)

## Project Structure

```
VanityNumber.Web/
├── Models/              # API data models
├── Services/            # API communication service
├── Pages/               # Razor components
│   └── Home.razor      # Main generator page
├── wwwroot/
│   ├── css/
│   │   └── app.css     # Custom styles
│   ├── appsettings.json
│   └── index.html
└── Program.cs          # App configuration
```

## Styling

The application uses custom CSS with CSS variables for easy theming:

- Primary color: Indigo (#6366f1)
- Responsive breakpoints
- Mobile-optimized spacing
- Dark mode support

## Browser Support

- Chrome/Edge (latest)
- Firefox (latest)
- Safari (latest)
- Mobile browsers (iOS Safari, Chrome Mobile)

## API Integration

The app communicates with the VanityNumber backend through:

- `POST /api/VanityNumber/convert` - Generate vanity numbers
- `GET /api/VanityNumber/dictionaries` - Get available dictionaries
- `GET /api/VanityNumber/toDigits/{vanityNumber}` - Convert back to digits

## Development

### Building for Production

```bash
dotnet publish -c Release
```

The output will be in `bin/Release/net10.0/publish/wwwroot/`

### Customization

- **Colors**: Modify CSS variables in `wwwroot/css/app.css`
- **API URL**: Update `wwwroot/appsettings.json`
- **Dictionaries**: Configure in `Services/VanityNumberService.cs`

## License

Same license as the main VanityNumber project.
