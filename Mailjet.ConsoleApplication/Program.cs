using Mailjet.Client;
using Mailjet.Client.Resources;
using System;
using System.Threading.Tasks;

namespace Mailjet.ConsoleApplication
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        private static async Task RunAsync()
        {
            //MailjetClientHandler clientHandler = new MailjetClientHandler()
            //{
            //    Proxy = new DefaultProxy("http://127.0.0.1:8888"),
            //    UseProxy = true,
            //};

            //MailjetClient client = new MailjetClient(Environment.GetEnvironmentVariable("MJ_APIKEY_PUBLIC"), Environment.GetEnvironmentVariable("MJ_APIKEY_PRIVATE"), clientHandler)
            //{
            //    BaseAdress = "https://api.mailjet.com",
            //};

            IMailjetClient client = new MailjetClient(Environment.GetEnvironmentVariable("MJ_APIKEY_PUBLIC"), Environment.GetEnvironmentVariable("MJ_APIKEY_PRIVATE"));

            MailjetRequest request = new MailjetRequest
            {
                Resource = Apikey.Resource,
            };

            MailjetResponse response = await client.GetAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Total: {response.GetTotal()}, Count: {response.GetCount()}\n");
                Console.WriteLine(response.GetData());
            }
            else
            {
                Console.WriteLine($"StatusCode: {response.StatusCode}\n");
                Console.WriteLine($"ErrorInfo: {response.GetErrorInfo()}\n");
                Console.WriteLine($"ErrorMessage: {response.GetErrorMessage()}\n");
            }

            Console.ReadLine();
        }
    }
}
