using System;
using System.ServiceModel;
using log4net.Config;

namespace CACCCheckInServiceLibraryDebugHost
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure log4net for use in service
            XmlConfigurator.Configure();

            Console.WriteLine("Creating service instance");
            ServiceHost host = new ServiceHost(typeof(CACCCheckInServiceLibrary.CACCCheckInService));

            Console.WriteLine("Opening service for clients");
            host.Open();

            Console.WriteLine("Hit <ENTER> to end service");            
            Console.ReadLine();
        }
    }
}
