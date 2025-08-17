# Cleanup script to remove publish directories that are causing duplicate compilation errors
Write-Host "Cleaning up publish directories that are causing duplicate compilation errors..." -ForegroundColor Yellow

# List of directories to remove
$directoriesToRemove = @(
    "publish-final",
    "publish-final-v2", 
    "publish-final-v3",
    "publish-final-v4",
    "publish-final-v6",
    "publish-folder",
    "publish-folder-new"
)

foreach ($dir in $directoriesToRemove) {
    if (Test-Path $dir) {
        Write-Host "Removing directory: $dir" -ForegroundColor Red
        Remove-Item -Recurse -Force $dir -ErrorAction SilentlyContinue
        if (Test-Path $dir) {
            Write-Host "Failed to remove $dir" -ForegroundColor Red
        } else {
            Write-Host "Successfully removed $dir" -ForegroundColor Green
        }
    } else {
        Write-Host "Directory $dir does not exist, skipping..." -ForegroundColor Gray
    }
}

Write-Host "Cleanup completed!" -ForegroundColor Green
Write-Host "You can now rebuild your project without duplicate compilation errors." -ForegroundColor Green
