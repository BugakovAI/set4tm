using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace set4tm_gui
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            // 0. Данные соединения
            string comNum = "COM8"; //Номер COM порта, к которму подключен счётчик
            byte id = 0x13;         //сетевой адрес счётчика

            // 1. Открываем последовательный порт 
            ComControl COM8 = new ComControl(comNum);
            COM8.Open();

            // 2. Открываем канал связи счётчика
            bool isOpen = COM8.OpenChannel(id);
            if (isOpen == true)
            {

                Console.WriteLine("Канал открыт!");

                // 3. Формируем запрос на чтение данных и отправляем его в порт
                byte[] request = COM8.GetReq(id);
                COM8.Write(request);

                // 4. Теперь читаем ответ из порта
                byte[] response = COM8.Read(id);

                // 5. Выводим значение в консоль
                byte[] byteValue = new byte[4]; // массив для значения параметра
                Array.Copy(response, 1, byteValue, 0, 4);
                float value = BitConverter.ToSingle(byteValue, 0);
                Console.WriteLine($"Текущее значение напряжения на фазе А = {value}");

                Console.WriteLine("Для выхода нажмите любую клавишу ...");
                //Console.Read();
            }
            else { Console.WriteLine("Канал не открыт..."); }

            COM8.Close();
        }


    }
}
