using EsilvGui;
using System;
using System.Text.RegularExpressions;

namespace jeuDeLaVie
{
    class Program
    {
        // Zone des variables globales
        static int[,] grille; // grille générale
        static int[,] grilleTemp; //grille temporaire
        static int[,] guerisonGrille;
        static int gen = 1; //numéro de la génération en cours
        static int stadeDeconfinement; //numéro de la génération à partir de laquelle on autorise le déconfinement (version Covid)
        static bool test, test1; // deux booléens pour les tryParse
        public class Stade
        { //permet d'associer un stade comprehensible par un humain avec une couleur du GUI. Facilite ainsi la modification d'une couleur sans devoir réécrire l'entiereté du code et permet une lecture plus facile.
            public int sain = 4;
            public int stade0 = 2;
            public int stade1 = 5;
            public int stade2 = 6;
            public int stade3 = 3;
            public int mort = 1;
            public int immunise = 0;
            public int confine = 7;
            public Stade()
            {
            }
        }
        public class ParamJeuDeLaVie
        {
            public ParamJeuDeLaVie(int _x, int _y, double _tauxRemplissage, int _versionChoice, bool _visuState, int _tailleCase, bool _affichageConsole)
            {
                this.x = _x;
                this.y = _y;
                this.tauxRemplissage = _tauxRemplissage;
                this.versionChoice = _versionChoice;
                this.visuState = _visuState;
                this.tailleCase = _tailleCase;
                this.affichageConsole = _affichageConsole;
            }

            public int x, y, versionChoice, tailleCase;
            public double tauxRemplissage;
            public bool visuState, affichageConsole;

        }
        public class ParamCovid
        {
            public ParamCovid(int _x, int _y, int _versionChoice, double _tauxContamination, double[] _poidsStat, double _tauxConfinement, int _tailleCase, double _tauxGuerison, int _nombrePatientsZero)
            {
                this.x = _x;
                this.y = _y;
                this.versionChoice = _versionChoice;
                this.tauxContamination = _tauxContamination;
                this.poidsStat = _poidsStat;
                this.tauxConfinement = _tauxConfinement;
                this.tailleCase = _tailleCase;
                this.tauxGuerison = _tauxGuerison;
                this.nombrePatientsZero = _nombrePatientsZero;
            }

            public int x, y, versionChoice, tailleCase, nombrePatientsZero;
            public double tauxContamination, tauxConfinement, tauxGuerison;
            public double[] poidsStat;
        }
        static Stade stade = new Stade();
        static Random random = new Random();
        static Fenetre gui;
        // Fin de zone des variables globales

        [STAThread()] // on utilise esilvGUI, esilvGUI est basé sur winForms, et la plateforme winForms nécéssite que tout les controlleurs soient géré par un et un seul Thread, STAThread (STA : Single-Threaded Apartment) permet de forcer le code a n'utiliser qu'un seul thread a l'inverse de la commande MTAThread.
        static void Main()
        {
            //on demande à l'utilisateur quelle version du jeu il veut utiliser
        
                    int version = -1;
                    do
                    {
                        Console.WriteLine("Menu Version :");
                        Console.WriteLine("[0]  Jeu De La Vie");
                        Console.WriteLine("[1]  Version Covid");
                        Console.Write("Votre choix > ");
                        test = int.TryParse(Console.ReadLine(), out version);
                    } while (version == -1 || version > 1 || !test);
                    
                    if (version == 0)
                    {
                        //version jeu de la vie
                        ParamJeuDeLaVie param = RecupererParametreJDLV();

                        grille = new int[param.x, param.y];
                        InitGrilleJDLV(param.x, param.y, param.tauxRemplissage, param.versionChoice + 1); // on initialise la grille avec soit 1 soit 2 populations puisque choisir la version revient a avoir 0 dans versionChoice pour la version classique et 1 pour la variante.
                        gui = new Fenetre(grille, param.tailleCase, 0, 0, "Jeu de la vie");

                        bool stop = false; // condition d'arret de la boucle.
                        int[,] grilleDeComparaison1;                     // grilleDeComparaison1 et 2 sont des historiques des modifitions apportés a grille, afin de voir si la grille est stable sur 3 générations consécutives.
                        int[,] grilleDeComparaison2 = new int[param.x, param.y];

                        while (!stop)
                        {
                            grilleDeComparaison1 = grilleDeComparaison2;    // on met a jour l'historique de grille.
                            grilleDeComparaison2 = grille.Clone() as int[,];

                            grilleTemp = grille.Clone() as int[,]; // grilleTemp est la grille sur laquelle on va faire les modifications d'évolution avant de les appliquer sur grille pour eviter les conflits et on ne peut pas modifier grille directement puisqu'on s'appuit sur grille pour determiner l'évolution.

                            for (int i = 0; i < param.x; i++)
                            {
                                for (int j = 0; j < param.y; j++)
                                {
                                    int[] e = EvoluerJDLV(i, j);
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
                            for (int i = 0; i < param.x; i++)
                            {
                                for (int j = 0; j < param.y; j++)
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
                            if (param.visuState)
                            {
                                Console.WriteLine("Futur " + gen);
                                AfficherMatrice(grilleTemp);
                            }

                            Console.WriteLine("après Evolution " + gen);
                            if (param.affichageConsole)
                            {
                                AfficherMatrice(grille);
                            }
                            if (ComparerGrilles(grille, grilleDeComparaison1, grilleDeComparaison2))
                            {
                                stop = true;
                            }
                            gui.RafraichirTout();
                            int[] pop = PopulationTotale(grille);
                            gui.changerMessage($"Génération {gen}. Actuellement {pop[0]} cellules vivantes" + (pop.Length > 1 ? $"de la population noire et {pop[1]} pour les verts" : ""));
                            System.Threading.Thread.Sleep(100);
                            gen++;

                        }
                        gui.changerMessage("Terminé");
                    }
                    else
                    {
                        //version covid
                        ParamCovid param = RecupererParametreCovid();

                        InitGrilleCovid(param.x, param.y, param.nombrePatientsZero, param.tauxConfinement);
                        gui = new Fenetre(grille, param.tailleCase, 0, 0, "Jeu de la vie - COVID");
                        Console.WriteLine("Appuyez sur `Entrée` pour démarrer la simulation.");
                        Console.ReadKey();
                        bool stop = false;
                        while (!stop)
                        {

                            grilleTemp = grille.Clone() as int[,]; // grilleTemp est la grille sur laquelle on va faire les modifications d'évolution avant de les appliquer sur grille pour eviter les conflits et on ne peut pas modifier grille directement puisqu'on s'appuit sur grille pour determiner l'évolution.

                            for (int i = 0; i < param.x; i++)
                            {
                                for (int j = 0; j < param.y; j++)
                                {
                                    EvoluerCovid(i, j, param.tauxContamination, param.poidsStat, param.tauxGuerison);
                                }
                            }
                            for (int i = 0; i < param.x; i++)
                            {
                                for (int j = 0; j < param.y; j++)
                                {
                                    grille[i, j] = grilleTemp[i, j];
                                }
                            }
                            gui.RafraichirTout();
                            int[] pop = Population();
                            gui.changerMessage("Génération " + gen + " | sains non-immunisés : " + pop[0] + " | sains immunisés : " + pop[6] + " | stade 0 : " + pop[1] + " | stade 1 : " + pop[2] + " | stade 2 : " + pop[3] + " | stade 3 : " + pop[4] + " | morts : " + pop[5] + " | confinés : " + pop[7]);
                            System.Threading.Thread.Sleep(100);
                            gen++;
                            if (pop[1] == 0 && pop[2] == 0 && pop[3] == 0 && pop[4] == 0)
                            {
                                //il n'y a plus de malade, on a atteint la stabilité
                                gui.changerMessage("Terminé | Gen " + gen + " | sains non-immunisés : " + pop[0] + " | sains immunisés : " + pop[6] + " | stade 0 : " + pop[1] + " | stade 1 : " + pop[2] + " | stade 2 : " + pop[3] + " | stade 3 : " + pop[4] + " | morts : " + pop[5] + " | confinés : " + pop[7]);
                                stop = true;
                            }
                        }
                    }

                    
                    Console.ReadKey();
            
        }
        static ParamJeuDeLaVie RecupererParametreJDLV()
        {
            int x, y;
            // On demande a l'utilisateur les dimensions de la grille souhaitée
            do
            {
                Console.Write("Dimensions de la grille [format : ?x? => ex : 10x12] > ");
                Regex regex = new Regex("^(.*?)x(.*?)$"); // on utilise une regular expression pour recuperer les infos d'un string selon un format particulier : ici on donne par exemple 10x12 et on obtiens 10 et 12
                Match match = regex.Match(Console.ReadLine());
                test = int.TryParse(match.Groups[1].Value, out x);
                test1 = int.TryParse(match.Groups[2].Value, out y);
            } while (x < 1 || y < 1 || !test || !test1);
            //on demande a l'utilisateur le taux de remplissage:
            double tauxRemplissage;
            do
            {
                Console.Write("Taux de remplissage [0,1 ; 0,9] > ");
                test = double.TryParse(Console.ReadLine().Replace('.', ','), out tauxRemplissage); // il faut remplacer les . dans le nombre si l'utilisateur en met parce que tryparse ne sait pas convertir un double contenant un point pour la virgule.
            } while (tauxRemplissage == 0.0d || tauxRemplissage < 0.1 || tauxRemplissage > 0.9 || !test);
            //on demande a l'utilisateur la taille des case du GUI
            int tailleCase;
            do
            {
                Console.Write("Taille des cases du GUI (> 1) > ");
                test = int.TryParse(Console.ReadLine(), out tailleCase);
            } while (tailleCase < 1 || !test);
            //on demande à l'utilisateur quelle version du jeu il veut utiliser
            int versionChoice = -1;
            do
            {
                Console.WriteLine("Menu Version :");
                Console.WriteLine("[0]  Jeu DLV classique");
                Console.WriteLine("[1]  Jeu DLV variante");
                Console.Write("Votre choix > ");
                test = int.TryParse(Console.ReadLine(), out versionChoice);
            } while (versionChoice == -1 || versionChoice > 1 || !test);
            //on demande a l'utilisateur si il veut voir les étapes intermédiaires
            int visuStateChoice = -1;
            do
            {
                Console.WriteLine("Menu Visualisation :");
                Console.WriteLine("[0]  Jeu DLV sans visualisation intermédiaire des états futurs ");
                Console.WriteLine("[1]  Jeu DLV avec visualisation des états futurs (à naître et à mourir)");
                Console.Write("Votre choix > ");
                test = int.TryParse(Console.ReadLine(), out visuStateChoice);
            } while (visuStateChoice == -1 || visuStateChoice > 1 || !test);
            bool visuState = visuStateChoice == 0 ? false : true;
            //on demande a l'utilisateur si il veut voir la grille dans la console
            int _affichageConsole = -1;
            do
            {
                Console.Write("Afficher la grille dans la console [N/o] > ");
                string c = Console.ReadLine().ToLower();
                if (c == "n" || c == "non" || c == "no" || c == "")
                {
                    _affichageConsole = 0;
                }
                else if (c == "o" || c == "y" || c == "oui" || c == "yes")
                {
                    _affichageConsole = 1;
                }
            } while (_affichageConsole == -1);
            bool affichageConsole = _affichageConsole == 0 ? false:true;
            ParamJeuDeLaVie res = new ParamJeuDeLaVie(x, y, tauxRemplissage, versionChoice, visuState, tailleCase,affichageConsole);
            return res;
        }
        static ParamCovid RecupererParametreCovid()
        {
            int x, y;
            // On demande a l'utilisateur les dimensions de la grille souhaitée
            do
            {
                Console.Write("Dimensions de la grille [format : ?x? => ex : 10x12] > ");
                Regex regex = new Regex("^(.*?)x(.*?)$"); // on utilise une regular expression pour recuperer les infos d'un string selon un format particulier : ici on donne par exemple 10x12 et on obtiens 10 et 12
                Match match = regex.Match(Console.ReadLine());
                test = int.TryParse(match.Groups[1].Value, out x);
                test1 = int.TryParse(match.Groups[2].Value, out y);
            } while (x < 1 || y < 1 || !test || !test1);

            // on demande a l'utilisateur le mode de la simulation:
            int versionChoice = -1;
            do
            {
                Console.WriteLine("Menu Mode :");
                Console.WriteLine("[0]  Sans confinement");
                Console.WriteLine("[1]  Avec confinement");
                Console.Write("Votre choix > ");
                test = int.TryParse(Console.ReadLine(), out versionChoice);
            } while (versionChoice == -1 || versionChoice > 1 || !test);

            // on demande a l'utilisateur s'il veut du  mode avancé.
            int advancedChoice = -1;
            do
            {
                Console.Write("Mode avancé (permet de modifier les paramètres) [N/o] > ");
                string c = Console.ReadLine().ToLower();
                if (c == "n" || c == "non" || c == "no" || c == "")
                {
                    advancedChoice = 0;
                }
                else if (c == "o" || c == "y" || c == "oui" || c == "yes")
                {
                    advancedChoice = 1;
                }
            } while (advancedChoice == -1);

            double tauxContamination = 0.3;
            double[] poidsStat = { 0.6, 0.2, 0.1, 0.02 };
            double tauxConfinement = versionChoice == 0.0 ? 0.0 : 0.8;
            int tailleCase = 0;
            double tauxGuerison = 0.2;
            int nombrePatientsZero = 1;
            stadeDeconfinement = -1;
            if (advancedChoice == 1)
            {
                // mode avancé

                //on demande a l'utilisateur le taux de contamination:
                double tauxContaminationAdvanced;
                do
                {
                    Console.Write("Taux de contamination [par default : 0,3] > ");
                    string c = Console.ReadLine();
                    if (c != "")
                    {
                        if (!double.TryParse(c.Replace(".", ","), out tauxContaminationAdvanced))
                        {
                            tauxContaminationAdvanced = 0.3;
                        }
                    }
                    else
                    {
                        tauxContaminationAdvanced = 0.3;
                    }

                } while (tauxContaminationAdvanced == 0.0d || tauxContaminationAdvanced < 0);
                tauxContamination = tauxContaminationAdvanced;

                for (int i = 0; i < 4; i++)
                {
                    double poidsStat0;
                    do
                    {
                        Console.Write($"Poids statistique pour passer du stade {i} à {i + 1} a la prochaine génération [{poidsStat[i] * 100}] > ");
                        string c = Console.ReadLine();
                        if (c != "")
                        {
                            if (double.TryParse(c.Replace(".", ","), out poidsStat0))
                            {
                                poidsStat0 /= 100;
                            }
                            else
                            {
                                poidsStat0 = poidsStat[i];
                            }

                        }
                        else
                        {
                            poidsStat0 = poidsStat[i];
                        }
                    } while (poidsStat0 == 0.0d);
                    poidsStat[i] = poidsStat0;
                }

                if (tauxConfinement != 0.0)
                {
                    double tauxConfinementAdvanced;
                    do
                    {
                        Console.Write("Taux de confinement [80] > ");
                        string c = Console.ReadLine();
                        if (c != "")
                        {
                            if (!double.TryParse(c.Replace(".", ","), out tauxConfinementAdvanced))
                            {
                                tauxConfinementAdvanced = 0.8;
                            }
                            else
                            {
                                tauxConfinementAdvanced /= 100;
                            }
                        }
                        else
                        {
                            tauxConfinementAdvanced = 0.8;
                        }
                    } while (tauxConfinementAdvanced == 0.0d || tauxConfinementAdvanced < 0);
                    tauxConfinement = tauxConfinementAdvanced;

                    int stadeDeconfinementAdvanced;
                    do
                    {
                        Console.Write("Stade de déconfinement > ");
                        string c = Console.ReadLine();
                        if (c != "")
                        {
                            if (!int.TryParse(c.Replace(".", ","), out stadeDeconfinementAdvanced))
                            {
                                stadeDeconfinementAdvanced = -1;
                            }
                        }
                        else
                        {
                            stadeDeconfinementAdvanced = -1;
                        }
                    } while (tauxConfinementAdvanced <= 0);
                    stadeDeconfinement = stadeDeconfinementAdvanced;
                }

                double tauxGuerisonAdvanced;
                do
                {
                    Console.Write("Taux de guerison [par default : 30] > ");
                    string c = Console.ReadLine();
                    if (c != "")
                    {
                        if (!double.TryParse(c.Replace(".", ","), out tauxGuerisonAdvanced))
                        {
                            tauxGuerisonAdvanced = 0.3;
                        }
                        else
                        {
                            tauxGuerisonAdvanced /= 100;
                        }
                    }
                    else
                    {
                        tauxGuerisonAdvanced = 0.3;
                    }

                } while (tauxGuerisonAdvanced == 0.0d || tauxGuerisonAdvanced < 0);
                tauxGuerison = tauxGuerisonAdvanced;

                //on demande a l'utilisateur la taille des case du GUI

                int nombrePatientsZeroAdvanced = 0;
                do
                {
                    Console.Write("Nombre de patients zero (> 0) > ");
                    test = int.TryParse(Console.ReadLine(), out nombrePatientsZeroAdvanced);
                } while (nombrePatientsZeroAdvanced == 0 || nombrePatientsZeroAdvanced < 0 || !test);
                nombrePatientsZero = nombrePatientsZeroAdvanced;
                do
                {
                    Console.Write("Taille des cases du GUI (> 1) > ");
                    test = int.TryParse(Console.ReadLine(), out tailleCase);
                } while (tailleCase < 1 || !test);


            }
            if (tailleCase == 0)
            {
                tailleCase = 20;
            }
            ParamCovid res = new ParamCovid(x, y, versionChoice, tauxContamination, poidsStat, tauxConfinement, tailleCase, tauxGuerison, nombrePatientsZero);
            return res;
        }
        static bool ComparerGrilles(int[,] grille1, int[,] grilleC1, int[,] grilleC2) // on prend en compte les grilles de 3 générations successives (n, n-1 et n-2  avec n la génération actuelle)
        {
            int[] pop = PopulationTotale(grille1);
            int[] popC1 = PopulationTotale(grilleC1);
            int[] popC2 = PopulationTotale(grilleC2);
            bool res = false;
            if (pop[0] == popC1[0] && pop[1] == popC1[1] && pop[0] == popC2[0] && pop[1] == popC2[1]) // si il y a autant de cellules vivantes de chaque famille, on considère que la grille s'est stabilisée
            {
                res = true;
            }
            return res;
        }
        static void InitGrilleJDLV(int x, int y, double taux, int nPop)
        {
            int nCase = x * y; // nombre de cases total dans la grille
            int n = (int)(taux * nCase); // nombre de case a remplir pour atteindre le taux spécifié
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
        static void InitGrilleCovid(int x, int y, int nombrePatientsZero, double tauxConfinement) // même principe que pour InitGrilleJDLV
        {

            grille = new int[x, y];
            guerisonGrille = new int[x, y];
            for (int i = 0; i < x * y; i++)
            {
                int iLigne = i / y;
                int iCol = i % y;
                guerisonGrille[iLigne, iCol] = 5;
            }

            int nombreConfine = (int)(x * y * tauxConfinement);

            for (int i = 0; i < nombreConfine; i++)
            {
                int iLigne = i / y;
                int iCol = i % y;
                grille[iLigne, iCol] = stade.confine;
            }

            for (int i = nombreConfine; i < x * y; i++)
            {
                int iLigne = i / y;
                int iCol = i % y;
                grille[iLigne, iCol] = stade.sain;

            }
            MelangerGrille();
            for (int i = 0; i < nombrePatientsZero; i++)
            {
                int individuZeroX = random.Next(x);
                int individuZeroY = random.Next(y);
                grille[individuZeroX, individuZeroY] = stade.stade0;
            }

        }
        static int[] VoisinsJDLV(int x, int y, int rang) // renvoi le nombre de voisins vivants autour d'une coordonnée donnée sous forme d'un tableau comprenant le numéro de la population et le nombre de voisins vivants de cette populations autour de la cellule observée.
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
        static int VoisinsCovid(int x, int y) // renvoi le nombre de voisins vivants autour d'une coordonnée donnée sous forme d'un tableau comprenant le numéro de la population et le nombre de voisins vivants de cette populations autour de la cellule observée.
        {
            //on récupère les dimensions de la grille
            int nLignes = grilleTemp.GetUpperBound(0) + 1; // GetUpperBound => on récupère l'index du dernier élements de la dimension n (ici 0)
            int nCols = grilleTemp.GetUpperBound(1) + 1;
            int voisins = 0;
            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int j = y - 1; j <= y + 1; j++)
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
                    if (grilleTemp[ti, tj] == stade.sain) //si la case est saine ajoute 1 dans l'index correspondant à sa couleur
                    {
                        voisins++;
                    }
                }
            }// on retire la cellule en question du comptage;
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
        static int[] EvoluerJDLV(int x, int y)
        {
            int[] vie = new int[2];
            int[] voisins = VoisinsJDLV(x, y, 1);
            int[] voisins2 = VoisinsJDLV(x, y, 2);

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
        static void EvoluerCovid(int x, int y, double tauxContamination, double[] poidsStats, double tauxGuerison)
        {
            if(gen == stadeDeconfinement && grille[x,y] == stade.confine){ // si la génération actuelle est celle où on commence le déconfinement et que la case était confinée, on la met en saine et non immunisée
                    grilleTemp[x,y] = stade.sain;
            }
            else if (grille[x, y] == stade.stade0 || grille[x, y] == stade.stade1 || grille[x, y] == stade.stade2 || grille[x, y] == stade.stade3) // si la cellule est malade...
            {
                int[][] contaminer = Contaminer(x, y, tauxContamination);
                for (int i = 0; i < contaminer.Length; i++) // infecte les cellules autour d'elles qui doivent être contaminées (coordonnées envoyées par la fonction Contaminer)
                {
                    if (contaminer[i][0] != -1 && contaminer[i][1] != -1)
                    {
                        grilleTemp[contaminer[i][0], contaminer[i][1]] = stade.stade0;
                    }
                }
                if (grille[x, y] == stade.stade0) // si elle est au stade 1
                {
                    
                    if (guerisonGrille[x, y] != 5) // si on est dans le stade de guerison
                    {
                        guerisonGrille[x, y]--;
                        if (guerisonGrille[x, y] == 0)
                        {
                            grilleTemp[x, y] = stade.immunise;
                        }
                    }
                    else // sinon...
                    {
                        if (Chance(poidsStats[0])) // si on la fonction Chance dit de passer au stade superieur
                        {
                            grilleTemp[x, y] = stade.stade1; // passe au stade suivant
                            guerisonGrille[x,y] += 5; // ajoute 5 tours de guérison
                        }
                        else
                        {
                            if (Chance(tauxGuerison)) // si la fonction Chance dit de commencer la guérison
                            {
                                guerisonGrille[x, y]--; //on enlève 1 au nombre de tours restants pour guérir
                            }
                        }
                    }
                }
                else if (grille[x, y] == stade.stade1) // même principe que quand la grille est au stade 0
                {
                    if (guerisonGrille[x, y] != 10)
                    {
                        guerisonGrille[x, y]--;
                        if (guerisonGrille[x, y] == 0)
                        {
                            grilleTemp[x, y] = stade.immunise;
                        }
                    }
                    else
                    {
                        if (Chance(poidsStats[1]))
                        {
                            grilleTemp[x, y] = stade.stade2;
                            guerisonGrille[x,y] += 5;
                        }
                        else
                        {
                            if (Chance(tauxGuerison))
                            {
                                guerisonGrille[x, y]--;
                            }
                        }
                    }
                    
                }
                else if (grille[x, y] == stade.stade2) // même principe que quand la grille est au stade 0
                {
                    if (guerisonGrille[x, y] != 15)
                    {
                        guerisonGrille[x, y]--;
                        if (guerisonGrille[x, y] == 0)
                        {
                            grilleTemp[x, y] = stade.immunise;
                        }
                    }
                    else
                    {
                        if (Chance(poidsStats[2]))
                        {
                            grilleTemp[x, y] = stade.stade3;
                            guerisonGrille[x,y] += 5;
                        }
                        else
                        {
                            if (Chance(tauxGuerison))
                            {
                                guerisonGrille[x, y]--;
                            }
                        }
                    }
                    
                }
                else if (grille[x, y] == stade.stade3) // même principe que quand la grille est au stade 0
                {
                    if (guerisonGrille[x, y] != 20)
                    {
                        guerisonGrille[x, y]--;
                        if (guerisonGrille[x, y] == 0)
                        {
                            grilleTemp[x, y] = stade.immunise;
                        }
                    }
                    else
                    {
                        if (Chance(poidsStats[3]))
                        {
                            grilleTemp[x, y] = stade.mort;
                            guerisonGrille[x,y] += 5;
                        }
                        else
                        {
                            if (Chance(tauxGuerison))
                            {
                                guerisonGrille[x, y]--;
                            }
                        }
                    }
                    
                }
            }
        }
        static int[][] Contaminer(int x, int y, double tauxContamination)
        {
            int[][] res = new int[8][];
            for (int i = 0; i < 8; i++)
            {
                res[i] = new int[2] { -1, -1 };

            }

            if (grille[x, y] != stade.mort && grille[x, y] != stade.immunise && grille[x, y] != stade.confine)
            {
                int nLignes = grille.GetUpperBound(0) + 1; // GetUpperBound => on récupère l'index du dernier élements de la dimension n (ici 0)
                int nCols = grille.GetUpperBound(1) + 1;
                int voisins = VoisinsCovid(x, y);

                int n = 0;
                for (int i = x - 1; i <= x + 1; i++)
                {
                    for (int j = y - 1; j <= y + 1; j++)
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
                        if (grilleTemp[ti, tj] == stade.sain) // si la case est saine
                        {
                            bool contamine = Chance(tauxContamination); // on utilise la fonction Chance pour savoir si on contamine cette cellule
                            if (contamine) // si on la contamine on met dans la case d'index "n" les coordonnées de la case à contaminer sous forme d'un tableau
                            {
                                res[n][0] = ti;
                                res[n][1] = tj;
                                n++; //on ajoute 1 à "n" pour que la prochaine cellule à contaminer ne prène pas la place de la précédente
                            }
                        }
                    }
                }
            }
            return res;
        }
        static bool Chance(double proba)
        {
            bool chance = false;
            int alea = 0;
            if (proba == 0) // si proba = 0 (certitude d'échec), alea = -1
            {
                alea = -1;
            }
            else // on multiplie la proba par 1000 et on tire une valeur aléatoire entre 1 et 1000
            {
                alea = random.Next(1000)+1;
            }
            chance = (alea < proba*1000) ? true : false; // si alea < proba*1000 on renvoi true, sinon on renvoi false

            return chance;
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
        static int[] Population()
        {
            int[] pop = new int[8];
            foreach (int item in grille)
            {
                if (item == stade.sain)
                {
                    pop[0] += 1;
                }
                else if (item == stade.stade0)
                {
                    pop[1] += 1;
                }
                else if (item == stade.stade1)
                {
                    pop[2] += 1;
                }
                else if (item == stade.stade2)
                {
                    pop[3] += 1;
                }
                else if (item == stade.stade3)
                {
                    pop[4] += 1;
                }
                else if (item == stade.mort)
                {
                    pop[5] += 1;
                }
                else if (item == stade.immunise)
                {
                    pop[6] += 1;
                }
                else if (item == stade.confine)
                {
                    pop[7] += 1;
                }
            }
            return pop;
        }
    }
}