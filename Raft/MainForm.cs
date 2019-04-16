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
using System.Threading;

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
        private int startPointX = 0;
        private int startPointY = 0;

        enum Direction : int
        {
            RIGHT = 0,
            DOWN = 1,
            LEFT = 2,
            UP = 3
        };
        
        struct Coordinates  //Структура для хранения координат
        {
            public int x;
            public int y;
            public Direction direct;
        }

        private Stack<Coordinates> stack = new Stack<Coordinates>(); //Стек для создания пути
        private Coordinates coordinates; //Поле для работы со структурой

        public MainForm()
        {
            try
            {
                InitializeComponent();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        /*суть данной функции - подогнать матрицу под размеры плота, чтобы плот любого размера занимал одну ячейку. Таким образом, в жертву
         приносится размер матрицы, что, в принципе, не должно сильно сказаться на происходящем на экране*/
        private int[,] Raft_and_square()
        {
            try
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
                for (int i2 = 0; i2 < heigth2; i2++)
                    for (int j2 = 0; j2 < width2; j2++)
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return null;
            }
        }
        
        /*Данная функция реализует алгоритм Ли*/
        private bool Lee_algorithm()
        {
            try
            {
                //сдвиги: направо, вниз, налево, вверх
                int[] dx = { 1, 0, -1, 0 };
                int[] dy = { 0, 1, 0, -1 };
                //проверка, сидит ли плот на мели
                if (field[startPointX, startPointY] == 1)
                {
                    isStrand = true;
                    return false;
                }
                //задание определяющего значения для пути для поля на старте, специальной переменной для отметки поля и булевой переменной для
                //выхода из цикла
                field[startPointX, startPointY] = 2;
                bool stop;
                int d = 2;
                //непосредственная реализация нахождения пути по алгоритму Ли
                int iy, ix;
                do
                {
                    stop = true;
                    for (int i = 0; i < heigth; i++)
                        for (int j = 0; j < width; j++)
                            if (field[i, j] == d)
                            {
                                for (int k = 0; k < 4; k++)
                                {
                                    //реализация сдвига
                                    iy = i + dy[k];
                                    ix = j + dx[k];
                                    if ((iy >= 0) && (iy < heigth) && (ix >= 0) && (ix < width) && (field[iy, ix] == 0))
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
                if (field[heigth - 1, width - 1] == 0)
                {
                    isStrand = false;
                    return false;
                }
                //реализуем восстановление пути из конечной точки
                d = field[heigth - 1, width - 1];
                int x = width - 1;
                int y = heigth - 1;
                //непосредственная реализация восстановления с отметкой пути в таблице
                while (d > 2)
                {
                    //dataGridView1[x, y].Value = Resources._2;
                    coordinates.x = x;
                    coordinates.y = y;
                    d--;
                    for (int k = 0; k < 4; k++)
                    {
                        iy = y + dy[k];
                        ix = x + dx[k];
                        if ((iy >= 0) && (iy < heigth) && (ix >= 0) && (ix < width) && (field[iy, ix] == d))
                        {
                            x += dx[k];
                            y += dy[k];
                            switch(k)
                            {
                                case 0:
                                    coordinates.direct = Direction.LEFT;
                                    break;
                                case 1:
                                    coordinates.direct = Direction.UP;
                                    break;
                                case 2:
                                    coordinates.direct = Direction.RIGHT;
                                    break;
                                case 3:
                                    coordinates.direct = Direction.DOWN;
                                    break;
                            }
                            break;
                        }
                    }
                    stack.Push(coordinates);
                }
                //dataGridView1[0, 0].Value = Resources._2;
                isStrand = false;
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
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
                    dataGridView1.Columns.Add(new DataGridViewImageColumn()
                    {
                        Width = (dataGridView1.Width - width) / width,
                        Image = Resources._0
                    });

                for (int i = 0; i < heigth; i++)
                    dataGridView1.Rows.Add(new DataGridViewRow() { Height = (dataGridView1.Height - heigth) / heigth });

                //активируем следующий этап с выводом файла с правилами
                if (!isRead)
                {
                    Rules rules = new Rules();
                    rules.Show();
                    isRead = true;
                }
                button5.Enabled = true;
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //задаем индексы колонки и строки
                int colInd = e.ColumnIndex;
                int rowInd = e.RowIndex;

                //Проверяем, устанавливаем ли начальную точку
                if (button5.Enabled)
                {
                    startPointX = rowInd;
                    startPointY = colInd;
                    dataGridView1[startPointX, startPointY].Value = Resources._3;
                }
                else
                {
                    //Проверяем, отмечено ли поле, и делаем соответсвующие изменения (_1 - поле отмечено, _0 - нет)
                    if (field[rowInd, colInd] == 0)
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                size = Convert.ToInt32(comboBox2.Text);

                //Создание плота
                for (int i = startPointX; i < size; i++)
                {
                    for (int j = startPointY; j < size; j++)
                    {
                        dataGridView1[i, j].Value = Resources._3;
                    }
                }

                //запускаем третью кнопку и блокируем остальные поля и кнопки
                button3.Enabled = true;
                comboBox3.Enabled = false;
                button2.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                //если размер плота не стандартный, то применяется ф-кция изменения матрицы
                if (size != 1)
                    field = Raft_and_square();
                //если алгоритм Ли выдал ложь, выводим нужное сообщение, иначе рисуем путь
                if (!Lee_algorithm())
                {
                    MsgBox msg = new MsgBox(isStrand);
                    msg.Show();
                }
                else
                {
                    coordinates.x = startPointX;
                    coordinates.y = startPointY;
                    while (stack.Count != 0)
                    {
                        dataGridView1[coordinates.x, coordinates.y].Value = Resources._2;
                        coordinates = stack.Pop();
                        for(int i = 0; i < size; i++)
                        {
                            for(int j = 0; j < size; j++)
                            {
                                dataGridView1[coordinates.x + i, coordinates.y + j].Value = Resources._3;
                            }
                        }
                        switch(coordinates.direct)
                        {
                            case Direction.RIGHT:
                                for(int i = 1; i < size; i++)
                                {
                                    dataGridView1[coordinates.x - 1, coordinates.y + i].Value = Resources._0;
                                }
                                break;
                            case Direction.DOWN:
                                for (int i = 1; i < size; i++)
                                {
                                    dataGridView1[coordinates.x + i, coordinates.y - 1].Value = Resources._0;
                                }
                                break;
                            case Direction.LEFT:
                                for (int i = 1; i < size; i++)
                                {
                                    dataGridView1[coordinates.x + size, coordinates.y + i].Value = Resources._0;
                                }
                                break;
                            case Direction.UP:
                                for (int i = 1; i < size; i++)
                                {
                                    dataGridView1[coordinates.x + 1, coordinates.y + size].Value = Resources._0;
                                }
                                break;
                        }
                        Application.DoEvents();
                        Thread.Sleep(1000);
                    }
                }
                //приводим программу в исходное состояние
                button1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;
                comboBox1.Enabled = comboBox2.Enabled = comboBox3.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        //Завершение работы с программой
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;
            button5.Enabled = false;
        }
    }
}
