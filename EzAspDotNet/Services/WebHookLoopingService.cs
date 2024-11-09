using EzAspDotNet.Exception;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EzAspDotNet.Services
{
    public class WebHookLoopingService(WebHookService webHookService) : LoopingService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await webHookService.HttpTaskRun();
                }
                catch (System.Exception e)
                {
                    e.ExceptionLog();
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
