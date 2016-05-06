using System;
using System.Collections.Specialized;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

namespace PackageManagerWpf
{
    public class PackageManagerViewExtension : IViewExtension
    {
        public delegate void RequestPackagePublishDialogHandler(PublishPackageViewModel publishViewModel);

        private DynamoViewModel dynamoViewModel;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string UniqueId { get; }
        public string Name { get; }
        public void Startup(ViewStartupParams p)
        { 
            throw new NotImplementedException();
        }

        public void Loaded(ViewLoadedParams p)
        {

        }

        public void Shutdown()
        {
            dynamoViewModel.RequestPackagePublishDialog -= DynamoViewModelRequestRequestPackageManagerPublish;
            dynamoViewModel.RequestManagePackagesDialog -= DynamoViewModelRequestShowInstalledPackages;
            dynamoViewModel.RequestPackageManagerSearchDialog -= DynamoViewModelRequestShowPackageManagerSearch;
            dynamoViewModel.RequestPackagePathsDialog -= DynamoViewModelRequestPackagePaths;
        }
    }
}
