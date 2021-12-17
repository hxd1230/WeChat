using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;

namespace WeChat.UI
{
    public class Startup
    {
        public static ILoggerRepository respository { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            respository = LogManager.CreateRepository("netRespository");
            XmlConfigurator.Configure(respository, new FileInfo("log4net.config"));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddCors(c =>
            //{
            //    c.AddPolicy("cors", builder =>
            //    {
            //        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            //    });
            //});
            //services.AddMemoryCache();
            //��Redis�ֲ�ʽ���������ӵ�������
            //services.AddDistributedRedisCache(options =>
            //{
            //    //��������Redis������  Configuration.GetConnectionString("RedisConnectionString")��ȡ������Ϣ�Ĵ�
            //    options.Configuration = "175.24.32.233:6379";// Configuration.GetConnectionString("RedisConnectionString");
            //    //Redisʵ����RedisDistributedCache
            //    options.InstanceName = "RedisDistributedCache";
            //});
            services.AddSingleton(new RedisHelper("175.24.32.233:6379", ""));
            services.Configure<KestrelServerOptions>(options => options.AllowSynchronousIO = true);
            services.AddLogging();
            services.AddControllersWithViews();
            #region Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v0.1.1",
                    Title = "WeChat API",
                    Description = "΢�Žӿ�˵���ĵ�",
                    TermsOfService = new Uri("http://www.baidu.com/"),
                    Contact = new OpenApiContact
                    {
                        Email = "675556820@qq.com",
                        Name = "hexiaodond.top",
                        Url = new Uri("http://www.hexiaodong.top"),
                        Extensions = null
                    }
                });
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "WeChat.UI.xml");//������Ǹո����õ�xml�ļ���
                c.IncludeXmlComments(xmlPath, true);
            });

            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseOptions(); // �ڷ����ʼ��λ�üӣ�����λ��û���ԡ�
            app.UseDeveloperExceptionPage();
            #region Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiHelp V1");
            });
            #endregion

            app.UseStaticFiles();
            app.UseRouting();
            //app.UseCors("cors");
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "admin",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");//.RequireCors("cors");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");//.RequireCors("cors");
            });
        }
    }
}
