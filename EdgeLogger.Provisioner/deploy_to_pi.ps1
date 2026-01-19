$publishDir = "bin\Release\net10.0\linux-arm64\publish"
$targetDir = "pi@picollect.local:/opt/provisioner/"

if (-Not (Test-Path $publishDir)) {
    Write-Host "Publish directory not found: $publishDir"
    exit 1
}

Write-Host "Copying files to Raspberry Pi..."
# ssh pi@picollect.local "sudo systemctl stop EdgeLogger.Provisioner"
ssh pi@picollect.local "rm -rf /opt/provisioner/*"
scp -r "$publishDir\*" $targetDir
ssh pi@picollect.local "sudo chmod +x /opt/provisioner/EdgeLogger.Provisioner"
# ssh pi@picollect.local "sudo systemctl start EdgeLogger.Provisioner"
Write-Host "Done."

