using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace TaskManager.Tests;

public class AuthEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(); // ASPNETCORE_URLS is random free port in TestHost
    }

    [Fact]
    public async Task Register_Succeeds_With_StrongPassword()
    {
        var (email, user, pass) = TestsHelper.NewUserTriplet();

        var resp = await TestsHelper.RegisterAsync(_client, email, user, pass);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("short", "too short")]
    [InlineData("alllowercasebuttoolong", "no uppercase and no symbol")]
    [InlineData("NoSymbolsButLongEnough", "no non-alphanumeric")]
    public async Task Register_Fails_With_WeakPassword(string password, string _why)
    {
        var (email, user, _) = TestsHelper.NewUserTriplet();

        var resp = await TestsHelper.RegisterAsync(_client, email, user, password);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var text = await resp.Content.ReadAsStringAsync();
        text.Should().NotBeNullOrWhiteSpace(); // Identity returns errors list
    }

    [Fact]
    public async Task Register_Fails_On_Duplicate_Email()
    {
        var (email, user, pass) = TestsHelper.NewUserTriplet();

        (await TestsHelper.RegisterAsync(_client, email, user, pass)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Different username, same email
        var resp2 = await TestsHelper.RegisterAsync(_client, email, $"{user}_dup", pass);
        resp2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_Fails_On_Duplicate_UserName()
    {
        var (email, user, pass) = TestsHelper.NewUserTriplet();

        (await TestsHelper.RegisterAsync(_client, email, user, pass)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Different email, same username
        var resp2 = await TestsHelper.RegisterAsync(_client, $"x_{Guid.NewGuid():N}@example.com", user, pass);
        resp2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ByEmail_Succeeds_And_Returns_Jwt()
    {
        var (email, user, pass) = TestsHelper.NewUserTriplet();

        await TestsHelper.RegisterAsync(_client, email, user, pass);

        var (resp, token) = await TestsHelper.LoginAsync(_client, email, pass);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        token.Should().NotBeNull();
        token!.Split('.').Length.Should().Be(3);
    }

    [Fact]
    public async Task Login_ByUserName_Succeeds_And_Returns_Jwt()
    {
        var (email, user, pass) = TestsHelper.NewUserTriplet();

        await TestsHelper.RegisterAsync(_client, email, user, pass);

        var (resp, token) = await TestsHelper.LoginAsync(_client, user, pass);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        token.Should().NotBeNull();
        token!.Split('.').Length.Should().Be(3);
    }

    [Fact]
    public async Task Login_Fails_With_WrongPassword()
    {
        var (email, user, pass) = TestsHelper.NewUserTriplet();

        await TestsHelper.RegisterAsync(_client, email, user, pass);

        var (resp, token) = await TestsHelper.LoginAsync(_client, email, "WrongPassword123!");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        token.Should().BeNull();
    }

    [Fact]
    public async Task Login_Fails_When_User_NotFound()
    {
        var (resp, token) = await TestsHelper.LoginAsync(_client, "unknown@example.com", "NoMatter123!");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        token.Should().BeNull();
    }

    [Fact]
    public async Task Me_Unauthorized_Without_Token()
    {
        var resp = await TestsHelper.WhoAmIAsync(_client, null);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_Returns_Profile_With_Valid_Token()
    {
        var (email, user, pass) = TestsHelper.NewUserTriplet();

        await TestsHelper.RegisterAsync(_client, email, user, pass);
        var (_, token) = await TestsHelper.LoginAsync(_client, email, pass);
        token.Should().NotBeNull();

        var resp = await TestsHelper.WhoAmIAsync(_client, token);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        string? id = null, userName = null, emailOut = null;
        foreach (var p in root.EnumerateObject())
        {
            if (string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase)) id = p.Value.GetString();
            if (string.Equals(p.Name, "userName", StringComparison.OrdinalIgnoreCase)) userName = p.Value.GetString();
            if (string.Equals(p.Name, "email", StringComparison.OrdinalIgnoreCase)) emailOut = p.Value.GetString();
        }

        id.Should().NotBeNullOrWhiteSpace();
        userName.Should().Be(user);
        emailOut.Should().Be(email);
    }
}
