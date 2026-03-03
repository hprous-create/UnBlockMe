// Hector Prous Arroyo
// Javier Hoyos Giunta

using System;
using System.IO;

internal class Program
{
    // coordenadas (x,y) para representar posiciones y direcciones de desplazamiento
    struct Coor
    {
        public int x, y;
    }

    struct Estado
    {// estado del juego

        public char[,] mat;
        // '#' muro; '.' libre; letras 'a','b' ... bloques

        public char obj;
        // char correspondiente al bloque objetivo (el que hay que sacar)

        public Coor act, sal; // posiciones del cursor y de la salida
        public bool sel;
        // indica si hay bloque seleccionado para mover o no
    }

    // Un movimiento queda definido por la posición del cursor y la dirección
    struct Jugada
    {
        public Coor pos; // posición del cursor cuando se realizó el movimiento
        public Coor dir; // dirección del movimiento
    }

    // Historial de movimientos realizados
    struct Memoria
    {
        public Jugada[] jugadas; // array de jugadas (máximo 100)
        public int indice;       // índice a la primera posición libre
    }

    // 'a'->1, 'b'->2... descartar el 0=negro
    static int BloqueToInt(char c)
    {
        return ((int)c) - ((int)'a') + 1;
    }

    static Estado LeeNivel(string file, int n) // A partir de un archivo extrae su información y crea un nivel con bordes y bloques
    {
        Estado est = new Estado();
        StreamReader sr = new StreamReader(file);
        string linea;
        string[] filas = new string[100];
        int numFilas = 0;
        int numCols = 0;

        bool encontrado = false;
        while (!encontrado && (linea = sr.ReadLine()) != null)
        {
            string[] partes = linea.Split(' ');
            if (partes.Length == 2 && partes[0] == "level" && int.Parse(partes[1]) == n)
            {
                encontrado = true;
            }
        }

        if (!encontrado)
        {
            sr.Close();
            Console.WriteLine("Nivel " + n + " no encontrado");
            return est;
        }

        est.obj = sr.ReadLine()[0];

        while ((linea = sr.ReadLine()) != null && linea != "")
        {
            filas[numFilas] = linea;
            numFilas++;
        }
        sr.Close();

        for (int i = 0; i < numFilas; i++)
        {
            if (filas[i].Length > numCols) numCols = filas[i].Length;
        }

        est.mat = new char[numFilas + 2, numCols + 2];

        for (int i = 0; i < numFilas + 2; i++)
        {
            for (int j = 0; j < numCols + 2; j++)
            {
                est.mat[i, j] = '#';
            }
        }

        for (int i = 0; i < numFilas; i++)
        {
            for (int j = 0; j < filas[i].Length; j++)
            {
                est.mat[i + 1, j + 1] = filas[i][j];
            }
        }

        est.act.x = 1;
        est.act.y = 1;
        est.sal.x = 0;
        est.sal.y = 0;
        est.sel = false;

        return est;
    }

    static void Render(Estado est) // Escribe en consola una representación visual del tablero de juego y la interfaz
    {
        ConsoleColor colorHueco = ConsoleColor.DarkGray;
        ConsoleColor colorMuro = ConsoleColor.Gray;

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

        int filas = est.mat.GetLength(0);
        int cols = est.mat.GetLength(1);

        Console.Clear();

        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                char celda = est.mat[i, j];
                bool esCursor = (i == est.act.x && j == est.act.y);
                bool esSalida = (i == est.sal.x && j == est.sal.y);

                if (esSalida)
                {
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
                    {
                        Console.Write("**");
                    }
                    else Console.Write("  ");
                }
                else
                {
                    ConsoleColor colorBloque;
                    if (celda == est.obj)
                    {
                        colorBloque = ConsoleColor.Green;
                    }
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
                        Console.ForegroundColor = colorBloque;
                        Console.Write("  ");
                    }
                }
            }

            Console.ResetColor();
            Console.WriteLine();
        }

        Console.ResetColor();
        Console.Write("\nObjetivo: ");
        Console.BackgroundColor = ConsoleColor.Green;
        Console.Write("  ");
        Console.ResetColor();
        Console.WriteLine();
    }

    static void MarcaSalida(ref Estado est) // Marca la salida del nivel en función de la orientación del bloque objetivo
    {
        int filas = est.mat.GetLength(0);
        int cols = est.mat.GetLength(1);

        int fObj = -1, cObj = -1;
        for (int i = 0; i < filas && fObj == -1; i++)
        {
            for (int j = 0; j < cols && fObj == -1; j++)
            {
                if (est.mat[i, j] == est.obj)
                {
                    fObj = i; cObj = j;
                }
            }
        }

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

    static void MueveCursor(ref Estado est, Coor dir) // Mueve el cursor en la dirección correspondiente transformando su posición
    {
        if (!est.sel)
        {
            int nx = est.act.x + dir.x;
            int ny = est.act.y + dir.y;

            int filas = est.mat.GetLength(0);
            int cols = est.mat.GetLength(1);

            bool dentroDelTablero = (nx >= 0 && nx < filas && ny >= 0 && ny < cols);
            bool noEsMuro = dentroDelTablero && est.mat[nx, ny] != '#';
            bool noEsSalida = !(nx == est.sal.x && ny == est.sal.y);

            if (dentroDelTablero && noEsMuro && noEsSalida)
            {
                est.act.x = nx;
                est.act.y = ny;
            }
        }
    }

    static Coor BuscaCabeza(ref Estado est, Coor dir) // Devuelve la "cabeza" del bloque seleccionado
    {
        char bloque = est.mat[est.act.x, est.act.y];
        Coor cabeza = est.act;

        int filas = est.mat.GetLength(0);
        int cols = est.mat.GetLength(1);

        int nx = cabeza.x + dir.x;
        int ny = cabeza.y + dir.y;

        while (nx >= 0 && nx < filas && ny >= 0 && ny < cols && est.mat[nx, ny] == bloque)
        {
            cabeza.x = nx;
            cabeza.y = ny;
            nx = cabeza.x + dir.x;
            ny = cabeza.y + dir.y;
        }

        return cabeza;
    }

    static void MueveBloque(ref Estado est, Coor dir) // Mueve un bloque en la dirección correspondiente si no hay ningún obstáculo delante y solo en la orientación (horizontal o vertical) del bloque
    {
        if (est.sel)
        {
            char bloque = est.mat[est.act.x, est.act.y];

            int filas = est.mat.GetLength(0);
            int cols = est.mat.GetLength(1);

            bool esBloque = (bloque != '.' && bloque != '#');

            bool horizontal = false;
            if (est.act.y + 1 < cols && est.mat[est.act.x, est.act.y + 1] == bloque)
            {
                horizontal = true;
            }
            if (est.act.y - 1 >= 0 && est.mat[est.act.x, est.act.y - 1] == bloque)
            {
                horizontal = true;
            }

            bool direccionValida = (horizontal && dir.x == 0) || (!horizontal && dir.y == 0);

            if (esBloque && direccionValida)
            {
                Coor cabeza = BuscaCabeza(ref est, dir);
                int nx = cabeza.x + dir.x;
                int ny = cabeza.y + dir.y;

                bool dentroDelTablero = (nx >= 0 && nx < filas && ny >= 0 && ny < cols);
                bool esSalida = (nx == est.sal.x && ny == est.sal.y);
                bool destinoLibre = dentroDelTablero && (est.mat[nx, ny] == '.' || esSalida);

                if (destinoLibre)
                {
                    Coor dirOpuesta = new Coor();
                    dirOpuesta.x = -dir.x;
                    dirOpuesta.y = -dir.y;
                    Coor cola = BuscaCabeza(ref est, dirOpuesta);

                    est.mat[nx, ny] = bloque;
                    est.mat[cola.x, cola.y] = '.';

                    est.act.x += dir.x;
                    est.act.y += dir.y;
                }
            }
        }
    }

    static void GuardaJugada(ref Memoria mem, Coor pos, Coor dir) // Añade el movimiento al historial si hay espacio disponible
    {
        if (mem.indice < mem.jugadas.Length)
        {
            mem.jugadas[mem.indice].pos = pos;
            mem.jugadas[mem.indice].dir = dir;
            mem.indice++;
        }
    }

    static void DeshaceJugada(ref Estado est, ref Memoria mem) // Deshace el último movimiento: mueve el cursor a donde estaba y aplica la dirección opuesta para revertir el bloque
    {
        if (mem.indice > 0)
        {
            mem.indice--;

            Coor posAnterior = mem.jugadas[mem.indice].pos;
            Coor dirOriginal = mem.jugadas[mem.indice].dir;

            // Volver el cursor a donde estaba cuando se hizo el movimiento
            est.act = posAnterior;

            // Construir la dirección opuesta
            Coor dirOpuesta = new Coor();
            dirOpuesta.x = -dirOriginal.x;
            dirOpuesta.y = -dirOriginal.y;

            // Activar sel temporalmente para poder llamar a MueveBloque
            est.sel = true;
            MueveBloque(ref est, dirOpuesta);
            est.sel = false;
        }
    }

    static void ProcesaInput(ref Estado est, ref Memoria mem, char c) // Actualiza el estado y la memoria según el input recibido. Guarda la jugada solo si el bloque se movió realmente.
    {
        Coor dir = new Coor();

        switch (c)
        {
            case 's':
                char celdaAct = est.mat[est.act.x, est.act.y];
                if (celdaAct != '.' && celdaAct != '#') est.sel = !est.sel;
                break;

            case 'u':
                dir.x = -1; dir.y = 0;
                if (est.sel)
                {
                    Coor posAntes = est.act;
                    MueveBloque(ref est, dir);
                    if (est.act.x != posAntes.x || est.act.y != posAntes.y)
                    {
                        GuardaJugada(ref mem, posAntes, dir);
                    }
                }
                else MueveCursor(ref est, dir);
                break;

            case 'd':
                dir.x = 1; dir.y = 0;
                if (est.sel)
                {
                    Coor posAntes = est.act;
                    MueveBloque(ref est, dir);
                    if (est.act.x != posAntes.x || est.act.y != posAntes.y)
                    {
                        GuardaJugada(ref mem, posAntes, dir);
                    }
                }
                else MueveCursor(ref est, dir);
                break;

            case 'l':
                dir.x = 0; dir.y = -1;
                if (est.sel)
                {
                    Coor posAntes = est.act;
                    MueveBloque(ref est, dir);
                    if (est.act.x != posAntes.x || est.act.y != posAntes.y)
                    {
                        GuardaJugada(ref mem, posAntes, dir);
                    }
                }
                else MueveCursor(ref est, dir);
                break;

            case 'r':
                dir.x = 0; dir.y = 1;
                if (est.sel)
                {
                    Coor posAntes = est.act;
                    MueveBloque(ref est, dir);
                    if (est.act.x != posAntes.x || est.act.y != posAntes.y)
                    {
                        GuardaJugada(ref mem, posAntes, dir);
                    }
                }
                else MueveCursor(ref est, dir);
                break;

            case 'z':
                DeshaceJugada(ref est, ref mem);
                break;
        }
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
                    case "LeftArrow": d = 'l'; break; // direcciones
                    case "UpArrow": d = 'u'; break;
                    case "RightArrow": d = 'r'; break;
                    case "DownArrow": d = 'd'; break;
                    case "Delete": d = 'z'; break; // deshacer jugada
                    case "Escape": d = 'q'; break; // salir
                    case "Spacebar": d = 's'; break; // selección de bloque
                }
            }
        }
        return d;
    }


    static void GestionaRecord(int nivel, int movimientos) // Busca el record del nivel en records.txt. Si no existe lo crea. Si existe compara y guarda el menor.
    {
        string[] lineas = new string[100];
        int numLineas = 0;
        StreamReader sr = new StreamReader("records.txt");

        // Leer el archivo si existe
        if (sr != null)
        {
            string linea;
            while ((linea = sr.ReadLine()) != null && numLineas < lineas.Length)
            {
                lineas[numLineas] = linea;
                numLineas++;
            }
            sr.Close();
        }

        // Buscar si ya hay un record para este nivel
        int posRecord = -1;
        for (int i = 0; i < numLineas && posRecord == -1; i++)
        {
            string[] partes = lineas[i].Split(' ');
            if (partes.Length == 2 && int.Parse(partes[0]) == nivel)
            {
                posRecord = i;
            }
        }

        bool nuevoRecord = false;

        if (posRecord == -1)
        {
            // Primera vez que se completa este nivel
            lineas[numLineas] = nivel + " " + movimientos;
            numLineas++;
            nuevoRecord = true;
        }
        else
        {
            // Ya existe: comparar con el record guardado
            int recordGuardado = int.Parse(lineas[posRecord].Split(' ')[1]);
            if (movimientos < recordGuardado)
            {
                lineas[posRecord] = nivel + " " + movimientos;
                nuevoRecord = true;
            }
        }

        // Reescribir el archivo con los datos actualizados
        StreamWriter sw = new StreamWriter("records.txt");
        for (int i = 0; i < numLineas; i++)
        {
            sw.WriteLine(lineas[i]);
        }
        sw.Close();

        // Informar al usuario del resultado
        if (nuevoRecord)
        {
            Console.WriteLine("¡Nuevo record en el nivel " + nivel + "! " + movimientos + " movimientos.");
        }

        else
        {
            int recordGuardado = int.Parse(lineas[posRecord].Split(' ')[1]);
            Console.WriteLine("Record del nivel " + nivel + ": " + recordGuardado + " movimientos. Esta vez: " + movimientos + ".");
        }
    }

    static void Main(string[] args)
    {
        Console.Write("Introduce el número de nivel: ");
        int nivel = int.Parse(Console.ReadLine());

        Estado est = LeeNivel("levels.txt", nivel);
        MarcaSalida(ref est);
        Render(est);

        // Inicializar memoria de jugadas
        Memoria mem = new Memoria();
        mem.jugadas = new Jugada[100];
        mem.indice = 0;

        char input = ' ';

        while (est.mat[est.sal.x, est.sal.y] != est.obj && input != 'q')
        {
            input = LeeInput();
            if (input != 'q')
            {
                ProcesaInput(ref est, ref mem, input);
                Render(est);
            }
        }

        Console.ResetColor();
        if (est.mat[est.sal.x, est.sal.y] == est.obj)
        {
            Console.WriteLine("¡Felicidades! Has completado el nivel en " + mem.indice + " movimientos.");
            GestionaRecord(nivel, mem.indice);
        }
        else Console.WriteLine("Juego terminado");
    }
}