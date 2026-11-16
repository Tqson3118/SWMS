var a=new Box { Amount=1 }; var b=a; b.Amount=2;
var x=new Money(1); var y=x; y=new Money(2);
Console.WriteLine($"{a.Amount} {x.Amount} {y.Amount}"); // 2 1 2
public sealed class Box { public long Amount {get;set;} }
public readonly record struct Money(long Amount);