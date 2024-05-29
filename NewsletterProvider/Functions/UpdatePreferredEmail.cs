using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NewsletterProvider.Functions;

public class UpdatePreferredEmail(ILogger<UpdatePreferredEmail> logger, DataContext context)
{
    private readonly ILogger<UpdatePreferredEmail> _logger = logger;
    private readonly DataContext _context = context;

    [Function("UpdatePreferredEmail")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
      try
        {
            var updatePreferredEmail = req.ReadFromJsonAsync<UpdatePreferredEmailModel>().Result;
            if (updatePreferredEmail != null)
            {
                var existingSubscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == updatePreferredEmail.OldEmail);
                if (existingSubscriber != null)
                {
                    existingSubscriber.Email = updatePreferredEmail.PreferredEmail;
                    await _context.SaveChangesAsync();
                    return new OkObjectResult(new { Status = 200, Message = "Preferred email was updated" });
                }
                return new NotFoundObjectResult(new { Status = 404, Message = "Subscriber not found in database." });
            }
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating preferred email");
            return new BadRequestObjectResult(new { Status = 400, Message = "Could not update preferred email" });
        }
        return new BadRequestObjectResult(new { Status = 400, Message = "Could not update preferred email" });

    }
}

public class UpdatePreferredEmailModel
{
    public string OldEmail { get; set; } = null!;
    public string PreferredEmail { get; set; } = null!;
}