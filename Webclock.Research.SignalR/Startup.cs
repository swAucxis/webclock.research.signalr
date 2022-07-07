using Akka.Bootstrap.Docker;
using Akka.Configuration;
using Akka.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Phobos.Actor;
using Phobos.Hosting;
using System.Net;
using System.Reflection;
using Webclock.Research.SignalR.Hubs;
using Webclock.Research.SignalR.Services;

namespace Webclock.Research.SignalR
{
    public class Startup
    {
        public const string JaegerAgentHostEnvironmentVar = "JAEGER_AGENT_HOST";

        public const string JaegerEndpointEnvironmentVar = "JAEGER_ENDPOINT";

        public const string JaegerAgentPortEnvironmentVar = "JAEGER_AGENT_PORT";

        public const int DefaultJaegerAgentPort = 6832;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSignalR().AddJsonProtocol();
            services.AddSingleton<ISignalRProcessor, AkkaService>();

            var resource = ResourceBuilder.CreateDefault()
                .AddService(Assembly.GetEntryAssembly().GetName().Name, serviceInstanceId: $"{Dns.GetHostName()}");

            // enables OpenTelemetry for ASP.NET / .NET Core
            services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    .SetResourceBuilder(resource)
                    .AddPhobosInstrumentation()
                    .AddSource("Webclock.Research.SignalR")
                    .AddHttpClientInstrumentation(options =>
                    {
                        // don't trace HTTP output to Seq
                        options.Filter = httpRequestMessage => !httpRequestMessage.RequestUri.Host.Contains("seq");
                    })
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = context => !context.Request.Path.StartsWithSegments("/metrics");
                    })
                    .AddJaegerExporter(opt =>
                    {
                        opt.AgentHost = Environment.GetEnvironmentVariable(JaegerAgentHostEnvironmentVar);
                    });
            });

            services.AddOpenTelemetryMetrics(builder =>
            {
                builder
                    .SetResourceBuilder(resource)
                    .AddPhobosInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddPrometheusExporter(opt => { });
            });

            // sets up Akka.NET
            ConfigureAkka(services);
        }

        public static void ConfigureAkka(IServiceCollection services)
        {
            services.AddAkka("akka-system", (builder, provider) =>
            {
                var config = ConfigurationFactory.ParseString(File.ReadAllText("app.conf"))
                    .BootstrapFromDocker();
                //.UseSerilog();

                builder.AddHocon(config)
                    .WithPhobos(AkkaRunMode.Local); // enable Phobos
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AuctionHub>("/auctionHub");
            });
        }
    }
}