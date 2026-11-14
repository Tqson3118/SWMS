Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead("D:\03_Study\Studxy\frontend\e2e\fixtures\personal-finance-course.zip")
$zip.Entries | Select-Object -First 30 | Select-Object FullName
$zip.Dispose()
