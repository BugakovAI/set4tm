﻿using System;
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

                        var Esbros = COM8.ReadE(id, 1);
                        Console.WriteLine($"" +
                            $"\nЭнергия активная прямая накопленная с момента сброса по сумме тарифов:   {Esbros.Item1*1000000} (кВт*ч)" +
                            $"\nЭнергия активная обратная накопленная с момента сброса по сумме тарифов:   {Esbros.Item2} (кВт*ч)" +
                            $"\nЭнергия реактивная прямая накопленная с момента сброса по сумме тарифов:   {Esbros.Item3} (кВт*ч)");
                        Console.WriteLine($"Энергия накопленная за прошлый месяц по сумме тарифов:      {COM8.ReadE(id, 2, 1, 0)} (кВт*ч)");
                        Console.WriteLine($"Энергия накопленная за позапрошлые сутки по сумме тарифов:      {COM8.ReadE(id, 2, 2, 0)} (кВт*ч)");

                        Console.WriteLine($"\nАктивная мощность P по сумме фаз:   {Math.Round(COM8.ReadP(id, 1, 1, 0), 2)} (кВт)");
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

                        Console.WriteLine($"\nНапряжение по фазе А:   {Math.Round(COM8.ReadP(id, 2, 1, 1), 2)} (В)");
                        Console.WriteLine($"Напряжение по фазе B:   {Math.Round(COM8.ReadP(id, 2, 1, 2), 2)} (В)");
                        Console.WriteLine($"Напряжение по фазе C:   {Math.Round(COM8.ReadP(id, 2, 1, 3), 2)} (В)");

                        Console.WriteLine($"\nМежфазное напряжение АB:    {COM8.ReadP(id, 2, 2, 1)} (В)");
                        Console.WriteLine($"Межфазное напряжение BC:    {COM8.ReadP(id, 2, 2, 2)} (В)");
                        Console.WriteLine($"Межфазное напряжение CА:    {COM8.ReadP(id, 2, 2, 3)} (В)");

                        Console.WriteLine($"\nНапряжение нулевой последовательности: {COM8.ReadP(id, 2, 3)} (В)");

                        Console.WriteLine($"\nТок по фазе А:  {COM8.ReadP(id, 3, 1)} (A)");
                        Console.WriteLine($"Ток по фазе B:  {COM8.ReadP(id, 3, 2)} (A)");
                        Console.WriteLine($"Ток по фазе C:  {COM8.ReadP(id, 3, 3)} (A)");

                        Console.WriteLine($"\nCOS(f) по сумме фаз: {Math.Round(COM8.ReadP(id, 4, 0), 2)}");
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


