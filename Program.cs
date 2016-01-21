using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Data;

namespace ThirdApp
{
    /*
     * !!!При первом запуске файл входных данных генерируется автоматически!!!
     * 
     * В магазине 5 касс, в каждый момент времени к кассе стоит очередь 
     * некоторой длины. Каждые 30 минут измеряется средняя длина очереди в 
     * каждую кассу и для каждой кассы это значение (число вещественное)
     * записывается в соответсвующий ей файл (всего 5 файлов), магазин работает
     * 8 часов в день. Рассматривается только один день. На момент запуска 
     * приложения все значения уже находятся в файлах. Написать программу, 
     * которая по данным замеров определяет интервал времени, когда в магазине
     * было наибольшее количество посетителей за день.
     */
    class Program
    {
        static void Main(string[] args)
        {
            DataSet ds = GetDataSet();
            DataTable full = new DataTable();
            foreach (DataTable table in ds.Tables) 
            {
                full.Merge(table);
            }

            var result = from tab in full.AsEnumerable()
                         group tab by tab["time"]
                             into groupDt
                             select new
                             {
                                 Group = groupDt.Key,
                                 Sum = groupDt.Sum(r => decimal.Parse(r["count"].ToString()))
                             };
            var max = result.OrderByDescending(x => x.Sum).First();
            DateTime maxTime = (DateTime)max.Group;
            CultureInfo ci = CultureInfo.InvariantCulture;

            Console.WriteLine("Максимальное количество посетителей "
                + max.Sum + " было в " + maxTime.ToString("HH:mm:ss", ci));

            Console.ReadKey();
        }

        static void SetArray()
        {
            Random r = new Random();
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day;

            try
            {
                DateTime openTime = new DateTime(year, month, day);
                int openHour = 10;
                CultureInfo ci = CultureInfo.InvariantCulture;

                //Console.WriteLine(date1.ToString("hh:mm:ss", ci));
                for (int i = 1; i <= 5; i++)
                {
                    using (StreamWriter generate = new StreamWriter("cash" + i + ".txt"))
                    {
                        for (int j = openHour * 60; j <= (openHour + 8) * 60; j += 30)
                        {
                            generate.WriteLine(
                                openTime.AddMinutes((double)j).ToString("HH:mm:ss", ci)
                                + ";" + (r.Next(0, 10)).ToString() + ";");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Houston we have a problem: " + e.Message);
            }
        }

        static DataSet GetDataSet()
        {
            DataSet ds = new DataSet();
            bool isInit = false;

            while (!isInit)
            {
                try
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        using (StreamReader input = new StreamReader("cash" + i + ".txt"))
                        {
                            string line;
                            isInit = true;
                            char[] separator = { ';' };
                            DataTable table = new DataTable("cash" + i);
                            DataColumn time = new DataColumn("time", typeof(DateTime));
                            DataColumn count = new DataColumn("count", typeof(int));
                            table.Columns.Add(time);
                            table.Columns.Add(count);

                            while ((line = input.ReadLine()) != null)
                            {
                                string[] values = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                                DataRow row = table.NewRow();
                                row[time] = DateTime.Parse(values[0]);
                                row[count] = Convert.ToInt32(values[1]);
                                table.Rows.Add(row);
                            }

                            ds.Tables.Add(table);
                        }
                    }
                }
                catch (FileNotFoundException e)
                {
                    SetArray();
                    Console.WriteLine("Массив сгенерирован.");
                }
            }

            return ds;
        }
    }
}
