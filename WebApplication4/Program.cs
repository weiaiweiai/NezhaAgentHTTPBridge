using WebApplication4.Services;

var builder = WebApplication.CreateBuilder(args);

// ������
builder.Services.AddControllers();

// ȫ���ſ� CORS�������ã����������ս���
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// WebSocket ���
builder.Services.AddSingleton<WebSocketMessageStore>();
builder.Services.AddHostedService<WebSocketClientBackgroundService>();

builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(); // �����ٴ�

var app = builder.Build();

// ��Ҫ���� MapControllers ֮ǰ
app.UseCors("AllowAll");

// �����м��������Ҫ���ɷ��ڴ˴���
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
