using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerCS
{
    class Program
    {
        static void Main(string[] args)
        {
            ComputingSystem system = new ComputingSystem(1, 0, 3600);
            system.addServer();
            system.addServer();
            system.simulation();
            system.getStatistics();
            Console.ReadLine();
        }
    }
}
