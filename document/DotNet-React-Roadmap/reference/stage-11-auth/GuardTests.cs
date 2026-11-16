using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

// Guard checks deliberately have no host/database fixture or asynchronous setup.
public sealed class GuardTests
{
    [Fact]
    public void Environment_cannot_hide_a_conflicting_configuration_URL()
    {
        var previous = Environment.GetEnvironmentVariable("URLS");
        try
        {
            Environment.SetEnvironmentVariable("URLS", "http://127.0.0.1:0");
            var config = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?> { ["urls"] = "http://0.0.0.0:5000" }).Build();
            Assert.Throws<InvalidOperationException>(() => AuthStartupGuard.Validate(config));
        }
        finally { Environment.SetEnvironmentVariable("URLS", previous); }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" http://127.0.0.1:0")]
    [InlineData("http://127.0.0.1:0 ")]
    [InlineData("http://127.0.0.1:5000")]
    [InlineData("http://0.0.0.0:0")]
    [InlineData("http://example.test:0")]
    [InlineData("http://127.0.0.1:0;http://127.0.0.1:1")]
    [InlineData("http://user:sentinel@127.0.0.1:0")]
    [InlineData("http://127.0.0.1:0/path")]
    [InlineData("http://127.0.0.1:0?x=y")]
    public void Configured_URL_must_be_exact(string value)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["urls"] = value }).Build();
        Assert.Throws<InvalidOperationException>(() => AuthStartupGuard.Validate(config));
    }

    [Fact]
    public void Empty_configuration_and_exact_URL_are_allowed()
    {
        AuthStartupGuard.Validate(new ConfigurationBuilder().Build());
        AuthStartupGuard.Validate(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["urls"] = "http://127.0.0.1:0" }).Build());
    }

    [Theory]
    [InlineData("Http")]
    [InlineData("Https")]
    [InlineData("Surprise")]
    public void All_named_endpoints_are_rejected(string endpoint)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { [$"Kestrel:Endpoints:{endpoint}:Url"] = "http://127.0.0.1:0" }).Build();
        Assert.Throws<InvalidOperationException>(() => AuthStartupGuard.Validate(config));
    }

    [Fact]
    public void Arbitrary_double_underscore_endpoint_environment_is_rejected()
    {
        const string key = "Kestrel__Endpoints__Unlisted__Url";
        var previous = Environment.GetEnvironmentVariable(key);
        try
        {
            Environment.SetEnvironmentVariable(key, "http://127.0.0.1:0");
            Assert.Throws<InvalidOperationException>(() => AuthStartupGuard.Validate(new ConfigurationBuilder().Build()));
        }
        finally { Environment.SetEnvironmentVariable(key, previous); }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("RoadmapAuth11_0123456789abcdef0123456789abcdef\n")]
    [InlineData("RoadmapAuth11_0123456789ABCDEF0123456789abcdef")]
    [InlineData("RoadmapAuth11_fixed")]
    public void Database_names_cannot_escape_the_fresh_name_shape(string? value)
        => Assert.False(AuthStartupGuard.IsValidDatabaseName(value));
}
