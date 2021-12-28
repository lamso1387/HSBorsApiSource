using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HSBors.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HSBors.Controllers;
using System.Reflection;
using System.IO;
using Newtonsoft.Json.Serialization;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using HSBors.Middleware;
using HSBors.Services;

namespace HSBors
{
#pragma warning disable CS1591
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            HSBorsDbContext = GetContext(Configuration);

        }
        HSBorsDb GetContext(IConfiguration configuration)
        { 
            return SRL.Database.CreateContext<HSBorsDb>(configuration, Constants.Setting.HsborsDbConfugrationValue);
        }
        public static HSBorsDb HSBorsDbContext { get; set; }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        { 
            // configure DI for application services for Authentication or midleware to inject UserService:
            services.AddScoped<UserService>();
            services.AddScoped<ILogger, Logger<DefaultController>>();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;//disables asp core automatic bad request
            }); 

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();//avoid cameCase or Uppercase or any convert just default
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;//avoid loop in db entity load reference foriegn entity (because of Include) BUT not works . use  [JsonIgnore] in properties instead or select new properies to return
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());//convert enum to api in api responce
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;//not result null item in api resporce
                                                                                                            // options.SerializerSettings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Error;//set request input to null if request has extra fields

                });


            services.AddDbContext<HSBorsDb>(builder =>
            {
                builder.UseSqlServer(Configuration[Constants.Setting.HsborsDbConfugrationValue]);
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Tarh API", Version = "v1" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                //addedby me: Also you need to enable building of xmldocs in your project's properties
              //  options.IncludeXmlComments(xmlPath);
            });

           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger Logger)
        { 
            app.UseCors(builder => builder.WithOrigins("http://localhost").AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
            
            app.UseGetRoutesMiddleware(GetRoutes);
             app.UseMiddleware<CustomAuthenticationMiddleware>();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseMvc(GetRoutes);
 

            app.UseSwagger();
            app.UseSwaggerUI(options =>
                        {
                            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tarh API V1");
                        });


            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<HSBorsDb>();
                context.Database.Migrate();

            }


        }

        private readonly Action<IRouteBuilder> GetRoutes =
    routes =>
    {
        routes.MapRoute(
        name: "default",
        template: "{controller=deposit}/{action=test}/{id?}");
    };
    }
#pragma warning restore CS1591

}
