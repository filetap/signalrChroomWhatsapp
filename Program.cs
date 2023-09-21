
using ChatRoom.Hubs;
using ChatRoom.Jobs;
using ChatRoom.Models;
using ChatRoom.Models.MessageContext;
using ChatRoom.Services.ContactService;
using ChatRoom.Services.FileConverter;
using ChatRoom.Services.GroupMessageService;
using ChatRoom.Services.GroupService;
using ChatRoom.Services.MessageDocumentService;
using ChatRoom.Services.MessagePaaswordService;
using ChatRoom.Services.MessageService;
using ChatRoom.Services.PersonGroupService;
using ChatRoom.Services.PersonService;
using ChatRoom.Services.UserSeenService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;

namespace ChatRoom
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			ConfigurationManager configuration = builder.Configuration; // allows both to access and to set up the config
			var mongoDbSettings = configuration.GetSection(nameof(MessageDbContext)).Get<MessageDbContext>();


			builder.Services.Configure<MessageDbContext>(
			builder.Configuration.GetSection("MessageDbContext"));

			builder.Services.AddIdentity<Person, ApplicationRole>(options =>
			{
				options.Password.RequiredUniqueChars = 0;
				options.Password.RequireDigit = true;
				options.Password.RequiredLength = 6;
				options.Password.RequireLowercase = true;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
				options.User.RequireUniqueEmail = false;
				options.User.AllowedUserNameCharacters =
				  "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$٪^*()_-+=";
				options.Lockout.AllowedForNewUsers = true;
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
				options.Lockout.MaxFailedAccessAttempts = 3;


			}).AddMongoDbStores<Person, ApplicationRole, Guid>
			 (
				 mongoDbSettings.ConnectionString, mongoDbSettings.Name
			 ).AddDefaultTokenProviders();


			//get current user in   Method


			builder.Services.AddResponseCaching();
			builder.Services.AddMemoryCache();
			// Add services to the container.

			builder.Services.AddControllersWithViews()
				 .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

			builder.Services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();



			//   builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
			//.AddNegotiate();
			//   builder.Services.AddAuthorization(options =>
			//   {
			//       options.FallbackPolicy = options.DefaultPolicy;
			//   });
			//   // builder.Services.AddSignalR();
			//   builder.Services.Configure<CookiePolicyOptions>(options =>
			//   {
			//       options.MinimumSameSitePolicy = SameSiteMode.Strict;
			//       options.OnAppendCookie = cookieContext =>
			//           CheckSameSite(cookieContext.Context,
			//                         cookieContext.CookieOptions);
			//       options.OnDeleteCookie = cookieContext =>
			//           CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
			//   });

			builder.Services.Configure<MvcOptions>(options =>
			{
				//  options.Filters.Add(new RequireHttpsAttribute());

			});

			builder.Services.AddDistributedMemoryCache();

			builder.Services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromDays(30);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
				// op.Cookie.Domain = "aftokhabar.ir";

			});

			builder.Services.AddSignalR();
			

			builder.Services.ConfigureApplicationCookie(options =>
			{


				options.ExpireTimeSpan = TimeSpan.FromDays(30);
				options.AccessDeniedPath = "/AccessDenied.html";
				options.LoginPath = "/Login";

				options.SlidingExpiration = true;

			});



			builder.Services.AddScoped<IPersonService, PersonService>();
			builder.Services.AddScoped<IMessageService, MessageService>();
			builder.Services.AddScoped<IGroupService, GroupService>();
			builder.Services.AddScoped<IMessageDocumentService, MessageDocumentService>();
			builder.Services.AddScoped<IGroupMessageService, GroupMessageService>();
			builder.Services.AddScoped<IPersonGroupService, PersonGroupService>();
			builder.Services.AddScoped<IContactService, ContactService>();
			builder.Services.AddScoped<IUserSeenService, UserSeenService>();
			builder.Services.AddScoped<IFileConverterService, FileConverterService>();
			builder.Services.AddScoped<IMessagePaaswordService, MessagePaaswordService>();



			builder.Services.AddQuartz(q =>
			{
				//q.UseMicrosoftDependencyInjectionScopedJobFactory();
				// Just use the name of your job that you created in the Jobs folder.
				var jobKey = new JobKey("UpdateUserSeenJob");
				q.AddJob<UpdateUserSeenJob>(opts => opts.WithIdentity(jobKey));

				q.AddTrigger(opts => opts
					.ForJob(jobKey)
					.WithIdentity("UpdateUserSeenJob-trigger")
					 .WithCronSchedule("0/5 * * * * ?")

				);
			});
			//builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			//استفاده از احراز هویت
			app.UseAuthentication();
			//استفاده از مجوز
			app.UseAuthorization();

			app.UseResponseCaching();

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
			app.MapHub<ChatHub>("/chatHub");
			app.Run();
		}

		private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
		{
			if (options.SameSite == SameSiteMode.None)
			{
				var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

			}
		}

	}
}