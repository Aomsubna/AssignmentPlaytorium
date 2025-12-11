using DiscountCampaignsBackend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    // Add services to the container.

    // builder.Services.AddControllers();
    builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    });
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSingleton<IPromotionRepository, PromotionRepository>();
    builder.Services.AddScoped<IDiscountCategory, CouponDiscount>();
    builder.Services.AddScoped<IDiscountCategory, OnTopDiscount>();
    builder.Services.AddScoped<IDiscountCategory, SeasonalDiscount>();
    builder.Services.AddScoped<DiscountCalculator>();

    // Register evaluators
    builder.Services.AddSingleton<IRuleEvaluator, PercentRuleEvaluator>();
    builder.Services.AddSingleton<IRuleEvaluator, FixedRuleEvaluator>();
    builder.Services.AddSingleton<IRuleEvaluator, TierRuleEvaluator>();
    builder.Services.AddSingleton<IRuleEvaluator, StepDiscountEvaluator>();
    builder.Services.AddSingleton<IRuleEvaluator, PointRuleEvaluator>();
    builder.Services.AddSingleton<IRuleEvaluator, BuyXGetYRuleEvaluator>();

    // Register condition evaluators
    builder.Services.AddSingleton<IConditionEvaluator, DateRangeConditionEvaluator>();
    builder.Services.AddSingleton<IConditionEvaluator, DayOfWeekConditionEvaluator>();
    builder.Services.AddSingleton<IConditionEvaluator, AnnualDateConditionEvaluator>();
    builder.Services.AddSingleton<IConditionEvaluator, CategoryInCartConditionEvaluator>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error during startup: {ex.Message}");
    Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.Error.WriteLine($"Inner exception: {ex.InnerException.Message}");
        Console.Error.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
    }
    throw;
}
