using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;
using Raft.Properties;
using System.Diagnostics;

namespace Raft
{
    public partial class MainForm : Form
    {
        private int width;  //ширина
        private int heigth; //высота
        private int size;   //размер плота
        private int[,] field;  //матрица для работы
        private bool isStrand;  //сидит ли плот на мели
        private bool isRead = false; //проверка на то, прочитан ли был файл

        public MainForm()
        {
            InitializeComponent();
        }

        /*суть данной функции - подогнать матрицу под размеры плота, чтобы плот любого размера занимал одну ячейку. Таким образом, в жертву
         приносится размер матрицы, что, в принципе, не должно сильно сказаться на происходящем на экране*/
        private int[,] Raft_and_square()
        {
            //задаем размеры матрицы и матрицу
            int width2 = width - size + 1;
            int heigth2 = heigth - size + 1;
            int[,] field2 = new int[heigth2, width2];

            //задаем индикатор для определния острова в матрице
            int indicator = 0;

            //заполняем все ячейки. Если сумма всех ячеек матрицы, которые будет покрывать плот заданного размера в нужном квадрате
            //равна 0, то соответственно и сама ячейка новой матрицы должна быть равна 0. Если встречается остров (индикатор != 0), то и в
            //данной ячейке матрицы будет остров
            for(int i2 = 0; i2 < heigth2; i2++)
                for(int j2 = 0; j2 < width2; j2++)
                {
                    for (int i = 0; i < size; i++)
                        for (int j = 0; j < size; j++)
                            indicator += field[i2 + i, j2 + j];
                    if (indicator == 0)
                        field2[i2, j2] = 0;
                    else
                        field2[i2, j2] = 1;
                    indicator = 0;
                }
            width = width2;
            heigth = heigth2;
            return field2;
        }
        
        /*Данная функция реализует алгоритм Ли*/
        private bool Lee_algorithm()
        {
            //сдвиги: направо, вниз, налево, вверх
            int[] dx = { 1, 0, -1, 0 };
            int[] dy = { 0, 1, 0, -1 };
            //проверка, сидит ли плот на мели
            if(field[0,0] == 1)
            {
                isStrand = true;
                return false;
            }
            //задание определяющего значения для пути для поля на старте, специальной переменной для отметки поля и булевой переменной для
            //выхода из цикла
            field[0, 0] = 2;
            bool stop;
            int d = 2;
            //непосредственная реализация нахождения пути по алгоритму Ли
            int iy, ix;
            do
            {
                stop = true;
                for (int i = 0; i < heigth; i++)
                    for (int j = 0; j < width; j++)
                        if(field[i,j] == d)
                        {
                            for(int k = 0; k < 4; k++)
                            {
                                //реализация сдвига
                                iy = i + dy[k];
                                ix = j + dx[k];
                                if((iy >= 0) && (iy < heigth) && (ix >= 0) && (ix < width) && (field[iy,ix] == 0))
                                {
                                    //если нет выхода за рамки поля - ставим отметку
                                    stop = false;
                                    field[iy, ix] = d + 1;
                                }
                            }
                        }
                d++;
            } while ((!stop) && (field[heigth - 1, width - 1] == 0));
            //если конечное поле не отмечено - завершаем работу функции, т. к. плот столкнулся со стеной
            if(field[heigth - 1, width - 1] == 0)
            {
                isStrand = false;
                return false;
            }
            //реализуем восстановление пути из конечной точки
            d = field[heigth - 1, width - 1];
            int x = width - 1;
            int y = heigth - 1;
            //непосредственная реализация восстановления с отметкой пути в таблице
            while(d > 2)
            {
                dataGridView1[x, y].Value = Resources._2;
                d--;
                for(int k = 0; k < 4; k++)
                {
                    iy = y + dy[k];
                    ix = x + dx[k];
                    if((iy >= 0) && (iy < heigth) && (ix >= 0) && (ix < width) && (field[iy,ix] == d))
                    {
                        x += dx[k];
                        y += dy[k];
                        break;
                    }
                }
            }
            dataGridView1[0, 0].Value = Resources._2;
            isStrand = false;
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //задаем размер области и плота
            width = Convert.ToInt32(comboBox1.Text);
            heigth = Convert.ToInt32(comboBox3.Text);
            size = Convert.ToInt32(comboBox2.Text);

            //удаляем матрицу и поле в таблице
            field = null;
            while (dataGridView1.Rows.Count != 0)
                dataGridView1.Rows.Remove(dataGridView1.Rows[dataGridView1.Rows.Count - 1]);
            while (dataGridView1.Columns.Count != 0)
                dataGridView1.Columns.Remove(dataGridView1.Columns[dataGridView1.Columns.Count - 1]);

            //создаем матрицу
            field = new int[heigth, width];
            for (int i = 0; i < heigth; i++)
                for (int j = 0; j < width; j++)
                    field[i, j] = 0;
            
            //задаем колонки и столбцы и заполняем их изображением пустого поля
            for (int i = 0; i < width; i++)
                dataGridView1.Columns.Add(new DataGridViewImageColumn() { Width = (dataGridView1.Width - width) / width, 
                    Image = Resources._0 });

            for (int i = 0; i < heigth; i++)
                dataGridView1.Rows.Add(new DataGridViewRow() { Height = (dataGridView1.Height - heigth) / heigth });

            //активируем следующий этап с выводом файла с правилами
            if (!isRead)
            {
                Process.Start(@"C:\\Users\" + System.Environment.UserName + @"\Dropbox\ДЛЯ ПРОГРАММИРОВАНИЯ\С#\Плот\Raft\Rules.txt");
                isRead = true;
            }
            button2.Enabled = true;
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //задаем индексы колонки и строки
            int colInd = e.ColumnIndex;
            int rowInd = e.RowIndex;

            //Проверяем, отмечено ли поле, и делаем соответсвующие изменения (_1 - поле отмечено, _0 - нет)
            if (field[rowInd,colInd] == 0)
            {
                dataGridView1[colInd, rowInd].Value = Resources._1;
                field[rowInd, colInd] = 1;
            }
            else
            {
                dataGridView1[colInd, rowInd].Value = Resources._0;
                field[rowInd, colInd] = 0;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            size = Convert.ToInt32(comboBox2.Text);

            

            //запускаем третью кнопку и блокируем остальные поля и кнопки
            button3.Enabled = true;
            comboBox3.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //если размер плота не стандартный, то применяется ф-кция изменения матрицы
            if (size != 1)
                field = Raft_and_square();
            //если алгоритм Ли выдал ложь, выводим нужное сообщение
            if(!Lee_algorithm())
            {
                MsgBox msg = new MsgBox(isStrand);
                msg.Show();
            }
            //приводим программу в исходное состояние
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            comboBox1.Enabled = comboBox2.Enabled = comboBox3.Enabled = true;
        }
    }
}
