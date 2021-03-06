using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Template10.Common;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Native;
using Unigram.Services;
using Unigram.Views.Settings;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels.Settings
{
    public class SettingsStorageViewModel : TLViewModelBase
    {
        public SettingsStorageViewModel(IProtoService protoService, ICacheService cacheService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(protoService, cacheService, settingsService, aggregator)
        {
            ChangeTtlCommand = new RelayCommand(ChangeTtlExecute);
            ClearCacheCommand = new RelayCommand(ClearCacheExecute);
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            IsLoading = true;

            ProtoService.Send(new GetStorageStatisticsFast(), result =>
            {
                if (result is StorageStatisticsFast stats)
                {
                    BeginOnUIThread(() => StatisticsFast = stats);
                }
            });

            ProtoService.Send(new GetStorageStatistics(25), result =>
            {
                if (result is StorageStatistics stats)
                {
                    BeginOnUIThread(() => Statistics = stats);
                }
            });

            TaskCompleted = true;

            return Task.CompletedTask;
        }

        private static string[] ExcludedFileNames = new[] { Constants.WallpaperFileName };

        public int FilesTtl
        {
            get
            {
                var enabled = CacheService.Options.UseStorageOptimizer;
                var ttl = CacheService.Options.StorageMaxTimeFromLastAccess;

                return CacheService.Options.UseStorageOptimizer ? ttl / 60 / 60 / 24 : 0;
            }
            //set
            //{
            //    Settings.FilesTtl = value;
            //    RaisePropertyChanged();
            //}
        }

        private StorageStatisticsFast _statisticsFast;
        public StorageStatisticsFast StatisticsFast
        {
            get
            {
                return _statisticsFast;
            }
            set
            {
                Set(ref _statisticsFast, value);
            }
        }

        private StorageStatistics _statistics;
        public StorageStatistics Statistics
        {
            get
            {
                return _statistics;
            }
            set
            {
                Set(ref _statistics, value);
                ProcessTotal(value);
            }
        }

        private StorageStatisticsByChat _totalStatistics;
        public StorageStatisticsByChat TotalStatistics
        {
            get
            {
                return _totalStatistics;
            }
            set
            {
                Set(ref _totalStatistics, value);
            }
        }

        private bool _taskCompleted;
        public bool TaskCompleted
        {
            get
            {
                return _taskCompleted;
            }
            set
            {
                Set(ref _taskCompleted, value);
            }
        }

        public RelayCommand ClearCacheCommand { get; }
        private void ClearCacheExecute()
        {
            var statistics = _totalStatistics;
            if (statistics == null)
            {
                return;
            }

            Clear(statistics);
        }

        public async void Clear(StorageStatisticsByChat byChat)
        {
            if (byChat == null)
            {
                return;
            }

            var dialog = new ContentDialogBase();
            var page = new SettingsStorageOptimizationPage(ProtoService, dialog, byChat);
            dialog.Content = page;

            var confirm = await dialog.ShowAsync();
            if (confirm != ContentDialogResult.Primary)
            {
                return;
            }

            var types = page.SelectedItems ?? new FileType[0];
            if (types.IsEmpty())
            {
                return;
            }

            var chatIds = new long[0];
            var excludedChatIds = new long[0];

            if (byChat.ChatId != 0)
            {
                chatIds = new[] { byChat.ChatId };
            }
            else if (byChat != _totalStatistics)
            {
                excludedChatIds = _statistics.ByChat.Select(x => x.ChatId).Where(x => x != 0).ToArray();
            }

            IsLoading = true;
            TaskCompleted = false;

            var response = await ProtoService.SendAsync(new OptimizeStorage(long.MaxValue, 0, int.MaxValue, 0, types, chatIds, excludedChatIds, 25));
            if (response is StorageStatistics statistics)
            {
                Statistics = statistics;
            }

            IsLoading = false;
            TaskCompleted = true;
        }

        private void ProcessTotal(StorageStatistics value)
        {
            var result = new StorageStatisticsByChat();
            result.ByFileType = new List<StorageStatisticsByFileType>();

            foreach (var chat in value.ByChat)
            {
                result.Count += chat.Count;
                result.Size += chat.Size;

                foreach (var type in chat.ByFileType)
                {
                    var already = result.ByFileType.FirstOrDefault(x => x.FileType.TypeEquals(type.FileType));
                    if (already == null)
                    {
                        already = new StorageStatisticsByFileType(type.FileType, 0, 0);
                        result.ByFileType.Add(already);
                    }

                    already.Count += type.Count;
                    already.Size += type.Size;
                }
            }

            TotalStatistics = result;
            IsLoading = false;
        }

        public RelayCommand ChangeTtlCommand { get; }
        private async void ChangeTtlExecute()
        {
            var enabled = CacheService.Options.UseStorageOptimizer;
            var ttl = CacheService.Options.StorageMaxTimeFromLastAccess;

            var dialog = new ContentDialog { Style = BootStrapper.Current.Resources["ModernContentDialogStyle"] as Style };
            var stack = new StackPanel();
            stack.Margin = new Thickness(12, 16, 12, 0);
            stack.Children.Add(new RadioButton { Tag = 3, Content = Locale.Declension("Days", 3), IsChecked = enabled && ttl == 3 * 60 * 60 * 24 });
            stack.Children.Add(new RadioButton { Tag = 7, Content = Locale.Declension("Weeks", 1), IsChecked = enabled && ttl == 7 * 60 * 60 * 24 });
            stack.Children.Add(new RadioButton { Tag = 30, Content = Locale.Declension("Months", 1), IsChecked = enabled && ttl == 30 * 60 * 60 * 24 });
            stack.Children.Add(new RadioButton { Tag = 0, Content = Strings.Resources.KeepMediaForever, IsChecked = !enabled });

            dialog.Title = Strings.Resources.KeepMedia;
            dialog.Content = stack;
            dialog.PrimaryButtonText = Strings.Resources.OK;
            dialog.SecondaryButtonText = Strings.Resources.Cancel;

            var confirm = await dialog.ShowQueuedAsync();
            if (confirm == ContentDialogResult.Primary)
            {
                var mode = 0;
                foreach (RadioButton current in stack.Children)
                {
                    if (current.IsChecked == true)
                    {
                        mode = (int)current.Tag;
                        break;
                    }
                }

                CacheService.Options.StorageMaxTimeFromLastAccess = mode * 60 * 60 * 24;
                CacheService.Options.UseStorageOptimizer = mode > 0;

                RaisePropertyChanged(() => FilesTtl);
            }
        }
    }
}