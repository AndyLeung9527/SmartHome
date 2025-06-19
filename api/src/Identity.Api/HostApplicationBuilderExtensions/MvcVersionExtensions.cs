namespace Identity.Api.HostApplicationBuilderExtensions;

public static class MvcVersionExtensions
{
    public static IHostApplicationBuilder AddMvcVersioning(this IHostApplicationBuilder builder)
    {
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = false;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        }).AddMvc();

        string[] versions = ["v1"];
        foreach (var version in versions)
        {
            builder.Services.AddOpenApi(version, options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    var versionDescriptionProvider = context.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                    var apiDescription = versionDescriptionProvider?.ApiVersionDescriptions.SingleOrDefault(description => description.GroupName == context.DocumentName);
                    if (apiDescription is null)
                    {
                        return Task.CompletedTask;
                    }
                    document.Info.Version = apiDescription.ApiVersion.ToString();
                    document.Info.Title = $"Identity API {apiDescription.ApiVersion}";
                    document.Info.Description = BuildDescription(apiDescription, $"Identity API for version {apiDescription.ApiVersion}");
                    return Task.CompletedTask;
                });
            });
        }

        return builder;
    }

    private static string BuildDescription(ApiVersionDescription api, string description)
    {
        var text = new StringBuilder(description);

        if (api.IsDeprecated)
        {
            if (text.Length > 0)
            {
                if (text[^1] != '.')
                {
                    text.Append('.');
                }

                text.Append(' ');
            }

            text.Append("This API version has been deprecated.");
        }

        if (api.SunsetPolicy is { } policy)
        {
            if (policy.Date is { } when)
            {
                if (text.Length > 0)
                {
                    text.Append(' ');
                }

                text.Append("The API will be sunset on ")
                    .Append(when.Date.ToShortDateString())
                    .Append('.');
            }

            if (policy.HasLinks)
            {
                text.AppendLine();

                var rendered = false;

                foreach (var link in policy.Links.Where(l => l.Type == "text/html"))
                {
                    if (!rendered)
                    {
                        text.Append("<h4>Links</h4><ul>");
                        rendered = true;
                    }

                    text.Append("<li><a href=\"");
                    text.Append(link.LinkTarget.OriginalString);
                    text.Append("\">");
                    text.Append(
                        StringSegment.IsNullOrEmpty(link.Title)
                        ? link.LinkTarget.OriginalString
                        : link.Title.ToString());
                    text.Append("</a></li>");
                }

                if (rendered)
                {
                    text.Append("</ul>");
                }
            }
        }

        return text.ToString();
    }
}
