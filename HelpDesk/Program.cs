using HelpDesk.Configuration;
using HelpDesk.Data;
using HelpDesk.Repositories;
using HelpDesk.Repositories.Interfaces;
using HelpDesk.Services;
using HelpDesk.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DbHelper>();

// ---------- Repositories ----------
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IGeneralStaffRepository, GeneralStaffRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketAssignmentRepository, TicketAssignmentRepository>();
builder.Services.AddScoped<IUserSupportStaffRepository, UserSupportStaffRepository>();
builder.Services.AddScoped<IItAdminRepository, ItAdminRepository>();
builder.Services.AddScoped<IUSSTicketTaskRepository, USSTicketTaskRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<ILookupRepository,          LookupRepository>();
builder.Services.AddScoped<ITechnicianAdminRepository, TechnicianAdminRepository>();
builder.Services.AddScoped<ITicketActivityRepository, TicketActivityRepository>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

// ---------- Services ----------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ITechnicianService, TechnicianService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IAdminUsersService, AdminUsersService>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IActivityService, ActivityService>();

// ---------- CORS ----------
builder.Services.AddCors(o => o.AddPolicy("AllowFrontend",
    p => p.AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowAnyMethod()));

var app = builder.Build();

// ---------- Middleware ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// ---------- Frontend ----------
var frontendPath = Path.Combine(
    app.Environment.ContentRootPath,
    "frontend"
);

app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath)
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath)
});

// ---------- API ----------
app.UseAuthorization();

app.MapControllers();

app.Run();