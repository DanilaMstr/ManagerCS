using System;
using System.Collections.Generic;
using System.Text;

namespace ManagerCS
{
    class Server
    {
        //для линейного режима работы №1
        public readonly double Tsmin = 1;
        public readonly int Tsmax = 5;

        //для показательного режима работы №2
        public readonly double twork = 2;

        //id сервара и номер режима работы
        public readonly uint WorkType;
        //время поступления заявки
        //время обработки заявки
        private double Twork { get; set; } = -1;

        //колличество обработанных запросов всеми серверами
        static public uint CountCompleted { get; private set; } = 0;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="WorkType">режим работы сервера</param>
        public Server(uint WorkType)
        {
            this.WorkType = WorkType;
        }

        /// <summary>
        /// занят ли сервер
        /// </summary>
        public bool isFree { get; private set; } = true;

        //получить время обработки заявки
        private double getTwork()
        {
            switch (WorkType)
            {
                case 1:
                    Random r = new Random();
                    return r.NextDouble() * (Tsmax - Tsmin) * Tsmin;
                case 2:
                    return twork;
            }
            return -1;
        }

        //добавляет запрос на сервер
        public bool addQuery(Query newQuery)
        {
            if (isFree)
            {
                isFree = false;
                Twork = getTwork();
                return true;
            }
            return false;
        }
        
        //функция работы сервера
        public void Work(double spendTime)
        {
            if (isFree)
                return;

            Twork -= spendTime;
            if (Twork <= 0)
            {
                isFree = true;
                CountCompleted++;
            }
        }
    }
}
