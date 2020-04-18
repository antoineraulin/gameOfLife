using EsilvGui;
using System;
using System.Text.RegularExpressions;

namespace jeuDeLaVie
{
    class Program
    {
        // Zone des variables globales
        static int[,] grille;

        [STAThread()] // on utilise esilvGUI, esilvGUI est basé sur winForms, et la plateforme winForms nécéssite que tout les controlleurs soient géré par un et un seul Thread, STAThread (STA : Single-Threaded Apartment) permet de forcer le code a n'utiliser qu'un seul thread a l'inverse de la commande MTAThread.
        static void Main()
        {
            object[] param = RecupererParametre();
            int x = (int)param[0];
            int y = (int)param[1];
            double t = (double)param[2];
            int versionChoice = (int)param[3];
            bool visuState = (bool)param[4];
            int tailleCase = (int)param[5];

            grille = new int[x, y];
            InitGrille(x, y, t, versionChoice + 1); // on initialise la grille avec soit 1 soit 2 populations puisque choisir la version revient a avoir 0 dans versionChoice pour la version classique et 1 pour la variante.
            Fenetre gui = new Fenetre(grille, tailleCase, 0, 0, "Jeu de la vie");

            bool stop = false; // condition d'arret de la boucle.
            int gen = 1; // numéro de la génération en cours
            int[,] grilleDeComparaison1;                     // grilleDeComparaison1 et 2 sont des historiques des modifitions apportés a grille, afin de voir si la grille est stable sur 3 générations consécutives.
            int[,] grilleDeComparaison2 = new int[x, y];

            while (!stop)
            {
                grilleDeComparaison1 = grilleDeComparaison2;    // on met a jour l'historique de grille.
                grilleDeComparaison2 = grille.Clone() as int[,];

                int[,] grilleTemp = grille.Clone() as int[,]; // grilleTemp est la grille sur laquelle on va faire les modifications d'évolution avant de les appliquer sur grille pour eviter les conflits et on ne peut pas modifier grille directement puisqu'on s'appuit sur grille pour determiner l'évolution.

                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        int[] e = Evoluer(i, j);
                        if (grille[i, j] == 0 && e[0] == 1)
                        {
                            grilleTemp[i, j] = e[1] == 1 ? 42 : 142;

                        }
                        else if (grille[i, j] != 0 && e[0] == 0)
                        {
                            grilleTemp[i, j] = e[1] == 1 ? 666 : 1666;
                        }
                    }
                }
                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        if (grilleTemp[i, j] == 42)
                        {
                            grille[i, j] = 1;
                        }
                        else if (grilleTemp[i, j] == 142)
                        {
                            grille[i, j] = 2;
                        }
                        else if (grilleTemp[i, j] == 666 || grilleTemp[i, j] == 1666)
                        {
                            grille[i, j] = 0;
                        }
                    }
                }
                if (visuState)
                {
                    Console.WriteLine("Futur " + gen);
                    AfficherMatrice(grilleTemp);
                }

                Console.WriteLine("après Evolution " + gen);
                AfficherMatrice(grille);
                if (ComparerGrilles(grille, grilleDeComparaison1, grilleDeComparaison2))
                {
                    stop = true;
                }
                gui.RafraichirTout();
                int[] pop = PopulationTotale(grille);
                gui.changerMessage($"Génération {gen}. Actuellement {pop[0]} cellules vivantes" + (pop.Length > 1 ? $"de la population noire et {pop[1]} pour les verts" : ""));
                System.Threading.Thread.Sleep(500);
                gen++;

            }
            gui.changerMessage("Terminé");
            Console.ReadKey();
        }
        static object[] RecupererParametre()
        {
            object[] res = new object[6];
            int x, y;
            // On demande a l'utilisateur les dimensions de la grille souhaitée
            do
            {
                Console.Write("Dimensions de la grille [format : ?x? => ex : 10x12] > ");
                Regex regex = new Regex("^(.*?)x(.*?)$"); // on utilise une regular expression pour recuperer les infos d'un string selon un format particulier : ici on donne par exemple 10x12 et on obtiens 10 et 12
                Match match = regex.Match(Console.ReadLine());
                int.TryParse(match.Groups[1].Value, out x);
                int.TryParse(match.Groups[2].Value, out y);
            } while (x < 1 || y < 1);
            res[0] = x;
            res[1] = y;
            //on demande a l'utilisateur le taux de remplissage:
            double t;
            do
            {
                Console.Write("Taux de remplissage [0,1 ; 0,9] > ");
                double.TryParse(Console.ReadLine().Replace('.', ','), out t); // il faut remplacer les . dans le nombre si l'utilisateur en met parce que tryparse ne sait pas convertir un double contenant un point pour la virgule.
            } while (t == 0.0d || t < 0.1 || t > 0.9);
            res[2] = t;

            //on demande a l'utilisateur la taille des case du GUI
            int tailleCase;
            do
            {
                Console.Write("Taille des cases du GUI (> 1) > ");
                int.TryParse(Console.ReadLine(), out tailleCase);
            } while (tailleCase < 1);
            res[5] = tailleCase;

            //on demande à l'utilisateur quelle version du jeu il veut utiliser
            int versionChoice = -1;
            do
            {
                Console.WriteLine("Menu Version :");
                Console.WriteLine("[0]  Jeu DLV classique");
                Console.WriteLine("[1]  Jeu DLV variante");
                Console.Write("Votre choix > ");
                int.TryParse(Console.ReadLine(), out versionChoice);
            } while (versionChoice == -1 || versionChoice > 1);
            res[3] = versionChoice;
            //on demande a l'utilisateur si il veut voir les étapes intermédiaires
            int visuStateChoice = -1;
            do
            {
                Console.WriteLine("Menu Visualisation :");
                Console.WriteLine("[0]  Jeu DLV sans visualisation intermédiaire des états futurs ");
                Console.WriteLine("[1]  Jeu DLV avec visualisation des états futurs (à naître et à mourir)");
                Console.Write("Votre choix > ");
                int.TryParse(Console.ReadLine(), out visuStateChoice);
            } while (visuStateChoice == -1 || visuStateChoice > 1);
            bool visuState = visuStateChoice == 0 ? false : true;
            res[4] = visuState;
            return res;
        }
        static bool ComparerGrilles(int[,] grille1, int[,] grilleC1, int[,] grilleC2)
        {
            int[] pop = PopulationTotale(grille1);
            int[] popC1 = PopulationTotale(grilleC1);
            int[] popC2 = PopulationTotale(grilleC2);
            bool res = false;
            if (pop[0] == popC1[0] && pop[1] == popC1[1] && pop[0] == popC2[0] && pop[1] == popC2[1])
            {
                res = true;
            }
            return res;
        }
        static void InitGrille(int x, int y, double t, int nPop)
        {
            int nCase = x * y; // nombre de cases total dans la grille
            int n = (int)(t * nCase); // nombre de case a remplir pour atteindre le taux spécifié
            int currentColor = 1; //couleur de la cellule, on commence a 1 parce que la couleur 0 est le noir et comme le fond de la console est noir ce sera illisible, de plus dans notre système le chiffre zero est reservé pour les cellules mortes ou inexistantes
            for (int i = 0; i < nCase; i++)
            {
                // on convertit i en position xy pour pouvoir savoir où il est dans la grille
                int iLigne = i / y;
                int iCol = i % y;

                if (currentColor == nPop + 1) //Si on depasse le nombre de population en terme de couleur on revient a 1.
                {
                    currentColor = 1;
                }

                grille[iLigne, iCol] = i < n ? currentColor : 0; // si i est dans l'interval [0,n[ alors la case doit contenir la valeur de currentColor (si on est en mode classique ce sera forcément 1, sinon ce sera soit 1 soit 2) parce qu'on est encore dans le cadre du taux fixé, sinon on met un 0.
                currentColor++;
            }
            //maintenant que la grille est remplit au taux fixé on la mélange
            MelangerGrille();
            AfficherMatrice(grille);
        }
        static int[] Voisins(int x, int y, int rang) // renvoi le nombre de voisins vivants autour d'une coordonnée donnée sous forme d'un tableau comprenant le numéro de la population et le nombre de voisins vivants de cette populations autour de la cellule observée.
        {
            //on récupère les dimensions de la grille
            int nLignes = grille.GetUpperBound(0) + 1; // GetUpperBound => on récupère l'index du dernier élements de la dimension n (ici 0)
            int nCols = grille.GetUpperBound(1) + 1;
            int[] voisins = new int[2];
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
                        ti -= nLignes;
                    }
                    if (tj > nCols - 1) // pareil pour tj
                    {
                        tj -= nCols;
                    }
                    if (grille[ti, tj] != 0) //si la case est vivante ajoute 1 dans l'index correspondant à sa couleur
                    {
                        voisins[grille[ti, tj] - 1] += 1;
                    }
                }
            }
            if (grille[x, y] != 0) // on retire la cellule observé du comptage si elle etait vivante
            {
                voisins[grille[x, y] - 1] -= 1;
            }
            return voisins;
        }
        static int[] PopulationTotale(int[,] g) // compte le nombre de cellules vivantes de chaque population
        {
            // on récupère les dimensions de la grille
            int nLignes = g.GetUpperBound(0) + 1; // GetUpperBound => on récupère l'index du dernier élements de la dimension n (ici 0)
            int nCols = g.GetUpperBound(1) + 1;
            int[] pop = new int[2];
            for (int i = 0; i < nLignes; i++)
            {
                for (int j = 0; j < nCols; j++)
                {
                    if (g[i, j] != 0)
                    {
                        pop[g[i, j] - 1] += 1;
                    }
                }
            }
            return pop;
        }
        static int[] Evoluer(int x, int y)
        {
            int[] vie = new int[2];
            int[] voisins = Voisins(x, y, 1);
            int[] voisins2 = Voisins(x, y, 2);

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
                    vie[1] = grille[x, y];
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
                        vie[1] = grille[x, y];
                    }
                }
                else if (nombreVoisins == 6) // s'il y a 6 cellules voisines, regarde si elles appartiennent à 2 familles avec 3 cellules par famille [R4b]
                {
                    int famille1 = -1;
                    int famille2 = -1;
                    int[] pop = PopulationTotale(grille);
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
                        else if (voisins2[famille1 - 1] < voisins2[famille2 - 1]) // s'il y a plus de cellules de la deuxième famille que de la première au rang 2 alors la cellule nait de la couleur de la deuxième famille au tour prochain
                        {
                            vie[0] = 1;
                            vie[1] = famille2;
                        }
                        else if (pop[0] != pop[1]) // si il n'y a pas autant de cellules vivantes de chaque population
                        {
                            if (pop[0] > pop[1]) // s'il y a au total plus de cellules de la première famille que de la deuxième, alors la cellule nait et sera de la couleur de la première famille
                            {
                                vie[0] = 1;
                                vie[1] = 1;
                            }
                            else if (pop[1] > pop[0]) // s'il y a au total plus de cellules de la deuxième famille que de la première, alors la cellule nait et sera de la couleur de la deuxième famille
                            {
                                vie[0] = 1;
                                vie[1] = 2;
                            }
                        }
                        else //sinon la cellule reste morte
                        {
                            vie[0] = 0;
                            vie[1] = grille[x, y];
                        }
                    }
                }
            }
            return vie;
        }
        static void AfficherMatrice(int[,] mat)
        {
            Console.Write("\t");
            for (int i = 0; i < mat.GetLength(1); i++)
            {
                Console.Write(i + "\t");
            }
            Console.WriteLine();
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                Console.Write(i + "\t");
                for (int j = 0; j < mat.GetLength(1); j++)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    switch (mat[i, j])
                    {
                        case 1:
                            Console.Write("#" + "\t");
                            break;
                        case 2:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("#" + "\t");
                            break;
                        case 0:
                            Console.Write("·" + "\t");
                            break;
                        case 42:
                            Console.Write("-" + "\t");
                            break;
                        case 142:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("-" + "\t");
                            break;
                        case 666:
                            Console.Write("*" + "\t");
                            break;
                        case 1666:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("*" + "\t");
                            break;
                    }
                }
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
            }
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
    }
}