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

                DateTime date = COM8.ReadTime(id);
                Console.WriteLine($"\nТекущее время счётчика: \nДата = {date.ToLongDateString()} \nВремя = {date.ToLongTimeString()}");

                var OnOff = COM8.ReadOnOff(id);
                Console.WriteLine($"\nВремя последнего выключения: {OnOff.Item1}");
                Console.WriteLine($"Время последнего включения: {OnOff.Item2}");

                var Cap = COM8.ReadCap(id);
                Console.WriteLine($"\nВремя последнего открытия крышки: {Cap.Item1}");
                Console.WriteLine($"Время последнего закрытия крышки: {Cap.Item2}");

                float U = COM8.ReadU(id, 2);
                Console.WriteLine($"\nНапряжение по фазе А: {U}");

                //float ua = (float)COM8.ReadParam(id, 1);
                //Console.WriteLine($"Напряжение по фазе A : \nUa = {ua} В");

            }
            else {Console.WriteLine("Не удалось открыть канал связи!");}

            Console.WriteLine("\nДля выхода нажмите любую клавишу ...");
            Console.ReadKey();
            COM8.Close();
        }
    }
}


