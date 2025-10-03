using FTP_Client.Enums;
using FTP_Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FTP_Client.ViewModels
{
    public class FilePanelViewModel : ObservableObject
    {
        public ContextMenu ContextMenu { get; set; }

        public string Title { get; }

        public event Action<string> NavigateRequested;

   

        public ObservableCollection<FileItem> FilesAndFolders { get; } = new();

        private FileItem _selectedFileItem;
        public FileItem SelectedFileItem
        {
            get => _selectedFileItem;
            set => SetProperty(ref _selectedFileItem, value);
        }

        private string _currentPath;
        public string CurrentPath
        {
            get => _currentPath;
            set => SetProperty(ref _currentPath, value);
        }

        public Stack<string> BackStack { get; } = new();
        public Stack<string> ForwardStack { get; } = new();

        //public ICommand NavigateBackCommand { get; }
        //public ICommand NavigateForwardCommand { get; }
        //public ICommand RefreshCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand GoForwardCommand { get; }
        public ICommand GoHomeCommand { get; }
        public ICommand ItemDoubleClickCommand { get; }

        public FilePanelViewModel(string title)
        {
            Title = title;
            GoBackCommand = new RelayCommand(_ => GoBack(), _ => CanGoBack());
            GoForwardCommand = new RelayCommand(_ => GoForward(), _ => CanGoForward());
            ItemDoubleClickCommand = new RelayCommand(_ => NavigateToSelected(), _ => CanNavigateToSelected());

            var homePath = title == "Локальный диск" ? MainViewModel.LocalRootPath : "/";
            GoHomeCommand = new RelayCommand(_ => NavigateHome(homePath));
        }


        private void OnItemDoubleClick(object parameter)
        {
            if (SelectedFileItem.IsDirectory)
            {
                BackStack.Push(CurrentPath);
                var newPath = Path.Combine(CurrentPath, SelectedFileItem.FileName);
                // Вызываем событие, чтобы MainViewModel выполнил навигацию
                NavigateRequested?.Invoke(newPath);
            }
            else
            {
                // Логика для двойного клика по файлу (например, просмотр)
            }
        }

        private bool CanGoBack() => BackStack.Count > 0;
        private void GoBack()
        {
            if (BackStack.Count > 0)
            {
                ForwardStack.Push(CurrentPath);
                var path = BackStack.Pop();
                NavigateRequested?.Invoke(path); 
            }
        }

        private bool CanGoForward() => ForwardStack.Count > 0;
        private void GoForward()
        {
            if (ForwardStack.Count > 0)
            {
                BackStack.Push(CurrentPath);
                var path = ForwardStack.Pop();
                NavigateRequested?.Invoke(path); 
            }
        }

        private void NavigateHome(string homePath)
        {
            BackStack.Clear();
            ForwardStack.Clear();
            NavigateRequested?.Invoke(homePath);
        }

        private bool CanNavigateToSelected() => SelectedFileItem != null && SelectedFileItem.IsDirectory;
        private void NavigateToSelected()
        {
            if (SelectedFileItem.IsDirectory)
            {
                BackStack.Push(CurrentPath);
                ForwardStack.Clear();

                var newPath = Path.Combine(CurrentPath, SelectedFileItem.FileName);
                NavigateRequested?.Invoke(newPath); 
            }
        }



        public void LoadItems(IEnumerable<FileItem> items, string newPath)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilesAndFolders.Clear();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        FilesAndFolders.Add(item);
                    }
                }
            });

            CurrentPath = newPath;

            //(GoBackCommand as RelayCommand)?.RaiseCanExecuteChanged();
            //(GoForwardCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }


        public void ClearFiles()
        {
            FilesAndFolders.Clear();
        }

        public void AddFile(FileItem file)
        {
            FilesAndFolders.Add(file);
        }
    }
}