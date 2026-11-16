using Microsoft.Extensions.Configuration;
using Xunit;

namespace Stage8IntegrationApiTests;

public sealed class StartupGuardTests
{
    [Fact]
    public void Explicit_kestrel_endpoint_override_is_rejected_before_host_start()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Kestrel:Endpoints:Other:Url"] = "http://0.0.0.0:5000"
        }).Build();
        Assert.Throws<InvalidOperationException>(() => ApiContract.ValidateEndpointOverrides(config));
    }

    [Fact]
    public void No_endpoint_override_keeps_the_literal_loopback_fixture()
    {
        var config = new ConfigurationBuilder().Build();
        ApiContract.ValidateEndpointOverrides(config);
        Assert.Equal("http://127.0.0.1:0", ApiContract.ValidateListenUrl(ApiContract.DefaultListenUrl));
    }
}
