using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;

class Program
{
    private static readonly HashSet<int> INVALID_STATUSES = new HashSet<int> { 0, 503, 5082, 4939, 4940, 4941, 12003, 5556 };
    private static string imagePath = "Image";

    static async Task Main(string[] args)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out int threadAmount))
        {
            Console.WriteLine("Usage: dotnet run (Number of threads)");
            return;
        }

        Console.WriteLine("=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=\nThis script is for educational purposes only! Use at your own responsibility!\n=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=");
        Console.WriteLine("Press ENTER if you have read and accept that you are fully responsible for using this script!");
        Console.ReadLine();

        if (!Directory.Exists(imagePath))
        {
            Directory.CreateDirectory(imagePath);
        }

        List<Task> tasks = new List<Task>();

        for (int i = 0; i < threadAmount; i++)
        {
            int threadId = i + 1; // Thread ID starts from 1
            tasks.Add(Task.Run(() => ScrapePictures(threadId)));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine($"Successfully started {threadAmount} threads.");
    }

    private static async Task ScrapePictures(int threadId)
    {
        using HttpClient client = new HttpClient();
        Random random = new Random();

        while (true)
        {
            string url = "http://i.imgur.com/";
            int length = random.Next(5, 7); // Randomly choose between 5 and 6

            if (length == 5)
            {
                url += GetRandomString(5);
            }
            else
            {
                url += GetRandomString(3) + GetRandomString(3, true);
            }

            url += $".jpg";
            string fileName = Path.GetFileName(url);
            string filePath = Path.Combine(imagePath, fileName);

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    byte[] content = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(filePath, content);

                    // Check if the image is valid
                    try
                    {
                        using var img = Image.FromFile(filePath);
                        if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                        {
                            Console.WriteLine($"[+] Valid: {url}");
                        }
                        else
                        {
                            Console.WriteLine($"[-] Invalid format (not JPEG): {url}");
                            File.Delete(filePath);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[-] Invalid image: {url}. Error: {e.Message}");
                        File.Delete(filePath);
                    }
                }
                else
                {
                    Console.WriteLine($"[-] Failed to download {url}: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[-] Error downloading {url}: {e.Message}");
            }
        }
    }

    private static string GetRandomString(int length, bool lowercase = false)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var randomChars = lowercase ? lowerChars : chars;

        char[] stringChars = new char[length];
        Random random = new Random();

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = randomChars[random.Next(randomChars.Length)];
        }

        return new string(stringChars);
    }
}
