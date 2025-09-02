using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManager.Api.Contracts;

namespace TaskManager.Tests;

public class TaskEndpointsTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;
    private string _jwt = default!;

    public TaskEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        var (email, username, password) = TestsHelper.NewUserTriplet();

        var regResp = await TestsHelper.RegisterAsync(_client, email, username, password);
        regResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResp = await TestsHelper.LoginAsync(_client, email, password);
        loginResp.resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await loginResp.resp.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        _jwt = auth!.Token;

        _client.UseBearer(loginResp.token!);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateTask_ShouldReturnCreatedTask()
    {
        var req = new TaskCreateRequest("Test Task", "desc", DateTime.UtcNow.AddDays(1));
        var resp = await _client.PostAsJsonAsync("/api/tasks", req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var task = await resp.Content.ReadFromJsonAsync<TaskResponse>();
        task.Should().NotBeNull();
        task!.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTasks_ShouldReturnListWithCreatedTask()
    {
        await _client.PostAsJsonAsync("/api/tasks", new TaskCreateRequest("Task A", null, null));

        var resp = await _client.GetAsync("/api/tasks");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await resp.Content.ReadFromJsonAsync<PagedResult<TaskResponse>>();
        list.Should().NotBeNull();
        list!.Items.Should().ContainSingle(t => t.Title == "Task A");
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnTask_WhenExists()
    {
        var createResp = await _client.PostAsJsonAsync("/api/tasks", new TaskCreateRequest("FindMe", null, null));
        var created = await createResp.Content.ReadFromJsonAsync<TaskResponse>();

        var resp = await _client.GetAsync($"/api/tasks/{created!.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var task = await resp.Content.ReadFromJsonAsync<TaskResponse>();
        task!.Title.Should().Be("FindMe");
    }

    [Fact]
    public async Task UpdateTask_ShouldChangeFields()
    {
        var createResp = await _client.PostAsJsonAsync("/api/tasks", new TaskCreateRequest("OldTitle", "old desc", null));
        var created = await createResp.Content.ReadFromJsonAsync<TaskResponse>();

        var updateReq = new TaskUpdateRequest("NewTitle", "new desc", true, null);
        var updateResp = await _client.PutAsJsonAsync($"/api/tasks/{created!.Id}", updateReq);

        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResp.Content.ReadFromJsonAsync<TaskResponse>();
        updated!.Title.Should().Be("NewTitle");
        updated.Description.Should().Be("new desc");
        updated.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTask_ShouldSoftDelete()
    {
        var createResp = await _client.PostAsJsonAsync("/api/tasks", new TaskCreateRequest("ToDelete", null, null));
        var created = await createResp.Content.ReadFromJsonAsync<TaskResponse>();

        var delResp = await _client.DeleteAsync($"/api/tasks/{created!.Id}");
        delResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResp = await _client.GetAsync("/api/tasks");
        var list = await listResp.Content.ReadFromJsonAsync<PagedResult<TaskResponse>>();
        list!.Items.Should().NotContain(t => t.Id == created!.Id);
    }

    [Fact]
    public async Task RestoreTask_ShouldBringBackSoftDeletedTask()
    {
        var createResp = await _client.PostAsJsonAsync("/api/tasks", new TaskCreateRequest("ToRestore", null, null));
        var created = await createResp.Content.ReadFromJsonAsync<TaskResponse>();

        await _client.DeleteAsync($"/api/tasks/{created!.Id}");

        var restoreResp = await _client.PostAsync($"/api/tasks/{created!.Id}/restore", null);
        restoreResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResp = await _client.GetAsync("/api/tasks");
        var list = await listResp.Content.ReadFromJsonAsync<PagedResult<TaskResponse>>();
        list!.Items.Should().Contain(t => t.Id == created!.Id);
    }

    [Fact]
    public async Task CompleteTask_ShouldMarkAsCompleted()
    {
        var createResp = await _client.PostAsJsonAsync("/api/tasks", new TaskCreateRequest("CompleteMe", null, null));
        var created = await createResp.Content.ReadFromJsonAsync<TaskResponse>();

        var completeResp = await _client.PostAsync($"/api/tasks/{created!.Id}/complete", null);
        completeResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResp = await _client.GetAsync($"/api/tasks/{created!.Id}");
        var got = await getResp.Content.ReadFromJsonAsync<TaskResponse>();
        got!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetStats_ShouldReturnValidNumbers()
    {
        await _client.PostAsJsonAsync("/api/tasks", new TaskCreateRequest("t1", null, null));
        await _client.PostAsJsonAsync("/api/tasks", new TaskCreateRequest("t2", null, DateTime.UtcNow.AddDays(-1)));

        var resp = await _client.GetAsync("/api/tasks/_stats");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var stats = await resp.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        stats.Should().ContainKeys("total", "open", "done", "overdue");
        stats!["total"].Should().BeGreaterThanOrEqualTo(2);
    }
}
