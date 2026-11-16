using var cts=new CancellationTokenSource();
await File.WriteAllTextAsync("fixture.txt","50000\n20000\n30000");
cts.Cancel();
try { Console.WriteLine(await ReadAsync("fixture.txt",cts.Token)); }
catch(OperationCanceledException) { Console.WriteLine("Đã hủy"); }
static async Task<string> ReadAsync(string path,CancellationToken token) {
 using var reader=new StreamReader(path);
 token.ThrowIfCancellationRequested();
 return await reader.ReadToEndAsync(token);
}