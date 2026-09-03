using FormBuilder.Application.Common;
using FormBuilder.Domain.Enums;
using FormBuilder.Domain.FormTemplates;
using FormBuilder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Api.Infrastructure;

/// <summary>Inserts one sample template on first run in Development, so the UI has something to show.</summary>
internal static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await context.FormTemplates.AnyAsync())
        {
            return;
        }

        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var template = new FormTemplate(
            "בקשת חופשה",
            "בקשת חופשה שנתית לכלל העובדים. מנותבת למנהל הישיר ולאחר מכן למשאבי אנוש.",
            "system",
            clock.UtcNow);

        template.AddField("שם העובד", FieldType.Text, isRequired: true, placeholder: "דנה לוי");
        template.AddField("תאריך התחלה", FieldType.Date, isRequired: true);
        template.AddField("מספר ימים", FieldType.Number, isRequired: true, placeholder: "5");

        template.AddApprovalStep("אישור מנהל ישיר", ApprovalActionType.Approve);
        template.AddApprovalStep("אימות משאבי אנוש", ApprovalActionType.Sign, "hr@tax.gov.il");

        template.Publish();

        context.FormTemplates.Add(template);
        await context.SaveChangesAsync();
    }
}
