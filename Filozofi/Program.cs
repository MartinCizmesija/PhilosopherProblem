using System;
using MPI;

namespace Filozofi
{
    class Program
    {
        public const string RAZMAK = "\t\t\t\t";
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Vilica lijevaVilica = new Vilica();
                Vilica desnaVilica = new Vilica();
                bool lijeviToken = false;
                bool desniToken = false;
                string pomak = "";

                //Inicijalizacija
                Intracommunicator comm = Communicator.world;
                int procesId = comm.Rank;
                int lijeviSusjed = (procesId - 1);
                int desniSusjed = (procesId + 1);

                if (lijeviSusjed < 0) lijeviSusjed = comm.Size - 1;
                if (desniSusjed > comm.Size - 1) desniSusjed = 0;

                for (int i = 0; i < procesId; ++i) pomak += RAZMAK;

                Console.WriteLine(pomak + "Filozof " + procesId);

                if (procesId == 0)
                {
                    lijevaVilica.ImamVilicu = true;
                    desnaVilica.ImamVilicu = true;
                }
                else if (procesId == (comm.Size - 1))
                {
                    desnaVilica.ImamVilicu = false;
                    lijevaVilica.ImamVilicu = false;
                    lijeviToken = true;
                    desniToken = true;
                }
                else
                {
                    desnaVilica.ImamVilicu = true;
                    lijevaVilica.ImamVilicu = false;
                    lijeviToken = true;
                }

                //beskonacno ponavljaj
                while (true)
                {
                    bool porukaLijevogSusjeda;
                    bool porukaDesnogSusjeda;

                    //Misli
                    Random rand = new Random();
                    int thinkingTime = rand.Next(2, 5);
                    Console.WriteLine(pomak + "Mislim");
                    for (int a = 0; a < thinkingTime; ++a)
                    {
                        porukaLijevogSusjeda = ProvjeriPorukeSusjeda(comm, lijeviSusjed);
                        porukaDesnogSusjeda = ProvjeriPorukeSusjeda(comm, desniSusjed);
                        if (porukaLijevogSusjeda)
                        {
                            comm.Receive<bool>(lijeviSusjed, 0);
                            lijeviToken = true;
                        }
                        if (porukaDesnogSusjeda)
                        {
                            comm.Receive<bool>(desniSusjed, 0);
                            desniToken = true;
                        }

                        if (lijeviToken) ProsljediVilicu(comm, lijevaVilica, lijeviSusjed);

                        if (desniToken) ProsljediVilicu(comm, desnaVilica, desniSusjed);

                        System.Threading.Thread.Sleep(1000);
                    }

                    //Gladan
                    Console.WriteLine(pomak + "Gladan");
                    while (true)
                    {
                        //Provjeri jel netko nesto treba
                        porukaLijevogSusjeda = ProvjeriPorukeSusjeda(comm, lijeviSusjed);
                        porukaDesnogSusjeda = ProvjeriPorukeSusjeda(comm, desniSusjed);
                        if (porukaLijevogSusjeda)
                        {
                            comm.Receive<bool>(lijeviSusjed, 0);
                            lijeviToken = true;
                        }
                        if (porukaDesnogSusjeda)
                        {
                            comm.Receive<bool>(desniSusjed, 0);
                            desniToken = true;
                        }
                        if (lijeviToken) ProsljediVilicu(comm, lijevaVilica, lijeviSusjed);

                        if (desniToken) ProsljediVilicu(comm, desnaVilica, desniSusjed);

                        //Provjerava ima li vilice za jelo
                        if (lijevaVilica.ImamVilicu && desnaVilica.ImamVilicu) break;
                        else
                        {
                            if (!lijevaVilica.ImamVilicu)
                            {
                                if (comm.ImmediateProbe(lijeviSusjed, 1) != null)
                                {
                                    Console.WriteLine(pomak + "Dobivam vilicu od " + lijeviSusjed);
                                    comm.Receive<bool>(lijeviSusjed, 1);
                                    lijevaVilica.ImamVilicu = true;
                                    lijevaVilica.Stanje = StanjeVilice.cista;
                                }
                            }
                            if (!lijevaVilica.ImamVilicu)
                            {
                                if (lijeviToken)
                                {
                                    comm.Send(true, lijeviSusjed, 0);
                                    Console.WriteLine(pomak + "Trazim vilicu od " + lijeviSusjed);
                                    lijeviToken = false;
                                }
                            }

                            if (!desnaVilica.ImamVilicu)
                            {
                                if (comm.ImmediateProbe(desniSusjed, 1) != null)
                                {
                                    Console.WriteLine(pomak + "Dobivam vilicu od " + desniSusjed);
                                    comm.Receive<bool>(desniSusjed, 1);
                                    desnaVilica.ImamVilicu = true;
                                    desnaVilica.Stanje = StanjeVilice.cista;
                                }
                            }
                            if (!desnaVilica.ImamVilicu)
                            {
                                if (desniToken)
                                {
                                    comm.Send(true, desniSusjed, 0);
                                    Console.WriteLine(pomak + "Trazim vilicu od " + desniSusjed);
                                    desniToken = false;
                                }
                            }
                        }

                    }

                    //Jedi
                    Console.WriteLine(pomak + "Jedem ");
                    lijevaVilica.Stanje = StanjeVilice.prljava;
                    desnaVilica.Stanje = StanjeVilice.prljava;

                    rand = new Random();
                    int eatingTime = rand.Next(2500, 5000);
                    System.Threading.Thread.Sleep(eatingTime);

                    //Provjeri treba li netko vilicu
                    porukaLijevogSusjeda = ProvjeriPorukeSusjeda(comm, lijeviSusjed);
                    porukaDesnogSusjeda = ProvjeriPorukeSusjeda(comm, desniSusjed);
                    if (porukaLijevogSusjeda)
                    {
                        comm.Receive<bool>(lijeviSusjed, 0);
                        lijeviToken = true;
                    }
                    if (porukaDesnogSusjeda)
                    {
                        comm.Receive<bool>(desniSusjed, 0);
                        desniToken = true;
                    }

                    if (lijeviToken) ProsljediVilicu(comm, lijevaVilica, lijeviSusjed);

                    if (desniToken) ProsljediVilicu(comm, desnaVilica, desniSusjed);
                }
            }
        }

        static private bool ProvjeriPorukeSusjeda (Intracommunicator comm, int susjed)
        {
            if (comm.ImmediateProbe(susjed, 0) != null) return true;
            else return false;
        }

        static private void ProsljediVilicu (Intracommunicator comm, Vilica vilica, int susjed)
        {
            if (vilica.ImamVilicu)
            {
                if (vilica.Stanje == StanjeVilice.cista) return;
                else if (vilica.Stanje == StanjeVilice.prljava)
                {
                    string pomak = "";
                    for (int i = 0; i < comm.Rank; ++i) pomak += RAZMAK;
                    
                    Console.WriteLine(pomak + "Saljem vilicu " + susjed);

                    vilica.ImamVilicu = false;
                    comm.Send(true, susjed, 1);
                }
            }
        }
    }

    class Vilica 
    {
        StanjeVilice stanje { get; set; }
        bool imamVilicu { get; set; }

        public Vilica()
        {
            stanje = StanjeVilice.prljava;
        }

        public StanjeVilice Stanje
        {
            get { return stanje; }
            set { stanje = value; }
        }

        public bool ImamVilicu
        {
            get { return imamVilicu; }
            set { imamVilicu = value; }
        }
    }

    enum StanjeVilice
    {
        prljava,
        cista
    };
}
