using Account.Data.Loan;
using Account.Data.Providers;
using Account.Data.Queries;
using Account.Data.Repositories;
using Account.Data.Stores;
using Account.Data.UnitOfWork;
using Account.Domain.Commands.Biometrics;
using Account.Domain.Framework.Layers.Data.Repositories;
using Account.Domain.Framework.Layers.Data.UnitOfWork;
using Account.Domain.Framework.Layers.Domain.Command;
using Account.Domain.Framework.Layers.Domain.Validations;
using Account.Domain.Providers;
using Account.Tasks.Jobs;
using Account.Domain.Queries.Biometrics;
using Account.Domain.Responses;
using Account.Domain.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddLogging();

builder.Services.AddSingleton<BiometricVerificationStore>();
builder.Services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBiometricVerificationQuery, BiometricVerificationQuery>();
builder.Services.AddScoped<IBiometricProvider, StatefulFakeBiometricProvider>();
builder.Services.AddSingleton<ILoanResources, InMemoryLoanResources>();
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped<IRequestHandler<ProcessBiometricVerificationCommand, BiometricVerificationResponse>, ProcessBiometricVerificationCommandHandler>();
builder.Services.AddHostedService<BiometricSyncBackgroundService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (BusinessException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = exception.Message }));
    }
});

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseRouting();

app.MapControllers();

app.Run();