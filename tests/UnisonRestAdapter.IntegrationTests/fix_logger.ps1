$files = Get-ChildItem -Recurse -Filter "*.cs"

foreach ($file in $files) {
    Write-Host "Processing: $($file.Name)"
    $content = Get-Content $file.FullName -Raw
    
    # Replace multi-line Logger statements with simple console output
    $content = $content -replace 'Logger\.Log[a-zA-Z]+\([^)]*(\([^)]*\))*[^)]*\);', 'Console.WriteLine("Logging statement");'
    
    # Replace any remaining single line Logger statements
    $content = $content -replace 'Logger\.[a-zA-Z]+\([^;]*?\);', 'Console.WriteLine("Logging statement");'
    
    # Replace specific patterns that might remain
    $content = $content -replace 'Logger\.[a-zA-Z]+\([^}]*?;', 'Console.WriteLine("Logging statement");'
    
    Set-Content -Path $file.FullName -Value $content
}
