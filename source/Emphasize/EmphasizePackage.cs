using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Emphasize;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(PackageGuidString)]
[ProvideOptionPage(typeof(OptionsPage), "Emphasize", "General", 0, 0, true)]
[ProvideService(typeof(OptionsProvider), IsAsyncQueryable = true)]
public sealed class EmphasizePackage : AsyncPackage {

    public const string PackageGuidString = "e2c482dd-e4b3-435a-afeb-65b7c747882d";


    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress) {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        AddService(
            typeof(OptionsProvider),
            async (container, cancellation, token) => {
                await JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
                return new OptionsProvider((OptionsPage)GetDialogPage(typeof(OptionsPage)));
            },
            true
        );
    }

}
