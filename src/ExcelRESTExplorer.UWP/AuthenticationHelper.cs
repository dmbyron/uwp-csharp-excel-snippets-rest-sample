﻿using System;
using System.Threading.Tasks;

using Windows.Storage;

using Microsoft.Identity.Client;

namespace ExcelServiceExplorer
{
    public class AuthenticationHelper
    {
        // The Client ID is used by the application to uniquely identify itself to the v2.0 authentication endpoint.
        static string clientId = "40e95fb5-7b03-429c-a12c-4310b3e7a3b0";

        public static PublicClientApplication IdentityClientApp = new PublicClientApplication(clientId);

        public static string TokenForUser = null;
        public static DateTimeOffset Expiration;
        public static ApplicationDataContainer _settings = ApplicationData.Current.RoamingSettings;

        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> GetTokenForUserAsync()
        {
            AuthenticationResult authResult;
            var scopes = new string[]
            {
                "User.Read",
                "User.ReadBasic.All",
                "Files.ReadWrite",
            };

            try
            {
                // Specify the "organizations" authority, for now, because Excel API currently works only with work and school accounts.
                string userId = (string)_settings.Values["userID"];
                authResult = await IdentityClientApp.AcquireTokenSilentAsync(scopes, userId, "https://login.microsoftonline.com/organizations/", null, false);
                TokenForUser = authResult.Token;
                // Save user ID in local storage
                _settings.Values["userID"] = authResult.User.UniqueId;
                _settings.Values["login_hint"] = authResult.User.Name;
                App.UserAccount = authResult.User;
            }

            catch (Exception e)
            {
                if (TokenForUser == null || Expiration <= DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    // Specify the "organizations" authority, for now, because Excel API currently works only with work and school accounts.
                    authResult = await IdentityClientApp.AcquireTokenAsync(scopes, "", UiOptions.SelectAccount, null, null, "https://login.microsoftonline.com/organizations/", null);
                    TokenForUser = authResult.Token;
                    Expiration = authResult.ExpiresOn;
                    // Save user ID in local storage
                    _settings.Values["userID"] = authResult.User.UniqueId;
                    _settings.Values["login_hint"] = authResult.User.Name;
                    App.UserAccount = authResult.User;
                }
            }

            return TokenForUser;
        }

        /// <summary>
        /// Signs the user out of the service.
        /// </summary>
        public static void SignOut()
        {
            foreach (var user in IdentityClientApp.Users)
            {
                user.SignOut();
            }

            TokenForUser = null;
            _settings.Values["userID"] = null;
            _settings.Values["login_hint"] = null;
        }

    }
}
