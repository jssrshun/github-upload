using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Osero
{
    public partial class Form1 : Form
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")] 
        private static extern bool AllocConsole();


        //2次元配列のボード
        int[,] board; 
        //boardは8×8マス
        const int M_num = 8;  
        

        //盤面の情報
        const int Black = 1;    
        const int White = 2;    //bool型だとemptyを表現できない
        const int Empty = 0;    //0じゃないと初期状態で真っ黒or真っ白   配列宣言時はすべて0だから

        //プレイヤーは2人　先攻は黒
        int player1 = Black;
        int player2 = White;

        public Form1()
        {
            //UI作成
            InitializeComponent();
            //コンソール表示
            AllocConsole();
            Console.WriteLine("=====GAME START=====");
            //UIの大きさ
            ClientSize = new Size()
            {
                Width = 560,
                Height = 520
            };
            //UIの色
            BackColor = Color.DarkGreen;
            //UIのタイトル
            Text = "2人でオセロ";
            BoardInitialize();
        }
        private void BoardInitialize()
        {
            //2次元配列の初期化
            board = new int[M_num, M_num];
            //石の初期配置
            board[M_num / 2 - 1, M_num / 2 - 1] = Black;  //[3,3]
            board[M_num / 2 - 1, M_num / 2] = White;      //[3,4] ●〇
            board[M_num / 2, M_num / 2] = Black;          //[4,4] 〇●
            board[M_num / 2, M_num / 2 - 1] = White;      //[4,3]


            player1 = Black;
            player2 = White;
        }
        
        //ボードを描画するメソッド
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Pen pen = new Pen(Color.Black, 2);
            //ボードのデザイン
            for(int i = 0; i < M_num + 1; i++)
            {
                e.Graphics.DrawLine(pen, i * 40 + 120, 100, i * 40 + 120, 420);
                e.Graphics.DrawLine(pen, 120, i * 40 + 100, 440, i * 40 + 100);
            }
            for (int x = 0; x < M_num; x++)
            {
                for (int y = 0; y < M_num; y++)
                {
                    StoneDesign(e, board[x, y], x * 40 + 122, y * 40 + 102);
                }
            }
            
        }
        
        //board[n,n]=1,2の時に石を描画するメソッド  0はEmpty
        private void StoneDesign(PaintEventArgs e, int board_num, int x, int y)
        {
            //直径35の円を描く
            if (board_num == Black)
            {
                e.Graphics.FillEllipse(Brushes.Black, x, y, 35, 35);
            }
            if (board_num == White)
            {
                e.Graphics.FillEllipse(Brushes.White, x, y, 35, 35);
            }
        }

        //クリックの判定　クリック可能な範囲を決定して石を置く
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            Point clickXY = e.Location;
            
            int x = (clickXY.X - 120) / 40;   //クリックしたX座標は配列上のどこに当たるのかを求める
            int y = (clickXY.Y - 100) / 40;   //クリックしたY座標は配列上のどこに当たるのかを求める
            Console.WriteLine($"Clicked Location : [{x+1}, {y+1}]");

            if (x >= 0 && x < M_num && y >= 0 && y < M_num && board[x, y] == 0)
            {
                var p = Put(x, y);
                if (p > 0)
                {
                    PlayerChange();
                    if (PlayerPass() == true)
                    {
                        PlayerChange();
                        Console.WriteLine("<<Player passed>>");
                        if (PlayerPass() == true)
                        {
                            Console.WriteLine("■Any player cannot put stone.");
                            Console.WriteLine("=====GAME OVER=====");
                        }
                    }
                    Refresh();
                }
            }
        }
        private bool PlayerPass()
        {
            int able_put_count = 0;
            bool passflag = false;

            for (int x = 0; x < M_num; x++)
            {
                for(int y = 0; y < M_num; y++)
                {
                    if (Put(x, y, passflag) != 0)
                    {
                        able_put_count++;
                    }
                }
            }
            if (able_put_count == 0)
            {
                passflag = true;
                return passflag;
            }
            return passflag;
        }
        private void PlayerChange()
        {
            var p = player1;
            player1 = player2;
            player2 = p;
        }

        //---------------------------Put()のオーバーロード-------------------------
        private int Put(int x, int y, bool flag)
        {
            int put_available = 0;

            //指定したマス目の近くに反対色の石があるかを確認する
            if (x >= 0 && x < M_num && y >= 0 && y < M_num && board[x, y] == 0)
            {
                //X座標、Y座標が0、7の場合はチェックする方向を絞る必要がある
                if (x == 0 || x == 1)
                {
                    if (y == 0 || y == 1)
                    {
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1, flag); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0, flag); }
                        if (board[x + 1, y + 1] == player2) { put_available += CheckDirection(x, y, 1, 1, flag); }
                    }
                    else if (y == 6 || y == 7)
                    {
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1, flag); }
                        if (board[x + 1, y - 1] == player2) { put_available += CheckDirection(x, y, 1, -1, flag); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0, flag); }
                    }
                    else
                    {
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1, flag); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0, flag); }
                        if (board[x + 1, y + 1] == player2) { put_available += CheckDirection(x, y, 1, 1, flag); }
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1, flag); }
                        if (board[x + 1, y - 1] == player2) { put_available += CheckDirection(x, y, 1, -1, flag); }
                    }
                }
                else if (x == 7 || x == 6)
                {
                    if (y == 0 || y == 1)
                    {
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0, flag); }
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1, flag); }
                        if (board[x - 1, y + 1] == player2) { put_available += CheckDirection(x, y, -1, 1, flag); }
                    }
                    else if (y == 6 || y == 7)
                    {
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0, flag); }
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1, flag); }
                        if (board[x - 1, y - 1] == player2) { put_available += CheckDirection(x, y, -1, -1, flag); }
                    }
                    else
                    {
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1, flag); }
                        if (board[x - 1, y + 1] == player2) { put_available += CheckDirection(x, y, -1, 1, flag); }
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0, flag); }
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1, flag); }
                        if (board[x - 1, y - 1] == player2) { put_available += CheckDirection(x, y, -1, -1, flag); }
                    }
                }
                else if (y == 0 || y == 1)
                {
                    if (x == 1 || x == 1)
                    {
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1, flag); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0, flag); }
                        if (board[x + 1, y + 1] == player2) { put_available += CheckDirection(x, y, 1, 1, flag); }
                    }
                    else if (x == 6 || x == 7)
                    {
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0, flag); }
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1, flag); }
                        if (board[x - 1, y + 1] == player2) { put_available += CheckDirection(x, y, -1, 1, flag); }
                    }
                    else
                    {
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1, flag); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0, flag); }
                        if (board[x + 1, y + 1] == player2) { put_available += CheckDirection(x, y, 1, 1, flag); }
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0, flag); }
                        if (board[x - 1, y + 1] == player2) { put_available += CheckDirection(x, y, -1, 1, flag); }
                    }
                }
                else if (y == 7 || y == 6)
                {
                    if (x == 0 || x == 1)
                    {
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1, flag); }
                        if (board[x + 1, y - 1] == player2) { put_available += CheckDirection(x, y, 1, -1, flag); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0, flag); }
                    }
                    else if (x == 6 || x == 7)
                    {
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0, flag); }
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1, flag); }
                        if (board[x - 1, y - 1] == player2) { put_available += CheckDirection(x, y, -1, -1, flag); }
                    }
                    else
                    {
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1, flag); }
                        if (board[x + 1, y - 1] == player2) { put_available += CheckDirection(x, y, 1, -1, flag); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0, flag); }
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0, flag); }
                        if (board[x - 1, y - 1] == player2) { put_available += CheckDirection(x, y, -1, -1, flag); }
                    }
                }
                else
                {
                    if (board[x - 1, y - 1] == player2) { put_available += CheckDirection(x, y, -1, -1, flag); }
                    if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0, flag); }
                    if (board[x - 1, y + 1] == player2) { put_available += CheckDirection(x, y, -1, 1, flag); }
                    if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1, flag); }
                    if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1, flag); }
                    if (board[x + 1, y - 1] == player2) { put_available += CheckDirection(x, y, 1, -1, flag); }
                    if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0, flag); }
                    if (board[x + 1, y + 1] == player2) { put_available += CheckDirection(x, y, 1, 1, flag); }
                }
            }
            return put_available;
        }

        //---------------------CheckDirection()のオーバーロード------------------------------
        private int CheckDirection(int x, int y, int dX, int dY, bool flag)
        {
            int myStone = GetMyStone(x, y, dX, dY, flag);   //引っくり返せる石の数
            return myStone;
        }
        //---------------------GetMyStone()のオーバーロード----------------------------------
        private int GetMyStone(int x, int y, int dX, int dY, bool flag)
        {
            int myStone = 0;
            int X = x + dX;
            int Y = y + dY;
            //何個反対色の石が続くか
            while (X >= 0 && X < M_num && Y >= 0 && Y < M_num && board[X, Y] == player2)
            {
                X = X + dX;
                Y = Y + dY;   //dX,dY上に探索範囲を伸ばす
                myStone++;
            }
            //反対色の石の先に自色の石があるか
            if (X >= 0 && X < M_num && Y >= 0 && Y < M_num && board[X, Y] == player1)
            {
                return myStone;
            }
            else
            {
                return 0;
            }
        }


        //石を置けるかを判定する
        private int Put(int x, int y)
        {
            int put_available = 0;

            //指定したマス目の近くに反対色の石があるかを確認する
            if (x >= 0 && x < M_num && y >= 0 && y < M_num && board[x, y] == 0)
            {
                //X座標、Y座標が0、7の場合はチェックする方向を絞る必要がある
                if (x == 0 || x == 1)
                {
                    if (y == 0 || y == 1)
                    {
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0); }
                        if (board[x + 1, y + 1] == player2) { put_available += CheckDirection(x, y, 1, 1); }
                    }else if(y == 6 || y == 7)
                    {
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1); }
                        if (board[x + 1, y - 1] == player2) { put_available += CheckDirection(x, y, 1, -1); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0); }
                    }else
                    {
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0); }
                        if (board[x + 1, y + 1] == player2) { put_available += CheckDirection(x, y, 1, 1); }
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1); }
                        if (board[x + 1, y - 1] == player2) { put_available += CheckDirection(x, y, 1, -1); }
                    }
                }else if (x == 7 || x == 6)
                {
                    if (y == 0 || y == 1)
                    {
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0); }
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1); }
                        if (board[x - 1, y + 1] == player2) { put_available += CheckDirection(x, y, -1, 1); }
                    }
                    else if (y == 6 || y ==7)
                    {
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0); }
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1); }
                        if (board[x - 1, y - 1] == player2) { put_available += CheckDirection(x, y, -1, -1); }
                    }
                    else
                    {
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1); }
                        if (board[x - 1, y + 1] == player2) { put_available += CheckDirection(x, y, -1, 1); }
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0); }
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1); }
                        if (board[x - 1, y - 1] == player2) { put_available += CheckDirection(x, y, -1, -1); }
                    }
                }else if (y == 0 || y == 1)
                {
                    if (x == 1 || x == 1)
                    {
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0); }
                        if (board[x + 1, y + 1] == player2) { put_available += CheckDirection(x, y, 1, 1); }
                    }
                    else if(x == 6 || x == 7)
                    {
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0); }
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1); }
                        if (board[x - 1, y + 1] == player2) { put_available += CheckDirection(x, y, -1, 1); }
                    }
                    else {
                        if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0); }
                        if (board[x + 1, y + 1] == player2) { put_available += CheckDirection(x, y, 1, 1); }
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0); }
                        if (board[x - 1, y + 1] == player2) { put_available += CheckDirection(x, y, -1, 1); }
                    }
                }else if(y == 7 || y == 6)
                {
                    if(x == 0 || x == 1)
                    {
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1); }
                        if (board[x + 1, y - 1] == player2) { put_available += CheckDirection(x, y, 1, -1); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0); }
                    }else if(x == 6 || x == 7)
                    {
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0); }
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1); }
                        if (board[x - 1, y - 1] == player2) { put_available += CheckDirection(x, y, -1, -1); }
                    }else
                    {
                        if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1); }
                        if (board[x + 1, y - 1] == player2) { put_available += CheckDirection(x, y, 1, -1); }
                        if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0); }
                        if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0); }
                        if (board[x - 1, y - 1] == player2) { put_available += CheckDirection(x, y, -1, -1); }
                    }
                }
                else
                {
                    if (board[x - 1, y - 1] == player2) { put_available += CheckDirection(x, y, -1, -1); }
                    if (board[x - 1, y] == player2) { put_available += CheckDirection(x, y, -1, 0); }
                    if (board[x - 1, y + 1] == player2) { put_available += CheckDirection(x, y, -1, 1); }
                    if (board[x, y - 1] == player2) { put_available += CheckDirection(x, y, 0, -1); }
                    if (board[x, y + 1] == player2) { put_available += CheckDirection(x, y, 0, 1); }
                    if (board[x + 1, y - 1] == player2) { put_available += CheckDirection(x, y, 1, -1); }
                    if (board[x + 1, y] == player2) { put_available += CheckDirection(x, y, 1, 0); }
                    if (board[x + 1, y + 1] == player2) { put_available += CheckDirection(x, y, 1, 1); }
                }
            }
            Console.WriteLine($"You can put stone. You got {put_available} stone.");
            if (put_available > 0)
            {
                board[x, y] = player1;
            }
            else
            {
                Console.WriteLine("You cannot put stone.");
            }
            return put_available;
        }
        //置いた石の直線上に反対色の色があるかをチェックし、色を反転させる
        private int CheckDirection(int x, int y, int dX, int dY)
        {
            int myStone = GetMyStone(x, y, dX, dY);   //引っくり返せる石の数
            for (int i = 0; i < myStone; i++)
            {
                board[x + dX * (i + 1), y + dY * (i + 1)] = player1;
            }
            return myStone;
        }
        //引っくり返せる石の数をカウントする
        private int GetMyStone(int x, int y, int dX, int dY)
        {
            int myStone = 0;
            int X = x + dX;
            int Y = y + dY;
            //何個反対色の石が続くか
            while (X >= 0 && X < M_num && Y >= 0 && Y < M_num && board[X, Y] == player2)
            {
                X = X + dX;
                Y = Y + dY;   //dX,dY上に探索範囲を伸ばす
                myStone++;
            }
            //反対色の石の先に自色の石があるか
            if (X >= 0 && X < M_num && Y >= 0 && Y < M_num && board[X, Y] == player1)
            {
                return myStone;
            }
            else
            {     
                return 0;
            }
        }
    }
}
