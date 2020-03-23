using System;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace set4tm_console
{
    class Program
    {
        static void Main(string[] args)
        {   
            // 0. Данные соединения
            string comNum = "COM8"; // Номер COM порта, к которму подключен счётчик
            byte id = 0x13;         // сетевой адрес счётчика          

            // 1. Открываем последовательный порт 
            ComControl COM8 = new ComControl(comNum);
            COM8.Open();

            // 2. Открываем канал связи счётчика
            if (COM8.OpenChannel(id) == true)
            {
                Console.WriteLine("Программа чтения данных со счётчика CЭТ4ТМ: \n********************************");
                int a;
                do
                {
                    Console.WriteLine("\nВыберите режим работы: \n    1 - Считать все параметры \n    2 - Выбрать и считать один параметр, \n    3 - Автоматический опрос одного параметра");
                }
                while (!Int32.TryParse(Console.ReadLine(), out a) || a < 1 || a > 1);

                switch (a)
                {
                    case 1:
                        Console.WriteLine("********************************");
                        var date = COM8.ReadJ(id, 0);
                        Console.WriteLine($"\nТекущее время счётчика: \nДата = {date.Item1.ToLongDateString()} \nВремя = {date.Item1.ToLongTimeString()}");

                        var OnOff = COM8.ReadJ(id, 1, 9);
                        Console.WriteLine($"\nВремя последнего выключения:    {OnOff.Item1}");
                        Console.WriteLine($"Время последнего включения:     {OnOff.Item2}");

                        var Cap = COM8.ReadJ(id, 2, 8);
                        Console.WriteLine($"\nВремя последнего открытия крышки:       {Cap.Item1}");
                        Console.WriteLine($"Время последнего закрытия крышки:       {Cap.Item2}");

                        float P = COM8.ReadP(id, 1, 1, 0);
                        Console.WriteLine($"\nАктивная мощность P по сумме фаз:   {Math.Round(P, 2)} (кВт)");
                        Console.WriteLine($"Активная мощность P по фазе A:      {COM8.ReadP(id, 1, 1, 1)} (кВт)");
                        Console.WriteLine($"Активная мощность P по фазе B:      {COM8.ReadP(id, 1, 1, 2)} (кВт)");
                        Console.WriteLine($"Активная мощность P по фазе C:      {COM8.ReadP(id, 1, 1, 3)} (кВт)");

                        Console.WriteLine($"\nРеактивная мощность Q по сумме фаз: {COM8.ReadP(id, 1, 2, 0)} (кВт)");
                        Console.WriteLine($"Реактивная мощность Q по фазе A:    {COM8.ReadP(id, 1, 2, 1)} (кВт)");
                        Console.WriteLine($"Реактивная мощность Q по фазе B:    {COM8.ReadP(id, 1, 2, 2)} (кВт)");
                        Console.WriteLine($"Реактивная мощность Q по фазе C:    {COM8.ReadP(id, 1, 2, 3)} (кВт)");

                        Console.WriteLine($"\nПолная мощность S по сумме фаз: {COM8.ReadP(id, 1, 3, 0)} (кВт)");
                        Console.WriteLine($"Полная мощность S по фазе A:    {COM8.ReadP(id, 1, 3, 1)} (кВт)");
                        Console.WriteLine($"Полная мощность S по фазе B:    {COM8.ReadP(id, 1, 3, 2)} (кВт)");
                        Console.WriteLine($"Полная мощность S по фазе C:    {COM8.ReadP(id, 1, 3, 3)} (кВт)");

                        float U = COM8.ReadP(id, 2, 1, 1);
                        Console.WriteLine($"\nНапряжение по фазе А:   {Math.Round(U, 2)} (В)");
                        Console.WriteLine($"Напряжение по фазе B:   {Math.Round(COM8.ReadP(id, 2, 1, 2), 2)} (В)");
                        Console.WriteLine($"Напряжение по фазе C:   {Math.Round(COM8.ReadP(id, 2, 1, 3), 2)} (В)");

                        U = COM8.ReadP(id, 2, 2, 1);
                        Console.WriteLine($"\nМежфазное напряжение АB:    {U} (В)");
                        Console.WriteLine($"Межфазное напряжение BC:    {COM8.ReadP(id, 2, 2, 2)} (В)");
                        Console.WriteLine($"Межфазное напряжение CА:    {COM8.ReadP(id, 2, 2, 3)} (В)");

                        Console.WriteLine($"\nНапряжение нулевой последовательности: {COM8.ReadP(id, 2, 3)} (В)");

                        float I = COM8.ReadP(id, 3, 1);
                        Console.WriteLine($"\nТок по фазе А:  {I} (A)");
                        Console.WriteLine($"Ток по фазе B:  {COM8.ReadP(id, 3, 2)} (A)");
                        Console.WriteLine($"Ток по фазе C:  {COM8.ReadP(id, 3, 3)} (A)");

                        float cos = COM8.ReadP(id, 4, 0);
                        Console.WriteLine($"\nCOS(f) по сумме фаз: {Math.Round(cos, 2)}");
                        Console.WriteLine($"COS(f) по фазе A:   {Math.Round(COM8.ReadP(id, 4, 1), 2)}");
                        Console.WriteLine($"COS(f) по фазе B:   {Math.Round(COM8.ReadP(id, 4, 2), 2)}");
                        Console.WriteLine($"COS(f) по фазе C:   {Math.Round(COM8.ReadP(id, 4, 3), 2)}");

                        Console.WriteLine($"\nЧастота сети   {COM8.ReadP(id, 5)} (Гц)");

                        Console.WriteLine($"\nТемпература внутри счётчика    {COM8.ReadP(id, 6)} (°C)");
                        break;
                }

            }
            else {Console.WriteLine("Не удалось открыть канал связи!");}

            Console.WriteLine("\nДля выхода нажмите любую клавишу ...");
            Console.ReadKey();
            COM8.Close();
        }
    }
}


