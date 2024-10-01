using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
namespace SqlEditor
{
    internal static class Program
    {
        
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            myMessageBox("1");
            ApplicationConfiguration.Initialize();
            myMessageBox("2");

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
            {
               services.AddScoped<DataGridViewForm>();
                myMessageBox("4");

                //Add Serilog -  not using parameter encoding: System.Text.Encoding.UTF8
                Log.Logger = new LoggerConfiguration()
                   .WriteTo.File(Application.CommonAppDataPath + "\\Logs\\log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
                   .MinimumLevel.Information()
                   .CreateLogger();

                services.AddLogging(x =>
               {
                  x.AddSerilog(logger: Log.Logger, dispose: true);
               });

           });
            myMessageBox("3");
            var host = builder.Build();
            myMessageBox("5");

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                myMessageBox("6");
                try
                {
                    Log.Logger.Information("Program.cs - Call Form Constructor");
                    var form1 = services.GetRequiredService<DataGridViewForm>();
                    Log.Logger.Information("Program.cs - Run Application");
                    Application.Run(form1);
                    Log.Logger.Information("Program.cs - Exit Application");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error loading program.");
                    Log.Logger.Fatal("Program.cs - Exception {msg}",ex.Message);
                }
            }

           static void myMessageBox(string message)
            {
                // MessageBox.Show(message);  // Comment this out to show nothing
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
        }


    }
}