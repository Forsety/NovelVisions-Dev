using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NovelVision.Services.Catalog.Application.Common.Interfaces;

namespace NovelVision.Services.Catalog.Infrastructure.Services.Email;

/// <summary>
/// Extension methods for IEmailService to support Identity operations
/// </summary>
public static class EmailServiceExtensions
{
    /// <summary>
    /// Sends email confirmation link to user
    /// </summary>
    public static async Task SendEmailConfirmationAsync(
        this IEmailService emailService,
        string email,
        string confirmationToken,
        IConfiguration configuration = null,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration?["Application:BaseUrl"] ?? "https://localhost:5001";
        var confirmationLink = $"{baseUrl}/api/v1/auth/confirm-email?token={Uri.EscapeDataString(confirmationToken)}&email={Uri.EscapeDataString(email)}";

        var subject = "Confirm Your Email - NovelVision";
        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; }}
                    .content {{ background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-radius: 0 0 10px 10px; }}
                    .button {{ display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .footer {{ text-align: center; color: #666; margin-top: 20px; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Welcome to NovelVision!</h1>
                    </div>
                    <div class='content'>
                        <h2>Confirm Your Email Address</h2>
                        <p>Thank you for registering with NovelVision. To complete your registration and access all features, please confirm your email address by clicking the button below:</p>
                        <a href='{confirmationLink}' class='button'>Confirm Email</a>
                        <p>Or copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #667eea;'>{confirmationLink}</p>
                        <p><strong>This link will expire in 24 hours.</strong></p>
                        <p>If you didn't create an account with NovelVision, you can safely ignore this email.</p>
                    </div>
                    <div class='footer'>
                        <p>© 2025 NovelVision. All rights reserved.</p>
                        <p>This is an automated message, please do not reply.</p>
                    </div>
                </div>
            </body>
            </html>";

        await emailService.SendEmailAsync(email, subject, body, true, cancellationToken);
    }

    /// <summary>
    /// Sends password reset link to user
    /// </summary>
    public static async Task SendPasswordResetAsync(
        this IEmailService emailService,
        string email,
        string resetToken,
        IConfiguration configuration = null,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration?["Application:BaseUrl"] ?? "https://localhost:5001";
        var resetLink = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(email)}";

        var subject = "Reset Your Password - NovelVision";
        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; }}
                    .content {{ background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-radius: 0 0 10px 10px; }}
                    .button {{ display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .warning {{ background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                    .footer {{ text-align: center; color: #666; margin-top: 20px; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Password Reset Request</h1>
                    </div>
                    <div class='content'>
                        <h2>Reset Your Password</h2>
                        <p>We received a request to reset your password for your NovelVision account. Click the button below to create a new password:</p>
                        <a href='{resetLink}' class='button'>Reset Password</a>
                        <p>Or copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #f5576c;'>{resetLink}</p>
                        <div class='warning'>
                            <strong>⚠️ Important:</strong>
                            <ul>
                                <li>This link will expire in 2 hours</li>
                                <li>For security reasons, this link can only be used once</li>
                                <li>If you didn't request a password reset, please ignore this email and your password will remain unchanged</li>
                            </ul>
                        </div>
                        <p>For additional security, we recommend:</p>
                        <ul>
                            <li>Using a strong, unique password</li>
                            <li>Enabling two-factor authentication</li>
                            <li>Not sharing your password with anyone</li>
                        </ul>
                    </div>
                    <div class='footer'>
                        <p>© 2025 NovelVision. All rights reserved.</p>
                        <p>This is an automated security message, please do not reply.</p>
                    </div>
                </div>
            </body>
            </html>";

        await emailService.SendEmailAsync(email, subject, body, true, cancellationToken);
    }

    /// <summary>
    /// Sends two-factor authentication code
    /// </summary>
    public static async Task SendTwoFactorCodeAsync(
        this IEmailService emailService,
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        var subject = "Your Two-Factor Authentication Code - NovelVision";
        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; }}
                    .content {{ background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-radius: 0 0 10px 10px; }}
                    .code-box {{ background: #f8f9fa; border: 2px dashed #4facfe; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; margin: 20px 0; border-radius: 5px; }}
                    .footer {{ text-align: center; color: #666; margin-top: 20px; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Two-Factor Authentication</h1>
                    </div>
                    <div class='content'>
                        <h2>Your Verification Code</h2>
                        <p>Enter this code to complete your login:</p>
                        <div class='code-box'>{code}</div>
                        <p><strong>This code will expire in 5 minutes.</strong></p>
                        <p>If you didn't attempt to log in to your NovelVision account, please change your password immediately.</p>
                    </div>
                    <div class='footer'>
                        <p>© 2025 NovelVision. All rights reserved.</p>
                        <p>This is an automated security message, please do not reply.</p>
                    </div>
                </div>
            </body>
            </html>";

        await emailService.SendEmailAsync(email, subject, body, true, cancellationToken);
    }
}
