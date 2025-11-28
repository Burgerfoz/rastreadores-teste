using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RastreadoresAPI.Services;

namespace RastreadoresAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            // Swagger pra testar se precisar
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RastreadoresAPI", Version = "v1" });
            });

            // Injetando direto as classes (sem Interface pra nao complicar agora)
            services.AddScoped<FileService>();
            services.AddScoped<EmailService>();

            // Libera geral o CORS pro front local
            services.AddCors(o => o.AddPolicy("TudoLiberado", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RastreadoresAPI v1"));
            }

            app.UseRouting();

            // Importante: CORS antes de Auth e Endpoints
            app.UseCors("TudoLiberado");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}