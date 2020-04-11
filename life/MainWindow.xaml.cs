using System;
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
        public MainWindow(int x, int y, double t, bool visuState)
        {
            //x et y : taille de la grille
            // t : le taux
            // visuState : le on/off de tout a l'heure

            InitializeComponent();
            l("données", $" x : {x} | y : {y} | t : {t} | visuState : {visuState}");
            grille = new int[x, y];
            InitGrille(x, y, t);


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
            int nCase = nLignes * nCols; // nombre de cases total dans la grille => inutile, on va l'enlever

            int vivants = 0;
            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (i < 0) //si i est trop a gauche de la grille on lui donne une valeur a droite puisque la grille est "ronde"
                    {
                        i = nCols + i;
                    }
                    if (j < 0) //pareil pour j
                    {
                        j = nLignes + j;
                    }
                    if(i > nCols - 1) //si i est trop a droite de la grille on lui donne une valeur a gauche
                    {
                        i = i - nCols;
                    }
                    if(j > nLignes - 1) // pareil pour j
                    {
                        j = j - nLignes;
                    }
                    if(grille[i,j] == 1 && i!=x && j!=y) //si la case observé n'est pas la case dont on cherches les voisins et que la cellule qu'elle contient est vivante on incrémente le nombre de voisins vivants
                    {
                        vivants++;
                    }
                }
            }
            return vivants;
        }
        public bool Evoluer(int x, int y, int[,] grille) //renvoie vrai ou faux si la cellule est vivante ou morte au prochain tour
        {
            bool Vie = false;
            if (grille[x, y] == 1)
            {
                if (Voisins(x, y) > 2 && Voisins(x, y) < 3)
                {
                    Vie = true;
                }
            }
            else if (grille[x, y] == 0)
            {
                if (Voisins(x, y) == 3)
                {
                    Vie = true;
                }
            }
            return Vie;
        }

        public static void Print2DArray<T>(T[,] matrix) //methode a supprimer avant envoi
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(matrix[i, j] + "\t");
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
