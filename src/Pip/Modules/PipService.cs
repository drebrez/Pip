using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using Autofac.Features.OwnedInstances;
using Caliburn.Micro;
using Dapplo.Addons;
using Dapplo.Windows.Desktop;
using Dapplo.Windows.Input.Keyboard;
using Pip.Configuration;
using Pip.Ui;
using Pip.Ui.ViewModels;

namespace Pip.Modules
{
    [Service(nameof(PipService), TaskSchedulerName = "ui")]
    public class PipService : IStartup
    {
        private readonly IPipConfiguration _pipConfiguration;
        private readonly LocationPool _locationPool;
        private readonly Dictionary<IntPtr, ThumbnailForm> _thumbnailForms = new Dictionary<IntPtr, ThumbnailForm>();

        private readonly IEventAggregator _eventAggregator;
        private readonly Func<Owned<StartupReadyToastViewModel>> _startupReadyToastViewModelFactory;

        public PipService(
            IPipConfiguration pipConfiguration,
            LocationPool locationPool,
            IEventAggregator eventAggregator,
            Func<Owned<StartupReadyToastViewModel>> startupReadyToastViewModelFactory)
        {
            _pipConfiguration = pipConfiguration;
            _locationPool = locationPool;
            _eventAggregator = eventAggregator;
            _startupReadyToastViewModelFactory = startupReadyToastViewModelFactory;
        }
        public void Startup()
        {
            var uiSynchronizationContext = SynchronizationContext.Current;

            var keyHandler = new KeyCombinationHandler(_pipConfiguration.HotKey)
            {
                CanRepeat = false,
                IsPassThrough = false
            };

            KeyboardHook.KeyboardEvents
                .Where(keyHandler)
                .SubscribeOn(uiSynchronizationContext)
                .ObserveOn(uiSynchronizationContext)
                .Subscribe(keyboardHookEventArgs =>
            {
                // Get the current active window
                var pipSource = InteropWindowQuery.GetForegroundWindow();
                while (pipSource.GetParent() != IntPtr.Zero)
                {
                    pipSource = InteropWindowFactory.CreateFor(pipSource.GetParent());
                }

                // If there is already a form, close it and remove it from the dictionary
                if (_thumbnailForms.TryGetValue(pipSource.Handle, out var thumbnailForm))
                {
                    thumbnailForm.Close();
                    _thumbnailForms.Remove(pipSource.Handle);
                    return;
                }

                // Check if we have a location available
                if (!_locationPool.HasAvailable)
                {
                    return;
                }
                thumbnailForm = new ThumbnailForm(_pipConfiguration, _locationPool, pipSource.Handle, uiSynchronizationContext);
                _thumbnailForms[pipSource.Handle] = thumbnailForm;
                thumbnailForm.Show();
            });

            var message = _startupReadyToastViewModelFactory();
            // Prepare to dispose the view model parts automatically if it's finished
            EventHandler<DeactivationEventArgs> disposeHandler = null;
            disposeHandler = (sender, args) =>
            {
                message.Value.Deactivated -= disposeHandler;
                message.Dispose();
            };
            message.Value.Deactivated += disposeHandler;

            // Show the ViewModel as toast 
            _eventAggregator.PublishOnCurrentThread(message.Value);
            _eventAggregator.PublishOnCurrentThread(message.Value);
            _eventAggregator.PublishOnCurrentThread(message.Value);
            _eventAggregator.PublishOnCurrentThread(message.Value);
            _eventAggregator.PublishOnCurrentThread(message.Value);
            _eventAggregator.PublishOnCurrentThread(message.Value);
            _eventAggregator.PublishOnCurrentThread(message.Value);
            _eventAggregator.PublishOnCurrentThread(message.Value);
            _eventAggregator.PublishOnCurrentThread(message.Value);
            _eventAggregator.PublishOnCurrentThread(message.Value);
        }
    }
}
