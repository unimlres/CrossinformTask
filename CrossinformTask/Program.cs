using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var triplets = new Dictionary<string, int>();
            using var source = new CancellationTokenSource();
            var token = source.Token;
            var timer = new Stopwatch();

            timer.Start();

            //var task = TripletsFinder.FindInFileAsync(path, triplets, token); // Требует много памяти
            var task = TripletsFinder.FindInBigFileAsync(path, triplets, token);

            task.ContinueWith(_ => timer.Stop());

            Console.WriteLine("Чтобы отобразить результат нажмите на любую клавишу.\n" +
                              "Запрос результата раньше времени приведет к остановке " + 
                              "поиска и выводу текущих результатов");
            Console.Write("Нажмите любую клавишу: ");

            Console.ReadKey();

            source.Cancel();
            Task.Delay(500).Wait();

            Console.WriteLine();
            foreach (var (key, value) in triplets.OrderBy(o => -o.Value).Take(10))
                Console.WriteLine($"{key} : {value}");

            Console.WriteLine($"Время выполнения: {timer.ElapsedMilliseconds} мс");
            Console.WriteLine($"\nСтатус задачи: {task.Status}");
            Console.ReadKey();
        }
    }
}
