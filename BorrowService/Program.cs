using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

//using Microsoft.OpenApi.Models;

namespace BorrowService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controller servislerini ekle
            builder.Services.AddControllers();

            // Swagger servislerini ekle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BorrowService API",
                    Version = "v1"
                });
            });

            var app = builder.Build();

            // Swagger middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BorrowService API v1");
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Controller map
            app.MapControllers();

            app.Run();
        }
    }
}
