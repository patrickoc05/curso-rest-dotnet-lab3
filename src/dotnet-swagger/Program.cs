using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using Core.Data;
using AdventureWorksDbContext = Core.Data.AdventureWorksDbContext;
using Microsoft.EntityFrameworkCore;

namespace Dotnet_Backend
{
    // dotnet run --urls="http://+:5000;https://+:5001"
    public class Program
    {
        public static void Main(string[] args)
            =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Program>();
                }).Build().Run();

        public Program(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Policy",
                                builder =>
                                {
                                    builder.AllowAnyOrigin()
                                           .AllowAnyHeader()
                                           .AllowAnyMethod();
                                });
            });
            services.AddControllers();

            // dotnet add package Swashbuckle.AspNetCore
            services.AddSwaggerGen();

            services.AddDbContext<AdventureWorksDbContext>(
                options =>
                    options.UseSqlServer(Configuration.GetConnectionString("AdventureWorks")));

            services.AddScoped<AdventureWorksDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors("Policy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGet("/", context =>
                {

                    context.Response.Redirect("/swagger");
                    return Task.CompletedTask;
                });

                endpoints.MapGet("db/hello", async context =>
                {
                    var adventureWorks = "data source=localhost,1433;initial catalog=Adventureworks;persist security info=True;user id=sa;password=Password.123;MultipleActiveResultSets=True;";

                    using (var connection = new SqlConnection(adventureWorks))
                    {
                        SqlCommand command = new("EXEC [dbo].[sp_HelloWorld]", connection);
                        command.Connection.Open();
                        var helloDb = command.ExecuteScalar() as string;

                        context.Response.StatusCode = 200;

                        await context.Response.WriteAsync(helloDb);
                    }
                });

                endpoints.MapGet("api/products", async context =>
                {
                    var dbContext = context.Request.HttpContext.RequestServices.GetRequiredService<AdventureWorksDbContext>();

                    await context.Response.WriteAsJsonAsync<object[]>(
                        dbContext.Products.Select(p =>
                            new {
                                Id = p.ProductId,
                                Name = p.Name,
                                ListPrice = p.ListPrice
                            }).ToArray()
                    );
                });
            });

        }
    }
}

namespace DotNet_Backend
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [Route("/api/v1/[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public ActionResult SayHello() => base.Ok("Hello, World!".Split(" "));
    }

}