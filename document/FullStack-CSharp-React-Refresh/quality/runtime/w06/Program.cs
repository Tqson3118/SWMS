var categories=new Dictionary<string,string>{{"f1","Food"},{"t1","Travel"}};
var seen=new HashSet<string>();
foreach(var id in new[]{"f1","f1","t1"}) {
 if(seen.Add(id)&&categories.TryGetValue(id,out var name)) Console.WriteLine(name);
}
var xs=new List<long>{50000,20000,30000};
Func<long,bool> large=x=>x>=30000;
Console.WriteLine(string.Join(",",xs.Where(large))); // 50000,30000
xs.RemoveAll(x=>x<30000);
Console.WriteLine(xs.Count); // 2