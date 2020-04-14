using System;
using System.Text.RegularExpressions;

namespace JeuDeLaVie
{

    class Program
    {
        static int[,] grille;
        static ConsoleColor[] colors = (ConsoleColor[])ConsoleColor.GetValues(typeof(ConsoleColor)); // liste des couleurs possible pour le texte de la console
        static void Main(string[] args)
        {
            int x, y = 0;
            // On demande a l'utilisateur les dimensions de la grille souhaitée
            do
            {
                Console.Write("Dimensions de la grille [format : ?x? => ex : 10x12] > ");
                Regex regex = new Regex("^(.*?)x(.*?)$"); // on utilise une regular expression pour recuperer les infos d'un string selon un format particulier : ici on donne par exemple 10x12 et on obtiens 10 et 12
                Match match = regex.Match(Console.ReadLine());
                //l("x", match.Groups[1].Value);
                //l("y", match.Groups[2].Value);
                Int32.TryParse(match.Groups[1].Value, out x);
                Int32.TryParse(match.Groups[2].Value, out y);
            } while (x < 1 || y < 1);
            //on demande a l'utilisateur le taux de remplissage:
            double t = 0.0d;
            do
            {
                Console.Write("Taux de remplissage [0,1 ; 0,9] > ");
                Double.TryParse(Console.ReadLine().Replace('.',','), out t); // il faut remplacer les . dans le nombre si l'utilisateur en met parce que tryparse ne sait pas convertir un double contenant un point pour la virgule.
            } while (t == 0.0d || t < 0.1 || t > 0.9);

            //on demande a l'utilisateur si il veut voir les étapes intermédiaires
            int visuStateChoice = -1;
            do
            {
                Console.WriteLine("Menu :");
                Console.WriteLine("[0]  Jeu DLV classique sans visualisation intermédiaire des états futurs ");
                Console.WriteLine("[1]  Jeu DLV classique avec visualisation des états futurs (à naître et à mourir)");
                Console.Write("Votre choix > ");
                Int32.TryParse(Console.ReadLine(),out visuStateChoice);
            } while (visuStateChoice == -1 || visuStateChoice > 1);
            bool visuState = visuStateChoice == 0 ? false:true;

            //il faut coder la recup des infos a l'utilisateur
            l("données", $" x : {x} | y : {y} | t : {t} | visuState : {visuState}");
            grille = new int[x, y];
            InitGrille(x, y, t);
            for (int z = 0; z < 5; z++)
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

                l("après Evolution " + z);
                Print2DArray(grille);
                System.Threading.Thread.Sleep(1000);
            }
        }

        static void InitGrille(int x, int y, double t)
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

        static void InitGrilleV2(int x, int y, double t, int nPop)
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
                if (currentColor == nPop + 1)
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

        static void MelangerGrille()
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

        static int Voisins(int x, int y) // renvoi le nombre de voisins vivants autour d'une coordonnée donnée
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
                    if (ti > nLignes - 1) //si ti est trop en bas de la grille on lui donne une valeur en haut
                    {
                        //l("voisins => Modif", "m2");
                        ti = ti - nLignes;
                    }
                    if (tj > nCols - 1) // pareil pour tj
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
            vivants -= grille[x, y]; // on retire la cellule observé du comptage si elle etait vivante
            return vivants;
        }

        static int[] VoisinsV2(int x, int y, int nPop, int rang)
        {
            //on récupère les dimensions de la grille
            int nLignes = grille.GetUpperBound(0) + 1; // GetUpperBound => on récupère l'index du dernier élements de la dimension n (ici 0)
            int nCols = grille.GetUpperBound(1) + 1;

            int[] voisins = new int[nPop];
            for (int i = x - (1 * rang); i <= x + (1 * rang); i++)
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
                    if (ti > nLignes - 1) //si ti est trop en bas de la grille on lui donne une valeur en haut
                    {
                        ti = ti - nLignes;
                    }
                    if (tj > nCols - 1) // pareil pour tj
                    {
                        tj = tj - nCols;
                    }
                    if (grille[ti, tj] != 0) //si la case est vivante ajoute 1 dans l'index correspondant à sa couleur
                    {
                        voisins[grille[ti, tj] - 1] += 1;
                    }
                }
            }
            if (grille[x, y] != 0)
            {
                voisins[grille[x, y] - 1] -= 1;
            }
            return voisins;
        }

        static int[] PopulationTotale(int nPop)
        {
            //on récupère les dimensions de la grille
            int nLignes = grille.GetUpperBound(0) + 1; // GetUpperBound => on récupère l'index du dernier élements de la dimension n (ici 0)
            int nCols = grille.GetUpperBound(1) + 1;
            int[] pop = new int[nPop];
            for (int i = 0; i < nLignes; i++)
            {
                for (int j = 0; j < nCols; j++)
                {
                    if (grille[i, j] != 0)
                    {
                        pop[grille[i, j] - 1] += 1;
                    }
                }
            }
            return pop;
        }

        static int Evoluer(int x, int y) //renvoie vrai ou faux si la cellule est vivante ou morte au prochain tour
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

        int[] EvoluerV2(int x, int y, int nPop)
        {
            int[] vie = new int[2];
            int[] voisins = VoisinsV2(x, y, nPop, 1);
            int[] voisins2 = VoisinsV2(x, y, nPop, 2);

            if (grille[x, y] != 0) // règles R1b et R2b
            {
                if (voisins[grille[x, y] - 1] >= 2 && voisins[grille[x, y] - 1] <= 3) // si la cellule est vivante et a 2 ou 3 voisins de sa population alors elle reste vivante
                {
                    vie[0] = 1;
                    vie[1] = grille[x, y];
                }
                else // sinon elle meurt
                {
                    vie[0] = 0;
                    vie[1] = 0;
                }
            }
            else // règles R3b et R4b
            {
                int nombreVoisins = 0;
                foreach (int element in voisins) // compte le nombre de cellules voisines au rang 1 (toutes populations confondues)
                {
                    nombreVoisins += element;
                }
                if (nombreVoisins == 3) // s'il y a 3 celules voisines, regarde si elles sont toutes de la même famille [R3b]
                {
                    int famille = -1;
                    for (int i = 0; i < voisins.Length; i++)
                    {
                        if (voisins[i] == 3)
                        {
                            famille = i + 1;
                        }
                    }
                    if (famille != -1) // si les 3 cellules sont de la même famille, alors la cellule nait
                    {
                        vie[0] = 1;
                        vie[1] = famille;
                    }
                    else // sinon elle reste morte
                    {
                        vie[0] = 0;
                        vie[1] = 0;
                    }
                }
                else if (nombreVoisins == 6) // s'il y a 6 cellules voisines, regarde si elles appartiennent à 2 familles avec 3 cellules par famille [R4b]
                {
                    int famille1 = -1;
                    int famille2 = -1;
                    for (int i = 0; i < voisins.Length; i++)
                    {
                        if (voisins[i] == 3) // si 3 cellules appartiennent à une seule famille
                        {
                            if (famille1 == -1) // enregistre la première famille
                            {
                                famille1 = i + 1;
                            }
                            else // enregistre la deuxième famille
                            {
                                famille2 = i + 1;
                            }
                        }
                    }
                    if (famille1 != -1 && famille2 != -1) // si les 6 cellules voisines sont réparties dans 2 familles avec 3 cellules par familles
                    {
                        if (voisins2[famille1 - 1] > voisins2[famille2 - 1]) // s'il y a plus de cellules de la première famille que de la deuxième au rang 2 alors la cellule nait de la couleur de la première famille au tour prochain
                        {
                            vie[0] = 1;
                            vie[1] = famille1;
                        }
                        else if (voisins2[famille1 - 1] < voisins2[famille2 - 1]) //s'il y a plus de cellules de la deuxième famille que de la prmière au rang 2 alors la cellule nait de la couleur de la deuxième famille au tour prochain
                        {
                            vie[0] = 1;
                            vie[1] = famille2;
                        }
                        else //sinon la cellule reste morte
                        {
                            vie[0] = 0;
                            vie[1] = 0;
                        }
                    }
                }
            }
            return vie;
        }

        static void Print2DArray(int[,] matrix) //methode a supprimer avant envoi
        {
            Console.Write("\t");
            for (int i = 0; i < matrix.GetLength(1); i++)
            {
                Console.Write(i + "\t");
            }
            Console.WriteLine();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                Console.Write(i + "\t");
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] == 1)
                    {
                        Console.Write("#" + "\t");
                    }
                    else if (matrix[i, j] == 0)
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

        static void l(string data)
        {
            Console.WriteLine(data);
        }
        static void l(string tag, string data)
        {
            Console.WriteLine("[" + tag + "] " + data);
        }


    }
}
