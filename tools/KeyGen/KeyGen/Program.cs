using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace MyShop.KeyGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "MyShop - Admin Key Generator";
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== MYSHOP LICENSE KEY GENERATOR ===");
                Console.Write("\n1. Nhap Machine ID: ");
                string machineId = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrEmpty(machineId)) continue;

                Console.Write("2. Nhap Prefix (VD: MyShop-V1-PRO) hoac enter de dung mac dinh: ");
                string prefix = Console.ReadLine()?.Trim() ?? "";
                if (string.IsNullOrEmpty(prefix)) prefix = "MYSH-OP25-FREE";

                string key = GenerateKey(machineId, prefix);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n=> LICENSE KEY: {key}");
                Console.ResetColor();

                Console.WriteLine("\nNhan phim bat ky de tiep tuc hoac enter de thoat...");
                if (Console.ReadKey().Key == ConsoleKey.Escape) break;
            }
        }

        static string GenerateKey(string machineId, string prefix)
        {
            string prefixUpper = prefix.ToUpper();
            string rawPrefix = prefixUpper.Replace("-", "");
            string rawData = rawPrefix + machineId;
            
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            string hashString = BitConverter.ToString(hashBytes).Replace("-", "");
            
            // Lấy 4 ký tự checksum khớp với logic trong LicenseService.cs
            string checksum = hashString.Substring(0, 4);
            return $"{prefixUpper}-{checksum}";
        }
    }
}