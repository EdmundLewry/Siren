﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

/*
 This implementation based off of the Oceans UI Service middleware
 */
namespace CBS.SirenUI.Backend.Middleware
{
    public class ReverseProxyMiddleware
    {
        private readonly RequestDelegate _nextMiddleware;
        public string Origin { get; set; } = "http://localhost:5000";
        public IHttpClientFactory HttpClientFactory { get; }

        public ReverseProxyMiddleware(RequestDelegate nextMiddleware, IHttpClientFactory httpClientFactory)
        {
            _nextMiddleware = nextMiddleware;
            HttpClientFactory = httpClientFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            if(!IsProxyRequest(context.Request.Path))
            {
                await _nextMiddleware(context);
                return;
            }

            var targetUri = ConstructTargetUri(context.Request.Path);

            await RouteMessageToTarget(context, targetUri);
        }

        private bool IsProxyRequest(PathString path)
        {
            return path.StartsWithSegments("/proxy");
        }

        private Uri ConstructTargetUri(PathString path)
        {
            if(path.StartsWithSegments("/proxy", out PathString target))
            {
                path = target;
            }

            return new Uri(Origin + path);
        }
        private async Task RouteMessageToTarget(HttpContext context, Uri targetUri)
        {
            HttpRequestMessage requestMessage = ConstructTargetRequest(context, targetUri);

            HttpClient httpClient = HttpClientFactory.CreateClient();
            using HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

            context.Response.StatusCode = (int)responseMessage.StatusCode;

            CopyResponseHeaders(context, responseMessage.Headers);
            CopyResponseHeaders(context, responseMessage.Content.Headers);
            context.Response.Headers.Remove("transfer-encoding");

            await responseMessage.Content.CopyToAsync(context.Response.Body);
        }

        private void CopyResponseHeaders<T>(HttpContext context, T headers) where T : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
        {
            foreach (var header in headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
        }

        private HttpRequestMessage ConstructTargetRequest(HttpContext context, Uri targetUri)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                RequestUri = targetUri
            };

            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);

            if(requestMessage.Method != HttpMethod.Get)
            {
                requestMessage.Content = new StreamContent(context.Request.Body);

                Console.WriteLine(requestMessage.Content.ReadAsStringAsync().Result);

                foreach (var header in context.Request.Headers)
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            return requestMessage;
        }

        private HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsPatch(method)) return HttpMethod.Patch;

            throw new ArgumentException("Unsupported request method found when proxying request", nameof(method));
        }
    }
}
