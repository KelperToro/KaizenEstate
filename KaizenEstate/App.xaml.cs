using Microsoft.Maui.Controls; 

namespace KaizenEstate
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();
        }

        // В .NET 9 это правильный способ задавать главное окно (вместо MainPage = ...)
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage());
        }
    }
}