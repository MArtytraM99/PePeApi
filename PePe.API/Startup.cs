using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoMapper;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using PePe.API.Model;
using PePe.DAO;
using PePe.Manager;
using PePe.Service;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Net.Http;

namespace PePe.API {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers().AddJsonOptions(opts => {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddLogging();

            services.AddScoped<IMonthConvertor, MonthConvertor>();
            services.AddScoped<ILoadedHtmlDocumentProvider, WebLoadedHtmlDocumentProvider>();
            services.AddScoped<IBasicInfoProvider, StaticBasicInfoProvider>();
            services.AddAutoMapper(typeof(APIProfile));

            services.AddScoped<IWebScraper, WebScraper>();
            services.AddScoped<IDateProvider, PragueDateProvider>();
            
            string connectionString = Configuration.GetConnectionString("mongoDB");
            string databaseName = Configuration.GetConnectionString("pepeDbName");
            string collectionName = Configuration.GetConnectionString("pepeMenuCollectionName");
            new ClassMap().RegisterMaps();
            services.AddSingleton<IMenuDao, MongoMenuDao>(serviceProvider => {
                return new MongoMenuDao(
                    connectionString,
                    databaseName,
                    collectionName,
                    serviceProvider.GetRequiredService<ILogger<MongoMenuDao>>()
                );
            });

            services.AddScoped<IMenuManager, MenuManager>();

            services.AddHealthChecks()
                .AddMongoDb(connectionString, name: "MongoDb connection")
                .AddMongoDb(connectionString, databaseName, name: "MongoDB database")
                .AddCheck("Single todays menu", new SingleMenuHealthCheck(connectionString, databaseName, collectionName, new PragueDateProvider()));

            services.AddHealthChecksUI().AddInMemoryStorage();

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "PePe API",
                    Version = "v1",
                    Description = "Simple API for getting menus from our favorite canteen PePe.",
                    Contact = new OpenApiContact {
                        Name = "Martin V�tek",
                        Email = "martulavitek@seznam.cz"
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.RoutePrefix = "";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PePe API");
            });

            if(Configuration.GetValue<bool>("UseHttps"))
                app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapHealthChecks(Configuration.GetSection("HealthCheck").GetValue<string>("StatusUri"), new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI(options =>
                {
                    options.UIPath = Configuration.GetSection("HealthCheck").GetValue<string>("UIUri");
                });
                endpoints.MapControllers();
            });
        }
    }
}
