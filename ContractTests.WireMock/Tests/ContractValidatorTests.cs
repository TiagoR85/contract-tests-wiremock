using ContractTests.WireMock.Clients;

namespace ContractTests.WireMock.Tests;

public sealed class ContractValidatorTests
{
    [Fact]
    public void ValidateJson_malformed_body_throws_contract_violation()
    {
        var ex = Assert.Throws<ContractViolationException>(
            () => ContractValidator.ValidateJson("httpbin-get.schema.json", "not-json"));

        Assert.Contains("not valid JSON", ex.Message);
    }

    [Fact]
    public void ValidateJson_non_object_body_throws_contract_violation()
    {
        var ex = Assert.Throws<ContractViolationException>(
            () => ContractValidator.ValidateJson("httpbin-get.schema.json", "[1,2,3]"));

        Assert.Contains("not a JSON object", ex.Message);
    }
}
