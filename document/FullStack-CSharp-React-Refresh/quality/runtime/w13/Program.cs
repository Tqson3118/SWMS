var builder=WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
var app=builder.Build();
app.UseExceptionHandler();
app.Use(async (ctx,next)=>{
 app.Logger.LogInformation("Before {Path}",ctx.Request.Path);
 await next(ctx);
 app.Logger.LogInformation("After {Status}",ctx.Response.StatusCode);
});
app.MapGet("/api/ping",()=>Results.Ok(new{message="pong"}));
app.MapGet("/api/fault", (Func<IResult>)(() =>
 throw new InvalidOperationException("Lab fault")));
app.Run();