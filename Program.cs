using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace HSBors
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
                .UseKestrel()
                .UseIISIntegration()
                .Build()
                .Run(); 

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>().ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    //logging.AddConsole(); //use for iis express host. see result in output win from web server
                    logging.AddDebug(); //use for debuging mode.see result in output win from debug
                    EventLogSettings event_ = new EventLogSettings();
                    event_.LogName = "HSBorsLog";
                    event_.SourceName = "HSBorsSource";
                    logging.AddEventLog(event_); // use anywere. see in event viewer

                });
    }
}
