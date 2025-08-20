
for a view as to how it works - please check the slides in the repo (pdf file)

# Honey Badger Detection API

A .NET 8 Web API that uses Microsoft Custom Vision to detect honey badgers in uploaded images. When a honey badger is detected with sufficient confidence, the image is automatically uploaded to Dropbox for storage.

## Features

- **Image Analysis**: Uses Microsoft Custom Vision AI to analyze uploaded images
- **Honey Badger Detection**: Specifically trained to identify honey badgers with confidence scoring
- **Automatic Storage**: Uploads detected honey badger images to Dropbox
- **RESTful API**: Clean HTTP API endpoints for easy integration
- **Comprehensive Logging**: Detailed logging for monitoring and debugging
- **Health Checks**: Built-in health endpoint for system monitoring
- **CORS Support**: Configured for cross-origin requests

## Prerequisites

- .NET 8 SDK
- Microsoft Custom Vision account and trained model
- Dropbox account with API access token

## Configuration

Create an `appsettings.json` file with the following structure:

```json
{
  "CustomVision": {
    "Endpoint": "https://your-custom-vision-endpoint/customvision/v3.0/Prediction/your-project-id/classify/iterations/your-iteration-name/image",
    "PredictionKey": "your-custom-vision-prediction-key"
  },
  "Dropbox": {
    "AccessToken": "your-dropbox-access-token"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Getting Your Custom Vision Credentials

1. Go to [Custom Vision Portal](https://customvision.ai/)
2. Create or select your honey badger detection project
3. Go to **Performance** tab → **Prediction URL**
4. Copy the endpoint URL and prediction key

### Getting Your Dropbox Access Token

1. Go to [Dropbox App Console](https://www.dropbox.com/developers/apps)
2. Create a new app or select existing one
3. Generate an access token in the **OAuth 2** section

## Installation & Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Honey_badger_detection
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure settings**
   - Update `appsettings.json` with your API credentials
   - For production, use `appsettings.Production.json` or environment variables

4. **Run the application**
   ```bash
   dotnet run
   ```

The API will be available at:
- HTTPS: `https://localhost:7061`
- HTTP: `http://localhost:5134`
- Swagger UI: `https://localhost:7061/swagger`

## API Endpoints

### Analyze Image
**POST** `/api/camera/analyze`

Analyzes an uploaded image to detect honey badgers.

**Request:**
- Content-Type: `multipart/form-data`
- Body: Form file with key `image`

**Response:**
```json
{
  "isHoneyBadger": true,
  "confidence": 0.89,
  "fullResponse": {
    "id": "...",
    "project": "...",
    "iteration": "...",
    "created": "2025-01-15T10:30:00Z",
    "predictions": [
      {
        "tagId": "...",
        "tagName": "honey-badger",
        "probability": 0.89
      }
    ]
  },
  "processingTimeMs": 1250,
  "dropboxPath": "/honey-badger/honey_badger_20250115_103000_a1b2c3d4.jpg",
  "dropboxUploadError": null
}
```

**Response Fields:**
- `isHoneyBadger`: Boolean indicating if honey badger detected (confidence > 70%)
- `confidence`: Confidence score (0.0 to 1.0)
- `fullResponse`: Complete Custom Vision API response
- `processingTimeMs`: Time taken for analysis
- `dropboxPath`: Path to uploaded image (only if honey badger detected)
- `dropboxUploadError`: Error message if Dropbox upload failed (optional)

### Health Check
**GET** `/health`

Returns the health status of the API.

**Response:**
```
Healthy! API is running
```

## Project Structure

```
Honey_badger_detection/
├── Controllers/
│   └── CameraController.cs       # Main API controller
├── Services/
│   ├── ICustomVisionService.cs   # Custom Vision interface
│   ├── CustomVisionService.cs    # Custom Vision implementation
│   ├── IDropboxService.cs        # Dropbox interface
│   └── DropboxService.cs         # Dropbox implementation
├── Models/
│   ├── AnalysisResult.cs         # API response model
│   └── CustomVisionResponse.cs   # Custom Vision response models
├── Configuration/
│   ├── CustomVisionSettings.cs   # Custom Vision config
│   └── DropboxSettings.cs        # Dropbox config
└── Program.cs                    # Application startup
```

## Key Features Explained

### Detection Logic
- Images are analyzed using Microsoft Custom Vision
- Honey badger detection threshold is set at 70% confidence
- Only images meeting this threshold trigger Dropbox uploads

### File Management
- Uploaded images are temporarily stored in memory
- Detected honey badger images are saved to Dropbox in `/honey-badger/` folder
- File names include timestamp and unique identifier for organization

### Error Handling
- Comprehensive error logging throughout the pipeline
- Graceful handling of Dropbox upload failures (analysis still succeeds)
- Proper HTTP status codes and error messages

### Performance
- 30-second timeout on Custom Vision requests
- Memory-efficient stream handling for large images
- Processing time tracking for performance monitoring

## Environment Variables

You can override configuration using environment variables:

```bash
# Custom Vision
CustomVision__Endpoint=https://your-endpoint
CustomVision__PredictionKey=your-key

# Dropbox
Dropbox__AccessToken=your-token
```

## Development

### Running in Development Mode
```bash
dotnet run --environment Development
```

### Running Tests
```bash
dotnet test
```

### Building for Production
```bash
dotnet publish -c Release -o ./publish
```

## Docker Support

Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Honey_badger_detection.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Honey_badger_detection.dll"]
```

Build and run:
```bash
docker build -t honey-badger-api .
docker run -p 8080:80 honey-badger-api
```

## Monitoring & Logging

The application includes comprehensive logging:

- **Information**: Successful operations, processing times
- **Warning**: Non-critical issues
- **Error**: Exceptions and failures

Logs include:
- Image analysis requests and results
- Processing times
- Dropbox upload status
- Error details with stack traces

## Security Considerations

- Store API keys securely (environment variables, Azure Key Vault, etc.)
- Use HTTPS in production
- Implement rate limiting for public APIs
- Consider authentication/authorization for production use
- Validate file types and sizes before processing

## Troubleshooting

### Common Issues

1. **Custom Vision Errors**
   - Verify endpoint URL includes full path with project ID and iteration
   - Check prediction key is valid and has proper permissions
   - Ensure model is published and iteration name is correct

2. **Dropbox Upload Failures**
   - Verify access token has file write permissions
   - Check network connectivity to Dropbox API
   - Ensure sufficient storage space in Dropbox account

3. **Image Processing Issues**
   - Supported formats: JPEG, PNG, GIF, BMP
   - Maximum file size depends on Custom Vision limits (typically 4MB)
   - Ensure images are valid and not corrupted

### Debug Mode
Enable detailed logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Honey_badger_detection": "Debug"
    }
  }
}
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

[Specify your license here]

## Support

For issues and questions:
- Create an issue in the repository
- Check the logs for detailed error information
- Verify configuration settings and API credentials
