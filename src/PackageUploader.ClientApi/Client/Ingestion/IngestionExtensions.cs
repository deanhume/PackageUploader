﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PackageUploader.ClientApi.Client.Ingestion.Config;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Timeout;
using System;
using System.Net.Http;
using System.Net.Mime;

namespace PackageUploader.ClientApi.Client.Ingestion;

internal static class IngestionExtensions
{
    public static IServiceCollection AddIngestionService(this IServiceCollection services)
    {
        services.AddSingleton<IValidateOptions<IngestionConfig>, IngestionConfigValidator>();
        services.AddOptions<IngestionConfig>().BindConfiguration(nameof(IngestionConfig));
        services.AddSingleton<IngestionSdkVersion>();
        services.AddScoped<IngestionAuthenticationDelegatingHandler>();
        services.AddHttpClient<IIngestionHttpClient, IngestionHttpClient>((serviceProvider, httpClient) =>
            {
                httpClient.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json);

                var ingestionConfig = serviceProvider.GetRequiredService<IOptions<IngestionConfig>>().Value;
                httpClient.BaseAddress = new Uri(ingestionConfig.BaseAddress);
            })
            .AddHttpMessageHandler<IngestionAuthenticationDelegatingHandler>()
            .AddPolicyHandler((serviceProvider, _) =>
            {
                var ingestionConfig = serviceProvider.GetRequiredService<IOptions<IngestionConfig>>().Value;
                var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(ingestionConfig.MedianFirstRetryDelayMs), ingestionConfig.RetryCount);
                return HttpPolicyExtensions.HandleTransientHttpError().Or<TimeoutRejectedException>().WaitAndRetryAsync(delay);
            })
            .AddPolicyHandler((serviceProvider, _) =>
            {
                var ingestionConfig = serviceProvider.GetRequiredService<IOptions<IngestionConfig>>().Value;
                return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(ingestionConfig.HttpTimeoutMs));
            });

        return services;
    }
}