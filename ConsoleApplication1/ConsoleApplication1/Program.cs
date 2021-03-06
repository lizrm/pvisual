/*  Adriana Mora Calvo        B24385
    Lisbeth Rojas Montero     B15745
    Para correr el programa se debe de poner los archivos de los hilos en la carpeta bin/Debug
    Con el numero 1 se indica que se quiere visualizar la ejecucion, con cualquier otro numero se ejecuta en modo rapido
    Leer la documentación externa para información más detallada
    */
using System;
using System.Threading;
using System.IO;
using System.Collections;

namespace MultiThread
{
    class Procesador
    {

        public static int[] memDatos;             // Memoria de datos compartida
        public static int[] memInstruc;           // Memoria de instrucciones compartida
        public static Contextos cola;             // Cola para poder cambiar de contexto entre hilillos, contiene PC, regitsros y contador de ciclos de reloj
        public static Contextos finalizados;      // Guarda el contexto de los hilillos al finalizar
        public static int total;                  // Total de hilillos
        public static int reloj;                  // Variable general del reloj
        public static int quantumTotal;           // Variable compartida, solo de lectura, no debe ser modificada por los hilos
        public static int[,] cacheDatos3;         // 4 columnas 6 filas, (fila 0 p0, fila 2 p1, fila 4 etiqueta, fila 5 valides)
        public static int[,] cacheDatos2;         // 4 columnas 6 filas, (fila 0 p0, fila 2 p1, fila 4 etiqueta, fila 5 valides)
        public static int[,] cacheDatos1;         // 4 columnas 6 filas, (fila 0 p0, fila 2 p1, fila 4 etiqueta, fila 5 valides)
        public static int RL1;                    // Registros RL (globales)
        public static int RL2;
        public static int RL3;
        public static int[] busD;                 // Bus de datos
        public static int[] busI;                 // Bus de instrucciones
        public static int llegan = 4;             // Variable para el uso de la barrera
        public static int enter, frecuencia;      // Para visualizacion lenta del programa
        public static bool lento;                 // Saber si se desea la visualizacion lenta del programa
        public static int[] vector;               // Para saber que hilillo corre en cada nucleo

        //Barrera de sincronizacion, lo que esta dentro se ejecuta una sola vez, cada vez que los 4 procesos llegan
        static Barrier barrier = new Barrier(llegan, (bar) =>  
        {
            reloj++;    //Se aumenta el contador de ciclos de reloj

            if (lento)  // Si la ejecucion es lenta
            {
                if (enter == reloj)     // La frecuencia con la que se visualiza en pantalla
                {
                    Console.Write("\nReloj: " + reloj + "\n"); 
                    for (int i = 1; i<= 3; ++i)
                    {
                        Console.Write("En el nucleo "+ i + " Corre el hilillo: " + vector[i-1] + "\n");
                        // Cual hilo corre en cada nucleo
                    }
                    
                    Console.Write("\nCache de Datos 1: \n");
                    for (int i = 0; i < 6; ++i)                             //Imprime la cahe de datos 1
                    {
                        for (int j = 0; j < 4; ++j)
                        {
                            Console.Write(cacheDatos1[i, j] + "  ");
                        }
                        Console.Write("\n");
                    }

                    Console.Write("\nCache de Datos 2: \n");
                    for (int i = 0; i < 6; ++i)                             //Imprime la cahe de datos 2
                    {
                        for (int j = 0; j < 4; ++j)
                        {
                            Console.Write(cacheDatos2[i, j] + "  ");
                        }
                        Console.Write("\n");
                    }

                    Console.Write("\nCache de Datos 3: \n");
                    for (int i = 0; i < 6; ++i)                             //Imprime la cahe de datos 3
                    {
                        for (int j = 0; j < 4; ++j)
                        {
                            Console.Write(cacheDatos3[i, j] + "  ");
                        }
                        Console.Write("\n");
                    }

                    enter += frecuencia;
                    Console.ReadKey();      //Espera a que se lea una tecla                    
                }
            }          
          
        }); //FIN de la Barrera


        //Metodo con el que se llama la barrera
        static public void TicReloj()
        {
            barrier.SignalAndWait();
        }//FIN de TicReloj

        // Se encicla los tick de reloj dependiendo que la instruccion que se esta ejecutando
        static public void FallodeCache(int ciclos)  
        {
            for (int i = 0; i < ciclos; ++i)            // Simulacion de que un fallo de cache
            {
                TicReloj();                             // Tic de reloj
            }
        }//FIN de Fallo de Cache

        static void Main()
        {
            //**Bloque de creacion**//

            memDatos = new int[96];         // 384/4                //Original 96
            memInstruc = new int[640];      // 40 bloques * 4 *4    //Original 640
            cola = new Contextos();
            finalizados = new Contextos();

            vector = new int[3];            // Creacion del vector de hilillos
            busD = new int[1];              // Creacion del Bus de Datos
            busI = new int[1];              // Creacion del Bus de Instrucciones

            cacheDatos1 = new int[6, 4];    // Creacion de la cache de Datos
            cacheDatos2 = new int[6, 4];    // Creacion de la cache de Datos
            cacheDatos3 = new int[6, 4];    // Creacion de la cache de Datos
            //*******************Fin de Bloque***********************//

            //*************Bloque de inicializacion******************//

            reloj = 0;            //Se inicializa el reloj
            lento = false;        //Se asume que no va a ir lento

            for (int i = 0; i < 96; ++i)    // Memoria principal inicilizada en uno para sincronizacion de candados
            {
                memDatos[i] = 1;        
                memInstruc[i] = 1;
            }

            for (int i = 96; i < 640; ++i)   // Memoria principal inicilizada en uno para sincronizacion de candados
            {
                memInstruc[i] = 1;
            }

            for (int i = 0; i < 4; ++i)     // Las caches se inicializadas en cero
            {
                for (int j = 0; j < 4; ++j)
                {
                    cacheDatos3[i, j] = 0;
                    cacheDatos2[i, j] = 0;
                    cacheDatos1[i, j] = 0;
                }
            }
            for (int i = 4; i < 6; ++i)     // Las caches se inicializadas en invalidas
            {
                for (int j = 0; j < 4; ++j)
                {
                    cacheDatos3[i, j] = -1;
                    cacheDatos2[i, j] = -1;
                    cacheDatos1[i, j] = -1;
                }
            }
            //*****************Fin de Bloque*************************//
            // Se pide al usuario los datos para correr el programa
            Console.Write("Ingrese el quantum \n");
            quantumTotal = int.Parse(Console.ReadLine());                   // Quantum de cada hilillo
            Console.Write("\nIngrese el numero de hilillos Totales \n");    // Cantidad de hilos a correr (max 8 hilos)
            total = int.Parse(Console.ReadLine());
            Console.Write("\nIngrese un 1 si desea visualizar la ejecucion, de lo contrario otro numero\n");  // Opciones de visualizacion de la ejecucion

            if ( 1 == (int.Parse(Console.ReadLine())))  //Si la ejecucion va lento
            {
                lento = true;
                Console.Write("\nIngrese la frecuencia de ciclos de reloj a visualizar \n");  // Cada cuantos ciclos de reloj se haran las impresiones
                frecuencia = int.Parse(Console.ReadLine());
                enter = frecuencia;
            }

            Console.Write("\nEjecutando hilillos porfavor espere\n");

            /*****Leer de archivos*******/
            int index = 0;
            Char delimiter = ' ';


            for (int i = 0; i < total; ++i)     // Se cargan los datos de los archivos a la memoria de Instrucciones
            {
                cola.Encolar(index+ 384, i);
                string[] lines = File.ReadAllLines(i +".txt");          // Los archivos de los hilillos deben estar en la carpeta bin/Debug y deben ser de la forma #.txt

                foreach(string line in lines)
                {
                    string[] substrings = line.Split(delimiter);        // Se recortan los espacios en blanco

                    foreach(var subestring in substrings)
                    {
                        memInstruc[index] = int.Parse(subestring);      // Se mete en la memoria de instrucciones   
                        ++index;
                    }
                }                
            }
            /******Fin leer de archivos*******/

            // Creacion de los 3 hilos que emulan los nucleos
             Thread thread1 = new Thread(() => Nucleos(quantumTotal));
             Thread thread2 = new Thread(() => Nucleos(quantumTotal));
             Thread thread3 = new Thread(() => Nucleos(quantumTotal));
             // Se les asigna un "id" a los hilos
             thread1.Name = "1";
             thread2.Name = "2";
             thread3.Name = "3";
             // Se inician los hilos
             thread1.Start();
             thread2.Start();
             thread3.Start();

             // Verificar que todos los hilillos finalizaron
             int cardinalidad;
             cardinalidad = 0;
             while (cardinalidad < total)
             {
                 if (!Monitor.TryEnter(finalizados))
                 {
                     TicReloj();
                 }
                 else
                 {
                     TicReloj();
                     cardinalidad = finalizados.Cantidad();
                     Monitor.Exit(finalizados);
                 }               
             }

             // Finaliza los 3 hilos que emulan los nucleos
             thread1.Abort();
             thread2.Abort();
             thread3.Abort();

            // Imprime la memoria de Datos, 4 Bloques por linea

            int y = 15;

            Console.Write("\nMemoria de Datos \n");
            for (int i = 0; i < 96; ++i)
            {
                Console.Write(memDatos[i] + "  ");
                if (i == y)
                {
                    y += 16;
                    Console.Write("\n");
                }
            }
            Console.ReadKey();

            // Impresiones de las cache de datos

            Console.Write("\nCache de Datos 1: \n");
            for (int i = 0; i < 6; ++i)                             // Imprime la cahe de datos 1
            {
                for (int j = 0; j < 4; ++j)
                {
                    Console.Write(cacheDatos1[i, j] + "  ");
                }
                Console.Write("\n");
            }

            Console.Write("\nCache de Datos 2: \n");
            for (int i = 0; i < 6; ++i)                             // Imprime la cahe de datos 2
            {
                for (int j = 0; j < 4; ++j)
                {
                    Console.Write(cacheDatos2[i, j] + "  ");
                }
                Console.Write("\n");
            }

            Console.Write("\nCache de Datos 3: \n");
            for (int i = 0; i < 6; ++i)                             // Imprime la cahe de datos 3
            {
                for (int j = 0; j < 4; ++j)
                {
                    Console.Write(cacheDatos3[i, j] + "  ");
                }
                Console.Write("\n");
            }
            Console.ReadKey();                                         //Espera por la lectura de una tecla
            finalizados.Imprimir();                                    // Imprime el contexto de Todos los hilillos           
        }// FIN de Main

        public static void Nucleos(int q) // Quantum
        {
            /**Bloque de Declaracion**/
            int[] reg;                                                      // Vector de registros
            int[][] cacheInstruc = new int[6][];                            // 16 columnas 6 filas, (fila 0 p0, fila 2 p1, fila 4 etiqueta, fila 5 valides)
            int PC;                                                         // Para el control de las instrucciones
            int cop, rf1, rf2, rd;                                          // Codigo de operacion, registro fuente, registro fuente2 o registro destino dependiendo de la instruccion, rd registro destino o inmediato dependiendo de la instruccion
            int bloque, posicion, palabra, iterador, quantum, inicioBloque; // Bloque es el bloque de memoria cache, quatum es el tiempo dado por el usuario
            int cpu;                                                        // Tic de reloj que dura un hilillo en ejecucion
            int inicioReloj;                                                //Cuando inicia a ejecutar en hilillo
            int ID;                                                         //Es el numero identificador del hilillo           
           
            /**Bloque de Creacion**/
            reg = new int[32];
            ID = -1;
            PC = -1;
            cacheInstruc[0] = new int[16];
            cacheInstruc[1] = new int[16];
            cacheInstruc[2] = new int[16];
            cacheInstruc[3] = new int[16];
            cacheInstruc[4] = new int[4];       //Para no desperdiciar memoria
            cacheInstruc[5] = new int[4];       //Para no desperdiciar memoria
            //*****************Fin bloque de creacion******************//

            //***********Bloque inicializacion***********************//

            for (int i = 0; i < 32; ++i)    // Los registros se incializan en cero
            {
                reg[i] = 0;
            }

            for (int i = 0; i < 4; ++i) // Las caches se inicializadas en cero
            {
                for (int j = 0; j < 16; ++j)
                {
                    cacheInstruc[i][j] = 0;
                }
            }
            for (int i = 4; i < 6; ++i) // La validez de las caches se inicializadas en -1 (invalidas)
            {
                for (int j = 0; j < 4; ++j)
                {
                    cacheInstruc[i][j] = -1;
                }
            }
            //**************Fin bloque inicilaizacion****************//
            
            while (true) // While que no deja que los hilos mueran
            {
                bool vacia = true;
                while(vacia)
                {
                    while (!Monitor.TryEnter(cola))
                    {
                        TicReloj();
                    }
                    TicReloj();
                    if (cola.Cantidad() > 0)  // si hay elementos en la cola
                    {
                        vacia = false;
                    }
                    else
                    {
                        Monitor.Exit(cola);
                    }                    
                }
                               
                switch (int.Parse(Thread.CurrentThread.Name)) // RL se inicializa en -1 porque no existe la direccion -1
                {
                    case 1:
                        while (!Monitor.TryEnter(cacheDatos1))  // RL asociados a la cache de Datos
                        {
                            TicReloj();
                        }
                        TicReloj();
                        RL1 = -1;
                        Monitor.Exit(cacheDatos1);
                        break;

                    case 2:
                        while (!Monitor.TryEnter(cacheDatos2))  // RL asociados a la cache de Datos
                        {
                            TicReloj();
                        }
                        TicReloj();
                        RL2 = -1;
                        Monitor.Exit(cacheDatos2);
                        break;

                    case 3:
                        while (!Monitor.TryEnter(cacheDatos3))  // RL asociados a la cache de Datos
                        {
                            TicReloj();
                        }
                        TicReloj();
                        RL3 = -1;
                        Monitor.Exit(cacheDatos3);
                        break;
                }
                
                cpu = 0;
                inicioReloj = reloj;    // Donde va a iniciar el hilillo
                cola.Sacar(ref PC, ref reg, ref cpu, ref ID);   //ID es el numero del hilillo
                Monitor.Exit(cola);
                quantum = q;        // Se inicializa el quantum del hilillo a ejecutar

                vector[(int.Parse(Thread.CurrentThread.Name)) - 1] = ID;//Se guarda el hilillo en ejecucion para que sea visible por el padre


                while (quantum > 0) // Manejo del quantum para cada hilo
                {
                 
                    bloque = PC / 16;           // Calculo el bloque
                    posicion = bloque % 4;      // Posicion en cache
                    palabra = (PC % 16) / 4;    //Caculo del numero de palabra
                   
                    if (!(cacheInstruc[4][posicion] == bloque) || !(cacheInstruc[4][posicion] == 1)) // 1 valido
                    {
                        // FALLO DE CACHE 
                        while (!Monitor.TryEnter(busI))
                        {
                            TicReloj();
                        }
                        TicReloj();

                        cacheInstruc[4][posicion] = bloque;
                        cacheInstruc[5][posicion] = 1;
                                                
                        inicioBloque = ((bloque-24) * 16); // Bloque en memoria de instrucciones del programa
                        for (int i = 0; i < 4; ++i)
                        {
                            iterador = posicion * 4;
                            for (int j = 0; j < 4; ++j)
                            {
                                cacheInstruc[i][iterador] = memInstruc[inicioBloque];                              
                                ++inicioBloque;
                                ++iterador;                       
                            }
                        }
                        FallodeCache(28);
                        Monitor.Exit(busI);
                    } //Fin fallo de cache
                    // Separación de las partes de la instruccion en variables para decodificarlas
                    iterador = posicion * 4;
                    cop = cacheInstruc[palabra][iterador];          // Codigo de operacion
                    rf1 = cacheInstruc[palabra][iterador + 1];      // Registro 1
                    rf2 = cacheInstruc[palabra][iterador + 2];      // Registro 2
                    rd = cacheInstruc[palabra][iterador + 3];       // Destino 
                    PC += 4;
                    // Se debe tomar en uenta que no siempre el destino se usa como el destino de las operaciones (ver la codificación en la documentacion externa)
                    
                    // Codificacion de las instrucciones recibidas
                    switch (cop) // cop es el codigo de operacion 		
                    {

                       case 8: //DADDI rf2 <------- rf1+ inm
                            reg[rf2] = reg[rf1] + rd;
                            break;

                        case 32: //DADD rd <------ rf1 + rf2
                            reg[rd] = reg[rf1] + reg[rf2];
                            break;

                        case 34: //DSUB  rd <------- rf1 - rf2 
                            reg[rd] = reg[rf1] - reg[rf2];                            
                            break;

                        case 12: //DMUL  rd <------ rf1 * rf2
                            reg[rd] = reg[rf1] * reg[rf2];
                            break;

                        case 14: //DIV  rd <------ rf1 / rf2
                            reg[rd] = reg[rf1] / reg[rf2];
                            break;

                        case 4: //BEZ si rf = 0 entonces SALTA
                            if (reg[rf1] == 0)
                            {
                                PC += (rd * 4);
                            }
                            break;

                        case 5: //BNEZ si rf z 0 o rf > 0 entonces SALTA
                            if (reg[rf1] != 0)
                            {
                                PC += (rd * 4);
                            }
                            break;

                        case 3: //JAL  reg 31 = PC
                            reg[31] = PC;
                            PC += rd;
                            break;

                        case 2:  //JR  PC = rf1
                            PC = reg[rf1];
                            break;

                        case 50: //LL
                            int dirr = reg[rf1] + rd;
                            bloque = dirr / 16;
                            posicion = bloque % 4;
                            palabra = (dirr % 16) / 4;     // Calculo palabra
                            switch (int.Parse(Thread.CurrentThread.Name))
                            {
                                case 1:
                                    bool conseguido = false;
                                    while (!conseguido)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos1))
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();
                                        if (!(bloque == cacheDatos1[4, posicion]) || !(cacheDatos1[5, posicion] == 1))
                                        {
                                            if (!Monitor.TryEnter(busD))
                                            {
                                                Monitor.Exit(cacheDatos1);
                                                TicReloj();
                                            }
                                            else
                                            {
                                                conseguido = true;
                                                TicReloj();
                                                inicioBloque = bloque * 4;    // Inicio del bloque a copiar

                                                for (int i = 0; i < 4; ++i) // Copia los datos de memoria a Cache
                                                {
                                                    cacheDatos1[i, posicion] = memDatos[inicioBloque];
                                                    inicioBloque++;
                                                }
                                                cacheDatos1[4, posicion] = bloque;
                                                cacheDatos1[5, posicion] = 1;
                                                FallodeCache(28);
                                                Monitor.Exit(busD);
                                            }
                                        }
                                        else
                                        {
                                            conseguido = true;
                                        }
                                    }
                                    reg[rf2] = cacheDatos1[palabra, posicion];
                                    RL1 = dirr;
                                    Monitor.Exit(cacheDatos1);
                                    break;

                                case 2:
                                    bool c2 = false;
                                    while (!c2)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos2))
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();
                                        if (!(bloque == cacheDatos2[4, posicion]) || !(cacheDatos2[5, posicion] == 1))
                                        {
                                            if (!Monitor.TryEnter(busD))
                                            {
                                                Monitor.Exit(cacheDatos2); 
                                                TicReloj();
                                            }
                                            else
                                            {
                                                c2 = true;
                                                TicReloj();
                                                inicioBloque = bloque * 4;    // Inicio del bloque a copiar

                                                for (int i = 0; i < 4; ++i) // Copia los datos de memoria a Cache
                                                {
                                                    cacheDatos2[i, posicion] = memDatos[inicioBloque];
                                                    inicioBloque++;
                                                }
                                                cacheDatos2[4, posicion] = bloque;
                                                cacheDatos2[5, posicion] = 1;
                                                FallodeCache(28);
                                                Monitor.Exit(busD);
                                            }
                                        }
                                        else
                                        {
                                            c2 = true;
                                        }
                                    }
                                    reg[rf2] = cacheDatos2[palabra, posicion];  // Se le entrega el dato al registro
                                    RL2 = dirr;
                                    Monitor.Exit(cacheDatos2);                  // Soltar mi cache                                                                 
                                    break;

                                case 3:
                                    bool c3 = false;
                                    while (!c3)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos3))
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();
                                        if (!(bloque == cacheDatos3[4, posicion]) || !(cacheDatos3[5, posicion] == 1))
                                        {
                                            if (!Monitor.TryEnter(busD))
                                            {
                                                Monitor.Exit(cacheDatos3);
                                                TicReloj();
                                            }
                                            else
                                            {
                                                c3 = true;
                                                TicReloj();
                                                inicioBloque = bloque * 4;    // Inicio del bloque a copiar

                                                for (int i = 0; i < 4; ++i)   // Copia los datos de memoria a Cache
                                                {
                                                    cacheDatos3[i, posicion] = memDatos[inicioBloque];
                                                    inicioBloque++;
                                                }
                                                cacheDatos3[4, posicion] = bloque;
                                                cacheDatos3[5, posicion] = 1;
                                                FallodeCache(28);  
                                                Monitor.Exit(busD);
                                            }
                                        }
                                        else
                                        {
                                            c3 = true;
                                        }
                                    }
                                    reg[rf2] = cacheDatos3[palabra, posicion];  // Se le entrega el dato al registro
                                    RL3 = dirr;                               
                                    Monitor.Exit(cacheDatos3);                  // Soltar mi cache                                                                
                                    break;
                            }
                            break;

                        case 51: //SC  Store Conditional
                            int direcc = reg[rf1] + rd;
                            bloque = direcc / 16;
                            posicion = bloque % 4;
                            palabra = (direcc % 16) / 4;
                            inicioBloque = bloque * 4;

                            switch (int.Parse(Thread.CurrentThread.Name))
                            {
                                case 1:
                               
                                    bool sc1 = false;
                                    while (!sc1)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos1))
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();

                                        if (RL1 != direcc) // RL es igual a la direccion donde quiero escribir?
                                        {
                                            reg[rf2] = 0;                                            
                                            sc1 = true;
                                        }
                                        else
                                        {
                                            if (!Monitor.TryEnter(busD))
                                            {
                                                Monitor.Exit(cacheDatos1);
                                                TicReloj();

                                            }
                                            else
                                            {
                                                TicReloj();
                                                if (!Monitor.TryEnter(cacheDatos2))
                                                {
                                                    Monitor.Exit(busD);
                                                    TicReloj();

                                                }
                                                else
                                                {
                                                    TicReloj();
                                                    if (bloque == cacheDatos2[4, posicion])
                                                    {
                                                        cacheDatos2[5, posicion] = -1;  // Invalido cache
                                                        if (RL2 == direcc)
                                                        {
                                                            RL2 = -1;
                                                        } 
                                                    }
                                                    Monitor.Exit(cacheDatos2);

                                                    if (!Monitor.TryEnter(cacheDatos3))
                                                    {
                                                        Monitor.Exit(busD);
                                                        Monitor.Exit(cacheDatos1);
                                                        TicReloj();
                                                    }
                                                    else
                                                    {
                                                        TicReloj();
                                                        if (bloque == cacheDatos3[4, posicion])
                                                        {
                                                            cacheDatos3[5, posicion] = -1;  // Invalido cache
                                                            if (RL3 == direcc)
                                                            {
                                                                RL3 = -1;
                                                            } 
                                                        }
                                                        Monitor.Exit(cacheDatos3);
                                                        sc1 = true;
                                                    }
                                                }
                                            }

                                            memDatos[inicioBloque + palabra] = reg[rf2]; // Registro donde viene
                                            FallodeCache(7);
                                            Monitor.Exit(busD);
                                            if ((bloque == cacheDatos1[4, posicion]) && (cacheDatos1[5, posicion] == 1))
                                            {
                                                cacheDatos1[palabra, posicion] = reg[rf2];  // Registro donde viene

                                            }
                                            RL1 = -1;
                                        }
                                        Monitor.Exit(cacheDatos1);
                                    }                                        
                                    break;

                                case 2:
                                    bool sc2 = false;
                                    while (!sc2)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos2))
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();

                                        if (RL2 != direcc) // RL es igual a la direccion donde quiero escribir?
                                        {
                                            reg[rf2] = 0;                                            
                                            sc2 = true;
                                        }
                                        else
                                        {
                                            if (!Monitor.TryEnter(busD))
                                            {
                                                Monitor.Exit(cacheDatos2);
                                                TicReloj();

                                            }
                                            else
                                            {
                                                TicReloj();
                                                if (!Monitor.TryEnter(cacheDatos1))
                                                {
                                                    Monitor.Exit(busD);
                                                    Monitor.Exit(cacheDatos2);
                                                    TicReloj();

                                                }
                                                else
                                                {
                                                    TicReloj();
                                                    if (bloque == cacheDatos1[4, posicion])
                                                    {
                                                        cacheDatos1[5, posicion] = -1;  // Invalido cache
                                                        if (RL1 == direcc)
                                                        {
                                                            RL1 = -1;
                                                        } 
                                                    }
                                                    Monitor.Exit(cacheDatos1);

                                                    if (!Monitor.TryEnter(cacheDatos3))
                                                    {
                                                        Monitor.Exit(busD);
                                                        Monitor.Exit(cacheDatos2);
                                                        TicReloj();
                                                    }
                                                    else
                                                    {
                                                        TicReloj();
                                                        if (bloque == cacheDatos3[4, posicion])
                                                        {
                                                            cacheDatos3[5, posicion] = -1;  // Invalido cache
                                                            if (RL3 == direcc)
                                                            {
                                                                RL3 = -1;
                                                            } 
                                                        }
                                                        Monitor.Exit(cacheDatos3);
                                                        sc2 = true;
                                                    }
                                                }
                                            }

                                            memDatos[inicioBloque + palabra] = reg[rf2]; // Registro donde viene 
                                            FallodeCache(7);
                                            Monitor.Exit(busD);
                                            if ((bloque == cacheDatos2[4, posicion]) && (cacheDatos2[5, posicion] == 1))
                                            {
                                                cacheDatos2[palabra, posicion] = reg[rf2]; // Registro donde viene

                                            }
                                            RL2 = -1;
                                        }
                                        Monitor.Exit(cacheDatos2);
                                    }                                        
                                    break;

                                case 3:
                                    bool sc3 = false;
                                    while (!sc3)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos3))
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();
                                        if (RL3 != direcc) // RL es igual a la direccion donde quiero escribir?
                                        {
                                            reg[rf2] = 0;
                                            sc3 = true;
                                        }
                                        else
                                        {
                                            if (!Monitor.TryEnter(busD))
                                            {
                                                Monitor.Exit(cacheDatos3);
                                                TicReloj();
                                            }
                                            else
                                            {
                                                TicReloj();
                                                if (!Monitor.TryEnter(cacheDatos1))
                                                {
                                                    Monitor.Exit(busD);
                                                    Monitor.Exit(cacheDatos3);
                                                    TicReloj();

                                                }
                                                else
                                                {
                                                    TicReloj();
                                                    if (bloque == cacheDatos1[4, posicion])
                                                    {
                                                        cacheDatos1[5, posicion] = -1;  // Invalido cache
                                                        if (RL1 == direcc)
                                                        {
                                                            RL1 = -1;
                                                        } 
                                                    }
                                                    Monitor.Exit(cacheDatos1);

                                                    if (!Monitor.TryEnter(cacheDatos2))
                                                    {
                                                        Monitor.Exit(busD);
                                                        Monitor.Exit(cacheDatos3);
                                                        TicReloj();
                                                    }
                                                    else
                                                    {
                                                        TicReloj();
                                                        if (bloque == cacheDatos2[4, posicion])
                                                        {
                                                            cacheDatos2[5, posicion] = -1;  // Invalido cache
                                                            if (RL2 == direcc)
                                                            {
                                                                RL2 = -1;
                                                            } 
                                                        }
                                                        Monitor.Exit(cacheDatos2);
                                                        sc3 = true;
                                                    }
                                                }
                                            }

                                            memDatos[inicioBloque + palabra] = reg[rf2]; // Registro donde viene 
                                            FallodeCache(7);
                                            Monitor.Exit(busD);
                                            if ((bloque == cacheDatos3[4, posicion]) && (cacheDatos3[5, posicion] == 1))
                                            {
                                                cacheDatos3[palabra, posicion] = reg[rf2]; // Registro donde viene

                                            }
                                            RL3 = -1;
                                        }
                                        Monitor.Exit(cacheDatos3);
                                    }                                          
                                    break;
                            }
                            break;
                                                   
                        case 35: //LW                            
                            int dir = reg[rf1] + rd;
                            bloque = dir / 16;      //Calculo del bloque
                            posicion = bloque % 4;
                            palabra = (dir % 16) / 4;     

                            switch (int.Parse(Thread.CurrentThread.Name))
                            {
                                case 1:
                                    bool conseguido = false;
                                    while (!conseguido)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos1)) 
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();
                                        if (!(bloque == cacheDatos1[4, posicion]) || !(cacheDatos1[5, posicion] == 1))
                                        {
                                            if (!Monitor.TryEnter(busD))
                                            {
                                                Monitor.Exit(cacheDatos1);
                                                TicReloj();
                                            }
                                            else
                                            {
                                                conseguido = true;
                                                TicReloj();
                                                inicioBloque = bloque * 4;    // Inicio del bloque a copiar en mi memoria de datos

                                                for (int i = 0; i < 4; ++i) // Copia los datos de memoria a Cache
                                                {
                                                    cacheDatos1[i, posicion] = memDatos[inicioBloque];
                                                    inicioBloque++;
                                                }
                                                cacheDatos1[4, posicion] = bloque;
                                                cacheDatos1[5, posicion] = 1;
                                                FallodeCache(28);
                                                Monitor.Exit(busD);                                                                    
                                            }
                                        }
                                        else
                                        {
                                            conseguido = true;
                                        }
                                    }
                                    reg[rf2] = cacheDatos1[palabra, posicion];                                    
                                    Monitor.Exit(cacheDatos1);
                                    break;

                                case 2:
                                    bool c2 = false;
                                    while (!c2)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos2)) 
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();
                                        if (!(bloque == cacheDatos2[4, posicion]) || !(cacheDatos2[5, posicion] == 1))
                                        {
                                            if (!Monitor.TryEnter(busD))
                                            {
                                                Monitor.Exit(cacheDatos2);
                                                TicReloj();
                                            }
                                            else
                                            {
                                                c2 = true;
                                                TicReloj();
                                                inicioBloque = bloque * 4;    // Inicio del bloque a copiar

                                                for (int i = 0; i < 4; ++i) // Copia los datos de memoria a Cache
                                                {
                                                    cacheDatos2[i, posicion] = memDatos[inicioBloque];
                                                    inicioBloque++;
                                                }
                                                cacheDatos2[4, posicion] = bloque;
                                                cacheDatos2[5, posicion] = 1;
                                                FallodeCache(28);
                                                Monitor.Exit(busD);                                                                                             
                                            }
                                        }
                                        else
                                        {
                                            c2 = true;
                                        }
                                    }                                   
                                    reg[rf2] = cacheDatos2[palabra, posicion];  // Se le entrega el dato al registro
                                    Monitor.Exit(cacheDatos2);                  // Soltar mi cache                                                                 
                                    break;

                                case 3:
                                    bool c3 = false;
                                    while (!c3)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos3))
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();
                                        if (!(bloque == cacheDatos3[4, posicion]) || !(cacheDatos3[5, posicion] == 1))
                                        {
                                            if (!Monitor.TryEnter(busD))
                                            {
                                                Monitor.Exit(cacheDatos3);
                                                TicReloj();
                                            }
                                            else
                                            {
                                                c3 = true;
                                                TicReloj();
                                                inicioBloque = bloque * 4;    // Inicio del bloque a copiar

                                                for (int i = 0; i < 4; ++i)   // Copia los datos de memoria a Cache
                                                {
                                                    cacheDatos3[i, posicion] = memDatos[inicioBloque];
                                                    inicioBloque++;
                                                }
                                                cacheDatos3[4, posicion] = bloque;
                                                cacheDatos3[5, posicion] = 1;
                                                FallodeCache(28);
                                                Monitor.Exit(busD);                                                                                     
                                            }
                                        }
                                        else
                                        {
                                            c3 = true;
                                        }
                                    }
                                    reg[rf2] = cacheDatos3[palabra, posicion];  // Se le entrega el dato al registro                                  
                                    Monitor.Exit(cacheDatos3);                  // Soltar mi cache                                                                
                                    break;
                            }
                            break;

                        case 43: //SW

                            int direccion = reg[rf1] + rd;
                            bloque = direccion / 16;
                            posicion = bloque % 4;
                            palabra = (direccion % 16) / 4;
                            inicioBloque = bloque * 4;

                            switch (int.Parse(Thread.CurrentThread.Name))
                            {
                                case 1:
                                   
                                    bool sw1 = false;
                                    while (!sw1)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos1))
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();

                                        if (!Monitor.TryEnter(busD))
                                        {
                                            Monitor.Exit(cacheDatos1); 
                                            TicReloj();

                                        }
                                        else
                                        {
                                            TicReloj();
                                            if (!Monitor.TryEnter(cacheDatos2))
                                            {
                                                Monitor.Exit(busD);
                                                Monitor.Exit(cacheDatos1);
                                                TicReloj();

                                            }
                                            else
                                            {
                                                TicReloj();
                                                if (bloque == cacheDatos2[4, posicion])
                                                {
                                                    cacheDatos2[5, posicion] = -1; 
                                                    if (RL2 == direccion)       // Revisar el rl antes de invalidar
                                                    {
                                                        RL2 = -1;
                                                    }
                                                }
                                                Monitor.Exit(cacheDatos2);

                                                if (!Monitor.TryEnter(cacheDatos3))
                                                {
                                                    Monitor.Exit(busD);
                                                    Monitor.Exit(cacheDatos1);
                                                    TicReloj();
                                                }
                                                else
                                                {
                                                    TicReloj();
                                                    if (bloque == cacheDatos3[4, posicion])
                                                    {
                                                        cacheDatos3[5, posicion] = -1; 
                                                        if (RL3 == direccion)       // Revisar el rl antes de invalidar
                                                        {
                                                            RL3 = -1;
                                                        }   
                                                    }
                                                    Monitor.Exit(cacheDatos3);
                                                    sw1 = true;
                                                }
                                            }
                                        }
                                    }
                                    memDatos[inicioBloque + palabra] = reg[rf2]; // Registro donde viene
                                    FallodeCache(7);
                                    Monitor.Exit(busD);
                                    if ((bloque == cacheDatos1[4, posicion]) && (cacheDatos1[5, posicion] == 1))
                                    {
                                        cacheDatos1[palabra, posicion] = reg[rf2];  // Registro donde viene

                                    }                                     
                                    Monitor.Exit(cacheDatos1);
                                    break;

                                case 2:
                                    bool sw2 = false;
                                    while (!sw2)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos2))
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();

                                        if (!Monitor.TryEnter(busD))
                                        {
                                            Monitor.Exit(cacheDatos2);
                                            TicReloj();

                                        }
                                        else
                                        {
                                            TicReloj();
                                            if (!Monitor.TryEnter(cacheDatos1))
                                            {
                                                Monitor.Exit(busD);
                                                Monitor.Exit(cacheDatos2);
                                                TicReloj();

                                            }
                                            else
                                            {
                                                TicReloj();
                                                if (bloque == cacheDatos1[4, posicion])
                                                {
                                                    cacheDatos1[5, posicion] = -1;  
                                                    if (RL1 == direccion)       // Revisar el rl antes de invalidar
                                                    {
                                                        RL1 = -1;
                                                    }   
                                                }
                                                Monitor.Exit(cacheDatos1);

                                                if (!Monitor.TryEnter(cacheDatos3))
                                                {
                                                    Monitor.Exit(busD);
                                                    Monitor.Exit(cacheDatos2);
                                                    TicReloj();
                                                }
                                                else
                                                {
                                                    TicReloj();
                                                    if (bloque == cacheDatos3[4, posicion])
                                                    {
                                                        cacheDatos3[5, posicion] = -1;  
                                                        if (RL3 == direccion)       
                                                        {
                                                            RL3 = -1;
                                                        }   
                                                    }
                                                    Monitor.Exit(cacheDatos3);
                                                    sw2 = true;
                                                }
                                            }
                                        }
                                    }
                                    memDatos[inicioBloque + palabra] = reg[rf2]; // Registro donde viene 
                                    FallodeCache(7);
                                    Monitor.Exit(busD);
                                    if ((bloque == cacheDatos2[4, posicion]) && (cacheDatos2[5, posicion] == 1))
                                    {
                                        cacheDatos2[palabra, posicion] = reg[rf2]; // Registro donde viene

                                    }                                
                                    Monitor.Exit(cacheDatos2);
                                    break;

                                case 3:
                                    bool sw3 = false;
                                    while (!sw3)
                                    {
                                        while (!Monitor.TryEnter(cacheDatos3))
                                        {
                                            TicReloj();
                                        }
                                        TicReloj();

                                        if (!Monitor.TryEnter(busD))
                                        {
                                            Monitor.Exit(cacheDatos3);
                                            TicReloj();

                                        }
                                        else
                                        {
                                            TicReloj();
                                            if (!Monitor.TryEnter(cacheDatos1))
                                            {
                                                Monitor.Exit(busD);
                                                Monitor.Exit(cacheDatos3);
                                                TicReloj();

                                            }
                                            else
                                            {
                                                TicReloj();
                                                if (bloque == cacheDatos1[4, posicion])
                                                {
                                                    cacheDatos1[5, posicion] = -1; 
                                                    if (RL1 == direccion)        // Revisar el rl antes de invalidar
                                                    {
                                                        RL1 = -1;
                                                    }   
                                                }
                                                Monitor.Exit(cacheDatos1);

                                                if (!Monitor.TryEnter(cacheDatos2))
                                                {
                                                    Monitor.Exit(busD);
                                                    Monitor.Exit(cacheDatos3);
                                                    TicReloj();
                                                }
                                                else
                                                {
                                                    TicReloj();
                                                    if (bloque == cacheDatos2[4, posicion])
                                                    {
                                                        cacheDatos2[5, posicion] = -1;  
                                                        if (RL2 == direccion)       // Revisar el rl antes de invalidar
                                                        {
                                                            RL2 = -1;
                                                        }   
                                                    }
                                                    Monitor.Exit(cacheDatos2);
                                                    sw3 = true;
                                                }
                                            }
                                        }
                                    }

                                    memDatos[inicioBloque + palabra] = reg[rf2]; // Registro donde viene 
                                    FallodeCache(7);
                                    Monitor.Exit(busD);
                                    if ((bloque == cacheDatos3[4, posicion]) && (cacheDatos3[5, posicion] == 1))
                                    {
                                        cacheDatos3[palabra, posicion] = reg[rf2]; // Registro donde viene

                                    }
                                    Monitor.Exit(cacheDatos3);
                                    break;
                            }
                            break;

                        case 63: //FIN                       
                            Console.Write("Instruccion de FIN del Hilillo: " + ID + "\n");                            
                            quantum = -1;  // Para tener el control de que la ultima instruccion fue FIN
                            break;
                    }

                    quantum--; // Lo resto al finalizar una instruccion

                    if (quantum < 0)//ultima fue FIN
                    {

                        while (!Monitor.TryEnter(finalizados))
                        {
                            TicReloj();
                        }
                        TicReloj();

                        cpu += (reloj - inicioReloj);                                   // Ciclos de reloj que duro el hilillo en ejecucion
                        finalizados.GuardarFinalizados(PC, ref reg, cpu, reloj, ID);    // Se guarda en la cola de finalizados                    
                        Monitor.Exit(finalizados);
                    }
                    else
                    {
                        if (quantum == 0) // Se termino el quantum
                        {
                            while (!Monitor.TryEnter(cola))
                            {
                                TicReloj();
                            }
                            TicReloj();
                            cpu += (reloj - inicioReloj);
                            cola.Guardar(PC, ref reg, cpu, ID);                             // Se guarda en la cola de los que todavia les falta correr                           
                            Console.Write("Se Termino el QUANTUM del Hilillo " + ID + "\n");
                            Monitor.Exit(cola);
                        }
                    }
                    TicReloj();
                }//FIN del quantum
            }//FIN del while(true) 
        } //FIN de Nucleos 
    }//FIN de la clase



    public class Contextos
    {
        private Queue queue;
        private int contador;
        private struct Contexto
        {   // Varables para poder almacenas los contextos cuando se le acabe el quantum a cada hilo
            public int pc;
            public int[] regist;
            public int relojCPU;
            public int relojTotal;
            public int Id;

            public Contexto(int p, ref int[] reg, int cpu, int id)  // Contextos con registros que no han finalizado
            {
                Id = id;
                pc = p;
                regist = new int[32];
                relojCPU = cpu;
                relojTotal = 0;
                
                for (int i = 1; i < 32; ++i)
                {
                    regist[i] = reg[i];
                }
            }

            public Contexto(int p, int id)      // Contextos que solo tiene el PC
            {
                Id = id;
                pc = p;
                relojCPU = 0;
                relojTotal = 0;           
                regist = new int[32];
                for (int i = 1; i < 32; ++i)
                {
                    regist[i] = 0;
                }
            }

            public Contexto(int p, ref int[] reg, int cpu, int total, int id)   // Contextos finalizados
            {
                Id = id;
                pc = p;
                regist = new int[32];
                relojCPU = cpu;
                relojTotal = total;

                for (int i = 1; i < 32; ++i)
                {
                    regist[i] = reg[i];
                }
            }
        }//FIN del struct

        public Contextos()
        {
            queue = new Queue();
            contador = 0;
        }

        ~Contextos() // Destructor de la clase
        {
            //cola.Finalize();
        }


        // reg se debe recibir por referencia 
        public void Guardar(int p, ref int[] reg, int cpu, int id)  // Guarda el contexto         
        {
            Contexto nueva = new Contexto(p, ref reg, cpu, id);
            queue.Enqueue(nueva);
            contador++;

        }//FIN de Guardar

        public void Sacar(ref int p, ref int[] reg, ref int relojActual, ref int id)  // Retorna el contexto
        {
            Contexto aux = (Contexto)queue.Dequeue();
            for (int i = 1; i < 32; ++i)
            {
                reg[i] = aux.regist[i];
            }
            relojActual += aux.relojCPU;
            p = aux.pc;
            id = aux.Id;
            contador--;
        }//FIN de Sacar

        //Solo guarda el PC de cada hilillo, cuando se carga en memoria de instrucciones
        public void Encolar(int p, int id)
        {
            Contexto nueva = new Contexto(p, id);
            queue.Enqueue(nueva);
            contador++;
        }//FIN de Encolar

        public int Cantidad() //Cantidad decontextos en la cola
        {
            return contador;

        }//FIN de Cantidad

        public void GuardarFinalizados(int p, ref int[] reg, int cpu, int total, int id) // Guarda los hilos ya finalizados
        {
            Contexto nueva = new Contexto(p, ref reg, cpu, total, id);
            queue.Enqueue(nueva);
            contador++;
        }//FIN de GuardarFinalizados

        public void Imprimir()
        {
            while(0 < contador)  // Impresiones de tiempos y registros
            {
                Contexto aux = (Contexto)queue.Dequeue();
                contador--;
                Console.WriteLine("\nID Hilillo: \t"+ aux.Id +"\nPC: \t" + aux.pc + "\nReloj CPU: \t" + aux.relojCPU + "\nReloj Total: \t" + aux.relojTotal + "\n");
                for (int i = 0; i < 32; ++i)
                {
                    Console.WriteLine("reg[" + i + "]= \t" + aux.regist[i]);
                }
                Console.ReadKey();
            }            
        }//FIN de Imprimir
    }//FIN de la clase Contextos
}//FIN del namespace
