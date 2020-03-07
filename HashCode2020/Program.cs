using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HashCode2020
{
    class Program
    {
        static public Dictionary<int, Book> Books;
        static public Dictionary<int, Library> Libraries;
        static public List<Library> SortedLibraries;

        static public int BookCount;
        static public int LibraryCount;
        static public int DayCount;

        static void Main(string[] args)
        {
            Books = new Dictionary<int, Book>();
            Libraries = new Dictionary<int, Library>();

            string[] inputs = new string[] 
            {
                //@"c:\hashcode\a_example.txt",
                //@"c:\hashcode\b_read_on.txt",
                @"c:\hashcode\c_incunabula.txt",
                //@"c:\hashcode\d_tough_choices.txt",
                //@"c:\hashcode\e_so_many_books.txt",
                //@"c:\hashcode\f_libraries_of_the_world.txt"
            };

            foreach (string str in inputs)
            {
                Read(str);
                int score = Score(str);
                Console.WriteLine(score);
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static int Score(string inputPath)
        {
            int totalScore = 0;
            int[] scoreAtDay = new int[DayCount + 1];
            int[] librarySignUpAtDay = Enumerable.Repeat(-1, DayCount).ToArray();

            List<int> librariesNotSignedUp = SortedLibraries.Select(x => x.Id).ToList();
            librariesNotSignedUp = librariesNotSignedUp.GetRange(0, 10);
            List<int> librariesSignedUp = new List<int>();
            
            bool isLibraryBeingSignedUp = false;
            
            for (int D = 0; D < DayCount; D++)
            {
                totalScore += scoreAtDay[D];

                #region Check for libraries finished signing up
                if (librarySignUpAtDay[D] > -1)
                {
                    int libraryId = librarySignUpAtDay[D];
                    librariesSignedUp.Add(libraryId);
                    isLibraryBeingSignedUp = false;
                }
                #endregion

                // Scan books from each library
                foreach (int libraryId in librariesSignedUp)
                {
                    Library library = Libraries[libraryId];
                    // scan some books
                    int score = Scan(library);
                    scoreAtDay[D + 1] += score;
                }

                // Sign up new library
                if (!isLibraryBeingSignedUp)
                {
                    isLibraryBeingSignedUp = true;
                    int libraryId = -1;
                    bool hasTimeToSignup = false;

                    while (librariesNotSignedUp.Count != 0)
                    {
                        // TODO: Pick the next library in a smarter way
                        libraryId = librariesNotSignedUp.First();
                        Library library = Libraries[libraryId];
                        librariesNotSignedUp.Remove(libraryId);

                        // Is there time to signup library?
                        if (library.SignupDays <= DayCount - D)
                        {
                            hasTimeToSignup = true;
                            break;
                        }
                        else
                        {
                            // We have sorted the libraries by signup, so no other library will have time
                            break;
                        }
                    }

                    if (hasTimeToSignup)
                    {
                        int signupDays = Libraries[libraryId].SignupDays;

                        if (signupDays == 0)
                        {
                            // TODO : is this possible?
                            throw new Exception("0 signup days???");
                        }
                        else
                        {
                            // At D + signupDays, libraryId will be signed up
                            librarySignUpAtDay[D + signupDays] = libraryId;
                        }
                    }
                }
            }

            #region OUTPUT

            string outputPath = inputPath.Replace(".txt", "_out.txt");
            File.Delete(outputPath);
            using (StreamWriter streamWriter = new StreamWriter(outputPath))
            {
                List<int> removeTheseIds = new List<int>();
                foreach(int id in librariesSignedUp)
                {
                    Library library = Libraries[id];
                    if (library.ScannedBooks.Count == 0)
                    {
                        removeTheseIds.Add(id);
                    }
                }

                foreach(int id in removeTheseIds)
                {
                    librariesSignedUp.Remove(id);
                }


                // The number of libraries signed up
                int signUpTotal = librariesSignedUp.Count;
                streamWriter.WriteLine(signUpTotal);

                // The order of libraries signed up
                foreach (int libraryId in librariesSignedUp)
                {
                    Library library = Libraries[libraryId];
                    int bookScanCount = library.ScannedBooks.Count;
                    streamWriter.WriteLine($"{libraryId} {bookScanCount}");

                    string bookIdOrder = string.Join(" ", library.ScannedBooks);
                    streamWriter.WriteLine(bookIdOrder);
                }
            }

            #endregion

            return totalScore;
        }

        static int Scan(Library library)
        {
            int score = 0;

            int bookCount = library.BooksScannedPerDay;

            for (int i = 0; i < bookCount; i++)
            {
                // TODO: choose a bookId better
                if (library.Books.Count > 0)
                {
                    int bookId = library.Books.First();
                    RemoveBookFromAllLibraries(bookId);
                    int bookValue = Books[bookId].Score;
                    library.ScannedBooks.Add(bookId);
                    score += bookValue;
                }
            }

            return score;
        }

        static void RemoveBookFromAllLibraries(int bookId)
        {
            Book book = Books[bookId];

            foreach (Library library in book.Libraries)
            {
                library.Books.Remove(bookId);
            }
        }

        static void Read(string fileName)
        {
            Libraries.Clear();
            Books.Clear();
            using (StreamReader streamReader = new StreamReader(fileName))
            {
                // Books Count, Libraries Count, Days Count
                string line = streamReader.ReadLine();
                string[] counts = line.Split(' ');
                BookCount = Convert.ToInt32(counts[0]);
                LibraryCount = Convert.ToInt32(counts[1]);
                DayCount = Convert.ToInt32(counts[2]);

                // Book values
                line = streamReader.ReadLine();
                List<string> bookList = line.Split(' ').ToList();
                for(int i = 0; i < bookList.Count; i++)
                {
                    int score = Convert.ToInt32(bookList[i]);
                    Book book = new Book(i, score);
                    Books.Add(i, book);
                }

                // Libraries
                for (int i = 0; i < LibraryCount; i++)
                {
                    // Book Count, Signup, Ship 
                    line = streamReader.ReadLine();
                    string[] libraryInfo = line.Split(' ');
                    int bookCount = Convert.ToInt32(libraryInfo[0]);
                    int signup = Convert.ToInt32(libraryInfo[1]);
                    int ship = Convert.ToInt32(libraryInfo[2]);

                    // Books in library
                    line = streamReader.ReadLine();
                    List<string> libraryBookList = line.Split(' ').ToList();
                    
                    List<int> libraryBookIdList = libraryBookList.Select(x => Convert.ToInt32(x)).ToList();
                    HashSet<int> bookIdHash = new HashSet<int>(libraryBookIdList);

                    Library library = new Library(i, signup, ship, bookIdHash);
                    foreach(int id in bookIdHash)
                    {
                        Book book = Books[id];
                        book.Libraries.Add(library);
                    }


                    Libraries.Add(i, library);
                }
                // SORT LIB DICTIONARY
                SortedLibraries = new List<Library>();
                SortedLibraries.AddRange(Libraries.Values);
                SortedLibraries.Sort(delegate (Library a, Library b) { return a.SignupDays.CompareTo(b.SignupDays); });


            }
        }
    }

    class Book
    {
        public int Id;
        public int Score;
        public List<Library> Libraries;

        public Book(int id, int score)
        {
            Libraries = new List<Library>();
            Id = id;
            Score = score;
        }
    }

    class Library
    {
        public int Id;
        public int SignupDays;
        public int BooksScannedPerDay; // books shipped per day
        public HashSet<int> Books;

        public HashSet<int> ScannedBooks;


        public Library(int id, int signUpDays, int booksScannedPerDay, HashSet<int> books)
        {
            Id = id;
            SignupDays = signUpDays;
            BooksScannedPerDay = booksScannedPerDay;
            Books = books;
            ScannedBooks = new HashSet<int>();
        }
    }

}
