using ERP.API.Hubs;
using ERP.Application.Options;
using ERP.Application.Services.Approval;
using ERP.Domain.Entities.Approval;
using ERP.Domain.Entities.System;
using ERP.Domain.Enums.Approval;
using ERP.Domain.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ERP.API.Services;

public sealed class ApprovalNotificationService(
    IUnitOfWork unitOfWork,
    IOptions<SmtpSettings> smtpOptions,
    IHubContext<ApprovalHub> hubContext,
    ILogger<ApprovalNotificationService> logger) : IApprovalNotificationService
{
    private readonly SmtpSettings _smtp = smtpOptions.Value;

    public async Task NotifyAsync(int requestId, int? stepId, int recipientUserId, ApprovalNotificationType type, string subject, string body, CancellationToken ct = default)
    {
        var recipient = await unitOfWork.Repository<SysUser>().GetByIdAsync(recipientUserId, ct);
        if (recipient is null)
        {
            logger.LogWarning("Cannot send approval notification: recipient user {UserId} not found.", recipientUserId);
            return;
        }

        var notification = new ApprovalNotification
        {
            RequestId = requestId,
            StepId = stepId,
            RecipientUserId = recipientUserId,
            NotificationType = type,
            Channel = ApprovalNotificationChannel.Both,
            Subject = subject,
            Body = body,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<ApprovalNotification>().AddAsync(notification, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await PushInAppAsync(recipientUserId, notification, ct);

        if (_smtp.Enabled && !string.IsNullOrWhiteSpace(recipient.Email))
        {
            await SendEmailAsync(notification, recipient.Email, recipient.FullName, ct);
        }
    }

    private async Task PushInAppAsync(int recipientUserId, ApprovalNotification notification, CancellationToken ct)
    {
        try
        {
            await hubContext.Clients.Group(ApprovalHub.GroupName(recipientUserId)).SendAsync("ReceiveApprovalNotification", new
            {
                notification.Id,
                notification.RequestId,
                notification.StepId,
                notification.NotificationType,
                notification.Subject,
                notification.Body,
                notification.CreatedAt
            }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to push SignalR approval notification {NotificationId} to user {UserId}.", notification.Id, recipientUserId);
        }
    }

    private async Task SendEmailAsync(ApprovalNotification notification, string recipientEmail, string recipientName, CancellationToken ct)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = notification.Subject;
            message.Body = new TextPart("plain") { Text = notification.Body };

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtp.Host, _smtp.Port, _smtp.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);

            if (!string.IsNullOrWhiteSpace(_smtp.Username))
            {
                await client.AuthenticateAsync(_smtp.Username, _smtp.Password, ct);
            }

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            notification.SentAt = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send approval notification email {NotificationId} to {Email}.", notification.Id, recipientEmail);
            notification.FailedAt = DateTimeOffset.UtcNow;
            notification.RetryCount += 1;
        }

        unitOfWork.Repository<ApprovalNotification>().Update(notification);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
