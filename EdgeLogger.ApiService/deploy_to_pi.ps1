$PiHost = "picollect.local"
$User = "pi"
$RemoteDir = "/opt/edgelogger"
$PublishDir = "bin\Debug\net10.0\linux-arm64\publish"


Write-Host "Publishing project..."
dotnet publish -c Debug -r linux-arm64 --self-contained true

Write-Host "Copying files to Pi..."
scp -r $PublishDir\* "$User@${PiHost}:$RemoteDir"

Write-Host "Setting permissions..."
ssh "$User@$PiHost" "sudo chown -R pi:pi $RemoteDir"

Write-Host "Deployment complete."

