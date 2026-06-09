using System.Text.Json.Serialization;
using Amazon.BedrockRuntime;
using Amazon.S3;
using Dermalog.Api.Data;
using Dermalog.Api.Infrastructure.Bedrock;
using Dermalog.Api.Services;
using Dermalog.Api.Services.AI;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );
builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddOpenApi();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonBedrockRuntime>();

builder.Services.Configure<PhotosOptions>(builder.Configuration.GetSection("Photos"));
builder.Services.Configure<BedrockOptions>(builder.Configuration.GetSection("Bedrock"));

builder.Services.AddSingleton<IPhotoUploadService, PhotoUploadService>();
builder.Services.AddSingleton<IBedrockClient, BedrockClient>();

builder.Services.AddDbContext<DermalogDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IPhotoComparisonService, PhotoComparisonService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

app.Run();

public partial class Program;
