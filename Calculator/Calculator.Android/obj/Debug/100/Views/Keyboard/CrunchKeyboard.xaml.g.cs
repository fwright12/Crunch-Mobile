//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::Xamarin.Forms.Xaml.XamlResourceIdAttribute("Calculator.Droid.Views.Keyboard.CrunchKeyboard.xaml", "Views/Keyboard/CrunchKeyboard.xaml", typeof(global::Calculator.CrunchKeyboard))]

namespace Calculator {
    
    
    [global::Xamarin.Forms.Xaml.XamlFilePathAttribute("C:\\Users\\Flint\\Documents\\Calculator Stuff\\Calculator\\Calculator\\Calculator\\Views\\" +
        "Keyboard\\CrunchKeyboard.xaml")]
    public partial class CrunchKeyboard : global::Xamarin.Forms.TouchableStackLayout {
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Xamarin.Forms.ScrollView Scroll;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Xamarin.Forms.Grid Keypad;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Xamarin.Forms.Grid PermanentKeys;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private global::Xamarin.Forms.Grid ArrowKeys;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        public global::Xamarin.Forms.LongClickableButton DockButton;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "2.0.0.0")]
        private void InitializeComponent() {
            global::Xamarin.Forms.Xaml.Extensions.LoadFromXaml(this, typeof(CrunchKeyboard));
            Scroll = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.ScrollView>(this, "Scroll");
            Keypad = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.Grid>(this, "Keypad");
            PermanentKeys = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.Grid>(this, "PermanentKeys");
            ArrowKeys = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.Grid>(this, "ArrowKeys");
            DockButton = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.LongClickableButton>(this, "DockButton");
        }
    }
}
