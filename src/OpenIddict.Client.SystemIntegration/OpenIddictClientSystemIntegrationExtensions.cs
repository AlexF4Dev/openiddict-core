﻿/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenIddict.Client;
using OpenIddict.Client.SystemIntegration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Exposes extensions allowing to register the OpenIddict client services.
/// </summary>
public static class OpenIddictClientSystemIntegrationExtensions
{
    /// <summary>
    /// Registers the OpenIddict client system integration services in the DI container.
    /// </summary>
    /// <param name="builder">The services builder used by OpenIddict to register new services.</param>
    /// <remarks>This extension can be safely called multiple times.</remarks>
    /// <returns>The <see cref="OpenIddictClientSystemIntegrationBuilder"/>.</returns>
    public static OpenIddictClientSystemIntegrationBuilder UseSystemIntegration(this OpenIddictClientBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
            !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException(SR.GetResourceString(SR.ID0389));
        }

        // Note: the OpenIddict activation handler service is deliberately registered as early as possible to
        // ensure protocol activations can be handled before another service can stop the initialization of the
        // application (e.g Dapplo.Microsoft.Extensions.Hosting.AppServices relies on an IHostedService to implement
        // single instantiation, which would prevent the OpenIddict service from handling the protocol activation
        // if the OpenIddict activation handler service was not registered before the Dapplo IHostedService).
        if (!builder.Services.Any(static descriptor =>
            descriptor.ServiceType == typeof(IHostedService) &&
            descriptor.ImplementationType == typeof(OpenIddictClientSystemIntegrationActivationHandler)))
        {
            builder.Services.Insert(0, ServiceDescriptor.Singleton<IHostedService, OpenIddictClientSystemIntegrationActivationHandler>());
        }

        // Register the services responsible for coordinating and managing authentication operations.
        builder.Services.TryAddSingleton<OpenIddictClientSystemIntegrationMarshal>();
        builder.Services.TryAddSingleton<OpenIddictClientSystemIntegrationService>();

        builder.Services.TryAddSingleton(static provider => provider.GetServices<IHostedService>()
            .OfType<OpenIddictClientSystemIntegrationHttpListener>()
            .Single());

        // Register the built-in filters used by the default OpenIddict client system integration event handlers.
        builder.Services.TryAddSingleton<RequireAuthenticationNonce>();
        builder.Services.TryAddSingleton<RequireEmbeddedWebServerEnabled>();
        builder.Services.TryAddSingleton<RequireHttpListenerContext>();
        builder.Services.TryAddSingleton<RequireInteractiveSession>();
        builder.Services.TryAddSingleton<RequireProtocolActivation>();
        builder.Services.TryAddSingleton<RequireSystemBrowser>();
        builder.Services.TryAddSingleton<RequireWebAuthenticationBroker>();
        builder.Services.TryAddSingleton<RequireWebAuthenticationResult>();

        // Register the built-in event handlers used by the OpenIddict client system integration components.
        // Note: the order used here is not important, as the actual order is set in the options.
        builder.Services.TryAdd(OpenIddictClientSystemIntegrationHandlers.DefaultHandlers.Select(descriptor => descriptor.ServiceDescriptor));

        // Register the option initializer and the background service used by the OpenIddict client system integration services.
        // Note: TryAddEnumerable() is used here to ensure the initializers and the background service are only registered once.
        builder.Services.TryAddEnumerable(
        [
            ServiceDescriptor.Singleton<IHostedService, OpenIddictClientSystemIntegrationHttpListener>(),
            ServiceDescriptor.Singleton<IHostedService, OpenIddictClientSystemIntegrationPipeListener>(),

            ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictClientOptions>, OpenIddictClientSystemIntegrationConfiguration>(),
            ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIddictClientOptions>, OpenIddictClientSystemIntegrationConfiguration>(),

            ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIddictClientSystemIntegrationOptions>, OpenIddictClientSystemIntegrationConfiguration>()
        ]);

        return new OpenIddictClientSystemIntegrationBuilder(builder.Services);
    }

    /// <summary>
    /// Registers the OpenIddict client system integration services in the DI container.
    /// </summary>
    /// <param name="builder">The services builder used by OpenIddict to register new services.</param>
    /// <param name="configuration">The configuration delegate used to configure the client services.</param>
    /// <remarks>This extension can be safely called multiple times.</remarks>
    /// <returns>The <see cref="OpenIddictClientBuilder"/>.</returns>
    public static OpenIddictClientBuilder UseSystemIntegration(
        this OpenIddictClientBuilder builder, Action<OpenIddictClientSystemIntegrationBuilder> configuration)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        configuration(builder.UseSystemIntegration());

        return builder;
    }
}
