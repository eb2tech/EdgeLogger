using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Serve static files (your HTML form)
app.UseStaticFiles(new StaticFileOptions
                   {
                       FileProvider = new PhysicalFileProvider(
                           Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
                   });

// Handle POST from the form
app.MapPost("/configure", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var ssid = form["ssid"].ToString();
    var password = form["password"].ToString();

    if (string.IsNullOrWhiteSpace(ssid) || string.IsNullOrWhiteSpace(password))
        return Results.BadRequest("SSID and password required.");

    // Load template from embedded resource
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = "EdgeLogger.Provisioner.Templates.basewifi.nmconnection.template";
    await using var stream = assembly.GetManifestResourceStream(resourceName);
    if (stream == null)
        return Results.Problem($"Template resource not found: {resourceName}");
    using var reader = new StreamReader(stream);
    var template = await reader.ReadToEndAsync();

    // Replace placeholders
    var output = template
                 .Replace("{{SSID}}", ssid)
                 .Replace("{{PASSWORD}}", password);

    // Write to NetworkManager directory
    var targetPath = $"/etc/NetworkManager/system-connections/{ssid}.nmconnection";
    await File.WriteAllTextAsync(targetPath, output);

    // Ensure correct permissions
    Process.Start("sudo", $"chmod 600 {targetPath}")?.WaitForExit();
    Process.Start("sudo", $"chown root:root {targetPath}")?.WaitForExit();

    // Restart NetworkManager
    Process.Start("sudo", "systemctl restart NetworkManager")?.WaitForExit();

    return Results.Content("<h2>Saved. Reconnecting...</h2>", "text/html");
});

app.Run("http://0.0.0.0:80");