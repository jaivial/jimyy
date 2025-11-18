using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowAutomation.Core.Constants;
using WorkflowAutomation.Core.Models;

namespace WorkflowAutomation.Engine.Nodes
{
    /// <summary>
    /// HTTP request node for making HTTP calls to external APIs
    /// </summary>
    public class HttpRequestNode : NodeExecutorBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpRequestNode(
            ILogger<HttpRequestNode> logger,
            IHttpClientFactory httpClientFactory) : base(logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public override string NodeType => "httpRequest";

        public override NodeDefinition GetDefinition()
        {
            return new NodeDefinition
            {
                Type = NodeType,
                Name = "HTTP Request",
                Description = "Make HTTP requests (GET, POST, PUT, DELETE, PATCH)",
                Category = NodeCategories.Actions,
                Icon = "globe",
                Parameters = new List<NodeParameter>
                {
                    new NodeParameter
                    {
                        Name = "method",
                        DisplayName = "HTTP Method",
                        Type = "string",
                        Required = true,
                        DefaultValue = "GET",
                        Description = "HTTP method to use",
                        Options = new List<ParameterOption>
                        {
                            new ParameterOption { Value = "GET", Name = "GET" },
                            new ParameterOption { Value = "POST", Name = "POST" },
                            new ParameterOption { Value = "PUT", Name = "PUT" },
                            new ParameterOption { Value = "DELETE", Name = "DELETE" },
                            new ParameterOption { Value = "PATCH", Name = "PATCH" }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "url",
                        DisplayName = "URL",
                        Type = "string",
                        Required = true,
                        Description = "The URL to send the request to",
                        Validation = new ParameterValidation
                        {
                            MinLength = 1,
                            Pattern = @"^https?://",
                            ErrorMessage = "URL must start with http:// or https://"
                        }
                    },
                    new NodeParameter
                    {
                        Name = "headers",
                        DisplayName = "Headers",
                        Type = "object",
                        Required = false,
                        Description = "HTTP headers to include in the request"
                    },
                    new NodeParameter
                    {
                        Name = "body",
                        DisplayName = "Request Body",
                        Type = "object",
                        Required = false,
                        Description = "Request body for POST, PUT, PATCH requests"
                    },
                    new NodeParameter
                    {
                        Name = "authentication",
                        DisplayName = "Authentication",
                        Type = "string",
                        Required = false,
                        DefaultValue = "None",
                        Description = "Authentication type",
                        Options = new List<ParameterOption>
                        {
                            new ParameterOption { Value = "None", Name = "None" },
                            new ParameterOption { Value = "Basic", Name = "Basic Auth" },
                            new ParameterOption { Value = "Bearer", Name = "Bearer Token" }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "username",
                        DisplayName = "Username",
                        Type = "string",
                        Required = false,
                        Description = "Username for Basic authentication",
                        DisplayOptions = new ParameterDisplayOptions
                        {
                            ShowIf = new Dictionary<string, object>
                            {
                                { "authentication", "Basic" }
                            }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "password",
                        DisplayName = "Password",
                        Type = "string",
                        Required = false,
                        Description = "Password for Basic authentication",
                        DisplayOptions = new ParameterDisplayOptions
                        {
                            ShowIf = new Dictionary<string, object>
                            {
                                { "authentication", "Basic" }
                            }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "token",
                        DisplayName = "Bearer Token",
                        Type = "string",
                        Required = false,
                        Description = "Bearer token for authentication",
                        DisplayOptions = new ParameterDisplayOptions
                        {
                            ShowIf = new Dictionary<string, object>
                            {
                                { "authentication", "Bearer" }
                            }
                        }
                    },
                    new NodeParameter
                    {
                        Name = "timeout",
                        DisplayName = "Timeout (seconds)",
                        Type = "number",
                        Required = false,
                        DefaultValue = 30,
                        Description = "Request timeout in seconds",
                        Validation = new ParameterValidation
                        {
                            Min = 1,
                            Max = 300
                        }
                    }
                },
                Outputs = new List<NodeOutput>
                {
                    new NodeOutput
                    {
                        Name = "statusCode",
                        Type = "number",
                        Description = "HTTP status code"
                    },
                    new NodeOutput
                    {
                        Name = "headers",
                        Type = "object",
                        Description = "Response headers"
                    },
                    new NodeOutput
                    {
                        Name = "body",
                        Type = "object",
                        Description = "Response body"
                    }
                }
            };
        }

        protected override async Task<object> ExecuteInternalAsync(
            Dictionary<string, object> parameters,
            object context,
            CancellationToken cancellationToken)
        {
            var method = GetRequiredParameter<string>(parameters, "method").ToUpper();
            var url = GetRequiredParameter<string>(parameters, "url");
            var headers = GetParameter<Dictionary<string, string>>(parameters, "headers", null);
            var body = GetParameter<object>(parameters, "body", null);
            var authentication = GetParameter<string>(parameters, "authentication", "None");
            var timeout = GetParameter<int>(parameters, "timeout", 30);

            Logger.LogDebug("HttpRequestNode: Sending {Method} request to {Url}", method, url);

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(timeout);

                var request = new HttpRequestMessage(new HttpMethod(method), url);

                // Add custom headers
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // Add authentication
                AddAuthentication(request, authentication, parameters);

                // Add body for POST, PUT, PATCH
                if (body != null && (method == "POST" || method == "PUT" || method == "PATCH"))
                {
                    var jsonBody = body is string str ? str : JsonSerializer.Serialize(body);
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }

                // Send request
                var response = await client.SendAsync(request, cancellationToken);

                // Read response
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                // Parse response body as JSON if possible
                object responseBody = responseContent;
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    try
                    {
                        responseBody = JsonSerializer.Deserialize<object>(responseContent);
                    }
                    catch
                    {
                        // Keep as string if not valid JSON
                        responseBody = responseContent;
                    }
                }

                // Convert headers to dictionary
                var responseHeaders = response.Headers
                    .Union(response.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                    .ToDictionary(
                        h => h.Key,
                        h => string.Join(", ", h.Value)
                    );

                Logger.LogDebug("HttpRequestNode: Received response with status {StatusCode}", (int)response.StatusCode);

                return CreateSuccessResult(new
                {
                    statusCode = (int)response.StatusCode,
                    statusText = response.ReasonPhrase,
                    headers = responseHeaders,
                    body = responseBody,
                    isSuccess = response.IsSuccessStatusCode
                });
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "HttpRequestNode: HTTP request failed");
                return CreateErrorResult($"HTTP request failed: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                Logger.LogError(ex, "HttpRequestNode: Request timeout");
                return CreateErrorResult("Request timeout", ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "HttpRequestNode: Unexpected error");
                return CreateErrorResult($"Unexpected error: {ex.Message}", ex);
            }
        }

        private void AddAuthentication(HttpRequestMessage request, string authenticationType, Dictionary<string, object> parameters)
        {
            switch (authenticationType)
            {
                case "Basic":
                    var username = GetParameter<string>(parameters, "username", "");
                    var password = GetParameter<string>(parameters, "password", "");
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                    }
                    break;

                case "Bearer":
                    var token = GetParameter<string>(parameters, "token", "");
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    break;

                case "None":
                default:
                    // No authentication
                    break;
            }
        }
    }
}
