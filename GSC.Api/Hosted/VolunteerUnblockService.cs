using GSC.Data.Dto.Volunteer;
using GSC.Domain.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GSC.Api.Hosted
{
    public class VolunteerUnblockService : IHostedService
    {
        private Timer _t;
        private readonly IServiceScopeFactory _scopeFactory;

        public VolunteerUnblockService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        private static int MilliSecondsUntilMidnight()
        {

            DateTime nowTime = DateTime.Now;
            DateTime oneAmTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 17, 0, 0, 0);
            if (nowTime > oneAmTime)
                oneAmTime = oneAmTime.AddDays(1);

            int tickTime = (int)(oneAmTime - nowTime).TotalMilliseconds;
            return tickTime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {          
            // set up a timer to be non-reentrant
            _t = new Timer(async _ => await OnTimerFiredAsync(cancellationToken),
                null, MilliSecondsUntilMidnight(), Timeout.Infinite);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _t?.Dispose();
            return Task.CompletedTask;
        }

        private async Task OnTimerFiredAsync(CancellationToken cancellationToken)
        {
            try
            {
                // do your work here
                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<GscContext>();

                var Volunteer = context.Volunteer.Where(x => x.IsBlocked == true).ToList();

                var NextDayDate = DateTime.Now;

                var blockHistoryGroupBy = context.VolunteerBlockHistory.GroupBy(x => x.VolunteerId)
                    .Select(v => new VolunteerBlockHistoryDto
                    {
                        Id = v.Max(c => c.Id),
                        VolunteerId = v.Key
                    }).ToList();

                var Data = context.VolunteerBlockHistory.Where(x => blockHistoryGroupBy.Select(v => v.Id).Contains(x.Id)).ToList();

                var VolunteerBlockHistory = Data.Where(x => !x.IsPermanently && x.IsBlock && x.ToDate.Value.Date < NextDayDate.Date).ToList();

                VolunteerBlockHistory.ForEach(x =>
                {
                    var item = x;

                    var volunteerToBlock = Volunteer.Find(z => z.Id == x.VolunteerId);
                    volunteerToBlock.IsBlocked = false;
                    context.Volunteer.Update(volunteerToBlock);

                    item.Id = 0;
                    item.IsPermanently = false;
                    item.IsBlock = false;
                    item.FromDate = null;
                    item.ToDate = null;
                    item.Note = "Auto Unblock";
                    context.VolunteerBlockHistory.Add(item);
                });

                context.Save();

                await Task.Delay(2000, cancellationToken);
            }
            catch (Exception ex)
            {
               //Empty code block
            }
            finally
            {               
                _t?.Change(MilliSecondsUntilMidnight(), Timeout.Infinite);
            }
        }

        public static void WriteToFile(string text, string Basepath)
        {

            string path = System.IO.Path.Combine(Basepath);
            FileInfo logFileInfo = new FileInfo(path);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            using (FileStream fileStream = new FileStream(path, FileMode.Append))
            {
                using (StreamWriter log = new StreamWriter(fileStream))
                {
                    log.WriteLine(text);
                }
            }
        }
    }
}