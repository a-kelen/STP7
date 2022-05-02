
using stp7Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            year.ItemsSource = Enumerable.Range(DateTime.Now.Year - 250,  251).ToList();

        }

        private stp7Entities dataService; 
       
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataService = new stp7Entities(new Uri("https://localhost:44354/WcfDataService.svc/"));
            var list = dataService.Book.Execute().ToList();
            list.Reverse();
            table.ItemsSource = list;
        }

        List<Author> newAuthors = new List<Author>();
        private void add_author_Click(object sender, RoutedEventArgs e)
        {
            string authorName = author_name.Text;
            if (string.IsNullOrWhiteSpace(authorName)) return;

            newAuthors.Add(new Author { name = authorName });
            authors_count.Content = newAuthors.Count.ToString() + " author(s)";
            author_name.Clear();
        }

        private void add_book_Click(object sender, RoutedEventArgs e)
        {
            string bookName = book_name.Text;
            string bookDescription = book_description.Text;
            string yearString = year.Text;
            int y = 0;
            try
            {
                y = (int)year.SelectedValue;
            } catch
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(bookName)) return;

            Book book = new Book { name = bookName, descrition = bookDescription, year = y };
            AddBook(book);
            Clear();
            Refresh();

        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();   
        }

        private void Clear()
        {
            book_name.Clear();
            book_description.Clear();
            year.SelectedItem = 2022;
            author_name.Clear();
            newAuthors.Clear();
            authors_count.Content = "";
        }

        private void AddBook(Book book)
        {
            dataService.AddToBook(book);
            dataService.SaveChanges();
            foreach(var a in newAuthors)
            {
                a.book_id = book.id;
                dataService.AddToAuthor(a);
            }
            dataService.SaveChanges();
        }
        
        private void Refresh()
        {
            var list = dataService.Book.Execute().ToList();
            list.Reverse();
            table.ItemsSource = list;
        }

        private void table_Selected(object sender, RoutedEventArgs e)
        {
            Book book = table.SelectedItem as Book;
            
            if (book != null)
            {
                selected_book.Text = book.name;
                selected_year.Text = book.year.ToString();
                var authors = dataService.Execute<Author>(new Uri($"Author?$filter=book_id eq {book.id}", UriKind.Relative), "GET", true).ToList();
                authors_string.Text = "Authors: " + string.Join(", ", authors.Select(x => x.name));
            }
        }
    }
}
