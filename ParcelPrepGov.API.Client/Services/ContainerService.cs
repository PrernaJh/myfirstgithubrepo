using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Threading;
using ParcelPrepGov.API.Client.Interfaces;
using ParcelPrepGov.API.Client.Data;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ParcelPrepGov.API.Client.Services
{
    public class ContainerService : IContainerService
    {
        private IErrorLogger errorLogger;
        private HttpClient client;
        private HttpClientHandler clientHandler;
        private CookieContainer cookieContainer;

        public ContainerService(IConfiguration configuration, IErrorLogger errorLogger)
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

        public void PostAssignActiveContainer(ref AssignContainer container, AccountLogin loggedInAccount)
        {
            try
            {
                container.username = loggedInAccount.username;

                clientHandler.CookieContainer.Add(loggedInAccount.cookie);

                var serializedAssignContainerRequest = JsonSerializer.Serialize(container);

                using (var request = new HttpRequestMessage(HttpMethod.Post, "/api/Container/AssignActiveContainer"))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                    request.Content = new StringContent(serializedAssignContainerRequest);

                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                    using (var response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;

                            container.response = JsonSerializer.Deserialize<AssignContainerResponse>(result);
                        }
                        else
                        {
                            using (StreamWriter writer = new StreamWriter(@"./Resources/Log.txt", true))
                            {
                                writer.WriteLine("Failed request: Assign active container.");
                                writer.WriteLine("Request: " + serializedAssignContainerRequest);
                                writer.WriteLine("Response: " + response.Content.ReadAsStringAsync().Result);
                            }
                            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                errorLogger.HTTPRequestError(ErrorTypes.BadRequest, "");
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
                errorLogger.HTTPRequestError(ErrorTypes.BadRequest, ex.Message);
                container.response.Message = ex.Message;
                container.response.ErrorCode = "Error001"; // Exception
                container.response.IsSuccessful = false;
            }
        }

        public void PostAssignNewContainer(ref AssignContainer container, AccountLogin loggedInAccount)
        {
            try
            {
                container.username = loggedInAccount.username;

                clientHandler.CookieContainer.Add(loggedInAccount.cookie);

                var serializedAssignContainerRequest = JsonSerializer.Serialize(container);

                using (var request = new HttpRequestMessage(HttpMethod.Post, "/api/Container/AssignNewContainer"))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                    request.Content = new StringContent(serializedAssignContainerRequest);

                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                    using (var response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;

                            container.response = JsonSerializer.Deserialize<AssignContainerResponse>(result);
                        }
                        else
                        {
                            using (StreamWriter writer = new StreamWriter(@"./Resources/Log.txt", true))
                            {
                                writer.WriteLine("Failed request: Assign new container.");
                                writer.WriteLine("Request: " + serializedAssignContainerRequest);
                                writer.WriteLine("Response: " + response.Content.ReadAsStringAsync().Result);
                            }
                            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                errorLogger.HTTPRequestError(ErrorTypes.BadRequest, "");
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
                errorLogger.HTTPRequestError(ErrorTypes.BadRequest, ex.Message);
                container.response.Message = ex.Message;
                container.response.ErrorCode = "Error001"; // Exception
                container.response.IsSuccessful = false;
            }
        }

        public void PostReplaceContainer(ref ReplaceContainer container, AccountLogin loggedInAccount)
        {
            try
            {
                container.username = loggedInAccount.username;

                clientHandler.CookieContainer.Add(loggedInAccount.cookie);

                var serializedReplaceContainerRequest = JsonSerializer.Serialize(container);

                using (var request = new HttpRequestMessage(HttpMethod.Post, "/api/Container/ReplaceContainer"))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                    request.Content = new StringContent(serializedReplaceContainerRequest);

                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                    using (var response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;

                            container.response = JsonSerializer.Deserialize<ReplaceContainerResponse>(result);
                        }
                        else
                        {
                            using (StreamWriter writer = new StreamWriter(@"./Resources/Log.txt", true))
                            {
                                writer.WriteLine("Failed request: Replace container.");
                                writer.WriteLine("Request: " + serializedReplaceContainerRequest);
                                writer.WriteLine("Response: " + response.Content.ReadAsStringAsync().Result);
                            }
                            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                errorLogger.HTTPRequestError(ErrorTypes.BadRequest, "");
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
                errorLogger.HTTPRequestError(ErrorTypes.BadRequest, ex.Message);
                container.response.Message = ex.Message;
                container.response.ErrorCode = "Error001"; // Exception
                container.response.IsSuccessful = false;
            }
        }
    }
}
