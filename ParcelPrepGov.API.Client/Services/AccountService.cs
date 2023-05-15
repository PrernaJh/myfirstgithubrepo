using Microsoft.Extensions.Configuration;
using ParcelPrepGov.API.Client.Interfaces;
using ParcelPrepGov.API.Client.Data;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ParcelPrepGov.API.Client.Services
{
    public class AccountService : IAccountService
    {
        private IErrorLogger errorLogger;
        private HttpClient client;
        private HttpClientHandler clientHandler;
        private CookieContainer cookieContainer;

        public AccountService(IConfiguration configuration, IErrorLogger errorLogger)
        {
            this.errorLogger = errorLogger;
            cookieContainer = new CookieContainer();

            clientHandler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer
            };

            client = new HttpClient(clientHandler);

            client.BaseAddress = new Uri(configuration.GetSection("Api").GetSection("Url").Value);

            client.Timeout = new TimeSpan(0, 0, 45);
        }


        private bool MatchAcceptedRoles(LoginResponse response)
        {
            return response.AuthorizationTier == Roles.Administrator ||
                   response.AuthorizationTier == Roles.SystemAdministrator ||
                   response.AuthorizationTier == Roles.Supervisor ||
                   response.AuthorizationTier == Roles.Operator ||
                   response.AuthorizationTier == Roles.QualityAssurance;
        }


        public AccountLogin PostAccountLogin(string username, string pwd)
        {
            AccountLogin accountLogin = new AccountLogin();
            try
            {
                accountLogin.username = username;

                var cookieUri = new Uri(client.BaseAddress + "/api/Account/Login");

                var loginRequest = new LoginRequest
                {
                    username = username,
                    password = pwd
                };

                var serializedLoginRequest = JsonSerializer.Serialize(loginRequest);


                using (var request = new HttpRequestMessage(HttpMethod.Post, "/api/Account/Login"))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                    request.Content = new StringContent(serializedLoginRequest);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                    using (var response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;

                            accountLogin.response = JsonSerializer.Deserialize<LoginResponse>(result);

                            CookieCollection cookies = cookieContainer.GetCookies(cookieUri);

                            if (cookies["ppgpro_api"] != null)
                            {
                                accountLogin.cookie = cookies["ppgpro_api"];
                            }

                            if (!MatchAcceptedRoles(accountLogin.response))
                            {
                                accountLogin.response.Succeeded = false;
                            }
                        }
                        else
                        {
                            if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                errorLogger.HTTPRequestError(ErrorTypes.BadRequest, "");
                                accountLogin.response.Succeeded = false;
                            }
                            else
                            {
                                response.EnsureSuccessStatusCode();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorLogger.HTTPRequestError(ErrorTypes.Login, ex.Message);
                accountLogin.response.Succeeded = false;
            }
            return accountLogin;
        }

        public bool PostAccountLogout(AccountLogin accountToLogout)
        {
            try
            {
                clientHandler.CookieContainer.Add(accountToLogout.cookie);

                using (var request = new HttpRequestMessage(HttpMethod.Post, "/api/Account/Logout"))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                    //request.Headers.Add(HttpRequestHeader.Cookie, new Cookie("ppgpro_api", accountToLogout.Token));

                    using (var response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result)
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public GetUserResponse GetUser(string username, AccountLogin loggedInAccount)
        {
            var user = new GetUserResponse() { Username = username };
            try
            {
                clientHandler.CookieContainer.Add(loggedInAccount.cookie);

                using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Account/GetUser?username={username}"))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                    //request.Headers.Add(HttpRequestHeader.Cookie, new Cookie("ppgpro_api", accountToLogout.Token));

                    using (var response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            user = JsonSerializer.Deserialize<GetUserResponse>(result);
                        }
                        else
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return user;
        }
    }
}
