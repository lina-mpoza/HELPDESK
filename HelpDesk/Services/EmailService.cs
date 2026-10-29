using HelpDesk.Configuration;
using HelpDesk.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HelpDesk.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtp;

    public EmailService(IOptions<SmtpSettings> smtp)
    {
        _smtp = smtp.Value;
    }

    public async Task SendTechnicianCredentialsAsync(
        string toEmail,
        string fullName,
        string username,
        string password,
        string staffNo,
        string officeLocation)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
        message.To.Add(new MailboxAddress(fullName, toEmail));
        message.Subject = "Your IT Helpdesk account credentials";

        var html = $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Segoe UI, Roboto, Arial, sans-serif; color:#1a2c3e; line-height:1.6;'>
                <div style='max-width:560px; margin:0 auto; padding:24px;'>
                    <h2 style='color:#0b7a3b; margin-bottom:8px;'>Welcome, {fullName}!</h2>
                    <p>Your IT Helpdesk account has been created by the Helpdesk Manager.</p>

                    <table style='margin:20px 0; border-collapse:collapse;'>
                        <tr>
                            <td style='padding:6px 12px 6px 0; font-weight:600;'>Username:</td>
                            <td style='padding:6px 0;'>{username}</td>
                        </tr>
                        <tr>
                            <td style='padding:6px 12px 6px 0; font-weight:600;'>Password:</td>
                            <td style='padding:6px 0;'>{password}</td>
                        </tr>
                        <tr>
                            <td style='padding:6px 12px 6px 0; font-weight:600;'>Staff Number:</td>
                            <td style='padding:6px 0;'>{staffNo}</td>
                        </tr>
                        <tr>
                            <td style='padding:6px 12px 6px 0; font-weight:600;'>Office:</td>
                            <td style='padding:6px 0;'>{officeLocation}</td>
                        </tr>
                    </table>

                    <p>Please change your password after your first sign-in.</p>
                    <p style='margin-top:24px; font-size:13px; color:#888;'>
                        O.R. Tambo District Municipality — IT Helpdesk
                    </p>
                </div>
            </body>
            </html>";

        message.Body = new TextPart("html") { Text = html };

        await SendAsync(message);
    }

    public async Task SendPasswordResetOtpAsync(string toEmail, string fullName, string otp)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
        message.To.Add(new MailboxAddress(fullName, toEmail));
        message.Subject = "Your password reset code — IT Helpdesk";

        var html = $@"
        <!DOCTYPE html>
        <html>
        <body style='font-family: Segoe UI, Roboto, Arial, sans-serif; color:#1a2c3e; line-height:1.6;'>
            <div style='max-width:560px; margin:0 auto; padding:24px;'>
                <h2 style='color:#0b7a3b; margin-bottom:8px;'>Password reset request</h2>
                <p>Hi {fullName},</p>
                <p>We received a request to reset your IT Helpdesk password.</p>

                <p style='margin:24px 0;'>
                    Your one-time code is:
                    <span style='display:inline-block; margin-left:8px; padding:8px 16px; background:#0b7a3b; color:#fff; font-size:22px; font-weight:800; border-radius:6px; letter-spacing:4px;'>{otp}</span>
                </p>

                <p>This code <strong>expires in 10 minutes</strong>.</p>
                <p>If you didn't request a password reset, you can safely ignore this email — your password will stay the same.</p>

                <p style='margin-top:24px; font-size:13px; color:#888;'>
                    O.R. Tambo District Municipality — IT Helpdesk
                </p>
            </div>
        </body>
        </html>";

        message.Body = new TextPart("html") { Text = html };

        await SendAsync(message);
    }

    // Shared send routine.
    private async Task SendAsync(MimeMessage message)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        using var client = new SmtpClient();
        client.Timeout = 12_000;

        // Port 465 => TLS from the first byte (SslOnConnect)
        // Port 587 => plain-text first, then upgrade (StartTls)
        // Pick the option based on the configured port.
        var secureOption = _smtp.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(_smtp.Host, _smtp.Port, secureOption, cts.Token);
        await client.AuthenticateAsync(_smtp.Username, _smtp.Password, cts.Token);
        await client.SendAsync(message, cts.Token);
        await client.DisconnectAsync(true, cts.Token);
    }
}