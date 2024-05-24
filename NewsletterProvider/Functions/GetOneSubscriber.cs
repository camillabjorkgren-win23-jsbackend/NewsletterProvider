using Data.Contexts;
using Data.Entities;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NewsletterProvider.Functions;

public class GetOneSubscriber(ILogger<GetOneSubscriber> logger, DataContext context)
{
    private readonly ILogger<GetOneSubscriber> _logger = logger;
    private readonly DataContext _context = context;

    [Function("GetOneSubscriber")]
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
                    return new OkObjectResult(existingSubscriber);
                }
            }
            return new NotFoundObjectResult(new { Status = 404, Message = "Subscriber not found in database." });
        }
        return new BadRequestObjectResult(new { Status = 400, Message = "Subscriber could not be sent." });
    }
}
