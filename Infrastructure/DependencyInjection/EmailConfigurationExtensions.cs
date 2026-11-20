using Microsoft.Extensions.Configuration;

namespace DocuArchi.Api.Infrastructure.DependencyInjection;

/// <summary>Explicit operator-owned private profile. Environment variables and CLI keep precedence.</summary>
public static class EmailConfigurationExtensions
{
    public static IConfigurationBuilder AddPrivateEmailConfiguration(
        this IConfigurationBuilder builder, string? filePath, string[] args)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(args);
        if (string.IsNullOrWhiteSpace(filePath)) return builder;
        if (!Path.IsPathFullyQualified(filePath) || !File.Exists(filePath))
            throw new InvalidOperationException("EMAIL_PRIVATE_CONFIGURATION_UNAVAILABLE");

        IConfigurationRoot profile;
        try
        {
            profile = new ConfigurationBuilder()
                .AddJsonFile(filePath, optional: false, reloadOnChange: false)
                .Build();
        }
        catch (Exception)
        {
            // Never copy parser fragments, file contents or physical paths into startup diagnostics.
            throw new InvalidOperationException("EMAIL_PRIVATE_CONFIGURATION_UNAVAILABLE");
        }
        builder.AddConfiguration(profile, shouldDisposeConfiguration: true);
        builder.AddEnvironmentVariables();
        if (args.Length > 0) builder.AddCommandLine(args);
        return builder;
    }
}
