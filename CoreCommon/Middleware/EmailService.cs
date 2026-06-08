using CoreCommon.Models.EmailSettings;
using CoreCommon.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace CoreCommon.Middleware
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<ResultData<string>> SendOtpEmailAsync(string toEmail, string userName, string otpCode)
        {
            try
            {
                var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_settings.SenderEmail, _settings.AppPassword),
                    EnableSsl = true
                };

                var mail = new MailMessage(_settings.SenderEmail, toEmail)
                {
                    Subject = "Your FBChat AI Verification Code",
                    Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 500px; margin: auto;'>
                        <div style='background: linear-gradient(to right, #2563eb, #7c3aed); 
                                    padding: 20px; text-align: center;'>
                            <h1 style='color: white; margin: 0;'>FBChat AI</h1>
                        </div>
                        <div style='padding: 30px; background: #f9f9ff;'>
                            <h2 style='color: #1e1e1e;'>Hi {userName},</h2>
                            <p style='color: #555;'>Your verification code is:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <span style='font-size: 36px; font-weight: bold;
                                             letter-spacing: 10px; color: #7c3aed;'>
                                    {otpCode}
                                </span>
                            </div>
                            <p style='color: #555;'>
                                This code expires in <strong>10 minutes</strong>.
                            </p>
                            <p style='color: #999; font-size: 12px;'>
                                If you did not request this, please ignore this email.
                            </p>
                        </div>
                        <div style='background: #eee; padding: 10px; text-align: center;'>
                            <p style='color: #999; font-size: 12px; margin: 0;'>
                                © 2026 FBChat AI
                            </p>
                        </div>
                    </div>",
                    IsBodyHtml = true   // ✅ important for HTML email
                };

                await client.SendMailAsync(mail);
                _logger.LogInformation("[EmailService] OTP sent to {Email}", toEmail);
                return ResultData<string>.Ok("OTP sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmailService] Failed to send OTP to {Email}", toEmail);
                return ResultData<string>.Fail("Failed to send OTP");
            }
        }
    }
}
