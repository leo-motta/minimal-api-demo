using MinimalApi.Data;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Models;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.Parse("8.0.28"))
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/provider", async (
    MinimalContextDb context) => 
    await context.Providers.ToListAsync())
    .WithName("GetProvider")
    .WithTags("Provider");

app.MapGet("/provider/{id}", async (
    Guid id, 
    MinimalContextDb context) => 

    await context.Providers.FindAsync(id)
        is Provider provider 
            ? Results.Ok(provider)
            : Results.NotFound())
    .Produces<Provider>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetProviderById")
    .WithTags("Provider");

app.MapPost("/provider", async (
    MinimalContextDb context,
    Provider provider) =>
    {
        if (!MiniValidator.TryValidate(provider, out var errors))
            return Results.ValidationProblem(errors);

        context.Providers.Add(provider);
        var result = await context.SaveChangesAsync();
     
        return result > 0
            //? Results.Created($"/provider/{provider.Id}", provider)
            ? Results.CreatedAtRoute("GetProviderById", new { id = provider.Id}, provider)
            : Results.BadRequest("There was a problem when saving the provider");
    })
    .ProducesValidationProblem()
    .Produces<Provider>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("PostProvider")
    .WithTags("Provider");

app.MapPut("/provider/{id}", async(
    Guid id,
    MinimalContextDb context,
    Provider provider) =>
    {
        var providerDB = await context.Providers.AsNoTracking<Provider>()
                                                    .FirstOrDefaultAsync(f => f.Id == id);
        if(providerDB == null) return Results.NotFound();

        if(!MiniValidator.TryValidate(provider, out var errors))
            return Results.ValidationProblem(errors);
        
        context.Providers.Update(provider);
        var result = await context.SaveChangesAsync();

        return result > 0
            ? Results.NoContent()
            : Results.BadRequest("There was a problem when saving the provider");
    })
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PutProvider")
    .WithTags("Provider");

app.MapDelete("/provider/{id}", async(
    Guid id,
    MinimalContextDb context) =>
    {
        var provider = await context.Providers.FindAsync(id);
        if(provider == null) return Results.NotFound();
        
        context.Providers.Remove(provider);
        var result = await context.SaveChangesAsync();

        return result > 0
            ? Results.NoContent()
            : Results.BadRequest("There was a problem when saving the provider");
    })
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("DeleteProvider")
    .WithTags("Provider");

app.Run();