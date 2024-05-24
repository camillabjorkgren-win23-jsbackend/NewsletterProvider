using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NewsletterProvider.Functions
{
    public class UpdateSubscriber(ILogger<UpdateSubscriber> logger, DataContext context)
    {
        private readonly ILogger<UpdateSubscriber> _logger = logger;
        private readonly DataContext _context = context;

        [Function("UpdateSubscriber")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function,  "post")] HttpRequest req)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(body))
            {
                var subscriber = JsonConvert.DeserializeObject<SubscribeEntity>(body);
                if (subscriber != null)
                {
                    var existingSubscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == subscriber.Email);
                    if (existingSubscriber != null)
                    {
                        _context.Entry(existingSubscriber).CurrentValues.SetValues(subscriber);
                        await _context.SaveChangesAsync();
                        return new OkObjectResult(new { Status = 200, Message = "Subscriber was updated" });
                    }
                }
                return new NotFoundObjectResult(new { Status = 404, Message = "Subscriber not found in database." });
            }
            return new BadRequestObjectResult(new { Status = 400, Message = "Could not update subscriber" });
        }
    }
}
