using Microsoft.AspNetCore.Razor.TagHelpers;
using Wombat.Infrastructure;
using Serilog;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Wombat.Web.Host.Filters;
using Newtonsoft.Json;
using Microsoft.AspNetCore.HttpOverrides;
using Wombat.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Reflection;
using NSwag;
using NSwag.Generation.Processors.Security;
using Wombat.Core;

namespace Wombat.Web.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LogHelper.Build();

            var builder = WebApplication.CreateBuilder(args);


            // log 日志配置
            builder.Host.UseSerilog();


            builder.Services.AddLogging(build =>
            {
                object p = build.AddSerilog(logger: LogHelper.Log);
            });

            //builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);


            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidFilterAttribute>();
                options.Filters.Add<GlobalExceptionFilter>();
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                //小驼峰命名
                options.SerializerSettings.ContractResolver = new LowercaseContractResolver();
                options.SerializerSettings.GetType().GetProperties().ForEach(aProperty =>
                {
                    var value = aProperty.GetValue(JsonExtensions.DefaultJsonSetting);
                    aProperty.SetValue(options.SerializerSettings, value);
                });
            });

            //上下文注入
            builder.Services.AddHttpContextAccessor();


            #region HttpContext、IMemoryCache

            builder.Host.UseIdHelper();

            builder.Host.UseCache();

            #endregion



            //// Add services to the container.
            //builder.Services.AddAuthorization();


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            // Register the Swagger generator, defining 1 or more Swagger documents
            #region 添加Swagger
            builder.Services.AddOpenApiDocument(config =>
            {
                config.Title = "Wombat.Web.Host";
                config.Description = "API";
                config.AllowReferencesWithProperties = true;
                config.AddSecurity("Token", Enumerable.Empty<string>(), new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    //参数添加在头部
                    In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                    //使用Authorize头部
                    //Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                    Type = OpenApiSecuritySchemeType.Http,

                    //内容为以 bearer开头
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

            });

            builder.Services.AddJwt(builder.Configuration);

            #endregion




            builder.Services.AddServicesPoxy();

            
            //builder.Services.


            builder.Configuration.UseCustomConfigurationProvider();


            var app = builder.Build();


            app.Services.UseCustomServiceProvider();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //app.UseSwagger();
                //app.UseSwaggerUI();
            }

            app.UseCors(x =>
            {  x.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .DisallowCredentials();
            }).UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMiddleware<RequestBodyMiddleware>();
            app.UseMiddleware<RequestLogMiddleware>();
            app.UseDeveloperExceptionPage()
                .UseStaticFiles(new StaticFileOptions
                {
                    ServeUnknownFileTypes = true,
                    DefaultContentType = "application/octet-stream"
                })
                .UseRouting();



            app.UseHttpsRedirection(); //重定向到Https

            app.UseAuthentication();
            app.UseAuthorization();

            ///匹配路由
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization();
            });

            #region 使用Swagger
            app.UseOpenApi();//添加swagger生成api文档（默认路由文档 /swagger/v1/swagger.json）
            app.UseSwaggerUi3(config =>
            {
            });//添加Swagger UI到请求管道中(默认路由: /swagger).
            #endregion

            app.Run();
        }
    }
}