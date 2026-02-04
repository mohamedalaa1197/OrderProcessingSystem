using OrderProcessing.Infrastructure;
using OrderProcessing.InventoryWorker;
using OrderProcessing.NotificationWorker;
using OrderProcessing.PaymentWorker;
using OrderProcessing.ShippingWorker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var rabbitConfig = new RabbitMQConfiguration();
builder.Services.AddSingleton(rabbitConfig);
builder.Services.AddSingleton<RabbitMQConnectionFactory>();
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddHostedService<PaymentProcessingWorker>();
builder.Services.AddHostedService<InventoryReservationWorker>();
builder.Services.AddHostedService<NotificationWorker>();
builder.Services.AddHostedService<ShippingPreparingWorker>();
builder.Services.AddHostedService<NotificationWorker>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();