using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerCS
{
    class Query
    {
        public int id;
        public double Tin;

        public Query(int id, double Tin)
        {
            this.id = id;
            this.Tin = Tin;
        }
    }

    class ComputingSystem
    {
        //режим работы
        public readonly uint WorkType;

        //для линейного режима работы №1
        public readonly double Tzmin = 0.5;
        public readonly double Tzmax = 0.83333;

        //для показательного режима работы №2
        public readonly double lambda = 1.5;

        //текущее время
        public double Tcurrent { get; set; } = 0;
        public readonly double Tend;

        //буфер и лист серверов
        private Buffer buffer;
        //лист серверов
        private List<Server> servers = new List<Server>();
        //кол-во обработанных заявок
        public uint CountQuery { get; set; } = 0;
        public uint CountReject { get; set; } = 0;
        public double CountAccepted { get; set; } = 0;

        private bool CompleteSimulation = false;

        private double P_1 = 0; //колличество ситуаций когда загружено 0 серверов;
        private double P1 = 0; //вероятность того, что в загружен 1 сервер
        private double P2 = 0; //вероятность того, что в загружены 2 сервера                  
        private double P3 = 0; //вероятность того, что в загружены 3 сервера
        private double P_4 = 0; //колличество ситуаций когда в буфере 0 программ;
        private double P4 = 0; //вероятность того, что в буфере 1-на программа 
        private double P5 = 0; //вероятность того, что в буфере 2-ве программы 
        private double P6 = 0; //вероятность того, что в буфере 3-ри программы
        private List<double> CountProgramIn = new List<double>(); //вектор колличеста программ в ВС
        private List<double> CountActiveServes = new List<double>(); //вектор колличеста занятых серверов
        private List<double> CountProgramInBuf = new List<double>(); //вектор колличеста программ в буфере
        public ComputingSystem(uint WorkType, uint buffSize, double Tend)
        {
            this.WorkType = WorkType;
            buffer = new Buffer(buffSize);
            this.Tend = Tend;
        }

        //получить время поступления заявки
        private double getTnext()
        {
            Random r = new Random();
            switch (WorkType)
            {
                case 1:
                    return Math.Round(((Tzmax - Tzmin) * r.NextDouble()) + Tzmin, 5);
                case 2:
                    return Math.Round(-(Convert.ToDouble(Math.Log(r.NextDouble()))) / lambda, 5);
            }
            return -1;
        }

        public void addServer()
        {
            Server server = new Server(WorkType);
            servers.Add(server);
        }

        //симуляция работы ВС
        public void simulation()
        {
            if (servers.Count == 0)
            {
                Console.WriteLine("Неудалось провести симуляцию из-за отсутствия серверов!");
                return;
            }
            else
            {
                Console.WriteLine($"Начало симуляции\n" +
                                  $"Колличество серверов: {servers.Count}\n" +
                                  $"Тип симуляции: {WorkType}");

                int id = 1;

                while (Tcurrent <= Tend)
                {
                    double Tnext = getTnext();
                    Query newQuery = new Query(id++, Tcurrent);

                    CollectStatistics();

                    CountQuery++;
                    if (isBusy())
                    {
                        WorkingServers(Tnext);
                        CountReject++;
                        continue;
                    }
                    else
                    {
                        CountAccepted++;
                        ProcessingQuery(Tnext, newQuery);
                    }
                }
                
            }
            CompleteSimulation = true;
        }

        private void CollectStatistics()
        {
            if (getCountActiveServer() == 0)
                P_1++;
            if (getCountActiveServer() == 1)
                P1++;
            if (getCountActiveServer() == 2)
                P2++;
            if (getCountActiveServer() == 3)
                P3++;
            if (buffer.getCountQuery() == 0)
                P_4++;
            if (buffer.getCountQuery() == 1)
                P4++;
            if (buffer.getCountQuery() == 2)
                P5++;
            if (buffer.getCountQuery() == 3)
                P6++;

            CountActiveServes.Add(servers.Count(temp => temp.isFree));
            CountProgramIn.Add(servers.Count(temp => temp.isFree) + buffer.getCountQuery());
            CountProgramInBuf.Add(buffer.getCountQuery());
        }

        //обработка принятого запроса
        private void ProcessingQuery(double Tnext, Query newQuery)
        {
            if (!buffer.isEmpty())
            {
                CleanBuffer();
            }
            if (haveFreeServer())
            {
                foreach (Server server in servers)
                {
                    if (server.isFree)
                    {
                        server.addQuery(newQuery);
                        break;
                    }
                }
            }
            else
            {
                buffer.Enqueue(newQuery);
            }
            WorkingServers(Tnext);
        }

        //функция отработки серверов
        private void WorkingServers(double Tworking)
        {
            Tcurrent += Tworking;
            foreach (Server server in servers)
                server.Work(Tworking);
        }
        //очистка буфера
        private void CleanBuffer()
        {
            while (!buffer.isEmpty() && haveFreeServer())
            {
                Query query = buffer.Dequeue();
                foreach (Server server in servers)
                {
                    if (server.isFree)
                    {
                        server.addQuery(query);
                        break;
                    }
                }
            }
        }
        //имеются ли свободные серверы
        private bool haveFreeServer()
        {
            foreach (Server server in servers)
            {
                if (server.isFree)
                {
                    return true;
                }
            }
            return false;
        }
        //проверяет свободна ли ВС для принятия
        private bool isBusy()
        {
            if (buffer.isFull())
            {
                foreach (Server server in servers)
                {
                    if (server.isFree)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        //колличесто не успевших обработаться
        public int CountRemaining()
        {
            int count = 0;
            foreach (Server server in servers)
            {
                if (!server.isFree)
                {
                    count++;
                }
            }
            return count + buffer.getCountQuery();
        }
        //получить количество свободных серверов
        public double getCountActiveServer()
        {
            double res = 0;
            foreach (Server server in servers)
            {
                if (server.isFree)
                    res++;
            }
            return res;
        }
        //получить статистику
        public void getStatistics()
        {
            if (CompleteSimulation)
            {
                PrintFirstStat();
                PrintSecondStat();
            }
            else
            {
                Console.WriteLine("Не была проведина симуляция!");
            }
        }
        //вывод первичной статистики
        private void PrintFirstStat()
        {
            Console.WriteLine($"Колличество поступивших заявок: {CountQuery}");
            Console.WriteLine($"Колличество заявок принятых ВС: {CountAccepted}");
            Console.WriteLine($"Колличество отклоненных заявок: {CountReject}");
            Console.WriteLine($"Колличество заявок которые не успели обработаться: {CountRemaining()}");
            Console.WriteLine($"Проверка: {(CountAccepted + CountReject) == CountQuery}");
        }
        //вывод остальной статистики
        private void PrintSecondStat()
        {
            double P0 = CountReject;
            P0 = Math.Round((1 - (P0 / CountQuery)), 5);
            P1 = Math.Round(P1 / (P_1 + P1 + P2 + P3), 5);
            P2 = Math.Round(P2 / (P_1 + P1 + P2 + P3), 5);

            Console.WriteLine($"P0 = {P0} - вероятность того, что ВС не загружена");
            Console.WriteLine($"P1 = {P1} - вероятность того, что в загружен 1 сервер");
            Console.WriteLine($"P2 = {P2} - вероятность того, что в загружены 2 сервера");
            Console.WriteLine($"Q = {Math.Round(1 - (1 - P0), 5)} - относительная пропускная способность");
            Console.WriteLine($"S = {lambda * Math.Round(1 - (1 - P0), 5)} - абсолютная пропускная способность");
            Console.WriteLine($"Pотк = {Math.Round((1 - P0), 5)} - вероятность отказа");
            Console.WriteLine($"K = {Math.Round(CountActiveServes.Average(), 5)} - среднее число занятых серверов");
        }
    }
}
