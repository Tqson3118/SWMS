using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
var b=WebApplication.CreateBuilder(args);
b.Services.AddRateLimiter(o=>{
 o.RejectionStatusCode=StatusCodes.Status429TooManyRequests;
 o.AddFixedWindowLimiter("lab",x=>{
  x.PermitLimit=3;x.Window=TimeSpan.FromMinutes(1);x.QueueLimit=0;
 });
});
var app=b.Build();app.UseRouting();app.UseRateLimiter();
app.MapGet("/api/ping",()=>Results.Ok()).RequireRateLimiting("lab");
app.Run();