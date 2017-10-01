﻿using System;
using System.Collections.Generic;

namespace OpenIddict.Core
{
    /// <summary>
    /// Represents an OpenIddict application descriptor.
    /// </summary>
    public class OpenIddictApplicationDescriptor
    {
        /// <summary>
        /// Gets or sets the client identifier
        /// associated with the application.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret associated with the application.
        /// Note: depending on the application manager used when creating it,
        /// this property may be hashed or encrypted for security reasons.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// associated with the application.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets the logout callback URLs
        /// associated with the application.
        /// </summary>
        public ISet<Uri> PostLogoutRedirectUris { get; } = new HashSet<Uri>();

        /// <summary>
        /// Gets the callback URLs
        /// associated with the application.
        /// </summary>
        public ISet<Uri> RedirectUris { get; } = new HashSet<Uri>();

        /// <summary>
        /// Gets or sets the application type
        /// associated with the application.
        /// </summary>
        public string Type { get; set; }
    }
}
