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

        string LogPath = null;
        string FileName = null;

        public VolunteerUnblockService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        private static int MilliSecondsUntilMidnight()
        {
            var time = (int)(DateTime.Now.AddHours(6.0) - DateTime.Now).TotalMilliseconds;
            return time;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            FileName = "volunteer_" + DateTime.Now.ToString("dd-MM-yyyy") + "_" + DateTime.Now.Ticks;
            LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Logs", FileName + ".txt");
            WriteToFile(DateTime.Now + "------Start Method" + Environment.NewLine, LogPath);

            // set up a timer to be non-reentrant
            _t = new Timer(async _ => await OnTimerFiredAsync(cancellationToken),
                null, MilliSecondsUntilMidnight(), Timeout.Infinite);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WriteToFile(DateTime.Now + "------Stop Method" + Environment.NewLine, LogPath);
            _t?.Dispose();
            return Task.CompletedTask;
        }

        private async Task OnTimerFiredAsync(CancellationToken cancellationToken)
        {
            try
            {
                // do your work here
                WriteToFile(DateTime.Now + "------Main Method" + Environment.NewLine, LogPath);
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

                var VolunteerBlockHistory = Data.Where(x => x.IsPermanently == false && x.IsBlock == true && x.ToDate.Value.Date < NextDayDate.Date).ToList();

                VolunteerBlockHistory.ForEach(x =>
                {
                    var item = x;

                    WriteToFile(DateTime.Now + "------UnBlock: " + x.VolunteerId + Environment.NewLine, LogPath);

                    var volunteerToBlock = Volunteer.Where(z => z.Id == x.VolunteerId).FirstOrDefault();
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
                WriteToFile(DateTime.Now + "------Error" + ex.Message.ToString() + " & " + ex.InnerException.ToString() + Environment.NewLine, LogPath);
            }
            finally
            {
                WriteToFile(DateTime.Now + "------Finally" + Environment.NewLine, LogPath);
                // set timer to fire off again
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