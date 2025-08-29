namespace TaskManager.Api.Common;

public static class Paging
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int page, int size) Normalize(int? page, int? size)
    {
        var p = page is > 0 ? page.Value : DefaultPage;

        var s = size is <= 0 or null
            ? DefaultPageSize
            : size is > MaxPageSize
                ? MaxPageSize
                : size!.Value;

        return (p, s);
    }
}