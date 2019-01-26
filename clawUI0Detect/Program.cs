using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace clawUI0Detect
{
    internal class Program
    {

        private static void Main(string[] args)
        {

            if (Environment.UserInteractive)
            {
                var service = new Service();
                service.UserInteractive(args);
            }
            else
            {
                ServiceController serviceController = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "clawUI0Detect");
                if (serviceController == null)
                {
                    var service = new Service();
                    service.UserInteractive(args);
                }
                else
                {
                    var servicesToRun = new ServiceBase[]
                    {
                    new Service()
                    };
                    ServiceBase.Run(servicesToRun);
                }

            }
        }
    }
}