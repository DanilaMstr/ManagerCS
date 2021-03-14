using System;
using System.Collections.Generic;
using System.Text;

namespace ManagerCS
{
    class Buffer
    {
        //размер буфера
        public uint size { get; private set; }
        //очередь для буфера
        private Queue<Query> requests = new Queue<Query>();

        public Buffer()
        { }

        public Buffer(uint maxCount)
        {
            this.size = maxCount;
        }

        //проверяет заполнен ли буфер
        public bool isFull()
        {
            if (requests.Count == size)
                return true;
            else
                return false;
        }
        //проверят пустой ли буфер
        public bool isEmpty() => (requests.Count == 0);
        //добавление в буфер
        public bool Enqueue(Query q)
        {
            if (isFull())
            {
                return false;
            }
            else
            {
                requests.Enqueue(q);
                return true;
            }
        }
        //извлечение из очереди
        public Query Dequeue() => requests.Dequeue();
        //количество запросов в буфере
        public int getCountQuery() => requests.Count;
    }
}
