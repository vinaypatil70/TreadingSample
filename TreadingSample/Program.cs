using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TreadingSample
{
    class Program
    {
        private static List<Thread> consumers = new List<Thread>();
        private static Queue<Action> tasks = new Queue<Action>();
        private static readonly object queuelock = new object();
        private static EventWaitHandle newtaskavailable = new AutoResetEvent(false);
        private static readonly object consolelock = new object();

        static void Main(string[] args)
        {
            consumers.Add(new Thread(() => { DoWork(ConsoleColor.Blue); }));
            consumers.Add(new Thread(() => { DoWork(ConsoleColor.Red); }));
            consumers.Add(new Thread(() => { DoWork(ConsoleColor.Green); }));
            consumers.ForEach((t) => { t.Start(); });

            while (true)
            {
                Random r = new Random();
                EnqueTask(() =>
                {

                    int number = r.Next(10);
                    Console.Write(number);

                });

                Thread.Sleep(r.Next(1000));

            }
        }

        private static void EnqueTask(Action task)
        {
            lock (queuelock)
            {
                tasks.Enqueue(task);
            }
            newtaskavailable.Set();
        }

        private static void DoWork(ConsoleColor color)
        {
            while (true)
            {
                Action task = null;
                lock (queuelock)
                {
                    if (tasks.Count > 0)
                    {
                        task = tasks.Dequeue();
                    }
                }
                if (task != null)
                {
                    lock (consolelock)
                    {
                        Console.ForegroundColor = color;
                    }

                    task();

                }
                else
                {
                    newtaskavailable.WaitOne();
                }

            }
        }
    }
}
