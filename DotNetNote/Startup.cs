using DotNetNote.Common;
using DotNetNote.Components;
using DotNetNote.Models;
using DotNetNote.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNetNote
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
            services.AddControllersWithViews();

            //[!] Configuration: JSON ������ �����͸� POCO Ŭ������ ����
            services.Configure<DotNetNoteSettings>(Configuration.GetSection("DotNetNoteSettings"));

            services.AddAuthentication("Cookies")
                .AddCookie(options =>
                {
                    options.LoginPath = "/User/Login/";
                    options.AccessDeniedPath = "/User/Forbidden/";
                });

            //[DI] ������ ����(Dependency Injection)
            DependencyInjectionContainer(services);
        }

        /// <summary>
        /// ������ ���� ���� �ڵ常 ���� ��Ƽ� ����
        /// - �������丮 ���
        /// </summary>
        private void DependencyInjectionContainer(IServiceCollection services)
        {
            //[?] ConfigureServices�� ȣ��Ǳ� ������ DI(���Ӽ� ����)�� �������� �ʽ��ϴ�.

            //[DNN][!] Configuration ��ü ����: 
            //    IConfiguration �Ǵ� IConfigurationRoot�� Configuration ��ü ����
            //    appsettings.json ������ �����ͺ��̽� ���� ���ڿ��� 
            //    �������丮 Ŭ�������� ����� �� �ֵ��� ����
            // IConfiguration ���� -> Configuration�� �ν��Ͻ��� ���� 
            services.AddSingleton<IConfiguration>(Configuration);

            //[User][5] ȸ�� ����
            services.AddTransient<IUserRepository, UserRepository>();
            // LoginFailedManager
            services.AddTransient<ILoginFailedRepository, LoginFailedRepository>();
            services.AddTransient<ILoginFailedManager, LoginFailedManager>();
            // ����� ���� ���� ���� ������Ʈ
            services.AddTransient<IUserModelRepository, UserModelRepository>();

            //[User][9] Policy ����
            services.AddAuthorization(options =>
            {
                // Users Role�� ������, Users Policy �ο�
                options.AddPolicy("Users", policy => policy.RequireRole("Users"));

                // Users Role�� �ְ� UserId�� DotNetNoteSettings:SiteAdmin�� ������ ��(���� ��� "Admin")�̸� "Administrators" �ο�
                // "UserId" - ��ҹ��� ����
                options.AddPolicy("Administrators", policy => policy.RequireRole("Users").RequireClaim("UserId", Configuration.GetSection("DotNetNoteSettings").GetSection("SiteAdmin").Value));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); 
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
