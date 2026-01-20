using System.Diagnostics;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("EdgeLogger.Provisioner starting up...");

app.MapGet("/", () => Results.Content("""
                      <html>
                      <body style="font-family: sans-serif; max-width: 400px; margin: 40px auto;">
                          <h2>picollect WiFi Setup</h2>
                          <form method="post" action="/configure">
                              <label>WiFi SSID</label><br/>
                              <input name="ssid" style="width: 100%; padding: 8px;" /><br/><br/>

                              <label>Password</label><br/>
                              <input name="password" type="password" style="width: 100%; padding: 8px;" /><br/><br/>

                              <button type="submit" style="padding: 10px 20px;">Connect</button>
                          </form>
                      </body>
                      </html>
                      """, "text/html"));

// Handle POST from the form
app.MapPost("/configure", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var ssid = form["ssid"].ToString();
    var password = form["password"].ToString();

    // Load template from embedded resource
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = "EdgeLogger.Provisioner.Templates.remotewifi.nmconnection.template";
    await using var stream = assembly.GetManifestResourceStream(resourceName);
    if (stream == null)
        return Results.Content($"Template resource not found: {resourceName}");

    using var reader = new StreamReader(stream);
    var template = await reader.ReadToEndAsync();

    // Replace placeholders
    var output = template
                 .Replace("{{SSID}}", ssid)
                 .Replace("{{PASSWORD}}", password)
                 .Replace("{{UUID}}", Guid.NewGuid().ToString());

    // Write to NetworkManager directory
    var targetPath = $"/etc/NetworkManager/system-connections/remotewifi.nmconnection";
    await File.WriteAllTextAsync(targetPath, output);
    await Cli("chmod 600 " + targetPath);
    await Cli("nmcli connection reload");
    await Cli("nmcli connection up remotewifi");

    return Results.Content("""
           <html><body>
           <h2>Connecting...</h2>
           <p>Your device is attempting to join the WiFi network.</p>
           <p>If successful, this access point will disappear.</p>
           </body></html>
           """, "text/html");
});

logger.LogInformation("EdgeLogger.Provisioner is listening...");

app.Run("http://0.0.0.0:80");
return;

static async Task Cli(string cmd)
{
    var psi = new ProcessStartInfo("bash", "-c \"" + cmd + "\"")
              {
                  RedirectStandardOutput = true,
                  RedirectStandardError = true
              };
    var p = Process.Start(psi);
    await p!.WaitForExitAsync();
}
