using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrossinformTask.Core;

namespace CrossinformTask
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Введите путь к файлу: ");

            var path = Console.ReadLine();
            var triplets = new ConcurrentDictionary<string, int>();
            using var source = new CancellationTokenSource();
            var token = source.Token;
            var timer = new Stopwatch();

            timer.Start();

            var task = ParallelTripletsFinder.FindInFileAsync(path, triplets, token, 32768);

            // Требует меньше памяти, но не работает с русскими символами
            //var task = ParallelTripletsFinder.FindInBigFileAsync(path, cTriplets, token, 32768); 

            var task2 = task.ContinueWith(t =>
            {
                timer.Stop();
                if (t.IsCompletedSuccessfully)
                    Console.WriteLine("Задача завершена\n" +
                                      "Нажмите любую клавишу для отображения результата:");
                else
                    Console.WriteLine("\nЗадача отменена");
            });

            Console.WriteLine("Нажмите любую клавишу, чтобы отменить задачу: ");

            Console.ReadKey();
            source.Cancel();
            Task.Delay(1000).Wait();

            Console.WriteLine();
            foreach (var (key, value) in triplets.OrderBy(o => -o.Value).Take(10))
                Console.WriteLine($"{key} : {value}");

            Console.WriteLine($"Время выполнения: {timer.ElapsedMilliseconds} мс");
            Console.WriteLine($"\nСтатус задачи: {task.Status}");
            Console.ReadKey();
        }
    }
}
