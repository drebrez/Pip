using Dapplo.CaliburnMicro.Toasts.ViewModels;
using Pip.Configuration;

namespace Pip.Ui.ViewModels
{
    /// <summary>
    /// The ViewModel for the startup toast
    /// </summary>
    public class StartupReadyToastViewModel : ToastBaseViewModel
    {
        /// <summary>
        ///     construct the ViewModel
        /// </summary>
        /// <param name="pipTranslations">IPipTranslations</param>
        public StartupReadyToastViewModel(IPipTranslations pipTranslations)
        {
            PipTranslations = pipTranslations;
        }

        /// <summary>
        ///     Used from the View
        /// </summary>
        public IPipTranslations PipTranslations { get; }
    }
}