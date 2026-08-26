using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using WasteBatteriesRegBackend.ExampleData.Models;

namespace WasteBatteriesRegBackend.Test.ExampleData.Models;

public class CreateExampleDataRequestTest
{
    [Fact]
    public void Test_validation_on_valid_request()
    {
        var req = new CreateExampleDataRequest { ExampleText = "Hello backend", UserId = "user-123" };
        var ctx = new ValidationContext(req);
        var results = new List<ValidationResult>();
        Assert.True(Validator.TryValidateObject(req, ctx, results, validateAllProperties: true));
    }

    [Fact]
    public void Test_validation_fails_on_invalid_request()
    {
        var req = new CreateExampleDataRequest { ExampleText = "", UserId = "" };
        var ctx = new ValidationContext(req);
        var results = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(req, ctx, results, validateAllProperties: true));
    }

    [Fact]
    public void Test_validation_fails_on_example_text_over_255_characters()
    {
        var req = new CreateExampleDataRequest { ExampleText = new string('a', 256), UserId = "user-123" };
        var ctx = new ValidationContext(req);
        var results = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(req, ctx, results, validateAllProperties: true));
    }

    [Fact]
    public void Test_json_serialization()
    {
        var req = new CreateExampleDataRequest { ExampleText = "Hello backend", UserId = "user-123" };
        var json = JsonSerializer.Serialize(req);
        Assert.Equal("{\"ExampleText\":\"Hello backend\",\"UserId\":\"user-123\"}", json);
    }

    [Fact]
    public void Test_json_deserialization()
    {
        var json = "{\"ExampleText\":\"Hello backend\",\"UserId\":\"user-123\"}";
        var req = JsonSerializer.Deserialize<CreateExampleDataRequest>(json);
        Assert.Equivalent(new CreateExampleDataRequest { ExampleText = "Hello backend", UserId = "user-123" }, req);
    }
}
