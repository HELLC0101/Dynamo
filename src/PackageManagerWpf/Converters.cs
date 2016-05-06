using System;
using System.Globalization;
using System.Windows.Data;
using Dynamo.PackageManager;
using Dynamo.Wpf.Properties;

namespace DynamoPackagesWpf
{
    class Converters
    {
        public class PackageSearchStateToStringConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                                  CultureInfo culture)
            {
                if (value is PackageManagerSearchViewModel.PackageSearchState)
                {
                    var st = (PackageManagerSearchViewModel.PackageSearchState)value;

                    if (st == PackageManagerSearchViewModel.PackageSearchState.NoResults)
                    {
                        return Resources.PackageSearchStateNoResult;
                    }
                    else if (st == PackageManagerSearchViewModel.PackageSearchState.Results)
                    {
                        return "";
                    }
                    else if (st == PackageManagerSearchViewModel.PackageSearchState.Searching)
                    {
                        return Resources.PackageSearchStateSearching;
                    }
                    else if (st == PackageManagerSearchViewModel.PackageSearchState.Syncing)
                    {
                        return Resources.PackageSearchStateSyncingWithServer;
                    }
                }

                return Resources.PackageStateUnknown;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return null;
            }
        }

        public class PackageUploadStateToStringConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
              CultureInfo culture)
            {
                if (value is PackageUploadHandle.State)
                {
                    var st = (PackageUploadHandle.State)value;

                    if (st == PackageUploadHandle.State.Compressing)
                    {
                        return Resources.PackageUploadStateCompressing;
                    }
                    else if (st == PackageUploadHandle.State.Copying)
                    {
                        return Resources.PackageUploadStateCopying;
                    }
                    else if (st == PackageUploadHandle.State.Error)
                    {
                        return Resources.PackageUploadStateError;
                    }
                    else if (st == PackageUploadHandle.State.Ready)
                    {
                        return Resources.PackageUploadStateReady;
                    }
                    else if (st == PackageUploadHandle.State.Uploaded)
                    {
                        return Resources.PackageUploadStateUploaded;
                    }
                    else if (st == PackageUploadHandle.State.Uploading)
                    {
                        return Resources.PackageUploadStateUploading;
                    }
                }

                return Resources.PackageStateUnknown;
            }

            public object ConvertBack(object value, Type targetType, object parameter,
              CultureInfo culture)
            {
                return null;
            }
        }

        public class PackageDownloadStateToStringConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
              CultureInfo culture)
            {
                if (value is PackageDownloadHandle.State)
                {
                    var st = (PackageDownloadHandle.State)value;

                    if (st == PackageDownloadHandle.State.Downloaded)
                    {
                        return Resources.PackageDownloadStateDownloaded;
                    }
                    else if (st == PackageDownloadHandle.State.Downloading)
                    {
                        return Resources.PackageDownloadStateDownloading;
                    }
                    else if (st == PackageDownloadHandle.State.Error)
                    {
                        return Resources.PackageDownloadStateError;
                    }
                    else if (st == PackageDownloadHandle.State.Installed)
                    {
                        return Resources.PackageDownloadStateInstalled;
                    }
                    else if (st == PackageDownloadHandle.State.Installing)
                    {
                        return Resources.PackageDownloadStateInstalling;
                    }
                    else if (st == PackageDownloadHandle.State.Uninitialized)
                    {
                        return Resources.PackageDownloadStateStarting;
                    }
                }

                return Resources.PackageStateUnknown;
            }

            public object ConvertBack(object value, Type targetType, object parameter,
              CultureInfo culture)
            {
                return null;
            }
        }
    }
}
