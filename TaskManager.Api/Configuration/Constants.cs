using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace TaskManager.Api.Configuration;


public static class Constants
{
    public const string CorsAllowFrontend = "AllowFrontend";
    public const string CorsAllowAll = "AllowAll";

    public const string TestingEnvironment = "Testing";
}