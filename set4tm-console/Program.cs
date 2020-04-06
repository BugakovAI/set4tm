    using System;
    using System.IO;
    using System.IO.Ports;
    using System.Runtime.InteropServices;
    using System.Text;

    namespace set4tm_console
    {
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
    public struct Union
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public float Float;
        [System.Runtime.InteropServices.FieldOffset(0)]
        public double Double;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public Int32 Int32;
        [System.Runtime.InteropServices.FieldOffset(0)]
        public UInt32 UInt32;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public UInt32 UInt32_0;
        [System.Runtime.InteropServices.FieldOffset(4)]
        public UInt32 UInt32_1;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public UInt64 UInt64;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public Int64 Int64;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public ushort UShort0;
        [System.Runtime.InteropServices.FieldOffset(2)]
        public ushort UShort1;
        [System.Runtime.InteropServices.FieldOffset(4)]
        public ushort UShort2;
        [System.Runtime.InteropServices.FieldOffset(6)]
        public ushort UShort3;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public short Int16;
        [System.Runtime.InteropServices.FieldOffset(0)]
        public short Int16_0;
        [System.Runtime.InteropServices.FieldOffset(2)]
        public short Int16_1;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public ushort UInt16;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public short Short0;
        [System.Runtime.InteropServices.FieldOffset(2)]
        public short Short1;
        [System.Runtime.InteropServices.FieldOffset(4)]
        public short Short2;
        [System.Runtime.InteropServices.FieldOffset(6)]
        public short Short3;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public byte Byte0;
        [System.Runtime.InteropServices.FieldOffset(1)]
        public byte Byte1;
        [System.Runtime.InteropServices.FieldOffset(2)]
        public byte Byte2;
        [System.Runtime.InteropServices.FieldOffset(3)]
        public byte Byte3;
        [System.Runtime.InteropServices.FieldOffset(4)]
        public byte Byte4;
        [System.Runtime.InteropServices.FieldOffset(5)]
        public byte Byte5;
        [System.Runtime.InteropServices.FieldOffset(6)]
        public byte Byte6;
        [System.Runtime.InteropServices.FieldOffset(7)]
        public byte Byte7;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public sbyte SByte0;
    }

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
                        Console.WriteLine($"\nА+ накопленная с момента сброса по сумме тарифов:   {Esbros.Item1} (кВт*ч)");
                        Console.WriteLine($"A+ за январь по сумме тарифов:      {(COM8.ReadE(id, 2, 1, 0)).Item1} (кВт*ч)"); // 1 - первый месяц
                        Console.WriteLine($"A+ за февраль по сумме тарифов:     {(COM8.ReadE(id, 2, 2, 0)).Item1} (кВт*ч)");
                        Console.WriteLine($"A+ за 1.04.2020 по сумме тарифов:     {(COM8.ReadE(id, 3, 1, 0)).Item1} (кВт*ч)"); // 1 - первое число

                        Console.WriteLine($"\nP по сумме фаз:   {Math.Round(COM8.ReadP(id, 1, 1, 0), 2)} (кВт)");
                        Console.WriteLine($"P по фазе A:      {COM8.ReadP(id, 1, 1, 1)} (кВт)");
                        Console.WriteLine($"P по фазе B:      {COM8.ReadP(id, 1, 1, 2)} (кВт)");
                        Console.WriteLine($"P по фазе C:      {COM8.ReadP(id, 1, 1, 3)} (кВт)");

                        Console.WriteLine($"\nQ по сумме фаз: {COM8.ReadP(id, 1, 2, 0)} (кВт)");
                        Console.WriteLine($"Q по фазе A:    {COM8.ReadP(id, 1, 2, 1)} (кВт)");
                        Console.WriteLine($"Q по фазе B:    {COM8.ReadP(id, 1, 2, 2)} (кВт)");
                        Console.WriteLine($"Q по фазе C:    {COM8.ReadP(id, 1, 2, 3)} (кВт)");

                        Console.WriteLine($"\nS по сумме фаз: {COM8.ReadP(id, 1, 3, 0)} (кВт)");
                        Console.WriteLine($"S по фазе A:    {COM8.ReadP(id, 1, 3, 1)} (кВт)");
                        Console.WriteLine($"S по фазе B:    {COM8.ReadP(id, 1, 3, 2)} (кВт)");
                        Console.WriteLine($"S по фазе C:    {COM8.ReadP(id, 1, 3, 3)} (кВт)");

                        Console.WriteLine($"\nU по фазе А:   {Math.Round(COM8.ReadP(id, 2, 1, 1), 2)} (В)");
                        Console.WriteLine($"U по фазе B:   {Math.Round(COM8.ReadP(id, 2, 1, 2), 2)} (В)");
                        Console.WriteLine($"U по фазе C:   {Math.Round(COM8.ReadP(id, 2, 1, 3), 2)} (В)");

                        Console.WriteLine($"\nU межфазное АB:    {COM8.ReadP(id, 2, 2, 1)} (В)");
                        Console.WriteLine($"U межфазное BC:    {COM8.ReadP(id, 2, 2, 2)} (В)");
                        Console.WriteLine($"U межфазное CА:    {COM8.ReadP(id, 2, 2, 3)} (В)");

                        Console.WriteLine($"\nU нулевой последовательности: {COM8.ReadP(id, 2, 3)} (В)");

                        Console.WriteLine($"I по фазе А:  {COM8.ReadP(id, 3, 1)} (A)");
                        Console.WriteLine($"I по фазе B:  {COM8.ReadP(id, 3, 2)} (A)");
                        Console.WriteLine($"I по фазе C:  {COM8.ReadP(id, 3, 3)} (A)");

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


