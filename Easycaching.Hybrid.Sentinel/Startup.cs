using Easycaching.Hybrid.Sentinel.Services;
using EasyCaching.Core.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Easycaching.Hybrid.Sentinel
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Easycaching.Hybrid.Sentinel", Version = "v1" });
            });

            services.AddEasyCaching(option =>
            {
                //local in memory cache
                option.UseInMemory("inmem1");

                //distributed cach
                option.UseRedis(config =>
                {
                    
                    config.DBConfig.Configuration = $"localhost:26379,serviceName=mymaster";

                    config.DBConfig.AsyncTimeout = 5000;
                    config.DBConfig.SyncTimeout = 5000;

                }, "redis1");

                option.UseHybrid(config =>
                {
                    config.TopicName = "topic";
                    //config.EnableLogging = true;
                    // specify the local cache provider name
                    config.LocalCacheProviderName = "inmem1";

                    // specify the distributed cache provider name
                    config.DistributedCacheProviderName = "redis1";
                })
                .WithRedisBus(busCfg =>
                {
                    busCfg.Endpoints.Add(new ServerEndPoint("localhost", 6379));
                });

            });

            //Register Custom Services
            services.AddSingleton<ICacheService, CacheService>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Easycaching.Hybrid.Sentinel v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
