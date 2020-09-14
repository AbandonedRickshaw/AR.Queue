using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EF.Playground.Data;
using StatefulQueue;
//using Serilog;

namespace EF.Playground
{
    class Program
    {
        static CancellationTokenSource _ctsMain;

        static void Main(string[] args)
        {
            _ctsMain = new CancellationTokenSource();

            try
            {
                var builder = CreateHostBuilder(args).Build();
                var task = builder.RunAsync(_ctsMain.Token);
                if (task.IsFaulted)
                    throw task.Exception;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Environment.ExitCode = -1;
            }
            finally
            {
                Environment.SetEnvironmentVariable("MigrationTool_JobID", null, EnvironmentVariableTarget.Process);
            }
        }

        public static readonly ILoggerFactory MyLoggerFactory
            = LoggerFactory.Create(builder => { builder.AddConsole(); });


        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.CancelKeyPress -= Console_CancelKeyPress;
            Console.Write("\r\nCancellation requested.  Press [Enter] to exit...");
            _ctsMain.Cancel();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder().ConfigureAppConfiguration((hostingContext, configApp) =>
            {
                configApp.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((hostingContext, services) =>
            {
                // add database context service
                services.AddDbContext<QueueDbContext<QueueItemBody, QueueItemStateInfo>>(options => options
                    .UseLoggerFactory(MyLoggerFactory)
                    .EnableSensitiveDataLogging(true)
                    .UseSqlServer(hostingContext.Configuration.GetValue<string>("ConnectionStrings:Default")));
            })
            //.UseConsoleLifetime()
            ;
    }
}
