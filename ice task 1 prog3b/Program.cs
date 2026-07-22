using System;
using System.Collections;
using System.Collections.Generic;

namespace StudentManagementSystem
{
    // =====================================================================
    // CUSTOM LIST IMPLEMENTATION
    // Array-backed generic list. Does NOT use System.Collections.Generic.List<T>
    // internally - it manages its own backing array and grows it manually,
    // similar to how the real List<T> works under the hood.
    // =====================================================================
    public class CustomList<T> : IEnumerable<T>
    {
        private T[] items;
        private int count;
        private const int DefaultCapacity = 4;

        public CustomList()
        {
            items = new T[DefaultCapacity];
            count = 0;
        }

        public CustomList(int initialCapacity)
        {
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));

            items = new T[initialCapacity == 0 ? DefaultCapacity : initialCapacity];
            count = 0;
        }

        /// <summary>Number of elements currently stored.</summary>
        public int Count => count;

        /// <summary>Current size of the backing array.</summary>
        public int Capacity => items.Length;

        /// <summary>Indexer for direct access, e.g. list[0].</summary>
        public T this[int index]
        {
            get
            {
                CheckIndex(index);
                return items[index];
            }
            set
            {
                CheckIndex(index);
                items[index] = value;
            }
        }

        // ---------- ADD ----------

        /// <summary>Adds an item to the end of the list.</summary>
        public void Add(T item)
        {
            EnsureCapacity(count + 1);
            items[count] = item;
            count++;
        }

        // ---------- INSERT ----------

        /// <summary>Inserts an item at a specific index, shifting later items right.</summary>
        public void Insert(int index, T item)
        {
            if (index < 0 || index > count)
                throw new ArgumentOutOfRangeException(nameof(index), "Insert index is out of range.");

            EnsureCapacity(count + 1);

            // Shift everything from 'index' one place to the right.
            for (int i = count; i > index; i--)
            {
                items[i] = items[i - 1];
            }

            items[index] = item;
            count++;
        }

        // ---------- SEARCH ----------

        /// <summary>Returns the index of the first item matching the predicate, or -1 if not found.</summary>
        public int FindIndex(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            for (int i = 0; i < count; i++)
            {
                if (match(items[i]))
                    return i;
            }
            return -1;
        }

        /// <summary>Returns the first item matching the predicate, or default(T) if not found.</summary>
        public T? Find(Predicate<T> match)
        {
            int index = FindIndex(match);
            return index == -1 ? default : items[index];
        }

        /// <summary>Returns every item matching the predicate as a new CustomList.</summary>
        public CustomList<T> FindAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var result = new CustomList<T>();
            for (int i = 0; i < count; i++)
            {
                if (match(items[i]))
                    result.Add(items[i]);
            }
            return result;
        }

        /// <summary>True if any item matches the predicate.</summary>
        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(items[i], item))
                    return true;
            }
            return false;
        }

        // ---------- REMOVE ----------

        /// <summary>Removes the item at the given index, shifting later items left.</summary>
        public void RemoveAt(int index)
        {
            CheckIndex(index);

            for (int i = index; i < count - 1; i++)
            {
                items[i] = items[i + 1];
            }

            items[count - 1] = default!;
            count--;
        }

        /// <summary>Removes the first item matching the predicate. Returns true if something was removed.</summary>
        public bool Remove(Predicate<T> match)
        {
            int index = FindIndex(match);
            if (index == -1) return false;

            RemoveAt(index);
            return true;
        }

        /// <summary>Removes the first occurrence of a specific item (uses default equality).</summary>
        public bool Remove(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(items[i], item))
                {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < count; i++)
                items[i] = default!;
            count = 0;
        }

        // ---------- SORT ----------

        /// <summary>
        /// Sorts the list in place using a simple insertion sort and the given comparer.
        /// Insertion sort is used deliberately (rather than Array.Sort) to keep the
        /// sorting logic fully self-implemented.
        /// </summary>
        public void Sort(IComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            for (int i = 1; i < count; i++)
            {
                T key = items[i];
                int j = i - 1;

                while (j >= 0 && comparer.Compare(items[j], key) > 0)
                {
                    items[j + 1] = items[j];
                    j--;
                }
                items[j + 1] = key;
            }
        }

        /// <summary>Sorts using a comparison delegate, e.g. list.Sort((a, b) => a.Age - b.Age).</summary>
        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            Sort(Comparer<T>.Create(comparison));
        }

        /// <summary>Sorts using T's natural ordering (T must implement IComparable&lt;T&gt;).</summary>
        public void Sort()
        {
            Sort(Comparer<T>.Default);
        }

        // ---------- HELPERS ----------

        private void EnsureCapacity(int minCapacity)
        {
            if (minCapacity <= items.Length) return;

            int newCapacity = items.Length == 0 ? DefaultCapacity : items.Length * 2;
            if (newCapacity < minCapacity) newCapacity = minCapacity;

            var newArray = new T[newCapacity];
            Array.Copy(items, newArray, count);
            items = newArray;
        }

        private void CheckIndex(int index)
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        public T[] ToArray()
        {
            var result = new T[count];
            Array.Copy(items, result, count);
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
                yield return items[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // =====================================================================
    // STUDENT MODEL
    // =====================================================================
    public class Student : IComparable<Student>
    {
        public string StudentNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Course { get; set; }
        public double AverageMark { get; set; }

        public Student(string studentNumber, string firstName, string lastName, int age, string course, double averageMark)
        {
            StudentNumber = studentNumber;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Course = course;
            AverageMark = averageMark;
        }

        /// <summary>Default ordering: by student number (used when Sort() is called with no comparer).</summary>
        public int CompareTo(Student? other)
        {
            if (other == null) return 1;
            return string.Compare(StudentNumber, other.StudentNumber, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return $"{StudentNumber,-12} {FirstName,-12} {LastName,-14} {Age,3}  {Course,-20} {AverageMark,6:F2}";
        }

        public static string Header()
        {
            return $"{"StudentNo",-12} {"FirstName",-12} {"LastName",-14} {"Age",3}  {"Course",-20} {"Avg",6}\n" +
                   new string('-', 72);
        }
    }

    // =====================================================================
    // PROGRAM - menu-driven console UI
    // =====================================================================
    internal class Program
    {
        private static readonly CustomList<Student> students = new CustomList<Student>();

        private static void Main(string[] args)
        {
            SeedSampleData();

            bool running = true;
            while (running)
            {
                PrintMenu();
                Console.Write("Choose an option: ");
                string? choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1": AddStudent(); break;
                    case "2": InsertStudent(); break;
                    case "3": SearchStudent(); break;
                    case "4": RemoveStudent(); break;
                    case "5": SortStudents(); break;
                    case "6": DisplayAll(); break;
                    case "0": running = false; break;
                    default:
                        Console.WriteLine("Invalid option, please try again.");
                        break;
                }

                Console.WriteLine();
            }

            Console.WriteLine("Goodbye!");
        }

        private static void PrintMenu()
        {
            Console.WriteLine("===== College Student Management System =====");
            Console.WriteLine("1. Add student (append)");
            Console.WriteLine("2. Insert student at position");
            Console.WriteLine("3. Search student");
            Console.WriteLine("4. Remove student");
            Console.WriteLine("5. Sort students");
            Console.WriteLine("6. Display all students");
            Console.WriteLine("0. Exit");
        }

        // ---------- ADD ----------
        private static void AddStudent()
        {
            Student s = ReadStudentFromConsole();
            students.Add(s);
            Console.WriteLine($"Added {s.FirstName} {s.LastName} to the end of the list.");
        }

        // ---------- INSERT ----------
        private static void InsertStudent()
        {
            Console.Write($"Enter position to insert at (0 - {students.Count}): ");
            if (!int.TryParse(Console.ReadLine(), out int position))
            {
                Console.WriteLine("Invalid number.");
                return;
            }

            if (position < 0 || position > students.Count)
            {
                Console.WriteLine("Position out of range.");
                return;
            }

            Student s = ReadStudentFromConsole();

            try
            {
                students.Insert(position, s);
                Console.WriteLine($"Inserted {s.FirstName} {s.LastName} at position {position}.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"Could not insert: {ex.Message}");
            }
        }

        // ---------- SEARCH ----------
        private static void SearchStudent()
        {
            Console.Write("Enter student number or last name to search for: ");
            string? term = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(term))
            {
                Console.WriteLine("Search term cannot be empty.");
                return;
            }

            Student? found = students.Find(s =>
                s.StudentNumber.Equals(term, StringComparison.OrdinalIgnoreCase) ||
                s.LastName.Equals(term, StringComparison.OrdinalIgnoreCase));

            if (found == null)
            {
                Console.WriteLine("No matching student found.");
            }
            else
            {
                Console.WriteLine("Found:");
                Console.WriteLine(Student.Header());
                Console.WriteLine(found);
            }
        }

        // ---------- REMOVE ----------
        private static void RemoveStudent()
        {
            Console.Write("Enter student number to remove: ");
            string? term = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(term))
            {
                Console.WriteLine("Student number cannot be empty.");
                return;
            }

            bool removed = students.Remove(s => s.StudentNumber.Equals(term, StringComparison.OrdinalIgnoreCase));

            Console.WriteLine(removed
                ? $"Student {term} removed."
                : $"No student found with number {term}.");
        }

        // ---------- SORT ----------
        private static void SortStudents()
        {
            Console.WriteLine("Sort by: 1) Student number  2) Last name  3) Age  4) Average mark");
            Console.Write("Choose: ");
            string? option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    students.Sort(); // uses Student.CompareTo -> student number
                    break;
                case "2":
                    students.Sort((a, b) => string.Compare(a.LastName, b.LastName, StringComparison.OrdinalIgnoreCase));
                    break;
                case "3":
                    students.Sort((a, b) => a.Age.CompareTo(b.Age));
                    break;
                case "4":
                    // Descending, highest average first
                    students.Sort((a, b) => b.AverageMark.CompareTo(a.AverageMark));
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    return;
            }

            Console.WriteLine("Students sorted.");
            DisplayAll();
        }

        // ---------- DISPLAY ----------
        private static void DisplayAll()
        {
            if (students.Count == 0)
            {
                Console.WriteLine("No students on record.");
                return;
            }

            Console.WriteLine(Student.Header());
            foreach (Student s in students)
            {
                Console.WriteLine(s);
            }
        }

        // ---------- HELPERS ----------
        private static Student ReadStudentFromConsole()
        {
            Console.Write("Student number: ");
            string studentNumber = Console.ReadLine() ?? "";

            Console.Write("First name: ");
            string firstName = Console.ReadLine() ?? "";

            Console.Write("Last name: ");
            string lastName = Console.ReadLine() ?? "";

            Console.Write("Age: ");
            int.TryParse(Console.ReadLine(), out int age);

            Console.Write("Course: ");
            string course = Console.ReadLine() ?? "";

            Console.Write("Average mark: ");
            double.TryParse(Console.ReadLine(), out double avg);

            return new Student(studentNumber, firstName, lastName, age, course, avg);
        }

        private static void SeedSampleData()
        {
            students.Add(new Student("ST10377152", "Tival", "Naidoo", 21, "IT - Software Dev", 74.5));
            students.Add(new Student("ST10022341", "Amahle", "Dlamini", 20, "Computer Science", 81.2));
            students.Add(new Student("ST10055678", "Jaco", "van der Merwe", 22, "IT - Networking", 63.8));
            students.Add(new Student("ST10099001", "Lindiwe", "Khumalo", 19, "IT - Software Dev", 88.0));
        }
    }
}
