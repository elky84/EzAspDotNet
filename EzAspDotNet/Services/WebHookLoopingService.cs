using EzAspDotNet.Exception;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EzAspDotNet.Services
{
    public class WebHookLoopingService : LoopingService
    {
        private readonly WebHookService _webHookService;

        public WebHookLoopingService(WebHookService webHookService)
        {
            _webHookService = webHookService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _webHookService.HttpTaskRun();
                }
                catch (System.Exception e)
                {
                    e.ExceptionLog();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
