namespace GraduationProject.Application.Services
{
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Auth.OAuth2.Flows;
    using Google.Apis.Auth.OAuth2.Responses;
    using Google.Apis.Gmail.v1;
    using Google.Apis.Gmail.v1.Data;
    using Google.Apis.Services;
    using GraduationProject.Application.Services.Interfaces;
    using GraduationProject.Domain.Models;
    using GraduationProject.StartupConfigurations;
    using Microsoft.AspNetCore.Authentication.Google;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using MimeKit;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class GmailServiceHelper : IGmailServiceHelper
    {
        private readonly string _applicationName = "Engenetic";
        private readonly MailingOptions _options;
        private readonly UserManager<AppUser> _userManager;

        public GmailServiceHelper(IOptions<MailingOptions> options, UserManager<AppUser> userManager)
        {
            _options = options.Value;
            _userManager = userManager;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var user = await _userManager.FindByEmailAsync(_options.Email);
            if (user == null)
                throw new ArgumentNullException($"{nameof(user)} is null!");

            var refreshToken = await _userManager.GetAuthenticationTokenAsync
                                    (user, GoogleDefaults.DisplayName, "refresh_token");

            if (refreshToken == null)
                throw new ArgumentNullException("token is null!");

            var credential = new UserCredential(
                new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _options.ClientId,
                        ClientSecret = _options.ClientSecret
                    },
                    Scopes = [GmailService.Scope.GmailSend]
                }),
                "me",
                new TokenResponse { RefreshToken = refreshToken }
            );

            // Automatically refresh access token if expired
            if (credential.Token.IsStale)
            {
                await credential.RefreshTokenAsync(CancellationToken.None);
            }


            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });

            // Create Email
            var emailContent = CreateEmail(to, _options.Email, subject, body);

            // Send Email
            // me is a default for currnetly authenticated user
            await service.Users.Messages.Send(emailContent, "me").ExecuteAsync();

        }

        private Message CreateEmail(string to, string from, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_applicationName, from));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using (var memoryStream = new MemoryStream())
            {
                message.WriteTo(memoryStream);
                return new Message
                {
                    Raw = Convert.ToBase64String(memoryStream.ToArray())
                        .Replace('+', '-')
                        .Replace('/', '_')
                        .Replace("=", "")
                };
            }
        }

    }


}
