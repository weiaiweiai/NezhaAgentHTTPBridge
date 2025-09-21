using WebApplication4.Services;

var builder = WebApplication.CreateBuilder(args);

// 控制器
builder.Services.AddControllers();

// 全量放开 CORS（调试用，生产建议收紧）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// WebSocket 相关
builder.Services.AddSingleton<WebSocketMessageStore>();
builder.Services.AddHostedService<WebSocketClientBackgroundService>();

builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(); // 如需再打开

var app = builder.Build();

// 重要：在 MapControllers 之前
app.UseCors("AllowAll");

// 其他中间件（如需要，可放在此处）
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
