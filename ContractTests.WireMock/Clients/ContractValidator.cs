using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Schema;
using Json.Schema;

namespace ContractTests.WireMock.Clients;

public static class ContractValidator
{
    private static readonly ConcurrentDictionary<string, JsonSchema> Schemas = new();

    public static JsonObject ValidateJson(string schemaFile, string json)
    {
        JsonObject node;
        try
        {
            node = JsonNode.Parse(json) as JsonObject
                ?? throw new ContractViolationException([$"{schemaFile}: response body is not a JSON object"]);
        }
        catch (JsonException ex)
        {
            throw new ContractViolationException([$"{schemaFile}: response body is not valid JSON ({ex.Message})"]);
        }

        var schema = Schemas.GetOrAdd(schemaFile, static name =>
            JsonSchema.Build(JsonDocument.Parse(ReadResource(name)).RootElement));

        var results = schema.Evaluate(
            JsonSerializer.SerializeToElement(node),
            new EvaluationOptions { OutputFormat = OutputFormat.List });
        if (!results.IsValid)
            throw new ContractViolationException(CollectErrors(results));

        return node;
    }

    private static List<string> CollectErrors(EvaluationResults results)
    {
        var errors = new List<string>();
        if (results.Errors is not null)
            errors.AddRange(results.Errors.Select(e => $"{e.Key}: {e.Value}"));
        if (results.Details is not null)
        {
            foreach (var detail in results.Details)
                errors.AddRange(CollectErrors(detail));
        }
        return errors;
    }

    public static XmlDocument ValidateXml(string xsdFile, string xml)
    {
        var document = new XmlDocument();
        document.LoadXml(xml);

        var schemas = new XmlSchemaSet();
        using (var reader = XmlReader.Create(new StringReader(ReadResource(xsdFile))))
            schemas.Add(null, reader);

        document.Schemas = schemas;
        var errors = new List<string>();
        document.Validate((_, e) => errors.Add(e.Message));
        if (errors.Count > 0)
            throw new ContractViolationException(errors);

        return document;
    }

    internal static string ReadResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.Ordinal))
            ?? throw new FileNotFoundException($"Embedded contract '{fileName}' was not found.");
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
