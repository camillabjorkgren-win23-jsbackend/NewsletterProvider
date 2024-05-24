using Data.Contexts;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NewsletterProvider.Functions;

public class DeleteSubscriber(ILogger<DeleteSubscriber> logger, DataContext context)
{
    private readonly ILogger<DeleteSubscriber> _logger = logger;
    private readonly DataContext _context = context;

    [Function("DeleteSubscriber")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        if (!string.IsNullOrWhiteSpace(body))
        {
            var subscriber = JsonConvert.DeserializeObject<Subscriber>(body);
            if (subscriber != null)
            {
                var existingSubscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == subscriber.Email);
                if (existingSubscriber != null)
                {
                    _context.Remove(existingSubscriber);
                    await _context.SaveChangesAsync();
                    return new OkObjectResult(new { Status = 200, Message = "Subscriber was unsubscribed" });
                }
            }
            return new NotFoundObjectResult(new { Status = 404, Message = "Subscriber not found in database." });
        }

        _logger.LogError("Unable to delete subscriber");
        return new BadRequestObjectResult(new { Status = 400, Message = "Unable to delete subscriber right now." });
    }
}