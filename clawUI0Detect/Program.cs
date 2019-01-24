using System;
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
                var servicesToRun = new ServiceBase[]
                {
                    new Service()
                };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}