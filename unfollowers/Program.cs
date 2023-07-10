using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using unfollowers.Data;
using unfollowers.Services.CronService;
using unfollowers.Services.InstagramService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddDbContext<ApplicationDbContext>(
       options => options.UseSqlServer(builder.Configuration.GetValue<string>("ConnectionStrings:unfollowers")));


builder.Services.AddHangfire(opt => opt.UseSqlServerStorage(builder.Configuration.GetValue<string>("ConnectionStrings:unfollowers")));

var sqlStorage = new SqlServerStorage(builder.Configuration.GetValue<string>("ConnectionStrings:unfollowers"));
JobStorage.Current = sqlStorage;


builder.Services.AddHangfireServer(backgroundJobServerOptions =>
{
    backgroundJobServerOptions.SchedulePollingInterval = TimeSpan.FromSeconds(10);
    backgroundJobServerOptions.WorkerCount = 12;
});



builder.Services.AddSingleton<IInstaService, InstaService>();
builder.Services.AddTransient<ICronService, CronService>();



var app = builder.Build();

//RecurringJob.AddOrUpdate<CronService>(RecurringJobId.checkUnfollowers.ToString(), job => job.CheckNewUnfollowers(), "*/10 * * * *", TimeZoneInfo.Utc);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());


app.UseAuthorization();

app.MapControllers();

app.Run();
