# Imgur-NetCore
Migration from Imgur.API  (Version 4.0.1) from Net Framework 4.5 to NetCore 2.1


## Quick Start
### Get Image
```csharp
public async Task GetImage()
{
    try
    {
        var client = new ImgurClient("CLIENT_ID", "CLIENT_SECRET");
        var endpoint = new ImageEndpoint(client);
        var image = await endpoint.GetImageAsync("IMAGE_ID");
        Debug.Write("Image retrieved. Image Url: " + image.Link);
    }
    catch (ImgurException imgurEx)
    {
        Debug.Write("An error occurred getting an image from Imgur.");
        Debug.Write(imgurEx.Message);
    }
}
```
### Get Image (synchronously - not recommended)
```csharp
public void GetImage()
{
    try
    {
        var client = new ImgurClient("CLIENT_ID", "CLIENT_SECRET");
        var endpoint = new ImageEndpoint(client);
        var image = endpoint.GetImageAsync("IMAGE_ID").GetAwaiter().GetResult();
        Debug.Write("Image retrieved. Image Url: " + image.Link);
    }
    catch (ImgurException imgurEx)
    {
        Debug.Write("An error occurred getting an image from Imgur.");
        Debug.Write(imgurEx.Message);
    }
}
```
### Upload Image
```csharp
public async Task UploadImage()
{
    try
    {
        var client = new ImgurClient("CLIENT_ID", "CLIENT_SECRET");
        var endpoint = new ImageEndpoint(client);
        IImage image;
        using (var fs = new FileStream(@"IMAGE_LOCATION", FileMode.Open))
        {
            image = await endpoint.UploadImageStreamAsync(fs);
        }
        Debug.Write("Image uploaded. Image Url: " + image.Link);
    }
    catch (ImgurException imgurEx)
    {
        Debug.Write("An error occurred uploading an image to Imgur.");
        Debug.Write(imgurEx.Message);
    }
}
```
### Upload Image (synchronously - not recommended)
```csharp
public void UploadImage()
{
    try
    {
        var client = new ImgurClient("CLIENT_ID", "CLIENT_SECRET");
        var endpoint = new ImageEndpoint(client);
        IImage image;
        using (var fs = new FileStream(@"IMAGE_LOCATION", FileMode.Open))
        {
            image = endpoint.UploadImageStreamAsync(fs).GetAwaiter().GetResult();
        }
        Debug.Write("Image uploaded. Image Url: " + image.Link);
    }
    catch (ImgurException imgurEx)
    {
        Debug.Write("An error occurred uploading an image to Imgur.");
        Debug.Write(imgurEx.Message);
    }
}
```
