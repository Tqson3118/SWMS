var xs=new List<long>{50000,20000};
var q=xs.Where(x=>x>=30000);
var snapshot=q.ToList();
xs.Add(30000);
Console.WriteLine(q.Sum()); // 80000
Console.WriteLine(snapshot.Sum()); // 50000
string? note=null;
Console.WriteLine(note?.Trim()??"(không ghi chú)");