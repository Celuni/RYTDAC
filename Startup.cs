using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCouch;
using Newtonsoft.Json;

namespace RYTDAC
{
    public class Startup
    {
        private const string VotesPath = "/votes";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Store = new MyCouchStore(
                Configuration.GetSection("CouchDb")["Url"],
                Configuration.GetSection("CouchDb")["Database"]
            );

            Client = new WebClient();
        }

        public IConfiguration Configuration { get; }

        private MyCouchStore Store { get; }

        private WebClient Client { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Add the reverse proxy to capability to the server
            var proxyBuilder = services.AddReverseProxy();
            // Initialize the reverse proxy from the "ReverseProxy" section of configuration
            proxyBuilder.LoadFromConfig(Configuration.GetSection("ReverseProxy"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next();
            });

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy(proxyPipeline =>
                {
                    proxyPipeline.Use(async (context, next) =>
                    {
                        var request = context.Request;

                        if (request.Path.Equals(VotesPath, StringComparison.OrdinalIgnoreCase))
                        {
                            var feature = context.GetReverseProxyFeature();

                            var backend = feature.Cluster.Config.Destinations["backend"].Address;

                            var videoId = request.Query["videoId"].FirstOrDefault();

                            var vote = await Store.Client.Entities.GetAsync<Vote>(videoId);

                            var fetch = CombineUriToString(backend, $"{request.Path}{request.QueryString}");

                            switch (vote.StatusCode)
                            {
                                case HttpStatusCode.NotFound:
                                    _ = Task.Run(() =>
                                    {
                                        var body = Client.DownloadString(fetch);

                                        var voteResult = JsonConvert.DeserializeObject<Vote>(body);

                                        //
                                        // Store so we know when to invalidate result
                                        // 
                                        voteResult.CachedAt = DateTime.UtcNow;

                                        Store.Client.Entities.PostAsync(voteResult);
                                    });
                                    break;
                                case HttpStatusCode.OK:
                                    vote.Content.FromCache = true;
                                    await context.Response.WriteAsJsonAsync(vote.Content);
                                    return;
                            }
                        }

                        await next();
                    });
                    proxyPipeline.UseSessionAffinity();
                    proxyPipeline.UseLoadBalancing();
                    proxyPipeline.UsePassiveHealthChecks();
                });
            });
        }

        private static Uri CombineUri(string baseUri, string relativeOrAbsoluteUri)
        {
            return new Uri(new Uri(baseUri), relativeOrAbsoluteUri);
        }

        private static string CombineUriToString(string baseUri, string relativeOrAbsoluteUri)
        {
            return new Uri(new Uri(baseUri), relativeOrAbsoluteUri).ToString();
        }
    }
}