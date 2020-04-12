﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace life
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int[,] grille;
        ConsoleColor[] colors = (ConsoleColor[])ConsoleColor.GetValues(typeof(ConsoleColor)); // liste des couleurs possible pour le texte de la console
        public MainWindow(int x, int y, double t, bool visuState, bool version, int nPop)
        {
            //x et y : taille de la grille
            // t : le taux
            // visuState : le on/off de tout a l'heure
            InitializeComponent();
            l("données", $" x : {x} | y : {y} | t : {t} | visuState : {visuState}");
            grille = new int[10, 10];
            InitGrille(x, y, t);
            for(int z = 0; z < 5; z++)
            {            
                int[,] tg = grille.Clone() as int[,];
                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        //l($"init -> Evoluer | {i}:{j}", Evoluer(i, j).ToString());
                        int e = Evoluer(i, j);
                        if (grille[i, j] != e)
                        {
                            tg[i, j] = e == 1 ? 42 : 666;
                        }
                        //grille[i, j] = e;
                    }
                }
                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        if (tg[i, j] == 42)
                        {
                            grille[i, j] = 1;
                        }
                        else if (tg[i, j] == 666)
                        {
                            grille[i, j] = 0;
                        }
                    }
                }
                if (visuState)
                {
                    l("Futur " + z);
                    Print2DArray(tg);
                }
                
                l("après Evolution "+z);
                Print2DArray(grille);
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void InitGrille(int x, int y, double t)
        {
            int nCase = x * y; // nombre de cases total dans la grille
            l("nCase", nCase.ToString());
            int n = (int)(t * nCase); // nombre de case a remplir pour atteindre le taux spécifié
            l("n", n.ToString());
            for (int i = 0; i < nCase; i++)
            {
                // on convertit i en position xy pour pouvoir savoir où il est dans la grille
                int iLigne = i / y;
                int iCol = i % y;

                grille[iLigne, iCol] = i < n ? 1 : 0; // si i est dans l'interval [0,n[ alors la case doit contenir un 1 parce qu'on est encore dans le cadre du taux fixé, sinon on met un 0.
            }

            //maintenant que la grille est remplit au taux fixé on la mélange
            Print2DArray(grille);
            MelangerGrille();
            l("après mélange");
            Print2DArray(grille);
        }

        public void InitGrilleV2(int x, int y, double t, int nPop)
        {
            int nCase = x * y; // nombre de cases total dans la grille
            l("nCase", nCase.ToString());
            int n = (int)(t * nCase); // nombre de case a remplir pour atteindre le taux spécifié
            l("n", n.ToString());
            int currentColor = 1; //couleur de la cellule, on commence a 1 parce que la couleur 0 est le noir et comme le fond de la console est noir ce sera illisible, de plus dans notre système le chiffre zero est reservé pour les cellules mortes ou inexistantes
            for (int i = 0; i < nCase; i++)
            {
                // on convertit i en position xy pour pouvoir savoir où il est dans la grille
                int iLigne = i / y;
                int iCol = i % y;
                if(currentColor == nPop+1)
                {
                    currentColor = 1;
                }
                grille[iLigne, iCol] = i < n ? currentColor : 0; // si i est dans l'interval [0,n[ alors la case doit contenir un 1 parce qu'on est encore dans le cadre du taux fixé, sinon on met un 0.
                currentColor++;
            }

            //maintenant que la grille est remplit au taux fixé on la mélange
            Print2DArray(grille);
            MelangerGrille();
            l("après mélange");
            Print2DArray(grille);
        }

        public void MelangerGrille()
        {
            //on récupère les dimensions de la grille
            int nLignes = grille.GetUpperBound(0) + 1; // GetUpperBound => on récupère l'index du dernier élements de la dimension n (ici 0)
            int nCols = grille.GetUpperBound(1) + 1;
            int nCase = nLignes * nCols; // nombre de cases total dans la grille

            Random random = new Random();
            for (int i = 0; i < nCase - 1; i++) // on se balade dans la grille
            {
                int j = random.Next(i, nCase); // on prend un nombre aléatoire entre la position i et la fin de notre grille [.Next(Int32, Int32) => Retourne un entier aléatoire qui se trouve dans une plage spécifiée.] ==> on déplace la case actuelle a une position aléatoire.

                // on convertit i et j en position xy pour pouvoir savoir où ils sont dans la grille
                int iLigne = i / nCols;
                int iCol = i % nCols;
                int jLigne = j / nCols;
                int jCol = j % nCols;

                // on echange les positions
                int temp = grille[iLigne, iCol]; // on place la valeur de la position i en memoire tampon
                grille[iLigne, iCol] = grille[jLigne, jCol];
                grille[jLigne, jCol] = temp;
            }
        }

        public int Voisins(int x, int y) // renvoi le nombre de voisins vivants autour d'une coordonnée donnée
        {
            //on récupère les dimensions de la grille
            int nLignes = grille.GetUpperBound(0) + 1; // GetUpperBound => on récupère l'index du dernier élements de la dimension n (ici 0)
            int nCols = grille.GetUpperBound(1) + 1;
            //l("voisins -> données", $"x : {x} | y : {y} | nLignes : {nLignes} | nCols : {nCols}");
            int vivants = 0;
            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int j = y - 1; j <= y + 1; j++)
                {
                    //l("voisins -> s1", $"{i}:{j}");
                    int ti = i; // on copie i et j avant modif dans une mémoire tampon pour pouvoir faire des modifs dessus
                    int tj = j;
                    if (ti < 0) //si ti est trop en haut de la grille on lui donne une valeur en bas puisque la grille est "ronde"
                    {
                        //l("voisins => Modif","m0");
                        ti = nLignes + ti;
                    }
                    if (tj < 0) //pareil pour tj
                    {
                        //l("voisins => Modif", "m1");
                        tj = nCols + tj;
                    }
                    if(ti > nLignes - 1) //si ti est trop en bas de la grille on lui donne une valeur en haut
                    {
                        //l("voisins => Modif", "m2");
                        ti = ti - nLignes;
                    }
                    if(tj > nCols - 1) // pareil pour tj
                    {
                        //l("voisins => Modif", "m3");
                        tj = tj - nCols;
                    }
                    //l("voisins -> s2", $"{ti}:{tj}");
                    //l("voisins -> v", grille[ti, tj].ToString());
                    vivants += grille[ti, tj];
                    //l("#########################");
                }
            }
            //l("##################################################");
            vivants -= grille[x,y]; // on retire la cellule observé du comptage si elle etait vivante
            return vivants;
        }
        
        public int[] VoisinsV2 (int x, int y, int nPop, int rang)
        {
            //on récupère les dimensions de la grille
            int nLignes = grille.GetUpperBound(0) + 1; // GetUpperBound => on récupère l'index du dernier élements de la dimension n (ici 0)
            int nCols = grille.GetUpperBound(1) + 1;
  
            int[] voisins = new int [nPop];
            for (int i = x - (1*rang); i <= x + (1*rang); i++)
            {
                for (int j = y - (1 * rang); j <= y + (1 * rang); j++)
                {
                    int ti = i; // on copie i et j avant modif dans une mémoire tampon pour pouvoir faire des modifs dessus
                    int tj = j;
                    if (ti < 0) //si ti est trop en haut de la grille on lui donne une valeur en bas puisque la grille est "ronde"
                    {
                        ti = nLignes + ti;
                    }
                    if (tj < 0) //pareil pour tj
                    {
                        tj = nCols + tj;
                    }
                    if(ti > nLignes - 1) //si ti est trop en bas de la grille on lui donne une valeur en haut
                    {
                        ti = ti - nLignes;
                    }
                    if(tj > nCols - 1) // pareil pour tj
                    {
                        tj = tj - nCols;
                    }
                    if (grille[ti, tj] != 0) //si la case est vivante ajoute 1 dans l'index correspondant à sa couleur
                    {
                        voisins[grille[ti, tj]-1] += 1;
                    }
                }
            }
            if (grille[x, y] != 0)
            {
                voisins[grille[x, y]-1] -= 1;
            }
            return voisins;
        }

        public int[] PopulationTotale(int nPop)
        {
            //on récupère les dimensions de la grille
            int nLignes = grille.GetUpperBound(0) + 1; // GetUpperBound => on récupère l'index du dernier élements de la dimension n (ici 0)
            int nCols = grille.GetUpperBound(1) + 1;
            int[] pop = new int[nPop];
            for (int i = 0 ; i < nLignes ; i++)
            {
                for (int j = 0 ; j < nCols ; j++)
                {
                    if (grille[i, j] != 0)
                    {
                        pop[grille[i, j]-1] += 1;
                    }
                }
            }
            return pop;
        }

        public int Evoluer(int x, int y) //renvoie vrai ou faux si la cellule est vivante ou morte au prochain tour
        {
            int vie = 0;
            if (grille[x, y] == 1)
            {
                if (Voisins(x, y) >= 2 && Voisins(x, y) <= 3) //si la cellule et vivante et à 2 ou 3 voisins elle le reste
                {
                    vie = 1;
                }
            }
            else if (grille[x, y] == 0)
            {
                if (Voisins(x, y) == 3) //si la cellule est morte et à exactement 3 voisins elle devient vivante
                {
                    vie = 1;
                }
            }
            return vie;
        }

        public int[] EvoluerV2(int x, int y, int nPop)
        {
            int[] vie = new int [2];
            int[] voisins = VoisinsV2(x, y, nPop, 1);
            int[] voisins2 = VoisinsV2(x, y, nPop, 2);
            
            
            for (int i = 0 ; i < voisins.Length; i++)
            {
                if (grille[x, y] != 0)
                {
                    if(voisins[grille[x, y] - 1] < 2) //sous-population
                    {
                        vie[0] = 0;
                        vie[1] = grille[x, y];
                    }
                    if (voisins[grille[x, y] - 1] > 3) //sur-population
                    {
                        vie[0] = 0;
                        vie[1] = grille[x, y];
                    }
                    if (voisins[grille[x, y]-1] == 2 || voisins[grille[x, y]-1] == 3) // reste du temps
                    {
                        vie[0] = 1;
                        vie[1] = grille[x, y];
                    }
                }
                else if (grille[x, y] == 0)
                {

                    if (voisins[grille[x, y] - 1] == 3) //R3b
                    {
                        int voisinsTotal = 0;
                        for (int j = 0; j < voisins.Length; j++)
                        {
                            if(j != grille[x, y] - 1)
                            {
                                voisinsTotal += voisins[j];
                            }
                        }
                        if(voisinsTotal == 3)
                        {
                            vie[0] = 1;
                            vie[1] = grille[x, y];
                        }
                        else
                        {
                            vie[0] = 0;
                            vie[1] = grille[x, y];
                        }
                    }


                }
            }
            return vie;
        }

        public static void Print2DArray(int[,] matrix) //methode a supprimer avant envoi
        {
            Console.Write("\t");
            for(int i = 0;i < matrix.GetLength(1); i++)
            {
                Console.Write(i + "\t");
            }
            Console.WriteLine();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                Console.Write(i + "\t");
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if(matrix[i, j] == 1)
                    {
                        Console.Write("#" + "\t");
                    }else if(matrix[i, j] == 0)
                    {
                        Console.Write("·" + "\t");
                    }
                    else if (matrix[i, j] == 42)
                    {
                        Console.Write("-" + "\t");
                    }
                    else if (matrix[i, j] == 666)
                    {
                        Console.Write("*" + "\t");
                    }
                }
                Console.WriteLine();
            }
        }

        public void l(string data)
        {
            Console.WriteLine(data);
        }
        public void l(string tag, string data)
        {
            Console.WriteLine("[" + tag + "] " + data);
        }
    }
}
