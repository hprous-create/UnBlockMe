// Hector Prous Arroyo
//Javier Hoyos Hiunta

using System;
using System.IO;

struct Coor
{
    public int x, y;
}

struct Estado
{
    public char[,] mat;
    public char obj;
    public Coor act;
    public Coor sal;
    public bool sel;
}

class Program
{
    // 'a'->1, 'b'->2, ...
    static int BloqueToInt(char c)
    {
        return ((int)c) - ((int)'a') + 1;
    }

    // ─────────────────────────────────────────
    // LeeNivel
    // ─────────────────────────────────────────
    static Estado LeeNivel(string file, int n)
    {
        Estado est = new Estado();
        StreamReader sr = new StreamReader(file);
        string linea;

        bool encontrado = false;
        while (!encontrado && (linea = sr.ReadLine()) != null)
        {
            string[] partes = linea.Split(' ');
            if (partes.Length == 2 && partes[0] == "level" && int.Parse(partes[1]) == n)
                encontrado = true;
        }

        if (!encontrado)
        {
            sr.Close();
            Console.WriteLine("Nivel " + n + " no encontrado.");
            return est;
        }

        est.obj = sr.ReadLine()[0];

        string[] filas = new string[100];
        int numFilas = 0;
        while ((linea = sr.ReadLine()) != null && linea != "")
        {
            filas[numFilas] = linea;
            numFilas++;
        }
        sr.Close();

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

        est.act.x = 1;
        est.act.y = 1;
        est.sal.x = 0;
        est.sal.y = 0;
        est.sel = false;

        return est;
    }

    // ─────────────────────────────────────────
    // Render
    // ─────────────────────────────────────────
    static void Render(Estado est)
    {
        ConsoleColor colorHueco = ConsoleColor.DarkGray; // fondo gris oscuro
        ConsoleColor colorMuro = ConsoleColor.Gray;     // paredes gris claro

        // Paleta de colores para los bloques (inspirada en Fig. 2)
        ConsoleColor[] coloresBloques = {
            ConsoleColor.Green,
            ConsoleColor.Blue,
            ConsoleColor.Red,
            ConsoleColor.Cyan,
            ConsoleColor.Magenta,
            ConsoleColor.Yellow,
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkYellow,
            ConsoleColor.DarkGreen
        };

        Console.Clear();

        int filas = est.mat.GetLength(0);
        int cols = est.mat.GetLength(1);

        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                char celda = est.mat[i, j];
                bool esCursor = (i == est.act.x && j == est.act.y);
                bool esSalida = (i == est.sal.x && j == est.sal.y);

                if (esSalida)
                {
                    // Apertura en el muro: mismo color que el hueco
                    Console.BackgroundColor = colorHueco;
                    Console.Write("  ");
                }
                else if (celda == '#')
                {
                    Console.BackgroundColor = colorMuro;
                    Console.Write("  ");
                }
                else if (celda == '.')
                {
                    Console.BackgroundColor = colorHueco;
                    Console.ForegroundColor = ConsoleColor.White;
                    if (esCursor)
                        Console.Write("**");
                    else
                        Console.Write("  ");
                }
                else
                {
                    // Bloque: objetivo siempre verde, resto por índice
                    ConsoleColor colorBloque;
                    if (celda == est.obj)
                        colorBloque = ConsoleColor.Green;
                    else
                    {
                        int idx = (BloqueToInt(celda) - 1) % coloresBloques.Length;
                        colorBloque = coloresBloques[idx];
                    }

                    Console.BackgroundColor = colorBloque;
                    if (esCursor)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(est.sel ? "<>" : "**");
                    }
                    else
                    {
                        Console.ForegroundColor = colorBloque; // mismo color → letra invisible
                        Console.Write("  ");
                    }
                }
            }

            Console.ResetColor();
            Console.WriteLine();
        }

        // Indicador del bloque objetivo
        Console.ResetColor();
        Console.Write("\nObjetivo: ");
        Console.BackgroundColor = ConsoleColor.Green;
        Console.Write("  ");
        Console.ResetColor();
        Console.WriteLine();
    }

    // ─────────────────────────────────────────
    // MarcaSalida
    // ─────────────────────────────────────────
    static void MarcaSalida(ref Estado est)
    {
        int filas = est.mat.GetLength(0);
        int cols = est.mat.GetLength(1);

        int fObj = -1, cObj = -1;
        for (int i = 0; i < filas && fObj == -1; i++)
            for (int j = 0; j < cols && fObj == -1; j++)
                if (est.mat[i, j] == est.obj)
                { fObj = i; cObj = j; }

        bool horizontal = (cObj + 1 < cols && est.mat[fObj, cObj + 1] == est.obj);

        if (horizontal)
        {
            est.sal.x = fObj;
            est.sal.y = cols - 1;
        }
        else
        {
            est.sal.x = filas - 1;
            est.sal.y = cObj;
        }
    }

    // ─────────────────────────────────────────
    // MueveCursor
    // ─────────────────────────────────────────
    static void MueveCursor(ref Estado est, Coor dir)
    {
        if (est.sel) return;

        int nx = est.act.x + dir.x;
        int ny = est.act.y + dir.y;

        int filas = est.mat.GetLength(0);
        int cols = est.mat.GetLength(1);

        if (nx < 0 || nx >= filas || ny < 0 || ny >= cols) return;
        if (est.mat[nx, ny] == '#') return;
        if (nx == est.sal.x && ny == est.sal.y) return;

        est.act.x = nx;
        est.act.y = ny;
    }

    // ─────────────────────────────────────────
    // BuscaCabeza (auxiliar de MueveBloque)
    // ─────────────────────────────────────────
    static Coor BuscaCabeza(ref Estado est, Coor dir)
    {
        char bloque = est.mat[est.act.x, est.act.y];
        Coor cabeza = est.act;

        int filas = est.mat.GetLength(0);
        int cols = est.mat.GetLength(1);

        while (true)
        {
            int nx = cabeza.x + dir.x;
            int ny = cabeza.y + dir.y;
            if (nx < 0 || nx >= filas || ny < 0 || ny >= cols) break;
            if (est.mat[nx, ny] != bloque) break;
            cabeza.x = nx;
            cabeza.y = ny;
        }

        return cabeza;
    }

    // ─────────────────────────────────────────
    // MueveBloque
    // Solo permite moverse en el eje natural del bloque:
    //   horizontal → izquierda/derecha
    //   vertical   → arriba/abajo
    // ─────────────────────────────────────────
    static void MueveBloque(ref Estado est, Coor dir)
    {
        if (!est.sel) return;

        char bloque = est.mat[est.act.x, est.act.y];
        if (bloque == '.' || bloque == '#') return;

        int filas = est.mat.GetLength(0);
        int cols = est.mat.GetLength(1);

        // Determinar si el bloque es horizontal
        bool horizontal = false;
        if (est.act.y + 1 < cols && est.mat[est.act.x, est.act.y + 1] == bloque)
            horizontal = true;
        if (est.act.y - 1 >= 0 && est.mat[est.act.x, est.act.y - 1] == bloque)
            horizontal = true;

        if (horizontal && dir.x != 0) return; // bloque horizontal no puede ir arriba/abajo
        if (!horizontal && dir.y != 0) return; // bloque vertical no puede ir izquierda/derecha

        Coor cabeza = BuscaCabeza(ref est, dir);
        int nx = cabeza.x + dir.x;
        int ny = cabeza.y + dir.y;

        if (nx < 0 || nx >= filas || ny < 0 || ny >= cols) return;

        char destino = est.mat[nx, ny];
        bool esSalida = (nx == est.sal.x && ny == est.sal.y);

        if (destino != '.' && !esSalida) return;

        Coor dirOpuesta = new Coor();
        dirOpuesta.x = -dir.x;
        dirOpuesta.y = -dir.y;
        Coor cola = BuscaCabeza(ref est, dirOpuesta);

        est.mat[nx, ny] = bloque;
        est.mat[cola.x, cola.y] = '.';

        est.act.x += dir.x;
        est.act.y += dir.y;
    }

    // ─────────────────────────────────────────
    // ProcesaInput
    // ─────────────────────────────────────────
    static void ProcesaInput(ref Estado est, char c)
    {
        Coor dir = new Coor();

        switch (c)
        {
            case 's':
                char celdaAct = est.mat[est.act.x, est.act.y];
                if (celdaAct != '.' && celdaAct != '#')
                    est.sel = !est.sel;
                break;

            case 'u':
                dir.x = -1; dir.y = 0;
                if (est.sel) MueveBloque(ref est, dir);
                else MueveCursor(ref est, dir);
                break;

            case 'd':
                dir.x = 1; dir.y = 0;
                if (est.sel) MueveBloque(ref est, dir);
                else MueveCursor(ref est, dir);
                break;

            case 'l':
                dir.x = 0; dir.y = -1;
                if (est.sel) MueveBloque(ref est, dir);
                else MueveCursor(ref est, dir);
                break;

            case 'r':
                dir.x = 0; dir.y = 1;
                if (est.sel) MueveBloque(ref est, dir);
                else MueveCursor(ref est, dir);
                break;

            case 'z':
                // Deshacer jugada (se implementará más adelante)
                break;
        }
    }

    // ─────────────────────────────────────────
    // LeeInput (proporcionado por el enunciado)
    // ─────────────────────────────────────────
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
                    case "LeftArrow": d = 'l'; break;
                    case "UpArrow": d = 'u'; break;
                    case "RightArrow": d = 'r'; break;
                    case "DownArrow": d = 'd'; break;
                    case "Delete": d = 'z'; break;
                    case "Escape": d = 'q'; break;
                    case "Spacebar": d = 's'; break;
                }
            }
        }
        return d;
    }

    // ─────────────────────────────────────────
    // Main
    // ─────────────────────────────────────────
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.Write("Introduce el número de nivel: ");
        int nivel = int.Parse(Console.ReadLine());

        Estado est = LeeNivel("levels.txt", nivel);
        MarcaSalida(ref est);
        Render(est);

        char input = ' ';

        while (est.mat[est.sal.x, est.sal.y] != est.obj && input != 'q')
        {
            input = LeeInput();
            if (input != 'q')
            {
                ProcesaInput(ref est, input);
                Render(est);
            }
        }

        Console.ResetColor();
        if (est.mat[est.sal.x, est.sal.y] == est.obj)
            Console.WriteLine("¡Felicidades! Has completado el nivel.");
        else
            Console.WriteLine("Juego terminado.");
    }
}