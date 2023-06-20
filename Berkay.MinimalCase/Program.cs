using Serilog;


var builder = WebApplication.CreateBuilder(args);

// remove default logging providers
builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("log.txt")
    .CreateLogger();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddSerilog();
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/pricing", () =>
{
    var pricing1 = new Pricing("Product A", 10.99m, DiscountType.Fixed, 2m, 0.18m);
    var pricing2 = new Pricing("Product B", 19.99m, DiscountType.Percentage, 0.2m, 0.08m);
    var pricing3 = new Pricing("Product C", 5.99m, DiscountType.Fixed, 1m, 0.1m);

    var pricings = new[] { pricing1, pricing2, pricing3 };

    return pricings;
})
.WithName("GetPricing");

try
{
    Log.Information("Starting up!");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "The Application Failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public enum DiscountType
{
    Fixed,
    Percentage
}

public record Pricing(string ProductName, decimal Price, DiscountType DiscountType, decimal DiscountAmount, decimal TaxRate)
{
    public decimal CalculatedAmount
    {
        get
        {
            decimal discount = 0m;

            if (DiscountType == DiscountType.Fixed)
            {
                discount = DiscountAmount;
                Log.Information("Fixed discount applied: {DiscountAmount}", DiscountAmount);
            }
            else if (DiscountType == DiscountType.Percentage)
            {
                discount = Price * DiscountAmount;
                Log.Information("Percentage discount applied: {DiscountAmount}%", DiscountAmount * 100);
            }
            decimal totalAmount = Price - discount;
            decimal tax = totalAmount * TaxRate;
            decimal totalPrice = totalAmount + tax;

            Log.Information("Calculated amount for {ProductName}: {TotalPrice}", ProductName, totalPrice);


            return totalPrice;
        }
    }
}