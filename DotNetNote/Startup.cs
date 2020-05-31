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

            //[!] Configuration: JSON 파일의 데이터를 POCO 클래스에 주입
            services.Configure<DotNetNoteSettings>(Configuration.GetSection("DotNetNoteSettings"));

            services.AddAuthentication("Cookies")
                .AddCookie(options =>
                {
                    options.LoginPath = "/User/Login/";
                    options.AccessDeniedPath = "/User/Forbidden/";
                });

            //[DI] 의존성 주입(Dependency Injection)
            DependencyInjectionContainer(services);
        }

        /// <summary>
        /// 의존성 주입 관련 코드만 따로 모아서 관리
        /// - 리포지토리 등록
        /// </summary>
        private void DependencyInjectionContainer(IServiceCollection services)
        {
            //[?] ConfigureServices가 호출되기 전에는 DI(종속성 주입)가 설정되지 않습니다.

            //[DNN][!] Configuration 개체 주입: 
            //    IConfiguration 또는 IConfigurationRoot에 Configuration 개체 전달
            //    appsettings.json 파일의 데이터베이스 연결 문자열을 
            //    리포지토리 클래스에서 사용할 수 있도록 설정
            // IConfiguration 주입 -> Configuration의 인스턴스를 실행 
            services.AddSingleton<IConfiguration>(Configuration);

            //[User][5] 회원 관리
            services.AddTransient<IUserRepository, UserRepository>();
            // LoginFailedManager
            services.AddTransient<ILoginFailedRepository, LoginFailedRepository>();
            services.AddTransient<ILoginFailedManager, LoginFailedManager>();
            // 사용자 정보 보기 전용 컴포넌트
            services.AddTransient<IUserModelRepository, UserModelRepository>();

            //[User][9] Policy 설정
            services.AddAuthorization(options =>
            {
                // Users Role이 있으면, Users Policy 부여
                options.AddPolicy("Users", policy => policy.RequireRole("Users"));

                // Users Role이 있고 UserId가 DotNetNoteSettings:SiteAdmin에 지정된 값(예를 들어 "Admin")이면 "Administrators" 부여
                // "UserId" - 대소문자 구분
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
