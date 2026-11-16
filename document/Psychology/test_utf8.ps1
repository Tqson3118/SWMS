$utf8 = [System.Text.UTF8Encoding]::new($false, $true)
$bytes = [System.IO.File]::ReadAllBytes('D:/03_Study/Roadmap/Psychology/manifest.yaml')
$utf8.GetString($bytes)
