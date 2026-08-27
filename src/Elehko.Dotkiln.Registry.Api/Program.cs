using Elehko.Dotkiln.Core.Loading;
using Elehko.Dotkiln.Core.Parsing;
using Elehko.Dotkiln.Core.Validation;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var registryRoot = builder.Configuration["DOTKILN_REGISTRY_ROOT"] ?? Path.Combine(app.Environment.ContentRootPath, "..", "..", "stacks");
var loader = new StackLoader(new StackYamlParser());
var validator = new StackValidator();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/stacks", () =>
{
    if (!Directory.Exists(registryRoot))
    {
        return Results.Ok(Array.Empty<string>());
    }

    var stacks = Directory.GetFiles(registryRoot, "*.dotkiln.yaml")
        .Select(Path.GetFileNameWithoutExtension)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    return Results.Ok(stacks);
});

app.MapGet("/stacks/search", (string term) =>
{
    if (!Directory.Exists(registryRoot))
    {
        return Results.Ok(Array.Empty<string>());
    }

    var matches = Directory.GetFiles(registryRoot, "*.dotkiln.yaml")
        .Where(path => Path.GetFileName(path).Contains(term, StringComparison.OrdinalIgnoreCase) || File.ReadAllText(path).Contains(term, StringComparison.OrdinalIgnoreCase))
        .Select(Path.GetFileName)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    return Results.Ok(matches);
});

app.MapPost("/stacks", async (HttpRequest request, CancellationToken cancellationToken) =>
{
    Directory.CreateDirectory(registryRoot);
    using var reader = new StreamReader(request.Body);
    var yaml = await reader.ReadToEndAsync(cancellationToken);
    var stack = new StackYamlParser().Parse(yaml);
    var result = validator.Validate(stack);
    if (!result.IsValid)
    {
        return Results.BadRequest(result.Errors);
    }

    var path = Path.Combine(registryRoot, $"{stack.Name}.dotkiln.yaml");
    await File.WriteAllTextAsync(path, yaml, cancellationToken);
    return Results.Created($"/stacks/{stack.Name}", new { stack.Name });
});

app.MapGet("/stacks/{name}", async (string name, CancellationToken cancellationToken) =>
{
    var path = Path.Combine(registryRoot, $"{name}.dotkiln.yaml");
    if (!File.Exists(path))
    {
        return Results.NotFound();
    }

    var stack = await loader.LoadAsync(path, cancellationToken);
    return Results.Ok(stack);
});

app.Run();
