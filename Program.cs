// Javier Hoyos Giunta
// Hector Prous 
using System;
using System.IO;
using System.Numerics;
namespace ConsoleApp1
{
    internal class Program
    {
        // coordenadas (x,y) para representar posiciones y direcciones de desplazamiento
        struct Coor
        {
            public int x, y;
        }
        struct Estado
        { // estado del juego

            public char[,] mat;
            // ’#’ muro; ’.’ libre; letras ’a’,’b’ ... bloques

            public char obj;
            // char correspondiente al bloque objetivo (el que hay que sacar)

            public Coor act, sal; // posiciones del cursor y de la salida
            public bool sel;
            // idica si hay bloque seleccionado para mover o no
        }

        static void Main(string[] args)
        {

        }

        static Estado LeeNivel(string file, int n)
        {

            Estado est = new Estado();

            StreamReader sr = new StreamReader(file)
            {
                string linea;

                // busca nivel 
                bool encontrado = false;
                while (!encontrado && (linea = sr.ReadLine()) != null)
                {
                    string[] partes = linea.Split(' ');
                    if (partes[0] == "level" && int.Parse(partes[1]) == n)
                        encontrado = true;
                }


                // leer bloque objetivo
                est.obj = sr.ReadLine()[0];

                // leer las filas 
                string[] filas = new string[100];
                int numFilas = 0;
                while ((linea = sr.ReadLine()) != null && linea != "")
                {
                    filas[numFilas] = linea;
                    numFilas++;
                }

                // construccion de la matriz
                int numCols = 0;
                for (int i = 0; i < numFilas; i++)
                    if (filas[i].Length > numCols) numCols = filas[i].Length;

                est.mat = new char[numFilas + 2, numCols + 2];

                for (int i = 0; i < numFilas + 2; i++)
                    for (int j = 0; j < numCols + 2; j++)
                        est.mat[i, j] = '#';

                for (int i = 0; i < numFilas; i++)
                    for (int j = 0; j < filas[i].Length; j++)
                        est.mat[i + 1, j + 1] = filas[i][j];

                // cursor
                est.act.x = 1;
                est.act.y = 1;
                est.sal.x = 0;
                est.sal.y = 0;
                est.sel = false;
            }
            return est;
        }

        static void Render(Estado est)
        {

        }

        // ’a’->1, ’b’->2... descartar el 0=negro
        static int BloqueToInt(char c)
        {
            return ((int)c) - ((int)'a') + 1;
        }

        static void MarcaSalida(Estado est)
        {
            int i = 0, j = 0, coordx = 0, coordy = 0, dirx = 0, diry = 0;
            char objetivo = est.obj;
            bool ok = false;

            while (i < est.mat.GetLength(0))
            {
                while (j < est.mat.GetLength(1))
                {
                    if (est.mat[i, j] == objetivo)
                    {
                        coordx = i;
                        coordy = j;

                        BuscaCabeza(); // aquí falta ajustar algo pq el metodo es void y no va a devolver nada, arreglar

                        ok = true;
                    }
                    j++;
                }
                i++;
            }

            if (dirx == 0) // diry tiene que ser uno, la dir es (0,1), vertical
            {
                est.sal.x = coordx;
                // falta aquí sal.y
            }
            else // dirx es 1, diry tiene que ser 0, la dir es (1,0), horizontal
            {
                est.sal.y = coordy;
                // falta aquí sal.x
            }
        }

        static void MueveCursor(Estado est, Coor dir)
        {
            if (!est.sel)
            {
                Coor nuevaPos = new Coor();
                nuevaPos = SumaCoor(est.act, dir);

                if (est.mat[nuevaPos.x, nuevaPos.y] != '#' && !ComparaCoor(nuevaPos, est.sal))
                {
                    est.act = nuevaPos;
                }
            }
        }

        static void MueveBloque(Estado est, Coor dir)
        {

        }

        static void BuscaCabeza()
        {

        }

        static void ProcesaInput(Estado est, char c)
        {

        }

        static char LeeInput()
        {
            char d = ' ';
            while (d == ' ')
            {
                if (Console.KeyAvailable)
                {
                    string tecla = Console.ReadKey().Key.ToString();
                    switch (tecla)
                    {
                        case "LeftArrow": d = 'l'; break; // direccones
                        case "UpArrow":
                            d = 'u'; break;
                        case "RightArrow": d = 'r'; break;
                        case "DownArrow": d = 'd'; break;
                        case "Delete":
                            d = 'z'; break; // deshacer jugada
                        case "Escape":
                            d = 'q'; break; // salir
                        case "Spacebar": d = 's'; break; // selección de bloque
                    }
                }
            }
            return d;
        }

        static bool ComparaCoor(Coor x, Coor y)
        {
            return x.x == y.x && x.y == y.y;
        }

        static Coor SumaCoor(Coor x, Coor y)
        {
            Coor nCoor = new Coor();
            nCoor.x = x.x + y.x;
            nCoor.y = x.y + y.y;
            return nCoor;
        }
    }
}

