using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NewsletterProvider.Functions;

public class GetSubscribers(ILogger<GetSubscribers> logger, DataContext context)
{
    private readonly ILogger<GetSubscribers> _logger = logger;
    private readonly DataContext _context = context;

    [Function("GetSubscribers")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        var subscribers = _context.Subscribers.ToList();
        return new OkObjectResult(subscribers);
    }
}
