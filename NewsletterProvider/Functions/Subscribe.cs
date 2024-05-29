using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NewsletterProvider.Functions;

public class Subscribe(ILogger<Subscribe> logger, DataContext context)
{
    private readonly ILogger<Subscribe> _logger = logger;
    private readonly DataContext _context = context;

    [Function("Subscribe")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
       var body = await new StreamReader(req.Body).ReadToEndAsync();
        if (!string.IsNullOrWhiteSpace(body))
        {
            var subscriber = JsonConvert.DeserializeObject<SubscribeToNewsletter>(body);
            if(subscriber != null)
            {
                var existingSubscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == subscriber.UserEmail || s.Email == subscriber.PreferredEmail);
                if(existingSubscriber != null)
                {
                    if(subscriber.PreferredEmail != null)
                    {
                        var updatedSubscriber = new SubscribeEntity
                        {
                            Email = subscriber.PreferredEmail,
                            AdvertisingUpdates = subscriber.AdvertisingUpdates,
                            WeekInReview = subscriber.WeekInReview,
                            Podcasts = subscriber.Podcasts,
                            StartupsWeekly = subscriber.StartupsWeekly,
                            DailyNewsletter = subscriber.DailyNewsletter,
                            EventUpdates = subscriber.EventUpdates
                        };
                        _context.Subscribers.Remove(existingSubscriber);
                        _context.Subscribers.Add(updatedSubscriber);
                    }
                    else
                    {
                        existingSubscriber.AdvertisingUpdates = subscriber.AdvertisingUpdates;
                        existingSubscriber.WeekInReview = subscriber.WeekInReview;
                        existingSubscriber.Podcasts = subscriber.Podcasts;
                        existingSubscriber.StartupsWeekly = subscriber.StartupsWeekly;
                        existingSubscriber.DailyNewsletter = subscriber.DailyNewsletter;
                        existingSubscriber.EventUpdates = subscriber.EventUpdates;
                    }

                    await _context.SaveChangesAsync();
                    return new OkObjectResult(new { Status = 200, Message = "Subscriber was updated" });
                }
                if(subscriber.PreferredEmail != null)
                {
                    SubscribeEntity subscribeEntity = new SubscribeEntity
                    {
                        Email = subscriber.PreferredEmail,
                        AdvertisingUpdates = subscriber.AdvertisingUpdates,
                        WeekInReview = subscriber.WeekInReview,
                        Podcasts = subscriber.Podcasts,
                        StartupsWeekly = subscriber.StartupsWeekly,
                        DailyNewsletter = subscriber.DailyNewsletter,
                        EventUpdates = subscriber.EventUpdates
                    };
                    _context.Subscribers.Add(subscribeEntity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    SubscribeEntity subscribeEntity = new SubscribeEntity
                    {
                        Email = subscriber.UserEmail,
                        AdvertisingUpdates = subscriber.AdvertisingUpdates,
                        WeekInReview = subscriber.WeekInReview,
                        Podcasts = subscriber.Podcasts,
                        StartupsWeekly = subscriber.StartupsWeekly,
                        DailyNewsletter = subscriber.DailyNewsletter,
                        EventUpdates = subscriber.EventUpdates
                    };
                    _context.Subscribers.Add(subscribeEntity);
                    await _context.SaveChangesAsync();
                }

                return new OkObjectResult(new { Status = 200, Message = "Subscribed sucessfully" });
            }
 
        }
        _logger.LogError("Unable to subscribe");
        return new BadRequestObjectResult(new { Status = 400, Message = "Unable to subscribe right now." });
    }
}

public class SubscribeToNewsletter
{
    public string UserEmail { get; set; } = null!;
    public string? PreferredEmail { get; set; }
    public bool AdvertisingUpdates { get; set; } = false;
    public bool WeekInReview { get; set; } = false;
    public bool Podcasts { get; set; } = false;
    public bool StartupsWeekly { get; set; } = false;
    public bool DailyNewsletter { get; set; } = false;
    public bool EventUpdates { get; set; } = false;
}