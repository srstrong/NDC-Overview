using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Continuations
{
    class Continuations
    {
        static void Main()
        {
            MultiTaskContinuation();

            ContinueWithExceptions();
        }

        private static void MultiTaskContinuation()
        {
            var tasks = new Task<Stream>[2];

            tasks[0] = new Task<Stream>(() => WebRequest.Create("http://www.google.com")
                                                     .GetResponse()
                                                     .GetResponseStream());

            tasks[1] = new Task<Stream>(() => WebRequest.Create("http://www.bing.com")
                                                     .GetResponse()
                                                     .GetResponseStream());

            tasks[1].Start();
            tasks[0].Start();

            var task3 = Task.Factory.ContinueWhenAny(tasks, winner =>
                            {
                                Console.WriteLine("Final task running");
                                return winner == tasks[0] ? "Google" : "Bing";
                            });

            Console.WriteLine("Waiting for tasks to complete");

            // Blocks until task3 completes
            Console.WriteLine("{0} wins!", task3.Result);
        }

        private static void ContinueWithExceptions()
        {
            var task = new Task<Stream>(() =>
                                        WebRequest.Create("http://www.goadfogle.com")
                                                  .GetResponse()
                                                  .GetResponseStream()
                );

            var successTask = task.ContinueWith(t => Console.WriteLine("Request successful"),
                                             TaskContinuationOptions.OnlyOnRanToCompletion);

            var errorTask = task.ContinueWith(t => Console.WriteLine("Task failed: {0}", t.Exception), 
                                           TaskContinuationOptions.OnlyOnFaulted);

            task.Start();

            try
            {
                Task.WaitAll(successTask, errorTask);
            }
            catch
            {
            }

            task.DisplayState("request task");
            successTask.DisplayState("success");
            errorTask.DisplayState("error");
        }
    }

    public static class Extensions
    {
        public static void DisplayState(this Task task, string name)
        {
            Console.WriteLine("{3} state: {{ Completed: {0}, Cancelled: {1}, Faulted: {2} }}", task.IsCompleted, task.IsCanceled, task.IsFaulted, name);
        }
    }

}
